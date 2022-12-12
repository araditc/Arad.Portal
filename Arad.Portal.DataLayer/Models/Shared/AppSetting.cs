using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class AppSetting
    {
        public AppSetting()
        {
            LogConfiguration = new LogConfiguration();
            DatabaseConfig = new DatabaseConfig();
            SmsEndPointConfig = new SmsEndPointConfig();
        }
        public LogConfiguration LogConfiguration { get; set; }
        public string AllowedHosts { get; set; }
        public string[] SupportedCultures { get; set; }
        public DatabaseConfig DatabaseConfig { get; set; }
        public SmsEndPointConfig SmsEndPointConfig { get; set; }
        public string LocalStaticFileStorage { get; set; }
        public string LocalStaticFileShown { get; set; }
        public string IsFirstRun { get; set; }
    }

    public class LogConfiguration
    {
        public string LogFileDirectory { get; set; }
        public string LogFileName { get; set; }
        public string FileSizeLimit { get; set; }
    }

    public class DatabaseConfig
    {
        public string ConnectionString { get; set; }

        public string DbName { get; set; }
    }

    
}
