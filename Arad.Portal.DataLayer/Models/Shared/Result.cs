namespace Arad.Portal.DataLayer.Models.Shared;

public class Result
{
    public bool Succeeded { get; set; } = false;
    public string Message { get; set; }
    public string Text { get; set; }
    public string Value { get; set; }
}
public class Result<TReturnType> where TReturnType : class
{
    public bool Succeeded { get; set; } = false;
    public string Message { get; set; }
    public string Text { get; set; }
    public string Value { get; set; }
    public TReturnType ReturnValue { get; set; }
}