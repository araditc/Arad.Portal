using Arad.Portal.DataLayer.Entities.General.ApplicationRole;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Repositories.Implementations.General.Domain.Mongo;
using Arad.Portal.DataLayer.Repositories.Implementations.General.Language.Mongo;
using Arad.Portal.DataLayer.Repositories.Implementations.General.Notification.Mongo;
using Arad.Portal.DataLayer.Repositories.Implementations.General.SendMessage.Mongo;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Notification;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SendMessage;
using Arad.Portal.DataLayer.Repositories.MongoDbContext;

using Microsoft.AspNetCore.Identity;
using NotificationService.Services;

using Scrutor;

namespace NotificationService;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Arad.Portal.DataLayer.CustomIdentity;
using Arad.Portal.DataLayer.Repositories.Implementations.General.User.Mongo;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Arad.Portal.DataLayer.Shared;
using Arad.Portal.DataLayer.Repositories.Implementations.General.Role.Mongo;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Role;

// Adjust the namespace as necessary

// Adjust the namespace as necessary

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                IConfigurationRoot config = new ConfigurationBuilder()
                                            .SetBasePath(AppContext.BaseDirectory)
                                            .AddJsonFile("appsettings.json", optional: true)
                                            .AddCommandLine(args)
                                            .Build();

                services.AddHttpClient();

                // Register MongoDbContext
                services.AddScoped<IMongoDbContext, MongoDbContext>(_ =>
                                                                        new MongoDbContext(config["DatabaseConfig:ConnectionString"]!, config["DatabaseConfig:DbName"]!)
                );

                // Register repositories
                services.AddScoped<IUserRepository, UserRepository>();

                services.AddScoped<SmsSenderService>();
                services.AddScoped<EmailSenderService>();

                services.Scan(scan => scan.FromAssemblies(typeof(IUserRepository).Assembly)
                                          .AddClasses(filter => filter.Where(x => x.Name.EndsWith("Repository")))
                                          .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                                          .AsMatchingInterface()
                                          .WithTransientLifetime());

                // Register role repository
                services.AddScoped<IRoleRepository, RoleRepository>(); // <-- Register IRoleRepository and its implementation

                // Register custom role and user stores
                services.AddScoped<IUserStore<ApplicationUser>, AradUserStore>()
                        .AddScoped<IRoleStore<ApplicationRole>, AradRoleStore>()
                        .AddMongoDbContext(config["DatabaseConfig:ConnectionString"]!, config["DatabaseConfig:DbName"]!);

                // Register Identity
                services.AddIdentity<ApplicationUser, ApplicationRole>()
                        .AddRoleStore<AradRoleStore>()
                        .AddUserStore<AradUserStore>()
                        .AddDefaultTokenProviders();

                // Configure Identity Options
                services.Configure<IdentityOptions>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 7;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                });

                // Register other services
                services.AddHostedService<Worker>(); // For background tasks

            }).UseWindowsService();
}