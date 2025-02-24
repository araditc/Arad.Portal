using Arad.Portal.DataLayer.Entities.General.SMS;
using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Shared;

public class AppSetting
{
    public AppSetting()
    {
        
    }
    public LogConfiguration LogConfiguration { get; set; }

    public DatabaseConfig DatabaseConfig { get; set; } = new DatabaseConfig();
    public List<string> SupportedCultures { get; set; } 
}

public class LogConfiguration
{
    public string LogFileDirectory { get; set; }
    public string LogFileName { get; set; }
    public long? FileSizeLimit { get; set; }
}

public class DatabaseConfig
{
    public string ConnectionString { get; set; }
    public string DbName { get; set; }
}