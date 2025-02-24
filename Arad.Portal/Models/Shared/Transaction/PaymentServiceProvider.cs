using Arad.Portal.DataLayer.Models.Shared;
using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Transaction;

public class PaymentServiceProvider
{
    public Enums.PspType Type { get; set; }
    public List<Parameter> Parameters { get; set; }
    public string IconAddress { get; set; }
}
public class Parameter
{
    public Parameter()
    {
    }
    public Parameter(string key, string value)
    {
        Key = key;
        Value = value;
    }
    public string Key { get; set; }
    public string Value { get; set; }
}