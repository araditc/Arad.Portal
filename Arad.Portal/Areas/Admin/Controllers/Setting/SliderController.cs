using Arad.Portal.Areas.Admin.Controllers.Product;
using Arad.Portal.DataLayer.Entities.General.SliderModule;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SliderModule;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.Helpers.Admin;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.SlideModule;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Serilog;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;

using Image = Arad.Portal.DataLayer.Models.Shared.Image;

namespace Arad.Portal.Areas.Admin.Controllers.Setting;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class SliderController(
    ISliderRepository sliderRepository,
    IMapper mapper,
    ILogger logger,
    MinioHelper minioHelper,
    IModificationRepository modificationRepository,
    ControllerHelper controllerHelper) : Controller
{

    private ClientValidationErrorModel ValidatorPic(string model)
    {
        string img = model;

        int cc = img.IndexOf("/", StringComparison.Ordinal);
        int kk = img.IndexOf(",", StringComparison.Ordinal);
        string extension = img.Substring(cc + 1, 4);

        string aa = img.Substring(kk + 1);
        byte[] bytes = Convert.FromBase64String(aa);

        double fileSizeKb = bytes.Length / 1024;

        return new() { Key = "0", ErrorMessage = "" };
    }

    [HttpGet]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        Result<Slider> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            Slider slider = await sliderRepository.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (slider == null)
            {
                opResult.Message = ConstMessages.ObjectNotFound;
                result = new(new
                {
                    Status = "error",
                    Message = UtilityLanguage.GetString("AlertAndMessage_EntityNotFound")
                });
            }
            else
            {
                opResult = await sliderRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);
                if (opResult.Succeeded)
                {
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, restoring slider with {slider.Id} id and {slider.Title} done successfully");
                    opResult.Message = ConstMessages.SuccessfullyDone;
                    result = new(new
                    {
                        Status = "success",
                        Message = UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully")
                    });
                }
                else
                {
                    opResult.Message = ConstMessages.ErrorInSaving;
                    result = new(new
                    {
                        Status = "error",
                        Message = UtilityLanguage.GetString("AlertAndMessage_TryLater")
                    });
                }
            }

        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SliderController)}/{nameof(Restore)}");
            opResult.Message = ConstMessages.ExceptionOccured;
            result = new(new
            {
                Status = "error",
                Message = UtilityLanguage.GetString("AlertAndMessage_TryLater")
            });
        }
        return result;
    }

    public async ValueTask<IActionResult> Index(CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            List<Slider> list;

            if (userDb.IsSystemAccount)
            {
                list = await sliderRepository.GetListAsync(_ => true, cancellationToken);
                ViewBag.Domains = controllerHelper.GetAllActiveDomains();

                ViewBag.DomainId = controllerHelper.GetCurrentUserDomain().Id;
            }
            else
            {
                list = await sliderRepository.GetListAsync(s => s.AssociatedDomainId == userDb.Domains.FirstOrDefault(a => a.IsOwner)!.DomainId, cancellationToken);

                string? domainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;

                if (domainId != null)
                {
                    ViewBag.DomainId = domainId;
                }
            }

            ViewBag.IsSysAcc = userDb.IsSystemAccount;
            return View(list);
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SliderController)}/{nameof(Index)}");
            return NotFound();
        }
    }

    [HttpPost]
    public async ValueTask<IActionResult> AddSlider(SliderAddView dto, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        List<ClientValidationErrorModel> errors = [];
        try
        {
            if (!ModelState.IsValid)
            {
                errors = ModelState.Generate();

                return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
            }

            Slider sliderModel = new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = dto.Title,
                AssociatedDomainId = dto.AssociatedDomainId,
                IsActive = true,
                CreatorUserId = userDb.Id,
                CreatorUserName = userDb.UserName,
                CreationDate = DateTime.Now
            };
            Result<Slider> result = await sliderRepository.InsertAsync(sliderModel, cancellationToken);

            if (!result.Succeeded)
            {
                return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = errors });
            }

            Modification modification = new()
            {
                Id = Guid.NewGuid().ToString(),
                ActionTypes = ActionTypes.Insert,
                CollectionType = CollectionType.Slider,
                Ip = controllerHelper.GetUserIpAddress(),
                ModifierId = userDb.Id,
                ModifierUserName = userDb.UserName,
                ModifyDateTime = DateTime.Now,
                RecordId = sliderModel.Id
            };
            Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

            logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},adding slider with {dto.Id} id and {dto.Title} done successfully");
            return Json(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_InsertionDoneSuccessfully"), ModelStateErrors = errors });
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SliderController)}/{nameof(AddSlider)}");
        }
        return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = errors });
    }

    [HttpGet]
    public IActionResult GetSlider([FromQuery] string sliderId)
    {
        Slider slide = controllerHelper.GetSlider(sliderId, "");
        return slide != null ? Json(new { Status = "success", Data = slide }) : Json(new { Status = "error", Data = slide, message = UtilityLanguage.GetString("AlertAndMessage_NotFound") });
    }

    [HttpPost]
    public async ValueTask<IActionResult> EditSlider(SliderAddView dto, CancellationToken cancellationToken)
    {
        List<ClientValidationErrorModel> errors = [];
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            if (!ModelState.IsValid)
            {
                errors = ModelState.Generate();
                return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
            }

            Slider slider = controllerHelper.GetSlider(dto.Id, "");

            Slider sliderModel = mapper.Map(dto, slider);

            Result<Slider> result = await sliderRepository.UpdateAsync(sliderModel, cancellationToken);

            if (!result.Succeeded)
            {
                return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = errors });
            }

            Modification modification = new()
            {
                Id = Guid.NewGuid().ToString(),
                ActionTypes = ActionTypes.Update,
                CollectionType = CollectionType.Slider,
                Ip = controllerHelper.GetUserIpAddress(),
                ModifierId = userDb.Id,
                ModifierUserName = userDb.UserName,
                ModifyDateTime = DateTime.Now,
                RecordId = slider.Id
            };
            Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
            logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},editing slider with {dto.Id} id and {dto.Title} done successfully");
            return Json(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully"), ModelStateErrors = errors });
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SliderController)}/{nameof(EditSlider)}");
            return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = errors }); ;
        }
    }

    [HttpGet]
    public async ValueTask<IActionResult> ChangeBeActiveSlider(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        Result<Slider> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            Slider slider = controllerHelper.GetSlider(id, "");
            if (slider == null)
            {
                opResult.Message = ConstMessages.ObjectNotFound;
                result = Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_EntityNotFound") });
            }
            else
            {
                if (slider.IsActive)
                {
                    opResult = await sliderRepository.UpdateAsync(c => c.Id == id, m => m.IsActive, false, cancellationToken);
                }
                else
                {
                    opResult = await sliderRepository.UpdateAsync(c => c.Id == id, m => m.IsActive, true, cancellationToken);
                }

                if (opResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Update,
                        CollectionType = CollectionType.Slider,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = slider.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, changing be active slider with {slider.Id} id and {slider.Title} done successfully");
                    opResult.Message = UtilityLanguage.GetString("AlertAndMessage_OperationDoneSuccessfully");
                    result = Json(new { Status = "success", opResult.Message, result = slider.IsActive.ToString() });
                }
                else
                {
                    result = Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
                }
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SliderController)}/{nameof(ChangeBeActiveSlider)}");
            result = Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
        }
        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> DeleteSlider(string id, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            Slider slider = controllerHelper.GetSlider(id, "");
            if (slider == null)
            {
                return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_EntityNotFound") });
            }
            else
            {
                Result<Slider> result = await sliderRepository.UpdateAsync(c => c.Id == id, c => c.IsDeleted, true, cancellationToken);
                if (result.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Delete,
                        CollectionType = CollectionType.Slider,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = slider.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, deleting slider with {slider.Id} id and {slider.Title} done successfully");
                    return Json(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_DeletionDoneSuccessfully") });
                }
                else
                {
                    return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_DeletionNotAllowed") });
                }
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SliderController)}/{nameof(DeleteSlider)}");
            return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_DeletionNotAllowed") });
        }
    }

    [HttpGet]
    public async ValueTask<IActionResult> Slides(string id, CancellationToken cancellationToken)
    {
        Slider slider = await sliderRepository.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false, cancellationToken);

        ViewBag.SliderId = slider.Id;

        return View("AddSlide", slider.Title);
    }

    [HttpPost]
    public async ValueTask<IActionResult> ListSlides([FromForm] SearchParamSlides searchParam, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(new PagedItems<SlidesListGridView> { Items = [] });
        }

        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            IQueryable<Slider> query = (await sliderRepository.GetListAsync(n => !n.IsDeleted && n.Id == searchParam.SliderId, cancellationToken)).AsQueryable();

            IQueryable<Slide> querySlides = query.SelectMany(s => s.Slides)
                                                 .Where(s => s.IsDeleted != 1);

            int count = querySlides.Count();

            querySlides = querySlides
                          .Skip((searchParam.CurrentPage - 1) * searchParam.PageSize)
                          .Take(searchParam.PageSize);

            List<Slide> news = querySlides.ToList();
            Domain domain = controllerHelper.GetCurrentUserDomain();

            PagedItems<Slide> data = new()
            {
                CurrentPage = searchParam.CurrentPage,
                ItemsCount = count % searchParam.PageSize == 0
                                                          ? count / searchParam.PageSize
                                                          : count / searchParam.PageSize + 1,
                PageSize = searchParam.PageSize,
                Items = (await Task.WhenAll(news.Select(async u =>
                                                        {
                                                            string objectName = $"{domain.Id}/{u.Id}.jpg";
                                                            string imageUrl = await GetImageUrlAsync(objectName); // Fetch image URL from MinIO

                                                            return new Slide
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
                                                                ImageUrl = imageUrl,
                                                                Layers = u.Layers,
                                                                Target = u.Target,
                                                                TransActionType = u.TransActionType
                                                            };
                                                        }))).ToList() // <-- Ensure to convert to List<Slide>
            };

            PagedItems<SlidesListGridView> dataGrid = new()
            {
                CurrentPage = data.CurrentPage,
                PageSize = data.PageSize,
                ItemsCount = data.ItemsCount,
                Items = data.Items.Select(n => new SlidesListGridView
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
                    PersianStartDate = n.StartDate.Value.ToPersianDdate(),
                    PersianExpireDate = n.ExpireDate.Value.ToPersianDdate()
                }).ToList()
            };

            //ViewBag.Permissions = dicKey;
            ViewBag.SliderId = searchParam.SliderId;
            return View(dataGrid);
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SliderController)}/{nameof(ListSlides)}");
            return View(new PagedItems<SlidesListGridView>());
        }
    }

    private async ValueTask<string> GetImageUrlAsync(string objectName)
    {
        try
        {
            CultureInfo current = new("en-US")
            {
                DateTimeFormat = new()
                {
                    Calendar = new GregorianCalendar()
                }
            };
            Thread.CurrentThread.CurrentCulture = current;
            string url = string.Empty;
            (bool success, byte[] imageData) = await minioHelper.GetObject("slideimage", objectName);
            if (success)
            {
                url = Convert.ToBase64String(imageData);
            }
            DataLayer.Entities.General.Language.Language defLang = controllerHelper.GetDefaultLanguage();
            CultureInfo current2 = new(defLang.Symbol)
            {
                DateTimeFormat = new()
                {
                    Calendar = new GregorianCalendar()
                }
            };
            Thread.CurrentThread.CurrentCulture = current2;
            return url;
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to get image URL from MinIO. ObjectName: {objectName}, Error: {ex.Message}");
            return null;
        }
    }

    [HttpGet]
    public async ValueTask<IActionResult> SlidesOfSlider(string id, CancellationToken cancellationToken)
    {
        Slider slider = await sliderRepository.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false, cancellationToken);
        return Json(new { Status = "", Data = slider.Slides });
    }

    [HttpPost]
    public async ValueTask<IActionResult> AddSlide(SlideView dto, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        List<ClientValidationErrorModel> errors = [];

        try
        {
            if (CultureInfo.CurrentCulture.Name == "fa-IR")
            {
                if (string.IsNullOrWhiteSpace(dto.PersianStartDate))
                {
                    ModelState.AddModelError(nameof(dto.StartDate), UtilityLanguage.GetString("AlertAndMessage_FieldEssential"));
                }
            }
            else
            {
                if (dto.StartDate == null)
                {
                    ModelState.AddModelError(nameof(dto.StartDate), UtilityLanguage.GetString("AlertAndMessage_FieldEssential"));
                }

                if (dto.ExpireDate != null && dto.ExpireDate <= dto.StartDate)
                {
                    ModelState.AddModelError(nameof(dto.ExpireDate), UtilityLanguage.GetString("AlertAndMessage_EndDateMustBeGreaterThanStartDate"));
                }
            }


            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                ClientValidationErrorModel validate = ValidatorPic(dto.ImageUrl);

                if (validate.Key == "1")
                {
                    ModelState.AddModelError(nameof(SlideView.ImageUrl), validate.ErrorMessage);
                }
            }


            if (!ModelState.IsValid)
            {
                errors = ModelState.Generate();

                return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
            }

            Slider slider = await sliderRepository.FirstOrDefaultAsync(c => c.Id == dto.SliderId, cancellationToken);
            dto.Id = Guid.NewGuid().ToString();
            Image img = new()
            {
                ImageId = dto.Id,
                Content = dto.ImageUrl,
                ImageRatio = ImageRatio.TwoToOne,
                ImageRatioId = 2,
                Title = dto.Title,
                IsMain = false,
                Url = dto.ImageUrl
            };
            Domain domain = controllerHelper.GetCurrentUserDomain();
            string objectName = $"{domain.Id}/{img.ImageId}.jpg";
            byte[] bytes = Convert.FromBase64String(img.Content.Replace("data:image/jpeg;base64,", ""));
            SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(bytes);
            MemoryStream ms = new();
            await image.SaveAsJpegAsync(ms, cancellationToken);
            ms.Seek(0, SeekOrigin.Begin);
            if (CultureInfo.CurrentCulture.Name == "fa-IR")
            {
                CultureInfo current = new("en-US")
                {
                    DateTimeFormat = new()
                    {
                        Calendar = new GregorianCalendar()
                    }
                };
                Thread.CurrentThread.CurrentCulture = current;
                img.ImageId = Guid.NewGuid().ToString();

                bool isBucket1 = await minioHelper.MakeBucket("slideimage");
                if (isBucket1)
                {
                    bool isSave = await minioHelper.Upload("slideimage", objectName, ms, "image/jpg", ms.Length);
                    if (isSave)
                    {
                        logger.Information($"image with {img.ImageId} id in slider with {slider.Id} id saved correctly");
                    }
                    else
                    {
                        logger.Error($"image not save correctly. stack trace: {nameof(SliderController)}/{nameof(AddSlide)}");
                    }
                }
                //var resultUpload = Upload(img);

                if (string.IsNullOrWhiteSpace(img.Content)) //image doesn't save successfully so the content isn't string.empty
                {
                    ClientValidationErrorModel obj = new()
                    {
                        Key = "ImageUrl",
                        ErrorMessage = UtilityLanguage.GetString("AlertAndMessage_ErrorInUploadFile"),
                    };
                    errors =
                    [
                        obj
                    ];

                    return new JsonResult(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
                }
                CultureInfo current2 = new("fa-IR")
                {
                    DateTimeFormat = new()
                    {
                        Calendar = new GregorianCalendar()
                    }
                };
                Thread.CurrentThread.CurrentCulture = current2;
                Slide slide1 = new()
                {
                    Id = dto.Id,
                    Alt = dto.Alt,
                    ColoredBackground = dto.ColoredBackground,
                    ExpireDate = dto.PersianExpireDate.ToEnglishDate(),
                    ImageFit = dto.ImageFit,
                    ImageUrl = img.Url,
                    Link = dto.Link,
                    StartDate = CultureInfo.CurrentCulture.Name == "fa-IR" && !string.IsNullOrWhiteSpace(dto.PersianStartDate) ? dto.PersianStartDate.ToEnglishDate() : dto.StartDate,
                    Target = dto.Target,
                    TransActionType = dto.TransActionType,
                    Title = img.Title,
                    IsActive = true,
                    IntervalTime = int.Parse(dto.IntervalTime)

                };

                slider.Slides.Add(slide1);

                Result<Slider> result1 = await sliderRepository.UpdateAsync(slider, cancellationToken);

                if (!result1.Succeeded)
                {
                    return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = errors });
                }

                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Insert,
                    CollectionType = CollectionType.Slider,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = slider.Id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},adding slide with {dto.Id} id and {dto.Title} done successfully");
                return Json(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_InsertionDoneSuccessfully"), ModelStateErrors = errors });
            }
            else
            {
                bool isBucket = await minioHelper.MakeBucket("slideimage");
                if (isBucket)
                {
                    bool isSave = await minioHelper.Upload("slideimage", objectName, ms, "image/jpg", ms.Length);
                    if (isSave)
                    {
                        logger.Information($"image with {img.ImageId} id in slider with {slider.Id} id saved correctly");
                    }
                    else
                    {
                        logger.Error($"image not save correctly. stack trace: {nameof(SliderController)}/{nameof(AddSlide)}");
                    }
                }
                //var resultUpload = Upload(img);

                if (string.IsNullOrWhiteSpace(img.Content)) //image doesn't save successfully so the content isn't string.empty
                {
                    ClientValidationErrorModel obj = new()
                    {
                        Key = "ImageUrl",
                        ErrorMessage = UtilityLanguage.GetString("AlertAndMessage_ErrorInUploadFile"),
                    };
                    errors =
                    [
                        obj
                    ];

                    return new JsonResult(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
                }

                Slide slide = new()
                {
                    Id = dto.Id,
                    Alt = dto.Alt,
                    ColoredBackground = dto.ColoredBackground,
                    ExpireDate = dto.PersianExpireDate.ToEnglishDate(),
                    ImageFit = dto.ImageFit,
                    ImageUrl = img.Url,
                    Link = dto.Link,
                    StartDate = CultureInfo.CurrentCulture.Name == "fa-IR" && !string.IsNullOrWhiteSpace(dto.PersianStartDate) ? dto.PersianStartDate.ToEnglishDate() : dto.StartDate,
                    Target = dto.Target,
                    TransActionType = dto.TransActionType,
                    Title = img.Title,
                    IsActive = true
                };

                slider.Slides.Add(slide);
                DataLayer.Entities.General.Language.Language defLang = controllerHelper.GetDefaultLanguage();
                CultureInfo current2 = new(defLang.Symbol)
                {
                    DateTimeFormat = new()
                    {
                        Calendar = new GregorianCalendar()
                    }
                };
                Thread.CurrentThread.CurrentCulture = current2;
                Result<Slider> result = await sliderRepository.UpdateAsync(slider, cancellationToken);

                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Insert,
                    CollectionType = CollectionType.Slider,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = slider.Id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                if (!result.Succeeded)
                {
                    return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = errors });
                }

                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},adding slide with {dto.Id} id and {dto.Title} done successfully");
                return Json(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_InsertionDoneSuccessfully"), ModelStateErrors = errors });
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductSpecificationGroupController)}/{nameof(AddSlide)}");
            return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = errors });
        }
    }

    [HttpGet]
    public async ValueTask<IActionResult> GetSlide(string slideId, CancellationToken cancellationToken)
    {
        SlideDto outputModel = new();
        Slide slide = (await sliderRepository.GetAllAsync(cancellationToken)).SelectMany(c => c.Slides).FirstOrDefault(c => c.Id == slideId);

        if (slide == null)
        {
            return Json(new { Status = "error", Data = outputModel, message = UtilityLanguage.GetString("AlertAndMessage_NotFound") });
        }

        outputModel = mapper.Map<SlideDto>(slide);

        if (outputModel.StartDate != null)
        {
            outputModel.PersianStartShowDate = outputModel.StartDate.Value.ToPersianDdate();
        }

        if (slide.ExpireDate == null)
        {
            return Json(new { Status = "success", Data = outputModel });
        }

        if (outputModel.ExpireDate != null)
        {
            outputModel.PersianEndShowDate = outputModel.ExpireDate.Value.ToPersianDdate();
        }

        return Json(new { Status = "success", Data = outputModel });

    }

    [HttpPost]
    public async ValueTask<IActionResult> EditSlide(SlideView model, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        List<ClientValidationErrorModel> errors = [];

        try
        {

            if (CultureInfo.CurrentCulture.Name == "fa-IR")
            {
                if (string.IsNullOrWhiteSpace(model.PersianStartDate))
                {
                    ModelState.AddModelError(nameof(model.StartDate), UtilityLanguage.GetString("AlertAndMessage_FieldEssential"));
                }
                model.StartDate = model.PersianStartDate.Split(" ")[0].ToEnglishDate();
                if (!string.IsNullOrWhiteSpace(model.PersianExpireDate))
                {
                    model.ExpireDate = model.PersianExpireDate.Split(" ")[0].ToEnglishDate();
                }

            }
            else
            {
                if (model.StartDate == null)
                {
                    ModelState.AddModelError(nameof(model.StartDate), UtilityLanguage.GetString("AlertAndMessage_FieldEssential"));
                }

                if (model.ExpireDate != null && model.ExpireDate <= model.StartDate)
                {
                    ModelState.AddModelError(nameof(model.ExpireDate), UtilityLanguage.GetString("AlertAndMessage_EndDateMustBeGreaterThanStartDate"));
                }
            }


            if (model.ExpireDate <= model.StartDate)
            {
                ModelState.AddModelError(nameof(model.ExpireDate), UtilityLanguage.GetString("AlertAndMessage_EndDateMustBeGreaterThanStartDate"));
            }
            if (!string.IsNullOrEmpty(model.ImageUrl))
            {
                if (model.ImageUrl.StartsWith("data"))
                {
                    ClientValidationErrorModel validate = ValidatorPic(model.ImageUrl);

                    if (validate.Key == "1")
                    {
                        ModelState.AddModelError(nameof(SlideView.ImageUrl), validate.ErrorMessage);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                errors = ModelState.Generate();

                return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
            }
            CultureInfo current = new("en-US")
            {
                DateTimeFormat = new()
                {
                    Calendar = new GregorianCalendar()
                }
            };
            Thread.CurrentThread.CurrentCulture = current;
            Image image = new();

            if (string.IsNullOrEmpty(model.ImageUrl))
            {
                return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = errors });
            }

            if (model.ImageUrl.StartsWith("data"))
            {
                Image img = new()
                {
                    ImageId = model.Id,
                    Content = model.ImageUrl,
                    ImageRatio = ImageRatio.TwoToOne,
                    ImageRatioId = 2,
                    Title = model.Title,
                    IsMain = false
                };
                Domain domain = controllerHelper.GetCurrentUserDomain();
                string objectName = $"{domain.Id}/{img.ImageId}.jpg";
                byte[] bytes = Convert.FromBase64String(img.Content.Replace("data:image/jpeg;base64,", ""));
                SixLabors.ImageSharp.Image images = SixLabors.ImageSharp.Image.Load(bytes);
                MemoryStream ms = new();
                await images.SaveAsJpegAsync(ms, cancellationToken);
                ms.Seek(0, SeekOrigin.Begin);
                bool isBucket = await minioHelper.MakeBucket("slideimage");
                if (isBucket)
                {
                    bool isDelete = await minioHelper.RemoveObject("slideimage", objectName);
                    bool isSave = await minioHelper.Upload("slideimage", objectName, ms, "image/jpg", ms.Length);
                    if (isSave)
                    {
                        logger.Information($"image with {img.ImageId} id in slide with {model.Id} id saved correctly");
                    }
                    else
                    {
                        logger.Error($"image not save correctly. stack trace: {nameof(SliderController)}/{nameof(EditSlide)}");
                    }
                }

                if (!string.IsNullOrWhiteSpace(image.Content)) //save doesn't occured successfully
                {
                    ClientValidationErrorModel obj = new()
                    {
                        Key = "ImageUrl",
                        ErrorMessage = UtilityLanguage.GetString("AlertAndMessage_ErrorInUploadFile"),
                    };
                    errors =
                    [
                        obj
                    ];

                    model.Title = image.Title;

                    return new JsonResult(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
                }
            }
            //Slide slide = new()
            //{
            //    Id = model.Id,
            //    Alt = model.Alt,
            //    ColoredBackground = model.ColoredBackground,
            //    ExpireDate = model.ExpireDate,
            //    ImageFit = model.ImageFit,
            //    ImageUrl = model.ImageUrl.StartsWith("data") ? image.Url : model.ImageUrl,
            //    Link = model.Link,
            //    StartDate = model.StartDate,
            //    Target = model.Target,
            //    TransActionType = model.TransActionType,
            //    Title = model.Title,
            //    IsActive = true
            //};
            Slider slider = await sliderRepository
                                .FirstOrDefaultAsync(s => s.Slides.Any(ss => ss.Id == model.Id), cancellationToken);

            if (slider == null)
            {
                return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = errors });
            }

            {
                Slide slideOld = slider.Slides
                                       .FirstOrDefault(s => s.Id == model.Id);

                if (slideOld != null)
                {
                    slideOld.Alt = model.Alt;
                    slideOld.ColoredBackground = model.ColoredBackground;
                    slideOld.ExpireDate = model.ExpireDate;
                    slideOld.ImageFit = model.ImageFit;
                    slideOld.ImageUrl = model.ImageUrl;
                    slideOld.Link = model.Link;
                    slideOld.StartDate = model.StartDate;
                    slideOld.Title = model.Title;
                    slideOld.Target = model.Target;
                    slideOld.TransActionType = model.TransActionType;
                    slideOld.IntervalTime = int.Parse(model.IntervalTime);

                }

                slider.Slides.RemoveAll(c => c.Id == slideOld.Id);
                slider.Slides.Add(slideOld);

                DataLayer.Entities.General.Language.Language defLang = controllerHelper.GetDefaultLanguage();
                CultureInfo current2 = new(defLang.Symbol)
                {
                    DateTimeFormat = new()
                    {
                        Calendar = new GregorianCalendar()
                    }
                };
                Thread.CurrentThread.CurrentCulture = current2;
                Result<Slider> result = await sliderRepository.UpdateAsync(slider, cancellationToken);

                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Update,
                    CollectionType = CollectionType.Slider,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = slider.Id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                if (!result.Succeeded)
                {
                    return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = errors });
                }

                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},editing slide with {slider.Id} id and {slider.Title} done successfully");
                return Json(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully"), ModelStateErrors = errors });
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SliderController)}/{nameof(EditSlide)}");
            return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain"), ModelStateErrors = errors });
        }

    }

    [HttpGet]
    public async ValueTask<IActionResult> ChangeBeActiveSlide(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            Slide slide = (await sliderRepository.GetAllAsync(cancellationToken)).SelectMany(c => c.Slides).FirstOrDefault();
            Result<Slider> opResult = await sliderRepository.UpdateAsync(c => c.Id == id, m => m.IsActive, true, cancellationToken);
            if (opResult.Succeeded)
            {
                if (slide != null)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Update,
                        CollectionType = CollectionType.Slider,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = slide.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
                }

                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},changing be active slide with {id} id done successfully");
                result = Json(new { Status = "success", opResult.Message, result = (slide is { IsActive: true }).ToString() });
            }
            else
            {
                result = Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SliderController)}/{nameof(ChangeBeActiveSlide)}");
            result = Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
        }
        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> DeleteSlide(string id, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        Slider slider = await sliderRepository.FirstOrDefaultAsync(c => c.Slides.Any(m => m.Id == id), cancellationToken);
        slider.Slides.RemoveAll(c => c.Id == id);
        try
        {
            Result<Slider> result = await sliderRepository.UpdateAsync(slider, cancellationToken);

            if (!result.Succeeded)
            {
                return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain") });
            }

            Modification modification = new()
            {
                Id = Guid.NewGuid().ToString(),
                ActionTypes = ActionTypes.Delete,
                CollectionType = CollectionType.Slider,
                Ip = controllerHelper.GetUserIpAddress(),
                ModifierId = userDb.Id,
                ModifierUserName = userDb.UserName,
                ModifyDateTime = DateTime.Now,
                RecordId = id
            };
            Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

            logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},deleting slide with {id} id done successfully");
            return Json(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_DeletionDoneSuccessfully") });
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SliderController)}/{nameof(DeleteSlide)}");
            return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain") });
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
    public async ValueTask<IActionResult> ListLayers([FromForm] SearchParamLayers searchParam, CancellationToken cancellationToken)
    {
        ViewBag.SliderId = searchParam.SliderId;
        ViewBag.SlideId = searchParam.SlideId;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(new PagedItems<LayersListGridView> { Items = [] });
        }

        try
        {
            List<Slider> query = await sliderRepository.GetListAsync(n => !n.IsDeleted && n.Id == searchParam.SliderId, cancellationToken);

            IEnumerable<Slide> querySlides = query.SelectMany(s => s.Slides)
                                                  .Where(s => s.IsDeleted != 1 && s.Id == searchParam.SlideId);

            IEnumerable<Layer> queryLayers = querySlides.SelectMany(s => s.Layers)
                                                        .Where(s => s.IsDeleted != 1);

            int count = queryLayers.Count();

            queryLayers = queryLayers
                          .Skip((searchParam.CurrentPage - 1) * searchParam.PageSize)
                          .Take(searchParam.PageSize); ;

            List<Layer> news = queryLayers.ToList();

            PagedItems<Layer> data = new()
            {
                CurrentPage = searchParam.CurrentPage,
                ItemsCount = count % searchParam.PageSize == 0
                                                          ? count / searchParam.PageSize
                                                          : count / searchParam.PageSize + 1,
                PageSize = searchParam.PageSize,
                Items = news.Select(u => new Layer
                {
                    Id = u.Id,
                    Link = u.Link,
                    Type = u.Type,
                    Content = u.Content,
                    Position = u.Position,
                    TransActionType = u.TransActionType
                }).ToList()
            };

            PagedItems<LayersListGridView> dataGrid = new()
            {
                CurrentPage = data.CurrentPage,
                PageSize = data.PageSize,
                ItemsCount = data.ItemsCount,
                Items = data.Items.Select(n => new LayersListGridView
                {
                    Id = n.Id,
                    Link = n.Link,
                    Type = n.Type,
                    Content = n.Content,
                    TransActionType = n.TransActionType,
                    Position = n.Position
                }).ToList()
            };
            return View(dataGrid);
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SliderController)}/{nameof(ListLayers)}");
            return View(new PagedItems<LayersListGridView>());
        }
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
    public async ValueTask<IActionResult> AddLayer(LayerView dto, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            ViewBag.Action = "Add";
            if (!ModelState.IsValid)
            {
                return View("ActionLayer", dto);
            }

            Slider slider = await sliderRepository.FirstOrDefaultAsync(c => c.Id == dto.SliderId, cancellationToken);

            Layer layer = new()
            {
                Id = Guid.NewGuid().ToString(),
                Content = dto.Content,
                Type = dto.Type,
                Link = dto.Link,
                Target = dto.Target,
                TransActionType = dto.TransActionType,
                Position = dto.Position,
                Styles = dto.Styles,
                Attributes = dto.Attributes
            };

            slider.Slides.FirstOrDefault(s => s.Id == dto.SlideId)?.Layers.Add(layer);

            Result<Slider> result = await sliderRepository.UpdateAsync(slider, cancellationToken);

            if (result.Succeeded)
            {
                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Insert,
                    CollectionType = CollectionType.Slider,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = layer.Id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, adding layer with {dto.Id} id and {layer.Content} done successfully");
                TempData["Success"] = UtilityLanguage.GetString("AlertAndMessage_InsertionDoneSuccessfully");
                return RedirectToAction("Layers", new { dto.SliderId, dto.SlideId });
            }

            ViewBag.Error = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain");
            return View("ActionLayer", dto);
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SliderController)}/{nameof(AddLayer)}");
            ViewBag.Error = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain");
            return View("ActionLayer", dto);
        }
    }

    [HttpGet]
    public async ValueTask<IActionResult> EditLayer(string sliderId, string slideId, string layerId, CancellationToken cancellationToken)
    {
        ViewBag.Action = "Edit";

        Layer layer = (await sliderRepository.GetAllAsync(cancellationToken)).AsQueryable().SelectMany(s => s.Slides).SelectMany(s => s.Layers).FirstOrDefault(l => l.Id == layerId);
        LayerView layerDto = mapper.Map<LayerView>(layer);
        layerDto.SliderId = sliderId;
        layerDto.SlideId = slideId;
        return View("ActionLayer", layerDto);
    }

    [HttpPost]
    public async ValueTask<IActionResult> EditLayer(LayerView model, CancellationToken cancellationToken)
    {
        ViewBag.Action = "Edit";
        if (!ModelState.IsValid)
        {
            return View("ActionLayer", model);
        }
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        Layer layer = (await sliderRepository.GetAllAsync(cancellationToken)).SelectMany(s => s.Slides).SelectMany(s => s.Layers).FirstOrDefault(l => l.Id == model.Id);

        if (layer != null)
        {
            layer.Content = model.Content;
            layer.Type = model.Type;
            layer.Link = model.Link;
            layer.Target = model.Target;
            layer.TransActionType = model.TransActionType;
            layer.Position = model.Position;
            layer.Styles = model.Styles;
            layer.Attributes = model.Attributes;

            try
            {
                Slider slider = await sliderRepository
                                    .FirstOrDefaultAsync(s => s.Slides.Any(ss => ss.Layers.Any(l => l.Id == layer.Id)), cancellationToken);

                if (slider != null)
                {
                    Layer layerOld = slider.Slides.SelectMany(s => s.Layers)
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

                    Result<Slider> result = await sliderRepository.UpdateAsync(slider, cancellationToken);

                    if (result.Succeeded)
                    {
                        Modification modification = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ActionTypes = ActionTypes.Update,
                            CollectionType = CollectionType.Slider,
                            Ip = controllerHelper.GetUserIpAddress(),
                            ModifierId = userDb.Id,
                            ModifierUserName = userDb.UserName,
                            ModifyDateTime = DateTime.Now,
                            RecordId = layer.Id
                        };
                        Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                        logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},editing layer with {model.Id} id and {model.Content} done successfully");
                        TempData["Success"] = UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully");

                        return RedirectToAction("Layers", new { model.SliderId, model.SlideId });
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SliderController)}/{nameof(EditLayer)}");
                ViewBag.Error = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain");

                return View("ActionLayer", model);
            }
        }

        ViewBag.Error = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain");
        return View("ActionLayer", model);
    }

    [HttpGet]
    public async ValueTask<IActionResult> ChangeBeActiveLayer(string id, bool isActive, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            Slider slider = await sliderRepository
                                .FirstOrDefaultAsync(s => s.Slides.Any(ss => ss.Layers.Any(l => l.Id == id)), cancellationToken);

            if (slider == null)
            {
                return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain") });
            }

            {
                Layer layerOld = slider.Slides.SelectMany(s => s.Layers)
                                       .FirstOrDefault(l => l.Id == id);

                if (layerOld != null)
                {
                    layerOld.IsActive = isActive;
                }

                Result<Slider> result = await sliderRepository.UpdateAsync(slider, cancellationToken);

                if (!result.Succeeded)
                {
                    return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain") });
                }

                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Update,
                    CollectionType = CollectionType.Slider,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                if (layerOld != null)
                {
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},changing activating layer with {layerOld.Id} id and {layerOld.Content} done successfully");
                }

                return Json(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_InsertionDoneSuccessfully") });
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SliderController)}/{nameof(ChangeBeActiveLayer)}");
            return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain") });
        }
    }

    [HttpGet]
    public async ValueTask<IActionResult> DeleteLayer(string id, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            Slider slider = await sliderRepository
                                .FirstOrDefaultAsync(s => s.Slides.Any(ss => ss.Layers.Any(l => l.Id == id)), cancellationToken);

            if (slider == null)
            {
                return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain") });
            }

            {
                Layer layerOld = slider.Slides.SelectMany(s => s.Layers)
                                       .FirstOrDefault(l => l.Id == id);

                if (layerOld != null)
                {
                    layerOld.IsDeleted = 1;
                }

                Result<Slider> result = await sliderRepository.UpdateAsync(slider, cancellationToken);

                if (!result.Succeeded)
                {
                    return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain") });
                }

                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Delete,
                    CollectionType = CollectionType.Slider,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                if (layerOld != null)
                {
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},deleting layer with {layerOld.Id} id and {layerOld.Content} done successfully");
                }

                return Json(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_DeletionDoneSuccessfully") });
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SliderController)}/{nameof(DeleteLayer)}");
            return Json(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_ErrorTryAgain") });
        }
    }
}