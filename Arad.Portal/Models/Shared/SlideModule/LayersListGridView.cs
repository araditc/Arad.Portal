using Arad.Portal.DataLayer.Entities.General.SliderModule;

namespace Arad.Portal.Models.Shared.SlideModule;

public class LayersListGridView
{
    public string Id { get; init; }
    public LayerType Type { get; init; }
    public string Content { get; init; }
    public string Link { get; init; }
    public Position Position { get; init; }
    public TransActionType TransActionType { get; set; }
}