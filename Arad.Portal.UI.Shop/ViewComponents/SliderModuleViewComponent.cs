using Arad.Portal.DataLayer.Contracts.General.SliderModule;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class SliderModuleViewComponent : ViewComponent
    {
        private readonly ISliderRepository _sliderRepository;

        public SliderModuleViewComponent(ISliderRepository sliderRepository)
        {
            _sliderRepository = sliderRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(string sliderId)
        {
            var slider = _sliderRepository.GetSlider(sliderId);
            Log.Fatal($"find slider={slider.SliderId}");
          
            return View(slider);
        }
    }
}
