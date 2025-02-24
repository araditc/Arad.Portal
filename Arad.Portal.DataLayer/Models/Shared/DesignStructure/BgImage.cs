namespace Arad.Portal.DataLayer.Models.Shared.DesignStructure;

public class BgImage
{
    public string ImageId { get; set; }
    public string Base64ImageContent { get; set; }

    public string ImageFileName { get; set; }

    public string SelectedRowGuid { get; set; }

    public string SelectedSection { get; set; }
}