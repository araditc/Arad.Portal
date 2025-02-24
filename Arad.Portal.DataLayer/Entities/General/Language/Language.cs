using Arad.Portal.DataLayer.Entities.Abstractions;

namespace Arad.Portal.DataLayer.Entities.General.Language;

public class Language : BaseEntity
{
    public string LanguageName { get; set; }

    public string Symbol { get; set; }

    /// <summary>
    /// whether this language is rtl or ltr
    /// </summary>
    public Direction Direction { get; set; }

    /// <summary>
    /// one language in the language document
    /// </summary>
    public bool IsDefault { get; set; }
}

public enum Direction
{
    Ltr,
    Rtl
}