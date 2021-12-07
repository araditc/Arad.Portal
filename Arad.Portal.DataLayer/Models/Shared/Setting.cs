using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class DatabaseConfig
    {
        public string ConnectionString { get; set; }

        public string DbName { get; set; }
    }
    public class Setting
    {
        public string DateFormat { get; set; }

        public string ServiceName { get; set; }

        public string LogPath { get; set; }

        public short HowManyEmailsSendEachTime { get; set; }

        public short HowManySmsSendEachTime { get; set; }

        public short GetCountLastAddedContentForShowKnowledgeBase { get; set; }
    }
}
