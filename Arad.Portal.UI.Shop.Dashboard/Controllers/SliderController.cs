using Arad.Portal.DataLayer.Contracts.General.SliderModule;
using Arad.Portal.DataLayer.Entities.General.SliderModule;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.SlideModule;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class SliderController : Controller
    {
        private readonly ISliderRepository _sliderRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public SliderController(ISliderRepository sliderRepository, IConfiguration configuration, IMapper mapper)
        {
            _sliderRepository = sliderRepository;
            _configuration = configuration;
            _mapper = mapper;
        }

        private ClientValidationErrorModel ValidatorPic(string model)
        {
            string img = model;

            var cc = img.IndexOf("/", StringComparison.Ordinal);
            var kk = img.IndexOf(",", StringComparison.Ordinal);
            var extension = img.Substring(cc + 1, 4);

            var aa = img.Substring(kk + 1);
            byte[] bytes = Convert.FromBase64String(aa);

            double fileSizeKb = bytes.Length / 1024;

            if (fileSizeKb > 500)
            {
                return new ClientValidationErrorModel() { Key = "1", ErrorMessage = "." };
            }

            return new ClientValidationErrorModel() { Key = "0", ErrorMessage = "" };
        }


        private Image Upload(Image imgObj)
        {
            try
            {

                var cc = imgObj.Content.IndexOf("/", StringComparison.Ordinal);
                var kk = imgObj.Content.IndexOf(",", StringComparison.Ordinal);
                var extension = imgObj.Content.Substring(cc + 1, 4);
                var aa = imgObj.Content.Substring(kk + 1);
                // byte[] bytes = Convert.FromBase64String(aa);

                var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                var path = "images/SliderModule";

                var res = ImageFunctions.SaveImageModel(imgObj, path, localStaticFileStorageURL);
                if (res.Key != Guid.Empty.ToString())
                {
                    imgObj.ImageId = res.Key;
                    imgObj.Url = res.Value;
                    imgObj.Content = "";
                }
            }
            catch (Exception ex)
            {

            }
            return imgObj;
        }

        public IActionResult Index()
        {
            var sliders = _sliderRepository.GetSliders();
            return View(sliders);
        }

        [HttpPost]
        public IActionResult AddSlider(SliderAddView model)
        {
            var errors = new List<ClientValidationErrorModel>();
            if (!ModelState.IsValid)
            {
                errors = ModelState.Generate();

                return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
            }

            var sliders = _sliderRepository.GetSliders();

            var slider = new Slider()
            {
                IsActive = !sliders.Any(),
                Title = model.Title
            };

            var result = _sliderRepository.AddSlider(slider);

            if (result)
            {
                return Json(new { Status = "success", Message = Language.GetString("AlertAndMessage_InsertionDoneSuccessfully"), ModelStateErrors = errors });
            }

            return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = errors });
        }

        [HttpGet]
        public IActionResult GetSlider(string sliderId)
        {
            Slider slide = _sliderRepository.GetSlider(sliderId);

            if (slide != null)
            {
                return Json(new { Status = "success", Data = slide });
            }

            return Json(new { Status = "error", Data = slide, message = Language.GetString("AlertAndMessage_NotFound") });
        }

        [HttpPost]
        public async Task<IActionResult> EditSlider(SliderAddView model)
        {
            var errors = new List<ClientValidationErrorModel>();
            if (!ModelState.IsValid)
            {
                errors = ModelState.Generate();

                return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
            }

            Slider slider = _sliderRepository.GetSlider(model.Id);

            slider.Title = model.Title;

            var result = await _sliderRepository.Update(slider);

            if (result)
            {
                return Json(new { Status = "success", Message = Language.GetString("AlertAndMessage_EditionDoneSuccessfully"), ModelStateErrors = errors });
            }

            return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = errors });
        }

        [HttpGet]
        public async Task<IActionResult> ChangeBeActiveSlider(string id)
        {
            JsonResult result;
            try
            {
                var res = await _sliderRepository.ActiveSlider(id);
                if (res.Succeeded)
                {
                    var slider = _sliderRepository.GetSlider(id);
                    result = Json(new { Status = "success", Message = res.Message, result = slider.IsActive.ToString() });
                }
                else
                {
                    result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
                }
            }
            catch (Exception e)
            {
                result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> DeleteSlider(string id)
        {
            try
            {
                bool result = await _sliderRepository.DeleteSlider(id);

                if (result)
                {
                    return Json(new { Status = "success", Message = Language.GetString("AlertAndMessage_DeletionDoneSuccessfully") });
                }
                return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_DeletionNotAllowed") });

            }
            catch (Exception e)
            {
                return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_DeletionNotAllowed") });

            }
        }

        [HttpGet]
        public IActionResult Slides(string id)
        {
            Slider slider = _sliderRepository.GetSlider(id);

            ViewBag.SliderId = slider.SliderId;

            return View("AddSlide", slider.Title);
        }

        [HttpPost]
        public IActionResult ListSlides([FromForm] SearchParamSlides searchParam)
        {
            if (ModelState.IsValid)
            {
                //var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);

                try
                {
                    PagedItems<Slide> data = _sliderRepository.ListSlides(searchParam);

                    var dataGrid = new PagedItems<SlidesListGridView>()
                    {
                        CurrentPage = data.CurrentPage,
                        PageSize = data.PageSize,
                        ItemsCount = data.ItemsCount,
                        Items = data.Items.Select(n => new SlidesListGridView()
                        {
                            Id = n.Id,
                            Title = n.Title,
                            IsActive = n.IsActive,
                            Link = n.Link,
                            Alt = n.Alt,
                            ColoredBackground = n.ColoredBackground,
                            ImageFit = n.ImageFit,
                            ImageUrl = n.ImageUrl,
                            Target = n.Target,
                            TransActionType = n.TransActionType,
                            StartDate = n.StartDate,
                            ExpireDate = n.ExpireDate,
                            PersianStartDate = DateHelper.ToPersianDdate(n.StartDate.Value),
                            PersianExpireDate = DateHelper.ToPersianDdate(n.ExpireDate.Value)
                        }).ToList()
                    };

                    //ViewBag.Permissions = dicKey;
                    ViewBag.SliderId = searchParam.SliderId;
                    return View(dataGrid);
                }
                catch (Exception e)
                {
                    return View(new PagedItems<SlidesListGridView>());
                }
            }
            return View(new PagedItems<SlidesListGridView>()
            {
                Items = new List<SlidesListGridView>()
            });
        }

        [HttpGet]
        public IActionResult SlidesOfSlider(string id)
        {
            Slider slider = _sliderRepository.GetSlider(id);

            return Json(new { Status = "", Data = slider.Slides });
        }

        [HttpPost]
        public async Task<IActionResult> AddSlide(SlideView model)
        {
            var errors = new List<ClientValidationErrorModel>();
            if (CultureInfo.CurrentCulture.Name == "fa-IR")
            {
                if (string.IsNullOrWhiteSpace(model.PersianStartDate))
                {
                    ModelState.AddModelError(nameof(model.StartDate), Language.GetString("AlertAndMessage_FieldEssential"));
                }

            }
            else
            {
                if (model.StartDate == null)
                {
                    ModelState.AddModelError(nameof(model.StartDate), Language.GetString("AlertAndMessage_FieldEssential"));
                }

                if (model.ExpireDate != null && model.ExpireDate <= model.StartDate)
                {
                    ModelState.AddModelError(nameof(model.ExpireDate), Language.GetString("AlertAndMessage_EndDateMustBeGreaterThanStartDate"));
                }
            }



            var validate = ValidatorPic(model.ImageUrl);

            if (validate.Key == "1")
            {
                ModelState.AddModelError(nameof(SlideView.ImageUrl), validate.ErrorMessage);
            }

            if (!ModelState.IsValid)
            {
                errors = ModelState.Generate();

                return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
            }

            Slider slider = _sliderRepository.GetSlider(model.SliderId);
            var img = new Image()
            {
                Content = model.ImageUrl,
                ImageRatio = ImageRatio.TwoToOne,
                ImageRatioId = 2,
                Title = model.Title,
                IsMain = false
            };

            var resultUpload = Upload(img);

            if (!string.IsNullOrWhiteSpace(resultUpload.Content)) //image doesnt save successfully so the content isnt string.empty
            {
                var obj = new ClientValidationErrorModel
                {
                    Key = "ImageUrl",
                    ErrorMessage = Language.GetString("AlertAndMessage_ErrorInUploadFile"),
                };
                errors = new List<ClientValidationErrorModel>();
                errors.Add(obj);

                return new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
            }

            var slide = new Slide()
            {
                Id = Guid.NewGuid().ToString(),
                Alt = model.Alt,
                ColoredBackground = model.ColoredBackground,
                ExpireDate = (CultureInfo.CurrentCulture.Name == "fa-IR" && !string.IsNullOrWhiteSpace(model.PersianExpireDate)) ? model.PersianExpireDate.ToEnglishDate() : model.ExpireDate,
                ImageFit = model.ImageFit,
                ImageUrl = resultUpload.Url,
                Link = model.Link,
                StartDate = (CultureInfo.CurrentCulture.Name == "fa-IR" && !string.IsNullOrWhiteSpace(model.PersianStartDate)) ? model.PersianStartDate.ToEnglishDate() : model.StartDate,
                Target = model.Target,
                TransActionType = model.TransActionType,
                Title = resultUpload.Title,
                IsActive = true
            };

            slider.Slides.Add(slide);

            bool result = await _sliderRepository.Update(slider);

            if (result)
            {
                return Json(new { Status = "success", Message = Language.GetString("AlertAndMessage_InsertionDoneSuccessfully"), ModelStateErrors = errors });
            }

            return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = errors });
        }

        [HttpGet]
        public IActionResult GetSlide(string slideId)
        {
            Slide slide = _sliderRepository.GetSlide(slideId);
            if (slide != null)
            {
                var outputModel = _mapper.Map<SlideDTO>(slide);
                outputModel.PersianStartShowDate = DateHelper.ToPersianDdate(outputModel.StartDate.Value);
                if (slide.ExpireDate != null)
                    outputModel.PersianEndShowDate = DateHelper.ToPersianDdate(outputModel.ExpireDate.Value);


                return Json(new { Status = "success", Data = outputModel });
            }

            return Json(new { Status = "error", Data = slide, message = Language.GetString("AlertAndMessage_NotFound") });
        }

        [HttpPost]
        public async Task<IActionResult> EditSlide(SlideView model)
        {
            var errors = new List<ClientValidationErrorModel>();

            if (CultureInfo.CurrentCulture.Name == "fa-IR")
            {
                if (string.IsNullOrWhiteSpace(model.PersianStartDate))
                {
                    ModelState.AddModelError(nameof(model.StartDate), Language.GetString("AlertAndMessage_FieldEssential"));
                }
                model.StartDate = DateHelper.ToEnglishDate(model.PersianStartDate);
                if(!string.IsNullOrWhiteSpace(model.PersianExpireDate))
                {
                    model.ExpireDate = DateHelper.ToEnglishDate(model.PersianExpireDate);
                }

            }
            else
            {
                if (model.StartDate == null)
                {
                    ModelState.AddModelError(nameof(model.StartDate), Language.GetString("AlertAndMessage_FieldEssential"));
                }

                if (model.ExpireDate != null && model.ExpireDate <= model.StartDate)
                {
                    ModelState.AddModelError(nameof(model.ExpireDate), Language.GetString("AlertAndMessage_EndDateMustBeGreaterThanStartDate"));
                }
            }


            if (model.ExpireDate <= model.StartDate)
            {
                ModelState.AddModelError(nameof(model.ExpireDate), Language.GetString("AlertAndMessage_EndDateMustBeGreaterThanStartDate"));
            }

            if (model.ImageUrl.StartsWith("data"))
            {
                var validate = ValidatorPic(model.ImageUrl);

                if (validate.Key == "1")
                {
                    ModelState.AddModelError(nameof(SlideView.ImageUrl), validate.ErrorMessage);
                }
            }

            if (!ModelState.IsValid)
            {
                errors = ModelState.Generate();

                return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
            }

            var image = new Image();
            if (model.ImageUrl.StartsWith("data"))
            {
                var img = new Image()
                {
                    Content = model.ImageUrl,
                    ImageRatio = ImageRatio.TwoToOne,
                    ImageRatioId = 2,
                    Title = model.Title,
                    IsMain = false
                };
                image = Upload(img);

                if (!string.IsNullOrWhiteSpace(image.Content)) //save doesnt occured successfully
                {
                    var obj = new ClientValidationErrorModel
                    {
                        Key = "ImageUrl",
                        ErrorMessage = Language.GetString("AlertAndMessage_ErrorInUploadFile"),
                    };
                    errors = new List<ClientValidationErrorModel>();
                    errors.Add(obj);

                    model.Title = image.Title;

                    return new JsonResult(new { Status = "error", Message = Language.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
                }
            }

            var slide = new Slide()
            {
                Id = model.Id,
                Alt = model.Alt,
                ColoredBackground = model.ColoredBackground,
                ExpireDate = model.ExpireDate,
                ImageFit = model.ImageFit,
                ImageUrl = model.ImageUrl.StartsWith("data") ? image.Url : model.ImageUrl,
                Link = model.Link,
                StartDate = model.StartDate,
                Target = model.Target,
                TransActionType = model.TransActionType,
                Title = model.Title,
                IsActive = true
            };

            bool result = await _sliderRepository.UpdateSlide(slide);

            if (result)
            {
                return Json(new { Status = "success", Message = Language.GetString("AlertAndMessage_EditionDoneSuccessfully"), ModelStateErrors = errors });
            }

            return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = errors });
        }

        [HttpGet]
        public async Task<IActionResult> ChangeBeActiveSlide(string id)
        {
            JsonResult result;
            try
            {
                var res = await _sliderRepository.ActiveSlide(id);
                if (res.Succeeded)
                {
                    var slide =  _sliderRepository.GetSlide(id);
                    result = Json(new { Status = "success", Message = res.Message, result = slide.IsActive.ToString() });
                }
                else
                {
                    result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
                }
            }
            catch (Exception e)
            {
                result = Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_TryLator") });
            }
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> DeleteSlide(string id)
        {
            try
            {
                bool result = await _sliderRepository.DeleteSlide(id);

                if (result)
                {
                    return Json(new { Status = "success", Message = Language.GetString("AlertAndMessage_DeletionDoneSuccessfully") });
                }
                return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_ErrorTryAgain") });
            }
            catch (Exception e)
            {
                return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_ErrorTryAgain") });
            }
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Layers(string sliderId, string slideId)
        {
            if (TempData["Success"] != null)
            {
                ViewBag.Success = TempData["Success"];
                TempData["Success"] = null;
            }

            ViewBag.SliderId = sliderId;
            ViewBag.SlideId = slideId;
            return View();
        }

        [HttpPost]
        public IActionResult ListLayers([FromForm] SearchParamLayers searchParam)
        {
            ViewBag.SliderId = searchParam.SliderId;
            ViewBag.SlideId = searchParam.SlideId;

            if (ModelState.IsValid)
            {
                //var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);

                try
                {
                    PagedItems<Layer> data = _sliderRepository.ListLayers(searchParam);

                    var dataGrid = new PagedItems<LayersListGridView>()
                    {
                        CurrentPage = data.CurrentPage,
                        PageSize = data.PageSize,
                        ItemsCount = data.ItemsCount,
                        Items = data.Items.Select(n => new LayersListGridView()
                        {
                            Id = n.Id,
                            Link = n.Link,
                            Type = n.Type,
                            Content = n.Content,
                            TransActionType = n.TransActionType,
                            Position = n.Position
                        }).ToList()
                    };

                    //ViewBag.Permissions = dicKey;

                    return View(dataGrid);
                }
                catch (Exception e)
                {
                    return View(new PagedItems<LayersListGridView>());
                }
            }
            return View(new PagedItems<LayersListGridView>()
            {
                Items = new List<LayersListGridView>()
            });
        }

        [HttpGet]
        public IActionResult AddLayer(string sliderId, string slideId)
        {
            ViewBag.Action = "Add";

            return View("ActionLayer", new LayerView()
            {
                SliderId = sliderId,
                SlideId = slideId
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddLayer(LayerView model)
        {
            ViewBag.Action = "Add";
            if (!ModelState.IsValid)
            {
                return View("ActionLayer", model);
            }

            Slider slider = _sliderRepository.GetSlider(model.SliderId);

            var layer = new Layer()
            {
                Id = Guid.NewGuid().ToString(),
                Content = model.Content,
                Type = model.Type,
                Link = model.Link,
                Target = model.Target,
                TransActionType = model.TransActionType,
                Position = model.Position,
                Styles = model.Styles,
                Attributes = model.Attributes
            };

            slider.Slides.FirstOrDefault(s => s.Id == model.SlideId)?.Layers.Add(layer);

            bool result = await _sliderRepository.Update(slider);

            if (result)
            {
                TempData["Success"] = Language.GetString("AlertAndMessage_InsertionDoneSuccessfully");
                return RedirectToAction("Layers", new { model.SliderId, model.SlideId });
            }

            ViewBag.Error = Language.GetString("AlertAndMessage_ErrorTryAgain");
            return View("ActionLayer", model);
        }

        [HttpGet]
        public IActionResult EditLayer(string sliderId, string slideId, string layerId)
        {
            ViewBag.Action = "Edit";

            Layer layer = _sliderRepository.GetLayer(layerId);

            return View("ActionLayer", new LayerView()
            {
                Id = layer.Id,
                SliderId = sliderId,
                SlideId = slideId,
                Styles = layer.Styles,
                Link = layer.Link,
                Type = layer.Type,
                Content = layer.Content,
                Position = layer.Position,
                Attributes = layer.Attributes,
                Target = layer.Target,
                TransActionType = layer.TransActionType
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditLayer(LayerView model)
        {
            ViewBag.Action = "Edit";
            if (!ModelState.IsValid)
            {
                return View("ActionLayer", model);
            }

            var layer = _sliderRepository.GetLayer(model.LayerId);

            layer.Content = model.Content;
            layer.Type = model.Type;
            layer.Link = model.Link;
            layer.Target = model.Target;
            layer.TransActionType = model.TransActionType;
            layer.Position = model.Position;
            layer.Styles = model.Styles;
            layer.Attributes = model.Attributes;

            bool result = await _sliderRepository.UpdateLayer(layer);

            if (result)
            {
                TempData["Success"] = Language.GetString("AlertAndMessage_EditionDoneSuccessfully");
                return RedirectToAction("Layers", new { model.SliderId, model.SlideId });
            }

            ViewBag.Error = Language.GetString("AlertAndMessage_ErrorTryAgain");
            return View("ActionLayer", model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeBeActiveLayer(string id, bool IsActive)
        {
            try
            {
                bool result = await _sliderRepository.ActiveLayer(id, IsActive);
                if (result)
                {
                    return Json(new { Status = "success", Message = Language.GetString("AlertAndMessage_InsertionDoneSuccessfully") });
                }
                return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_ErrorTryAgain") });
            }
            catch (Exception e)
            {
                return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_ErrorTryAgain") });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteLayer(string id)
        {
            try
            {
                bool result = await _sliderRepository.DeleteLayer(id);

                if (result)
                {
                    return Json(new { Status = "success", Message = Language.GetString("AlertAndMessage_DeletionDoneSuccessfully") });
                }
                return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_ErrorTryAgain") });
            }
            catch (Exception e)
            {
                return Json(new { Status = "error", Message = Language.GetString("AlertAndMessage_ErrorTryAgain") });
            }
        }
    }
}
