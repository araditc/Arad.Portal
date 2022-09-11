using Arad.Portal.DataLayer.Entities.General.User;
using AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Globalization;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Arad.Portal.DataLayer.Contracts.General.User;
using Arad.Portal.DataLayer.Repositories.General.User.Mongo;
using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Repositories.General.Currency.Mongo;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Repositories.General.Language.Mongo;
using Arad.Portal.DataLayer.Contracts.General.Permission;
using Arad.Portal.DataLayer.Repositories.General.Permission.Mongo;
using Arad.Portal.DataLayer.Contracts.General.Role;
using Arad.Portal.DataLayer.Repositories.General.Role.Mongo;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Repositories.Shop.ProductGroup.Mongo;
using Arad.Portal.DataLayer.Repositories.Shop.ProductSpecification.Mongo;
using Arad.Portal.DataLayer.Contracts.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Contracts.Shop.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Contracts.Shop.ProductUnit;
using Arad.Portal.DataLayer.Repositories.Shop.ProductUnit.Mongo;
using Arad.Portal.DataLayer.Contracts.Shop.Promotion;
using Arad.Portal.DataLayer.Repositories.Shop.Promotion.Mongo;
using Arad.Portal.DataLayer.Contracts.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Repositories.Shop.ShoppingCart.Mongo;
using Arad.Portal.DataLayer.Contracts.Shop.Transaction;
using Arad.Portal.DataLayer.Repositories.Shop.Transaction.Mongo;
using Arad.Portal.DataLayer.Repositories.Shop.ProductSpecificationGroup.Mongo;
using Microsoft.AspNetCore.Http;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Arad.Portal.DataLayer.Contracts.General.Notification;
using Arad.Portal.DataLayer.Repositories.General.Notification.Mongo;
using Arad.Portal.DataLayer.Contracts.General.MessageTemplate;
using Arad.Portal.DataLayer.Repositories.General.MessageTemplate.Mongo;
using Arad.Portal.DataLayer.Repositories.General.ContentCategory.Mongo;
using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Contracts.General.Content;
using Arad.Portal.DataLayer.Repositories.General.Content.Mongo;
using Arad.Portal.DataLayer.Contracts.General.Comment;
using Arad.Portal.DataLayer.Repositories.General.Comment.Mongo;
using Arad.Portal.DataLayer.Contracts.General.Menu;
using Arad.Portal.DataLayer.Repositories.General.Menu.Mongo;
using System;
using Microsoft.AspNetCore.CookiePolicy;
using Arad.Portal.DataLayer.Contracts.General.BasicData;
using Arad.Portal.DataLayer.Repositories.General.BasicData.Mongo;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Helpers;
using Arad.Portal.DataLayer.Contracts.General.SystemSetting;
using Arad.Portal.DataLayer.Repositories.General.SystemSetting.Mongo;
using Arad.Portal.DataLayer.Contracts.General.Email;
using Arad.Portal.DataLayer.Repositories.General.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Email.Mongo;
using Arad.Portal.DataLayer.Contracts.General.Error;
using Arad.Portal.DataLayer.Repositories.General.Error.Mongo;
using System.Linq;
using Arad.Portal.DataLayer.Contracts.Shop.Setting;
using Arad.Portal.DataLayer.Repositories.Shop.Setting.Mongo;
using Arad.Portal.DataLayer.Contracts.General.Services;
using Arad.Portal.DataLayer.Repositories.General.Service.Mongo;
using Arad.Portal.DataLayer.Contracts.General.CountryParts;
using Arad.Portal.DataLayer.Repositories.General.CountryParts.Mongo;
using Arad.Portal.DataLayer.Contracts.General.DesignStructure;
using Arad.Portal.DataLayer.Repositories.General.DesignStructure.Mongo;
using Arad.Portal.DataLayer.Contracts.General.SliderModule;
using Arad.Portal.DataLayer.Repositories.General.SliderModule.Mongo;
using Serilog;
using MongoDB.Driver;
using Arad.Portal.DataLayer.Services;
using Lucene.Net.Store;
using Lucene.Net.Index;

