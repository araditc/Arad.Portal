using Arad.Portal.DataLayer.Models.Shared.DesignStructure;
using Arad.Portal.DataLayer.Models.Shared.IconBlock;

using DocumentFormat.OpenXml.Office2010.ExcelAc;

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Arad.Portal.Areas.Shared.ViewComponents;

public class IconBlocksViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(List<IconBlock> iconBlocks)
    {
        return View("~/Areas/Shared/Views/Shared/Components/IconBlocks/Default.cshtml", iconBlocks);
    }
}
