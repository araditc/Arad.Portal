using Arad.Portal.GeneralLibrary.CustomAttributes;
using System.ComponentModel.DataAnnotations;

namespace Arad.Portal.Models.Shared.SlideModule;

public class SearchParamLayers
{
    public string SliderId { get; set; }
    public string SlideId { get; set; }
    public int PageSize { get; set; }

    [Required]
    [ErrorMessage("AlertAndMessage_FieldEssential")]
    public int CurrentPage { get; set; }
}