namespace Arad.Portal.UI.Shop.Dashboard
{
    public class Startup
    {
        private readonly IWebHostEnvironment _environment;
        public IConfiguration Configuration { get; }
        public static string ApplicationPath { get; set; }

        //private static readonly IBasicDataRepository basicDataRepository;
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            GeneralLibrary.Utilities.Language._hostingEnvironment = env.WebRootPath;
            ApplicationPath = env.ContentRootPath;
            _environment = env;
        }

       
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddDistributedMemoryCache();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<HtmlEncoder>(
                HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.BasicLatin,
                    UnicodeRanges.Arabic }));

            DatabaseConfig databaseConfig = new();
            Configuration.Bind(nameof(DatabaseConfig), databaseConfig);
            services.AddSingleton(databaseConfig);

            Setting setting = new();
            Configuration.Bind(nameof(Setting), setting);
            services.AddSingleton(setting);

            SendSmsConfig sendSmsConfig = new();
            Configuration.Bind(nameof(SendSmsConfig), sendSmsConfig);
            services.AddSingleton(sendSmsConfig);
          
            services.ConfigureApplicationCookie(options =>
            {
                if (!_environment.IsDevelopment())
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                }
            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.Name = ".web.Session";
            });

            services.AddIdentityMongoDbProvider<ApplicationUser, ApplicationRole, string>(identityOptions =>
            {
                identityOptions.Password.RequiredLength = 6;
                identityOptions.Password.RequireLowercase = true;
                identityOptions.Password.RequireUppercase = false;
                identityOptions.Password.RequireNonAlphanumeric = false;
                identityOptions.Password.RequireDigit = true;

            }, mongoIdentityOptions =>
            {
                mongoIdentityOptions.ConnectionString = Configuration["DatabaseConfig:ConnectionString"];
            });

            if (!_environment.IsDevelopment())
            {
                services.AddAntiforgery(options =>
                {
                    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });
            }
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                 .AddCookie(opt =>
                 {
                     //opt.Cookie.HttpOnly = true;
                 });
            services.ConfigureApplicationCookie(options =>
            {
                //options.Cookie.HttpOnly = true;
                options.AccessDeniedPath = "/Account/unAuthorize";
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.Cookie.Name = "IdentityCookie";

            });
           
            services.AddTransient<IAuthorizationHandler, RoleHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Role", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new RoleRequirement());
                });
            });
            services.AddAutoMapper(typeof(Startup));

            //services.AddTransient<IPermissionView, PermissionView>();
            services.AddTransient<RemoteServerConnection>();
            services.AddTransient<CreateNotification>();
          
            
            services.AddProgressiveWebApp();
            AddRepositoryServices(services);
            services.AddTransient<CodeGenerator>();

            #region luceneIndexes

            ServiceProvider sp = services.BuildServiceProvider();
            CheckAndConfigureLuceneIndexs(sp);
            #endregion

        }


        private void CheckAndConfigureLuceneIndexs(ServiceProvider sp)
        {
            DomainContext domainContext = sp.GetService<DomainContext>();
            ContentContext contentContext = sp.GetService<ContentContext>();
            ProductContext productContext = sp.GetService<ProductContext>();
            LuceneService luceneService = sp.GetService<LuceneService>();
            List<DataLayer.Entities.General.Content.Content> contentList = new List<DataLayer.Entities.General.Content.Content>();
            List<DataLayer.Entities.Shop.Product.Product> productList = new List<DataLayer.Entities.Shop.Product.Product>();

            if (domainContext != null)
            {
                var domainIds = domainContext.Collection.Find(_ => _.IsActive && !_.IsDeleted).ToList().Select(_ => _.DomainId);
                var supportedCultures = Configuration.GetSection("SupportedCultures").Get<string[]>().ToList();
                var mainPath = Path.Combine(Configuration["LocalStaticFileStorage"], "LuceneIndexes");

                foreach (var dom in domainIds)
                {
                    var domainEntity = domainContext.Collection.Find(_ => _.DomainId == dom).FirstOrDefault();
                    if (contentContext != null)
                    {
                        // contentList = contentContext.Collection.Find(_ => _.AssociatedDomainId == dom).ToList();
                        //test 
                        contentList = contentContext.Collection.Find(_ => true).ToList();
                    }
                    if (productContext != null)
                    {
                        //if(domainEntity.IsDefault)
                        //{
                        //    productList = productContext.ProductCollection.Find(_ => _.AssociatedDomainId == dom || _.IsPublishedOnMainDomain).ToList();
                        //}else
                        //{
                        //    productList = productContext.ProductCollection.Find(_ => _.AssociatedDomainId == dom).ToList();
                        //}
                        //test
                        productList = productContext.ProductCollection.Find(_ => true).ToList();
                    }
                    var mainDir = Path.Combine(mainPath, dom);
                    List<string> dirs = new List<string>()
                    {
                        Path.Combine(mainDir, "Content")
                    };
                    foreach (var cul in supportedCultures)
                    {
                        dirs.Add(Path.Combine(mainDir, "Product", cul.Trim()));
                    }
                    foreach (var dir in dirs)
                    {
                        if (!System.IO.Directory.Exists(dir))
                        {
                            System.IO.Directory.CreateDirectory(dir);
                        }
                        FSDirectory luceneIndexDirectory = FSDirectory.Open(dir);
                        var isExist = DirectoryReader.IndexExists(luceneIndexDirectory);
                        if (!isExist)
                        {
                            if (luceneService != null)
                                if (dir.Replace("\\", "/").Contains("/Product"))
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
            }
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipelinelucene
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
               
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspuseSnetcore-hsts.
                //app.UseHsts();
            }

            try
            {
                if (!System.IO.Directory.Exists(Configuration["LocalStaticFileStorage"]))
                {
                    System.IO.Directory.CreateDirectory(Configuration["LocalStaticFileStorage"]);
                }
                var path1 = Path.Combine(Configuration["LocalStaticFileStorage"], "images/Contents");
                var path2 = Path.Combine(Configuration["LocalStaticFileStorage"], "images/ProductGroups");
                var path3 = Path.Combine(Configuration["LocalStaticFileStorage"], "images/Products");
                var path4 = Path.Combine(Configuration["LocalStaticFileStorage"], "images/UserProfiles");
                var path5 = Path.Combine(Configuration["LocalStaticFileStorage"], "images/SliderModule");
                var path6 = Path.Combine(Configuration["LocalStaticFileStorage"], "images/DomainDesign");
                var path7 = Path.Combine(Configuration["LocalStaticFileStorage"], "ckEditorContentImages");
                var path8 = Path.Combine(Configuration["LocalStaticFileStorage"], "ckEditorDomainImages");
                var path9 = Path.Combine(Configuration["LocalStaticFileStorage"], "ckEditorProductImages");
                var path10 = Path.Combine(Configuration["LocalStaticFileStorage"], "Log");
                var path11 = Path.Combine(Configuration["LocalStaticFileStorage"], "ProductFiles");
                var path12 = Path.Combine(Configuration["LocalStaticFileStorage"], "LuceneIndexes");
                List<string> pathes = new List<string>() { path1, path2, path3, path4, path5, path6, path7, path8, path9, path10, path11, path12 };

                foreach (var path in pathes)
                {
                    if (!System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"Couldnt Find Or Create one of default directories for storage ex= {ex.ToString()}");
            }
            

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Configuration["LocalStaticFileStorage"]),
                RequestPath = new PathString("/Images"),
                //EnableDirectoryBrowsing = false
            });

            app.UseRequestLocalization(AddMultilingualSettings());
           
            app.UseRouting();
            var options = new CookiePolicyOptions()
            {
                HttpOnly = HttpOnlyPolicy.Always,
                MinimumSameSitePolicy = SameSiteMode.Strict
            };
            app.UseCookiePolicy(options);
           
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseSeedDatabase(ApplicationPath);
        }
        private RequestLocalizationOptions AddMultilingualSettings()
        {
            var supportedCultures = Configuration.GetSection("SupportedCultures")
               .Get<string[]>().Select(x => new CultureInfo(x)).ToList();

            RequestLocalizationOptions options = new RequestLocalizationOptions()
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
                RequestCultureProviders = new List<IRequestCultureProvider>()
                {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider()
                }
            };
            return options;
        }

        private void AddRepositoryServices(IServiceCollection services)
        {

            #region contextes
            services.AddTransient<CurrencyContext>();
            services.AddTransient<DomainContext>();
            services.AddTransient<LanguageContext>();
            services.AddTransient<PermissionContext>();
            services.AddTransient<RoleContext>();
            services.AddTransient<UserContext>();
            //services.AddTransient<OrderContext>();
            services.AddTransient<ProductContext>();
            services.AddTransient<PromotionContext>();
            services.AddTransient<ShoppingCartContext>();
            services.AddTransient<TransactionContext>();
            services.AddTransient<NotificationContext>();
            services.AddTransient<ContentCategoryContext>();
            services.AddTransient<MessageTemplateContext>();
            services.AddTransient<ContentContext>();
            services.AddTransient<CommentContext>();
            services.AddTransient<MenuContext>();
            services.AddTransient<BasicDataContext>();
            services.AddTransient<SystemSettingContext>();
            services.AddTransient<SMTPContext>();
            services.AddTransient<POPContext>();
            services.AddTransient<EmailOptionContext>();
            services.AddTransient<ErrorLogContext>();
            services.AddTransient<ShippingSettingContext>();
            services.AddTransient<ProviderContext>();
            services.AddTransient<CountryContext>();
            services.AddTransient<ModuleContext>();
            services.AddTransient<SliderContext>();
            services.AddTransient<LuceneService>();
            #endregion


            services.AddTransient<ICurrencyRepository, CurrencyRepository>();
            services.AddTransient<IDomainRepository, DomainRepository>();
            services.AddTransient<ILanguageRepository, LanguageRepository>();
            services.AddTransient<IPermissionRepository, PermissionRepository>();
            services.AddTransient<IRoleRepository, RoleRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<UserExtensions, UserExtensions>();
            //services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<IProductGroupRepository, ProductGroupRepository>();
            services.AddTransient<IProductSpecificationRepository, ProductSpecificationRepository>();
            services.AddTransient<IProductSpecGroupRepository, ProductSpecGroupRepository>();
            services.AddTransient<IProductUnitRepository, ProductUnitRepository>();
            services.AddTransient<IPromotionRepository, PromotionRepository>();
            services.AddTransient<IShoppingCartRepository, ShoppingCartRepository>();
            services.AddTransient<ITransactionRepository, TransactionRepository>();
            services.AddTransient<INotificationRepository, NotificationRepository>();
            services.AddTransient<IMessageTemplateRepository, MessageTemplateRepository>();
            services.AddTransient<IContentCategoryRepository, ContentCategoryRepository>();
            services.AddTransient<IContentRepository, ContentRepository>();
            services.AddTransient<ICommentRepository, CommentRepository>();
            services.AddTransient<IMenuRepository, MenuRepository>();
            services.AddTransient<IBasicDataRepository, BasicDataRepository>();
            services.AddTransient<ISystemSettingRepository, SystemSettingRepository>();
            services.AddTransient<ISMTPRepository, SMTPRepository>();
            services.AddTransient<IPOPRepository, POPRepository>();
            services.AddTransient<IEmailOptionRepository, EmailOptionRepository>();
            services.AddTransient<IErrorLogRepository, ErrorLogRepository>();
            services.AddTransient<IShippingSettingRepository, ShippingSettingRepository>();
            services.AddTransient<IProviderRepository, ProviderRepository>();
            services.AddTransient<ICountryRepository, CountryRepository>();
            services.AddTransient<IModuleRepository, ModuleRepository>();
            services.AddTransient<ISliderRepository, SliderRepository>();
           
        }

    }
}
