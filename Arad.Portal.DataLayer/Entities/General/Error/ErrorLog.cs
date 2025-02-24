using Arad.Portal.DataLayer.Entities.Abstractions;

namespace Arad.Portal.DataLayer.Entities.General.Error;

public class ErrorLog : BaseEntity
{
    /// <summary>
    /// action which this error occured
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// content of error
    /// </summary>
    public string Error { get; set; }


    /// <summary>
    /// IP where this error occured
    /// </summary>
    public string Ip { get; set; }
}