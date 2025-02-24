using System.Collections.Generic;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.Models.Shared.Product;

public class ServiceSettingDto
{
    public string SerializeProductList { get; set; }
    public int ProductUnitIndex { get; set; }
    public List<ProductUnitDto>? ProductUnitDtOs { get; set; } = new();
    public ProductUnitDto ProductUnit { get; set; } = new();
    public StateType State { get; set; }
}