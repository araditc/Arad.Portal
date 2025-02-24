using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.General.BasicData;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.BasicData;
using Arad.Portal.Models.Shared.Multilingual;

using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.Areas.Shared.ViewComponents;


public class MultiLingual(IBasicDataRepository basicDataRepository) : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        List<BasicData>? supportedCultures = basicDataRepository.GetList(c => c.GroupKey == "SupportedCultures");
        List<CultureInfoModel> countryCodes = supportedCultures.Select(item =>
                                                                       {
                                                                           CultureInfo cultureInfo = new CultureInfo(item.Text);
                                                                           RegionInfo regionInfo = new RegionInfo(cultureInfo.LCID);
                                                                           return new CultureInfoModel
                                                                                  {
                                                                                      CultureCode = item.Text,
                                                                                      RegionCode = regionInfo.TwoLetterISORegionName.ToLower(),
                                                                                      DisplayName = cultureInfo.DisplayName
                                                                                  };
                                                                       }).ToList();

        ViewBag.CountryCodes = countryCodes;
        return View("~/Areas/Shared/Views/Shared/Components/MultiLingual/Default.cshtml");
    }
}