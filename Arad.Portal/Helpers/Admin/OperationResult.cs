namespace Arad.Portal.Helpers.Admin;

public class OperationResult
{
    public bool Succeeded { get; init; } = false;

    public string Message { get; init; }

    public string Url { get; set; }
}