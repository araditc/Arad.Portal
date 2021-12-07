using System.Reflection;
using Arad.Portal.DataLayer.Contracts.General.Notification;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.General.Notification.Mongo;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arad.Portal.EmailNotificationService
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .ConfigureAppConfiguration(config => config.AddUserSecrets(Assembly.GetExecutingAssembly()))
                       .ConfigureServices((hostContext, services) =>
                       {
                           DatabaseConfig databaseConfig = new();
                           hostContext.Configuration.Bind(nameof(DatabaseConfig), databaseConfig);
                           services.AddSingleton(databaseConfig);

                           Setting setting = new();
                           hostContext.Configuration.Bind(nameof(Setting), setting);
                           services.AddSingleton(setting);

                           services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
                           services.AddTransient<NotificationContext, NotificationContext>();
                           services.AddTransient<INotificationRepository, NotificationRepository>();

                           services.AddHostedService<SendEmailWorker>();
                       })
                       .UseWindowsService();
        }
    }
}
