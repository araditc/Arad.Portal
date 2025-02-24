using Arad.Portal.GeneralLibrary.CustomAttributes;
using System.ComponentModel.DataAnnotations;

using Arad.Portal.DataLayer.Entities.General.SliderModule;

namespace Arad.Portal.Models.Shared.SlideModule;

public class LayerView
{
    public string SliderId { get; set; }
    public string SlideId { get; set; }
    public string? Id { get; init; }
    [Required]
    [ErrorMessage("AlertAndMessage_FieldEssential")]
    public LayerType Type { get; init; }
    [Required]
    [ErrorMessage("AlertAndMessage_FieldEssential")]
    public string Content { get; init; }
    public string? Link { get; init; }
    public Target Target { get; init; }
    public Position? Position { get; init; }
    public TransActionType TransActionType { get; init; }
    public Style? Styles { get; init; }
    public DataLayer.Entities.General.SliderModule.Attribute? Attributes { get; init; }
    public int IsDeleted { get; init; }
    public bool IsActive { get; init; }
}