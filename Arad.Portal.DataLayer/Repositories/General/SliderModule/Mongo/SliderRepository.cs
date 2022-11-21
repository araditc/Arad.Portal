using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Contracts.General.SliderModule;
using Arad.Portal.DataLayer.Entities.General.SliderModule;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.SlideModule;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Arad.Portal.DataLayer.Repositories.General.SliderModule.Mongo
{
    public class SliderRepository: BaseRepository, ISliderRepository
    {
        private readonly SliderContext _context;
        private readonly DomainContext _domainContext;
        private readonly UserManager<ApplicationUser> _userManager;
       
        public SliderRepository(SliderContext context,
            DomainContext domainContext,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env):base(httpContextAccessor, env)
        {
            _context = context;
            _domainContext = domainContext;
            _userManager = userManager;
        }

        public async Task<List<Slider>> GetSliders(string currentUserId)
        {
            var userDb = await _userManager.FindByIdAsync(currentUserId);
            List<Slider> list = new List<Slider>();
            if(userDb.IsSystemAccount)
            {
               list = _context.Collection
               .Find(_ => !_.IsDeleted && _.IsActive).ToList();
            }else
            {
                list = _context.Collection
                .Find(_ => !_.IsDeleted && _.IsActive && _.AssociatedDomainId == userDb.Domains.FirstOrDefault(a => a.IsOwner).DomainId).ToList();
            }
            return list;
        }

        public bool AddSlider(Slider model)
        {
            try
            {

                model.CreationDate = DateTime.Now;
                var currentUserId = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                model.CreatorUserId = currentUserId;
                model.CreatorUserName = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

                model.SliderId = Guid.NewGuid().ToString();
                _context.Collection.InsertOne(model);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public Slider GetSlider(string id)
        {
            try
            {
                return _context.Collection.Find(s => s.SliderId == id).FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<bool> Update(Slider model)
        {
            try
            {
                if(_context.Collection.Find(s => s.SliderId == model.SliderId).Any())
                {
                    var slider = _context.Collection.Find(s => s.SliderId == model.SliderId).FirstOrDefault();

                    var result = await _context.Collection.ReplaceOneAsync(c => c.SliderId == model.SliderId, model);

                    if (result.IsAcknowledged)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public Slide GetSlide(string slideId)
        {
            return _context.Collection.AsQueryable()
                .SelectMany(s => s.Slides).FirstOrDefault(s => s.Id == slideId);
        }

        public PagedItems<Slide> ListSlides(SearchParamSlides searchParam)
        {
            try
            {
                var query = _context.Collection.AsQueryable()
                    .Where(n => !n.IsDeleted && n.SliderId == searchParam.SliderId);

                var querySlides = query.SelectMany(s => s.Slides)
                    .Where(s => s.IsDeleted != 1);

                var count = querySlides.Count();

                querySlides = querySlides
                    .Skip(((searchParam.CurrentPage) - 1) * searchParam.PageSize)
                    .Take(searchParam.PageSize); ;

                List<Slide> news = querySlides.ToList();

                var res = new PagedItems<Slide>()
                {
                    CurrentPage = searchParam.CurrentPage,
                    ItemsCount = count % (searchParam.PageSize) == 0
                        ? count / (searchParam.PageSize)
                        : count / (searchParam.PageSize) + 1,
                    PageSize = searchParam.PageSize,
                    Items = news.Select(u => new Slide()
                    {
                        Id = u.Id,
                        Title = u.Title,
                        StartDate = u.StartDate,
                        ExpireDate = u.ExpireDate,
                        IsActive = u.IsActive,
                        Link = u.Link,
                        Alt = u.Alt,
                        ColoredBackground = u.ColoredBackground,
                        ImageFit = u.ImageFit,
                        ImageUrl = u.ImageUrl,
                        Layers = u.Layers,
                        Target = u.Target,
                        TransActionType = u.TransActionType
                    }).ToList()
                };

                return res;
            }
            catch (Exception e)
            {
                return new PagedItems<Slide>();
            }
        }

        public PagedItems<Layer> ListLayers(SearchParamLayers searchParam)
        {
            try
            {
                var query = _context.Collection.AsQueryable()
                    .Where(n => !n.IsDeleted  && n.SliderId == searchParam.SliderId);

                var querySlides = query.SelectMany(s => s.Slides)
                    .Where(s => s.IsDeleted != 1 && s.Id == searchParam.SlideId);

                var queryLayers = querySlides.SelectMany(s => s.Layers)
                    .Where(s => s.IsDeleted != 1);

                var count = queryLayers.Count();

                queryLayers = queryLayers
                    .Skip(((searchParam.CurrentPage) - 1) * searchParam.PageSize)
                    .Take(searchParam.PageSize); ;

                List<Layer> news = queryLayers.ToList();

                var res = new PagedItems<Layer>()
                {
                    CurrentPage = searchParam.CurrentPage,
                    ItemsCount = count % (searchParam.PageSize) == 0
                        ? (int)count / (searchParam.PageSize)
                        : (int)count / (searchParam.PageSize) + 1,
                    PageSize = searchParam.PageSize,
                    Items = news.Select(u => new Layer()
                    {
                        Id = u.Id,
                        Link = u.Link,
                        Type = u.Type,
                        Content = u.Content,
                        Position = u.Position,
                        TransActionType = u.TransActionType
                    }).ToList()
                };

                return res;
            }
            catch (Exception e)
            {
                return new PagedItems<Layer>();
            }
        }

        public Slider GetActiveSlider()
        {
            var slider = _context.Collection.AsQueryable()
                .FirstOrDefault(s => s.IsActive && !s.IsDeleted );

            if (slider != null)
            {
                var slidesForShow = slider.Slides
                    .Where(s => s.StartDate <= DateTime.UtcNow &&
                                (s.ExpireDate >= DateTime.UtcNow || s.ExpireDate == null) &&
                                s.IsDeleted != 1).ToList();

                slidesForShow
                    .ForEach(s => s.Layers = s.Layers.Where(l => l.IsDeleted != 1)
                        .ToList());

                slider.Slides = slidesForShow;
            }

            return slider;
        }

        public Layer GetLayer(string layerId)
        {
            return _context.Collection.AsQueryable().SelectMany(s => s.Slides)
                .SelectMany(s => s.Layers).FirstOrDefault(l => l.Id == layerId);
        }

        public async Task<bool> UpdateLayer(Layer layer)
        {
            try
            {
                var slider = _context.Collection.AsQueryable()
                    .FirstOrDefault(s => s.Slides.Any(ss => ss.Layers.Any(l => l.Id == layer.Id)));

                if (slider != null)
                {
                    var layerOld = slider.Slides.SelectMany(s => s.Layers)
                        .FirstOrDefault(l => l.Id == layer.Id);

                    if (layerOld != null)
                    {
                        layerOld.Attributes = layer.Attributes;
                        layerOld.Content = layer.Content;
                        layerOld.Link = layer.Link;
                        layerOld.Position = layer.Position;
                        layerOld.Target = layer.Target;
                        layerOld.TransActionType = layer.TransActionType;
                        layerOld.Type = layer.Type;
                        layerOld.Styles = layer.Styles;
                    }

                    var result = await _context.Collection
                        .ReplaceOneAsync(c => c.SliderId == slider.SliderId, slider);

                    if (result.IsAcknowledged)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> UpdateSlide(Slide slide)
        {
            try
            {
                var slider = _context.Collection.AsQueryable()
                    .FirstOrDefault(s => s.Slides.Any(ss => ss.Id == slide.Id));

                if (slider != null)
                {
                    var slideOld = slider.Slides
                        .FirstOrDefault(s => s.Id == slide.Id);

                    if (slideOld != null)
                    {
                        slideOld.Alt = slide.Alt;
                        slideOld.ColoredBackground = slide.ColoredBackground;
                        slideOld.ExpireDate = slide.ExpireDate;
                        slideOld.ImageFit = slide.ImageFit;
                        slideOld.ImageUrl = slide.ImageUrl;
                        slideOld.Link = slide.Link;
                        slideOld.StartDate = slide.StartDate;
                        slideOld.Title = slide.Title;
                        slideOld.Target = slide.Target;
                        slideOld.TransActionType = slide.TransActionType;
                        slideOld.Layers = slideOld.Layers;
                    }

                    var result = await _context.Collection
                        .ReplaceOneAsync(c => c.SliderId == slider.SliderId, slider);

                    if (result.IsAcknowledged)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> DeleteSlider(string id)
        {
            try
            {
                var slider = _context.Collection.Find(s => s.SliderId == id).FirstOrDefault();
                if(slider != null)
                {
                    var filter = Builders<Slider>.Filter.Eq("SliderId", id);
                    var update = Builders<Slider>.Update.Set("IsDeleted", true);
                    var resultUpdate = await _context.Collection.UpdateOneAsync(filter, update);
                    var ack = resultUpdate.IsAcknowledged;

                    if (ack)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<Result> ActiveSlider(string sliderId)
        {
            var result = new Result();
            try
            {
                var entity = _context.Collection.Find(_ => _.SliderId == sliderId).FirstOrDefault();
                if (entity != null)
                {
                    entity.IsActive = !entity.IsActive;
                    var updateResult = await _context.Collection.ReplaceOneAsync(_ => _.SliderId == sliderId, entity);
                    if (updateResult.IsAcknowledged)
                    {
                        result.Succeeded = true;
                        result.Message = ConstMessages.SuccessfullyDone;
                    }
                    else
                    {
                        result.Message = ConstMessages.ErrorInSaving;
                    }
                }
                else
                {
                    result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
                }

            }
            catch (Exception e)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }

            return result;
        }

        public async Task<Result> ActiveSlide(string slideId)
        {
            var result = new Result();
            try
            {
                var slider = _context.Collection.Find(s => s.Slides.Any(ss => ss.Id == slideId)).FirstOrDefault();

                if (slider != null)
                {
                    var slideOld = slider.Slides
                        .FirstOrDefault(s => s.Id == slideId);

                    if (slideOld != null)
                    {
                        slideOld.IsActive = !slideOld.IsActive;
                    }

                    var updateResult = await _context.Collection
                        .ReplaceOneAsync(c => c.SliderId == slider.SliderId, slider);

                    if (updateResult.IsAcknowledged)
                    {
                        result.Succeeded = true;
                        result.Message = ConstMessages.SuccessfullyDone;
                    }
                    else
                    {
                        result.Message = ConstMessages.ErrorInSaving;
                    }
                }
                else
                {
                    result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
                }
            }
            catch (Exception e)
            {
                result.Message = ConstMessages.ErrorInSaving;
            }
            return result;
        }

        public async Task<bool> DeleteSlide(string id)
        {
            try
            {
                var slider = _context.Collection.AsQueryable()
                    .FirstOrDefault(s => s.Slides.Any(ss => ss.Id == id));

                if (slider != null)
                {
                    var slideOld = slider.Slides
                        .FirstOrDefault(s => s.Id == id);

                    if (slideOld != null)
                    {
                        slideOld.IsDeleted = 1;
                    }

                    var result = await _context.Collection
                        .ReplaceOneAsync(c => c.SliderId == slider.SliderId, slider);

                    if (result.IsAcknowledged)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> ActiveLayer(string id, bool isActive)
        {
            try
            {
                var slider = _context.Collection.AsQueryable()
                    .FirstOrDefault(s => s.Slides.Any(ss => ss.Layers.Any(l => l.Id == id)));

                if (slider != null)
                {
                    var layerOld = slider.Slides.SelectMany(s => s.Layers)
                        .FirstOrDefault(l => l.Id == id);

                    if (layerOld != null)
                    {
                        layerOld.IsActive = isActive;
                    }

                    var result = await _context.Collection
                        .ReplaceOneAsync(c => c.SliderId == slider.SliderId, slider);

                    if (result.IsAcknowledged)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> DeleteLayer(string id)
        {
            try
            {
                var slider = _context.Collection.AsQueryable()
                    .FirstOrDefault(s => s.Slides.Any(ss => ss.Layers.Any(l => l.Id == id)));

                if (slider != null)
                {
                    var layerOld = slider.Slides.SelectMany(s => s.Layers)
                        .FirstOrDefault(l => l.Id == id);

                    if (layerOld != null)
                    {
                        layerOld.IsDeleted = 1;
                    }

                    var result = await _context.Collection
                        .ReplaceOneAsync(c => c.SliderId == slider.SliderId, slider);

                    if (result.IsAcknowledged)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public List<SelectListModel> ActiveSliderList(string domainId)
        {
            var result = new List<SelectListModel>();
             result = _context.Collection.Find(_ => _.IsActive && !_.IsDeleted && _.AssociatedDomainId == domainId)
                     .Project(_ => new SelectListModel()
                     {
                         Text = _.Title,
                         Value = _.SliderId
                     }).ToList();

            return result;
        }
    }
}
