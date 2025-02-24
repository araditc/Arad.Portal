using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.ApplicationRole;
using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Role;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Helpers.UI;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.Domain;
using Arad.Portal.Models.Shared.Role;
using Arad.Portal.Models.Shared.User;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Serilog;

using Modification = Arad.Portal.DataLayer.Entities.General.Modification.Modification;

namespace Arad.Portal.Areas.Admin.Controllers.Account;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class AccountController(
    UserManager<ApplicationUser> userManager,
    IModificationRepository modificationRepository,
    IRoleRepository roleRepository,
    IMapper mapper,
    IUserRepository userRepository,
    IDomainRepository domainRepository,
    ControllerHelper controllerHelper,
    ILogger logger)
    : Controller
{
    [HttpGet]
    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        int pageSize = 10;
        int page = 1;
        PagedItems<UserListView> result = new();

        try
        {
            ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
            Domain domain = controllerHelper.GetCurrentUserDomain();

            IQueryable<ApplicationUser> users;

            if (domain.OwnerUserId == userDb.Id)
            {
                users = userManager.Users.Where(c => c.Domains.Any(userDomain => userDomain.DomainId == domain.Id) && c.Id != userDb.Id);
            }
            else
            {
                users = userDb.IsSystemAccount
                            ? userManager.Users.Where(_ => true)
                            : userManager.Users.Where(c => c.Domains.Any(userDomain => userDomain.DomainId == domain.Id) && c.AssociateDomainId == domain.Id && c.Id != userDb.Id);
            }

            NameValueCollection queryParams = HttpUtility.ParseQueryString(Request.QueryString.ToString());

            if (!string.IsNullOrWhiteSpace(queryParams["page"]))
            {
                page = Convert.ToInt32(queryParams["page"]);
            }

            if (!string.IsNullOrWhiteSpace(queryParams["pageSize"]))
            {
                pageSize = Convert.ToInt32(queryParams["pageSize"]);
            }

            if (!string.IsNullOrWhiteSpace(queryParams["role"]))
            {
                users = users.Where(u => u.UserRoleId == queryParams["role"]);
            }

            if (!string.IsNullOrWhiteSpace(queryParams["domain"]))
            {
                users = users.Where(u => u.AssociateDomainId == queryParams["domain"]);
            }

            if (!string.IsNullOrWhiteSpace(queryParams["name"]))
            {
                users = users.Where(u => u.Profile.FirstName.ToLower().Contains(queryParams["name"]!.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(queryParams["userName"]))
            {
                users = users.Where(u => u.UserName != null && u.UserName.ToLower().Contains(queryParams["userName"]!.ToLower()));
            }


            long count = users.Count();

            List<UserListView> usersDto = mapper.Map<List<UserListView>>(users).OrderByDescending(v => v.CreationDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();

            foreach (UserListView user in usersDto.Where(user => !string.IsNullOrWhiteSpace(user.UserRoleId)))
            {
                ApplicationRole role = await roleRepository.FirstOrDefaultAsync(c => c.Id == user.UserRoleId, cancellationToken);
                user.RoleName = role != null ? role.Name : "";
            }

            result = new() { CurrentPage = page, PageSize = pageSize, ItemsCount = count, Items = usersDto };

            List<ApplicationRole> roleListModel = await roleRepository.GetAllAsync(cancellationToken);
            List<RoleListView> roleListDto = mapper.Map<List<RoleListView>>(roleListModel);
            ViewBag.Roles = roleListDto;
            ViewBag.IsSystemAccount = userDb.IsSystemAccount;
            if (userDb.IsSystemAccount)
            {
                List<Domain>? domains = await domainRepository.GetListAsync(c => c.IsActive && !c.IsDeleted, cancellationToken);
                List<DomainDto>? domainsDto = mapper.Map<List<DomainDto>>(domains);
                ViewBag.Domains = domainsDto;
            }

        }
        catch (Exception)
        {
            // ignored
        }

        return View(result);
    }

    [HttpGet]
    public IActionResult AccessDenied(string returnUrl)
    {
        return View();
    }

    [HttpGet]
    public IActionResult PageOrItemNotFound()
    {
        return View();
    }

    public async ValueTask<IActionResult> AddUser(CancellationToken cancellationToken)
    {
        RegisterUserModel model = new();

        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        ViewBag.LangList = controllerHelper.GetAllActiveLanguage();
        ViewBag.LangId = controllerHelper.GetDefaultLanguage().Id;

        if (userDb.IsSystemAccount)
        {
            ViewBag.IsSystem = true;
            ViewBag.DomainList = controllerHelper.GetAllActiveDomains();
        }
        else
        {
            ViewBag.IsSystem = false;
        }

        Domain domain = controllerHelper.GetCurrentUserDomain();

        // Ensure that rolesModel is not null
        List<RoleListView> rolesModel = (await roleRepository.GetListAsync(c => c.IsActive && !c.IsDeleted && c.AssociatedDomainId == domain.Id, cancellationToken))
                                        .Select(c => new RoleListView { Name = c.Name, Id = c.Id })
                                        .ToList();

            // Add "Choose" option
            rolesModel.Insert(0, new() { Id = "-1", Name = UtilityLanguage.GetString("Choose") });

        model.Roles = rolesModel;

        return View(model); // Pass model with Roles initialized
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> Add([FromForm] RegisterUserModel dto, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        List<ClientValidationErrorModel> errors = []; // Properly initialize the list
        JsonResult result;

        // Validate user role
        if (dto.UserRoleId == "-1")
        {
            ModelState.AddModelError("UserRoleId", UtilityLanguage.GetString("AlertAndMessage_UserRoleRequired"));
        }

        // Check if ModelState is valid
        if (!ModelState.IsValid)
        {
            errors.AddRange(from modelStateKey in ModelState.Keys
                            let modelStateVal = ModelState[modelStateKey]
                            from error in modelStateVal.Errors
                            select new ClientValidationErrorModel
                            {
                                Key = modelStateKey,
                                ErrorMessage = error.ErrorMessage == "AlertAndMessage_MinLength"
                                                          ? modelStateKey == "UserName"
                                                                ? UtilityLanguage.GetString(error.ErrorMessage).Replace("0", UtilityLanguage.GetString("AlertAndMessage_Username")).Replace("1", "3")
                                                                : UtilityLanguage.GetString(error.ErrorMessage).Replace("0", UtilityLanguage.GetString("AlertAndMessage_Password")).Replace("1", "6")
                                                          : UtilityLanguage.GetString(error.ErrorMessage)
                            });

            logger.Warning($"userId: {userDb.Id}, userName: {userDb.UserName}, ModelState is not valid in Adding User");

            return new JsonResult(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
        }

        try
        {
            // Check if the user already exists by phone number or username
            ApplicationUser existUser = await userRepository.FirstOrDefaultAsync(c => c.PhoneNumber == dto.FullMobile.Replace("+", "") && !c.IsDeleted, cancellationToken) ?? await userManager.FindByNameAsync(dto.FullMobile);

            #region Fetch currency from language
            Language lan = controllerHelper.FetchLanguage(dto.DefaultLanguageId);
            CultureInfo userCultureInfo = new(lan.Symbol, false);
            RegionInfo ri = new(userCultureInfo.LCID);
            string currencyPrefix = ri.ISOCurrencySymbol;
            Currency currency = controllerHelper.GetCurrencyByItsPrefix(currencyPrefix);
            #endregion

            dto.DefaultCurrencyId = currency.Id;
            dto.DefaultCurrencyName = currency.CurrencyName;
            Domain domain = controllerHelper.GetCurrentUserDomain();

            if (existUser == null)
            {
                // Create new user
                ApplicationUser user = mapper.Map<ApplicationUser>(dto);
                user.Profile.FirstName = dto.Name;
                user.Profile.LastName = dto.LastName;
                user.Profile.FullName = $"{dto.Name} {dto.LastName}";
                user.Profile.DefaultLanguageId = dto.DefaultLanguageId;
                user.Id = Guid.NewGuid().ToString();
                user.CreationDate = DateTime.Now;
                user.PhoneNumber = dto.FullMobile[1..];
                user.AssociateDomainId = domain.Id;
                UserDomain userDomain = new() { DomainId = domain.Id, DomainName = domain.DomainName, IsOwner = false };

                user.Domains.Add(userDomain);
                IdentityResult res = await userManager.CreateAsync(user, dto.Password);

                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Insert,
                    CollectionType = CollectionType.ApplicationUser,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = user.Id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                if (!res.Succeeded)
                {
                    // Handle identity errors
                    errors.AddRange(res.Errors.Select(error => new ClientValidationErrorModel
                    {
                        Key = error.Code switch
                        {
                            "DuplicateUserName" => "UserName",
                            "PasswordRequiresLower" => "Password",
                            "PasswordRequiresUpper" => "Password",
                            "PasswordRequiresNonAlphanumeric" => "Password",
                            _ => "Error"
                        },
                        ErrorMessage = error.Code switch
                        {
                            "DuplicateUserName" => UtilityLanguage.GetString("AlertAndMessage_DuplicateUsername"),
                            "PasswordRequiresLower" => UtilityLanguage.GetString("AlertAndMessage_PasswordRequiresLower"),
                            "PasswordRequiresUpper" => UtilityLanguage.GetString("AlertAndMessage_PasswordRequiresUpper"),
                            "PasswordRequiresNonAlphanumeric" => UtilityLanguage.GetString("AlertAndMessage_PasswordRequiresUpper"),
                            _ => error.Description
                        }
                    }));

                    result = new(new { Status = "error", Message = "", ModelStateErrors = errors });
                }
                else
                {
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, User Added Successfully. created userId: {user.Id}, created userName: {user.UserName}, created userFullName: {user.Profile.FullName}");
                    result = new(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_UserCreatedSuccessfully") });
                }
            }
            else
            {
                result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_DuplicatePhoneNumber"), ModelStateErrors = errors });
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"userId: {userDb.Id}, userName: {userDb.UserName} error occurred in AddUser.");

            result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater"), ModelStateErrors = errors });
        }

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> EditUser(string id, CancellationToken cancellationToken)
    {
        UserEdit dto = new();
        ApplicationUser user = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            ApplicationUser userDb = await userManager.FindByIdAsync(id);

            ViewBag.IsSystem = userDb is { IsSystemAccount: true };
            ViewBag.LangList = controllerHelper.GetAllActiveLanguage();

            if (user.IsSystemAccount)
            {
                ViewBag.IsSystem = true;
                ViewBag.DomainList = controllerHelper.GetAllActiveDomains();
            }
            else
            {
                ViewBag.IsSystem = false;
            }

            dto = mapper.Map<UserEdit>(userDb);

            if (userDb != null)
            {
                dto.FirstName = userDb.Profile.FirstName;
                dto.LastName = userDb.Profile.LastName;
                dto.DefaultLanguageId = userDb.Profile.DefaultLanguageId;
                dto.FullMobile = userDb.PhoneNumber;

                if (userDb.PhoneNumber != null)
                {
                    dto.PhoneNumber = userDb.PhoneNumber[2..];
                }
            }

            Domain domain = controllerHelper.GetCurrentUserDomain();
            List<RoleListView> rolesModel = (await roleRepository.GetListAsync(c => c.IsActive == true && c.IsDeleted == false && c.AssociatedDomainId == domain.Id, cancellationToken))
                                            .Select(c => new RoleListView { Id = c.Id, Name = c.Name })
                                            .ToList();
            rolesModel.Insert(0, new() { Id = "-1", Name = UtilityLanguage.GetString("Choose") });
            ViewBag.Roles = rolesModel;
        }
        catch (Exception)
        {
            logger.Error($"userId: {user.Id}, userName: {user.UserName} error occured in Editing User with id: {id}. stack trace: {nameof(AccountController)}/{nameof(EditUser)}");
        }

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> Edit([FromForm] UserEdit dto, CancellationToken cancellationToken)
    {
        List<ClientValidationErrorModel> errors = [];
        ApplicationUser userDb = new();

        JsonResult result = new(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully") });

        if (!string.IsNullOrWhiteSpace(dto.Id))
        {
            userDb = await userManager.FindByIdAsync(dto.Id);

            if (userDb == null)
            {
                ModelState.AddModelError("FirstName", UtilityLanguage.GetString("AlertAndMessage_NoUserWasFound"));
            }

            bool isPhoneNumberUnique = false;

            try
            {
                string modelPhone = dto.PhoneNumber.Replace("+", "");
                List<ApplicationUser> usersModel = userManager.Users.Where(u => u.PhoneNumber == modelPhone).ToList();

                int count = usersModel.Count;

                if (count == 0)
                {
                    usersModel = userManager.Users.Where(u => u.UserName == modelPhone).ToList();
                    count = usersModel.Count;

                    if (usersModel.Count == 0)
                    {
                        isPhoneNumberUnique = true;
                    }
                }

                if (count == 1)
                {
                    if (userDb != null && modelPhone == userDb.PhoneNumber)
                    {
                        isPhoneNumberUnique = true;
                    }
                }
            }
            catch (Exception e)
            {
                isPhoneNumberUnique = false;

                if (userDb != null)
                {
                    logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occurred. stack trace: {nameof(AccountController)}/{nameof(Edit)}");
                }
            }

            if (!isPhoneNumberUnique)
            {
                ModelState.AddModelError("PhoneNumber", UtilityLanguage.GetString("AlertAndMessage_DuplicatePhoneNumber"));
            }

            string langName = controllerHelper.FetchLanguage(dto.DefaultLanguageId).LanguageName;

            if (!string.IsNullOrEmpty(langName))
            {
                dto.DefaultLanguageName = langName;
            }
        }

        if (ModelState.IsValid)
        {
            Result<ApplicationUser> upResult = new();
            ApplicationUser user = await controllerHelper.GetCurrentUser(cancellationToken);

            try
            {
                if (user.Id == dto.Id)
                {
                    result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_DontAccess"), ModelStateErrors = errors });
                }
                else
                {
                    #region Fetch currency from language
                    Language lanModel = controllerHelper.GetDefaultLanguage();
                    CultureInfo userCultureInfo = new(lanModel.Symbol, false);
                    RegionInfo ri = new(userCultureInfo.LCID);
                    string currencyPrefix = ri.ISOCurrencySymbol;
                    Currency currency = controllerHelper.GetCurrencyByItsPrefix(currencyPrefix);
                    #endregion Fetch currency from language

                    if (userDb != null)
                    {
                        userDb.Profile.FirstName = dto.FirstName;
                        userDb.Profile.LastName = dto.LastName;
                        userDb.Profile.FullName = dto.FirstName + " " + dto.LastName;
                        userDb.Profile.DefaultLanguageId = dto.DefaultLanguageId;
                        userDb.Profile.DefaultLanguageName = dto.DefaultLanguageName;

                        await userRepository.UpdateAsync(c => c.Id == userDb.Id, m => m.Profile, userDb.Profile, cancellationToken);
                        await userRepository.UpdateAsync(c => c.Id == userDb.Id, m => m.IsVendor, dto.IsVendor, cancellationToken);
                        await userRepository.UpdateAsync(c => c.Id == userDb.Id, m => m.IsSiteUser, dto.IsSiteUser, cancellationToken);
                        await userRepository.UpdateAsync(c => c.Id == userDb.Id, m => m.PhoneNumber, dto.FullMobile.Replace("+", ""), cancellationToken);
                        await userRepository.UpdateAsync(c => c.Id == userDb.Id, m => m.Email, dto.Email, cancellationToken);
                        upResult = await userRepository.UpdateAsync(c => c.Id == userDb.Id, m => m.UserRoleId, dto.UserRoleId, cancellationToken);

                        Modification modification = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ActionTypes = ActionTypes.Update,
                            CollectionType = CollectionType.ApplicationUser,
                            Ip = controllerHelper.GetUserIpAddress(),
                            ModifierId = userDb.Id,
                            ModifierUserName = userDb.UserName,
                            ModifyDateTime = DateTime.Now,
                            RecordId = dto.Id
                        };
                        Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
                    }

                    if (userDb != null)
                    {
                        logger.Information($"userId: {dto.Id}, userName: .{userDb.UserName} editing user done successfully.");
                    }

                    upResult.Succeeded = true;
                    upResult.Message = ConstMessages.SuccessfullyDone;
                }
            }
            catch (Exception e)
            {
                logger.Error($"userId: {user.Id}, userName: {user.UserName} error {e.Message} occured in Editing User with id: {dto.Id}. stack trace: {nameof(AccountController)}/{nameof(EditUser)}");
            }
        }
        else
        {
            errors.AddRange(from modelStateKey in ModelState.Keys
                            let modelStateVal = ModelState[modelStateKey]
                            where modelStateVal != null
                            from error in modelStateVal.Errors
                            select new ClientValidationErrorModel { Key = modelStateKey, ErrorMessage = UtilityLanguage.GetString(error.ErrorMessage) });
            result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_FillEssentialFields"), ModelStateErrors = errors });
        }

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        if (userDb.Id == id)
        {
            result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_NoAccess") });
        }
        else
        {
            try
            {
                ApplicationUser user = await userManager.FindByIdAsync(id);

                if (user == null)
                {
                    result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_NoUserWasFound") });
                }
                else
                {
                    Result<ApplicationUser> res = await userRepository.UpdateAsync(c => c.Id == user.Id, m => m.IsDeleted, false, cancellationToken);

                    if (res.Succeeded)
                    {
                        Modification modification = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ActionTypes = ActionTypes.Restore,
                            CollectionType = CollectionType.ApplicationUser,
                            Ip = controllerHelper.GetUserIpAddress(),
                            ModifierId = userDb.Id,
                            ModifierUserName = userDb.UserName,
                            ModifyDateTime = DateTime.Now,
                            RecordId = user.Id
                        };
                        Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                        result = new(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully") });
                        logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName} restoring user with id: {user.Id} done successfully");
                    }
                    else
                    {
                        result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
                    }
                }
            }
            catch (Exception e)
            {
                result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
                logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(AccountController)}/{nameof(Restore)}");
            }
        }

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> ChangeActivation(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            ApplicationUser user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
            }

            if (user is { IsActive: true })
            {
                await userRepository.UpdateAsync(c => c.Id == user.Id, m => m.IsActive, false, cancellationToken);
            }
            else
            {
                await userRepository.UpdateAsync(c => c.Id == user.Id, m => m.IsActive, true, cancellationToken);
            }

            result = new(new { Status = "success", result = user is { IsActive: true }, Message = UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully") });
            Modification modification = new()
            {
                Id = Guid.NewGuid().ToString(),
                ActionTypes = ActionTypes.ChangePassword,
                CollectionType = CollectionType.ApplicationUser,
                Ip = controllerHelper.GetUserIpAddress(),
                ModifierId = userDb.Id,
                ModifierUserName = userDb.UserName,
                ModifyDateTime = DateTime.Now,
                RecordId = id
            };
            Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(AccountController)}/{nameof(ChangeActivation)}");

            return new JsonResult(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
        }

        return result;
    }

    [HttpPut]
    public async ValueTask<IActionResult> ChangePassword(string id, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = controllerHelper.GetCurrentUser(cancellationToken).Result;

        // Generate a new random password
        string pass = Utilities.GenerateRandomPassword(new() { RequireDigit = true, RequireLowercase = true, RequireNonAlphanumeric = true, RequireUppercase = true, RequiredLength = 10, RequiredUniqueChars = 0 });

        // Ensure the password meets criteria
        while (!Password.PasswordIsValid(true, true, true, false, false, pass))
        {
            pass = Utilities.GenerateRandomPassword(new() { RequireDigit = true, RequireLowercase = true, RequireNonAlphanumeric = true, RequireUppercase = true, RequiredLength = 10, RequiredUniqueChars = 0 });
        }

        // Find user by ID
        ApplicationUser user = await userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        // Generate password reset token and reset password
        string token = await userManager.GeneratePasswordResetTokenAsync(user);
        IdentityResult result = await userManager.ResetPasswordAsync(user, token, pass);

        if (!result.Succeeded)
        {
            return Ok(new { Result = result.Succeeded });
        }

        Modification modification = new()
        {
            Id = Guid.NewGuid().ToString(),
            ActionTypes = ActionTypes.ChangePassword,
            CollectionType = CollectionType.ApplicationUser,
            Ip = controllerHelper.GetUserIpAddress(),
            ModifierId = userDb.Id,
            ModifierUserName = userDb.UserName,
            ModifyDateTime = DateTime.Now,
            RecordId = id
        };
        Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
        string currentUserId = User.GetUserId();

        return Ok(new { Result = result.Succeeded, Data = pass });
    }

    [HttpGet]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        JsonResult result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_NoAccess") });
        ApplicationUser userDb = controllerHelper.GetCurrentUser(cancellationToken).Result;

        if (userDb.Id == id)
        {
            return result;
        }

        try
        {
            ApplicationUser user = await userRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (user == null)
            {
            }

            Result<ApplicationUser> res = await userRepository.UpdateAsync(c => user != null && c.Id == user.Id, m => m.IsDeleted, true, cancellationToken);

            if (res.Succeeded)
            {
                result = new(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_DeletionDoneSuccessfully") });
                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName} deleting user done successfully");
                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Delete,
                    CollectionType = CollectionType.ApplicationUser,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
            }
            else
            {
                result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
            }
        }
        catch (Exception e)
        {
            result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(AccountController)}/{nameof(Delete)}");
        }

        return result;
    }

    [AllowAnonymous]
    public IActionResult UnAuthorize()
    {
        return View();
    }
}