using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Models.Shared.PictureWithLabel;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.Helpers.Shared;

using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.Areas.Shared.ViewComponents
{
    public class PictureWithLabelViewComponent(MinioHelper minioHelper, ControllerHelper controllerHelper,IDomainRepository domainRepository) : ViewComponent
    {
        public async ValueTask<IViewComponentResult> Invoke(List<PictureWithLabel> pictureWithLabels)
        {
            foreach (PictureWithLabel item in pictureWithLabels)
            {
                CultureInfo current = new("en-US")
                                      {
                                          DateTimeFormat = new()
                                                           {
                                                               Calendar = new GregorianCalendar()
                                                           }
                                      };
                Thread.CurrentThread.CurrentCulture = current;

                string domainName = controllerHelper.GetCurrentDomainName();
                Domain domain = await domainRepository.FirstOrDefaultAsync(c => c.DomainName == "https://"+domainName);
                string objectName = $"PictureWithLabels/{domain.Id}/{item.Image.ImageId}/RandomPictureWithLabels.png";

                (bool success, byte[] imageData) = await minioHelper.GetObject("pagedesign", objectName);
                if (success)
                {
                    item.Image.Content = Convert.ToBase64String(imageData);
                }
            }
            
            return View("~/Areas/Shared/Views/Shared/Components/PictureWithLabel/Default.cshtml", pictureWithLabels);
        }
    }
}
