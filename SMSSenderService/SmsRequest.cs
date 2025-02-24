namespace Arad.Portal.Shared;

public class SmsRequest
{
    public string SourceAddress { get; set; }
    public string DestinationAddress { get; set; }
    public string MessageText { get; set; }
}