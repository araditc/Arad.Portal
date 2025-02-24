namespace Arad.Portal.Models.Shared;

public class ErrorViewModel
{
    public string RequestId { get; init; }

    public bool ShowRequestId => !string.IsNullOrWhiteSpace(RequestId);
}