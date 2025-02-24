using Microsoft.AspNetCore.Http;

namespace Arad.Portal.Models.Shared.Setting;

public class LanguageDictionaryModel
{
    public string LanguageId { get; init; }

    public IFormFile LanguageUploadFile { get; init; }
}