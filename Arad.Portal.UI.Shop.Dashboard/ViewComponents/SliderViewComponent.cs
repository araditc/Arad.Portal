using Arad.Portal.DataLayer.Contracts.General.SliderModule;
using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
    public class Slider : ViewComponent
    {
        private readonly ISliderRepository _sliderRepository;

        public Slider(ISliderRepository sliderRepository)
        {
            _sliderRepository = sliderRepository;
        }

        public IViewComponentResult Invoke(string sliderId)
        {
            var slider = _sliderRepository.GetSlider(sliderId);
            
            return View("Default", slider);
        }
    }
}
