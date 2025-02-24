using System;

namespace Arad.Portal.Models.Shared;

public class CaptchaModel
{
    public string Code { get; init; }
    public DateTime ExpirationDate { get; init; }
}