using System.Reflection;
using Arad.Portal.DataLayer.Contracts.General.Notification;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.General.Notification.Mongo;
using Arad.Portal.SmsNotificationService.Mapping;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;



namespace Arad.Portal.SmsNotificationService
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            IMapper mapper = mapperConfig.CreateMapper();
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
                           services.AddSingleton(mapper);
                           services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
                           services.AddTransient<NotificationContext, NotificationContext>();
                           services.AddTransient<INotificationRepository, NotificationRepository>();

                           services.AddHostedService<SendSmsWorker>();
                       }).UseWindowsService();

        }
    }
}
