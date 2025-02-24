using System;

namespace Arad.Portal.Models.Shared;

public class AjaxValidationErrorModel
{
    public string Key { get; set; }
    public string ErrorMessage { get; init; }
    public Exception Exception { get; set; }
}