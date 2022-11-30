using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.SliderModule;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class SliderModuleViewComponent : ViewComponent
    {
        private readonly ISliderRepository _sliderRepository;
        private readonly IDomainRepository _domainRepository;

        public SliderModuleViewComponent(ISliderRepository sliderRepository, IDomainRepository domainRepository)
        {
            _sliderRepository = sliderRepository;
            _domainRepository = domainRepository;
        }

        public IViewComponentResult Invoke(string sliderId)
        {
            var domainName = HttpContext.Request.Host.ToString();
            var domainEntity = _domainRepository.FetchByName(domainName, false).ReturnValue; 
            var slider = _sliderRepository.GetSlider(sliderId, domainEntity.DomainId);
            Log.Fatal($"find slider={slider.SliderId}");
          
            return View(slider);
        }
    }
}
