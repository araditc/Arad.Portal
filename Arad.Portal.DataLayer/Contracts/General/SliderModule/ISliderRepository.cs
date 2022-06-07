using Arad.Portal.DataLayer.Entities.General.SliderModule;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.SlideModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.SliderModule
{
    public interface ISliderRepository
    {
        List<Slider> GetSliders();
        bool AddSlider(Slider model);
        Slider GetSlider(string id);
        Task<bool> Update(Slider slider);
        Slide GetSlide(string slideId);
        PagedItems<Slide> ListSlides(SearchParamSlides searchParam);
        PagedItems<Layer> ListLayers(SearchParamLayers searchParam);
        Slider GetActiveSlider();
        Layer GetLayer(string layerId);
        Task<bool> UpdateLayer(Layer layer);
        Task<bool> UpdateSlide(Slide slide);
        Task<bool> DeleteSlider(string id);
        Task<Result> ActiveSlider(string sliderId);
        Task<Result> ActiveSlide(string slideId);
        Task<bool> DeleteSlide(string id);
        Task<bool> ActiveLayer(string id, bool isActive);
        Task<bool> DeleteLayer(string id);

        List<SelectListModel> ActiveSliderList(string domianId);
    }
}
