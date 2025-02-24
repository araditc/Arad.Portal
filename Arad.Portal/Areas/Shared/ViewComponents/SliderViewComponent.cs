using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SliderModule;
using Arad.Portal.Helpers.Shared;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Entities.General.SliderModule;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.Areas.Shared.ViewComponents;

public class SliderViewComponent(ISliderRepository sliderRepository, MinioHelper minioHelper, IDomainRepository domainRepository, ControllerHelper controllerHelper)
    : ViewComponent
{

    public IViewComponentResult Invoke(string sliderId, Enums.SliderType sliderType)
    {
        Task<ApplicationUser> userDb = controllerHelper.GetCurrentUser();
        string domainName = controllerHelper.GetCurrentDomainName();
        Domain domain = domainRepository.FirstOrDefault(c => c.DomainName == "https://" + domainName);
        Slider slider = sliderRepository.FirstOrDefault(c => c.Id == sliderId && c.AssociatedDomainId == domain.Id);
        CultureInfo current = new("en-US")
                              {
                                  DateTimeFormat = new()
                                                   {
                                                       Calendar = new GregorianCalendar()
                                                   }
                              };
        Thread.CurrentThread.CurrentCulture = current;

        if (slider == null)
        {
            return View("~/Areas/Shared/Views/Shared/Components/Slider/Default.cshtml", slider);
        }

        foreach (Slide sliderItem in slider.Slides)
        {
            string imageId = sliderItem.Id;
            string objectName = $"{domain.Id}/{imageId}.jpg";
            (bool success, byte[] imageData) = minioHelper.GetObject("slideimage", objectName).Result;
            if (success)
            {
                sliderItem.ImageUrl = Convert.ToBase64String(imageData);
            }
        }
        Language defLang = controllerHelper.GetDefaultLanguage();
        CultureInfo current2 = new(defLang.Symbol)
                               {
                                   DateTimeFormat = new()
                                                    {
                                                        Calendar = new GregorianCalendar()
                                                    }
                               };
        Thread.CurrentThread.CurrentCulture = current2;

        return sliderType switch
               {
                   Enums.SliderType.BootstrapCarousel => View("~/Areas/Shared/Views/Shared/Components/Slider/Default.cshtml", slider),
                   Enums.SliderType.Swiper => View("~/Areas/Shared/Views/Shared//Components/Slider/SwiperSlider.cshtml", slider),
                   _ => null
               };
    }
}