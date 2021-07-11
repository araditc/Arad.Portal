using System;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers.SMS
{
    public class ResultSMS
    {
        public bool IsSuccessful { get; set; }
        public int StatusCode { get; set; }
        public Guid BatchId { get; set; }
    }
}