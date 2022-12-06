using Arad.Portal.UI.Shop.Dashboard.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Microsoft.AspNetCore.Http;

namespace Arad.Portal.UI.Shop.Dashboard
{
    public class Program
    {
        private static string _path;
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddCommandLine(args)
                .Build();
            var logAddress = config["LogConfiguration:LogFileDirectory"];
            if (!string.IsNullOrWhiteSpace(logAddress))
            {
                if (!Directory.Exists(logAddress))
                {
                    Directory.CreateDirectory(logAddress);
                }
                var fileName = config["LogConfiguration:LogFileName"];
                _path = Path.Combine(logAddress, fileName);

            }
            else
            {

                var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Log", "StoreLog");
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
            var config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true)
               .AddCommandLine(args)
               .Build();

            long fileSizeLimit;
            try
            {
                fileSizeLimit = long.Parse(config["LogConfiguration:FileSizeLimit"]);
            }
            catch (Exception e)
            {
                fileSizeLimit = 10000000;
            }

            return Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<ConfigureMongoDbIndexesService>();
                })
               .UseSerilog(
                   (hostingContext, loggerConfiguration) => loggerConfiguration
                       .MinimumLevel.Information()
                       .WriteTo.File(_path,
                           rollingInterval: RollingInterval.Day,
                           rollOnFileSizeLimit: true,
                           fileSizeLimitBytes: fileSizeLimit));

            
        }
    }
}
