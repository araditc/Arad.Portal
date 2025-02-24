using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;

using Arad.Portal.Authorization;
using Arad.Portal.DataLayer.CustomIdentity;
using Arad.Portal.DataLayer.Entities.General.ApplicationRole;
using Arad.Portal.DataLayer.Entities.General.Content;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.SMS;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Helpers;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Implementations.General.Content.Mongo;
using Arad.Portal.DataLayer.Repositories.Implementations.General.Domain.Mongo;
using Arad.Portal.DataLayer.Repositories.Implementations.Shop.Product.Mongo;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;
using Arad.Portal.DataLayer.Services;
using Arad.Portal.DataLayer.Shared;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.Helpers.Admin;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Helpers.UI;
using Arad.Portal.Mapping;
using Arad.Portal.Middlewares;
using Arad.Portal.Scheduling;
using Arad.Portal.Services;

using AutoMapper;

using Lucene.Net.Index;
using Lucene.Net.Store;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Minio;

using MongoDB.Driver;

using Scrutor;

using Serilog;

using Directory = System.IO.Directory;

namespace Arad.Portal;

public class Program
{
    private static string? _path;
    public static readonly ConcurrentDictionary<string, OTP> OTP = new();
    private readonly IWebHostEnvironment _environment;

    public Program(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration;
        UtilityLanguage.HostingEnvironment = Configuration["DictionaryFolderPath"];
        ApplicationPath = env.ContentRootPath;
        _environment = env;
    }

    private IConfiguration Configuration { get; }

    private static string? ApplicationPath { get; set; }

    public static void Main(string[] args)
    {
        IConfigurationRoot config = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json", true)
                                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                                    .AddEnvironmentVariables()
                                    .Build();
        string? logAddress = config["LogConfiguration:LogFileDirectory"];

        if (!string.IsNullOrWhiteSpace(logAddress))
        {
            if (!Directory.Exists(logAddress))
            {
                Directory.CreateDirectory(logAddress);
            }

            string? fileName = config["LogConfiguration:LogFileName"];

            if (fileName != null)
            {
                _path = Path.Combine(logAddress, fileName);
            }
        }
        else
        {
            string dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Log", "StoreLog");

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _path = Path.Combine(dir, "StoreLogs.txt");
        }

        if (!File.Exists(_path))
        {
            File.Create(_path);
        }

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true)
            .AddCommandLine(args)
            .Build();

        const long FILE_SIZE_LIMIT = 10000000;

