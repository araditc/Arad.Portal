using Arad.Portal.DataLayer.Entities.General.Language;

namespace Arad.Portal.Models.Shared.Language;

public class LanguageDto
{
    public string Id { get; init; }

    public string LanguageName { get; init; }

    public string Symbol { get; init; }

    public Direction Direction { get; init; }

    public string ModificationReason { get; init; }

    public bool IsActive { get; init; }
}