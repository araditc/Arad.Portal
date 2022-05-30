using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Contracts.General.SliderModule;
using Arad.Portal.DataLayer.Entities.General.SliderModule;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.SlideModule;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;

namespace Arad.Portal.DataLayer.Repositories.General.SliderModule.Mongo
{
    public class SliderRepository: BaseRepository, ISliderRepository
    {
        private readonly SliderContext _context;

        public SliderRepository(SliderContext context,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env):base(httpContextAccessor, env)
        {
            _context = context;
        }

        public List<Slider> GetSliders()
        {
            var list = _context.Collection
                .Find(_ => ! _.IsDeleted).ToList();
               
            return list;
        }

        public bool AddSlider(Slider model)
        {
            try
            {
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
                var slider = _context.Collection.AsQueryable().FirstOrDefault(s => s.SliderId == model.SliderId);

                var result = await _context.Collection.ReplaceOneAsync(c => c.SliderId == model.SliderId, model);

                if (result.IsAcknowledged)
                {
                    return true;
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
                    .Where(s => s.StartDate <= DateTime.Now &&
                                (s.ExpireDate >= DateTime.Now || s.ExpireDate == null) &&
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
                var slider = _context.Collection.AsQueryable().FirstOrDefault(s => s.SliderId == id);

                if (slider != null)
                {
                    if (slider.IsActive)
                    {
                        return false;
                    }
                }

                var filter = Builders<Slider>.Filter.Eq("SliderId", id);
                var update = Builders<Slider>.Update.Set("IsDeleted", true);
                var resultUpdate = await _context.Collection.UpdateOneAsync(filter, update);
                var ack = resultUpdate.IsAcknowledged;

                if (ack)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<bool> ActiveSlider(string id, bool isActive)
        {
            try
            {
                if (isActive)
                {
                    var activeNow = _context.Collection.AsQueryable().FirstOrDefault(s => s.IsActive);

                    if (activeNow != null)
                    {
                        var filter1 = Builders<Slider>.Filter.Eq("SliderId", activeNow.SliderId);
                        var update1 = Builders<Slider>.Update.Set("IsActive", false);
                        var resultUpdate1 = await _context.Collection.UpdateOneAsync(filter1, update1);
                    }
                }

                var filter = Builders<Slider>.Filter.Eq("SliderId",  id);
                var update = Builders<Slider>.Update.Set("IsActive", isActive);

                var resultUpdate = await _context.Collection.UpdateOneAsync(filter, update);
                var ack = resultUpdate.IsAcknowledged;

                if (ack)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> ActiveSlide(string id, bool isActive)
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
                        slideOld.IsActive = isActive;
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
    }
}
