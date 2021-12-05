using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class OTP
    {
        public string Code { get; set; }

        public DateTime ExpirationDate { get; set; }

        public string Mobile { get; set; }

        public bool IsSent { get; set; }
    }
}