        return Host.CreateDefaultBuilder(args)
                   .ConfigureWebHostDefaults(webBuilder =>
                                             {
                                                 webBuilder.UseStartup<Program>();
                                             })
                   .ConfigureServices(_ =>
                   {
                       // services.AddHostedService<ConfigureMongoDbIndexesService>();
                   })
                   .UseSerilog(
                       (_, loggerConfiguration) => loggerConfiguration
                                                   .MinimumLevel.Information()
                                                   .Enrich.WithClientIp()
                                                   .WriteTo.File(_path,
                                                                 rollingInterval: RollingInterval.Day,
                                                                 rollOnFileSizeLimit: true,
                                                                 fileSizeLimitBytes: FILE_SIZE_LIMIT));
    }

    public void ConfigureServices(IServiceCollection services)
    {
        try
        {
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(5);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = Guid.NewGuid().ToString();
            });
            services.AddHttpClient();
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<LayoutContentProcess>();
            services.AddScoped<ControllerHelper>();
            services.AddSingleton(
                HtmlEncoder.Create([
                    UnicodeRanges.BasicLatin,
                    UnicodeRanges.Arabic
                ]));
            DatabaseConfig databaseConfig = new();
            Configuration.Bind(nameof(DatabaseConfig), databaseConfig);
            services.AddSingleton(databaseConfig);

            Setting setting = new();
            Configuration.Bind(nameof(Setting), setting);
            services.AddSingleton(setting);

            Sms sendSmsConfig = new();
            Configuration.Bind(nameof(Sms), sendSmsConfig);
            services.AddSingleton(sendSmsConfig);

            services.ConfigureApplicationCookie(options =>
            {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.Name = Guid.NewGuid().ToString();
                    options.Cookie.IsEssential = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    options.LoginPath = "/Shared/Account/Login";
                    options.AccessDeniedPath = "/Shared/Account/AccessDenied";
                    options.SlidingExpiration = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                
            });

            services.AddScoped<IMongoDbContext, MongoDbContext>();

            services.AddIdentity<ApplicationUser, ApplicationRole>().AddDefaultTokenProviders();

            services.AddTransient<IUserStore<ApplicationUser>, AradUserStore>().AddMongoDbContext(Configuration["DatabaseConfig:ConnectionString"], Configuration["DatabaseConfig:DbName"]);

            services.AddSingleton<IRoleStore<ApplicationRole>, AradRoleStore>().AddMongoDbContext(Configuration["DatabaseConfig:ConnectionString"], Configuration["DatabaseConfig:DbName"]);

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 7;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            });

            if (!_environment.IsDevelopment())
            {
                services.AddAntiforgery(_ =>
                {
                    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });
            }

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(_ =>
                               {
                                   _.Cookie.Name = Guid.NewGuid().ToString();
                               });

            services.ConfigureApplicationCookie(options =>
            {
                //options.Cookie.HttpOnly = true;
                options.AccessDeniedPath = "/Shared/Account/unAuthorize";
                options.LoginPath = "/Shared/Account/Login";
                options.LogoutPath = "/Shared/Account/Logout";
                options.Cookie.Name = Guid.NewGuid().ToString();
            });

            services.AddTransient<IAuthorizationHandler, RoleHandler>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Role",
                                  policy =>
                                  {
                                      policy.RequireAuthenticatedUser();
                                      policy.Requirements.Add(new RoleRequirement());
                                  });
            });
            services.AddAutoMapper(typeof(MappingProfile));

            services.AddTransient<RemoteServerConnection>();

            services.AddTransient<CreateNotification>();

            services.AddTransient<IRazorPartialToStringRenderer, RazorPartialToStringRenderer>();

            services.Configure<MinioSetting>(Configuration.GetSection("MinioSetting"));

            services.AddSingleton(provider =>
            {
                MinioSetting minioSettings = provider.GetRequiredService<IOptions<MinioSetting>>().Value;

                IMinioClient minioClient = new MinioClient()
                                           .WithEndpoint(minioSettings.Endpoint, minioSettings.Port)
                                           .WithCredentials(minioSettings.AccessKey, minioSettings.SecretKey)
                                           .Build();

                return minioClient;
            });

            // Register MinIOService
            services.AddSingleton<MinioHelper>();

            services.AddProgressiveWebApp();

            AddRepositoryServices(services);

            services.AddTransient<CodeGenerator>();

            services.AddTransient<CacheCleanerService>();

            services.AddTransient<SharedRuntimeData>();

            ServiceProvider sp = services.BuildServiceProvider();

            CacheCleanerService cacheCleaner = sp.GetService<CacheCleanerService>();

            cacheCleaner?.StartTimer();

            #region luceneIndexes
            CheckAndConfigureLuceneIndexes(sp);
            #endregion
        }
        catch (Exception e)
        {
            Log.Error($"error {e.Message} occured. stack trace: {nameof(Program)}");
        }
    }

    private void CheckAndConfigureLuceneIndexes(ServiceProvider sp)
    {
        try
        {
            DomainRepository domainContext = sp.GetService<DomainRepository>();

            ContentRepository contentContext = sp.GetService<ContentRepository>();

            ProductRepository productContext = sp.GetService<ProductRepository>();

            LuceneService luceneService = sp.GetService<LuceneService>();

            List<Content> contentList = [];

            List<Product> productList = [];

            if (domainContext == null)
            {
                return;
            }

            IEnumerable<string> domainIds = domainContext.Collection.Find(d => d.IsActive && !d.IsDeleted).ToList().Select(d => d.Id);

            List<string> supportedCultures = Configuration.GetSection("SupportedCultures").Get<string[]>().ToList();

            string mainPath = Path.Combine(Configuration["LocalStaticFileStorage"] ?? string.Empty, "LuceneIndexes");

            foreach (string dom in domainIds)
            {
                Domain domainEntity = domainContext.Collection.Find(d => d.Id == dom).FirstOrDefault();

                if (contentContext != null)
                {
                    contentList = contentContext.Collection.Find(c => c.AssociatedDomainId == dom).ToList();

                    //test 
                    //contentList = contentContext.Collection.Find(_ => true).ToList();
                }

                if (productContext != null)
                {
                    productList = domainEntity.IsDefault
                                      ? productContext.Collection.Find(p => p.AssociatedDomainId == dom || p.IsPublishedOnMainDomain).ToList()
                                      : productContext.Collection.Find(p => p.AssociatedDomainId == dom).ToList();

                    //test
                    //productList = productContext.ProductCollection.Find(_ => true).ToList();
                }

                string mainDir = Path.Combine(mainPath, dom);
                List<string> dirs = [Path.Combine(mainDir, "Content")];
                dirs.AddRange(supportedCultures.Select(cul => Path.Combine(mainDir, "Product", cul.Trim())));

                foreach (string dir in dirs)
                {
                    if (!Directory.Exists(dir))
                    {
                        if (dir != null)
                        {
                            Directory.CreateDirectory(dir);
                        }
                    }

                    FSDirectory luceneIndexDirectory = FSDirectory.Open(dir);
                    bool isExist = DirectoryReader.IndexExists(luceneIndexDirectory);

                    if (isExist)
                    {
                        continue;
                    }

                    if (luceneService == null)
                    {
                        continue;
                    }

                    if (dir != null && dir.Replace("\\", "/").Contains("/Product"))
                    {
                        luceneService.BuildProductIndexesPerLanguage(productList, Path.Combine(mainDir, "Product"));
                    }
                    else
                    {
                        luceneService.BuildContentIndexesPerLanguage(contentList, Path.Combine(mainDir, "Content"));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline lucene
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        try
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseHsts();
            }

            if (!Directory.Exists(Configuration["LocalStaticFileStorage"]))
            {
                Directory.CreateDirectory(Configuration["LocalStaticFileStorage"] ?? string.Empty);
            }

            string path1 = Path.Combine(Configuration["LocalStaticFileStorage"] ?? string.Empty, "images/Contents");
            string path2 = Path.Combine(Configuration["LocalStaticFileStorage"] ?? string.Empty, "images/ProductGroups");
            string path3 = Path.Combine(Configuration["LocalStaticFileStorage"] ?? string.Empty, "images/Products");
            string path4 = Path.Combine(Configuration["LocalStaticFileStorage"] ?? string.Empty, "images/UserProfiles");
            string path5 = Path.Combine(Configuration["LocalStaticFileStorage"] ?? string.Empty, "images/SliderModule");
            string path6 = Path.Combine(Configuration["LocalStaticFileStorage"] ?? string.Empty, "images/DomainDesign");
            string path7 = Path.Combine(Configuration["LocalStaticFileStorage"] ?? string.Empty, "ckEditorContentImages");
            string path8 = Path.Combine(Configuration["LocalStaticFileStorage"] ?? string.Empty, "ckEditorDomainImages");
            string path9 = Path.Combine(Configuration["LocalStaticFileStorage"] ?? string.Empty, "ckEditorProductImages");
            string path10 = Path.Combine(Configuration["LocalStaticFileStorage"] ?? string.Empty, "Log/DashboardLogs");
            string path11 = Path.Combine(Configuration["LocalStaticFileStorage"] ?? string.Empty, "Log/StoreLogs");
            string path12 = Path.Combine(Configuration["LocalStaticFileStorage"] ?? string.Empty, "ProductFiles");
            string path13 = Path.Combine(Configuration["LocalStaticFileStorage"] ?? string.Empty, "LuceneIndexes");
            List<string> paths = [path1, path2, path3, path4, path5, path6, path7, path8, path9, path10, path11, path12, path13];

            foreach (string path in paths.Where(path => !Directory.Exists(path)))
            {
                Directory.CreateDirectory(path);
            }
        }
        catch (Exception ex)
        {
            Log.Fatal($"Couldn't Find Or Create one of default directories for storage ex= {ex}");
        }
        app.UseStaticFiles();
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Configuration["LocalStaticFileStorage"] ?? string.Empty),
            RequestPath = new("/Images")

            //EnableDirectoryBrowsing = false
        });

        app.UseRequestLocalization(AddMultilingualSettings());
        app.UseRouting();
        CookiePolicyOptions options = new() { HttpOnly = HttpOnlyPolicy.Always, MinimumSameSitePolicy = SameSiteMode.Strict };
        app.UseCookiePolicy(options);
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHttpsRedirection();
        app.ApplyLanguageMapper();

        if (Convert.ToBoolean(Configuration["IsFirstRun"]))
        {
            app.UseEndpoints(endpoints =>
            {
                // Route for the main site during installation
                endpoints.MapControllerRoute(
                    "default",
                    "{lang}/{controller=Home}/{action=Index}/{id?}",
                    new { area = "" });

                // Specific route for the installation process
                endpoints.MapControllerRoute(
                    "install",
                    "{lang}/Install/Index",
                    new { controller = "Install", action = "Index" });
            });
        }
        else
        {
            app.UseEndpoints(endpoints =>
                             {
                                 // Route for areas (e.g., Admin)
                                 endpoints.MapControllerRoute(
                                     "area",
                                     "{lang}/{area}/{controller=Home}/{action=Index}/{id?}",
                                     constraints: new { lang = new RegexRouteConstraint("^[a-z]{2}-[a-z]{2}$") });

                                 // Route for the main site (Public-facing website)
                                 endpoints.MapControllerRoute(
                                     "default",
                                     "{lang}/{controller=Home}/{action=Index}/{id?}",
                                     new { area = "" },
                                     constraints: new { lang = new RegexRouteConstraint("^[a-z]{2}-[a-z]{2}$") });
                             });
        }

     

        MapperConfiguration mapperConfig = new(cfg => { cfg.AddProfile<MappingProfile>(); });

        mapperConfig.CreateMapper();

        app.UseSeedDatabase(ApplicationPath);
    }

    private RequestLocalizationOptions AddMultilingualSettings()
    {
        List<CultureInfo> supportedCultures = (Configuration.GetSection("SupportedCultures")
                                                            .Get<string[]>() ?? [])
                                                           .Select(x => new CultureInfo(x))
                                                           .ToList();

        RequestLocalizationOptions options = new()
        {
            DefaultRequestCulture = new("en-US"),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures,
            RequestCultureProviders = new List<IRequestCultureProvider> { new QueryStringRequestCultureProvider(), new CookieRequestCultureProvider() }
        };

        return options;
    }

    private void AddRepositoryServices(IServiceCollection services)
    {
        services.AddTransient<LuceneService>();
        services.Scan(scan => scan.FromAssemblies(typeof(IUserRepository).Assembly)
                                  .AddClasses(filter => filter.Where(x => x.Name.EndsWith("Repository")))
                                  .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                                  .AsMatchingInterface()
                                  .WithTransientLifetime());
    }
}
