using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Domain;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.General.User.Mongo;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Entities.General.Email;
using Arad.Portal.DataLayer.Entities.General.Domain;
using static Arad.Portal.DataLayer.Models.Shared.Enums;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Repositories.General.BasicData.Mongo;

namespace Arad.Portal.DataLayer.Repositories.General.Domain.Mongo
{
    public class DomainRepository : BaseRepository, IDomainRepository
    {
        private readonly DomainContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _accessor;
        private readonly BasicDataContext _basicDataContext;
        public DomainRepository(DomainContext context,
                                IHttpContextAccessor httpContextAccessor,
                                IWebHostEnvironment env,
                                BasicDataContext basicDataContext,
                                IMapper mapper) : base(httpContextAccessor, env)
        {
            _context = context;
            _mapper = mapper;
            _accessor = httpContextAccessor;
            _basicDataContext = basicDataContext;
        }
        public async Task<Result> AddDomain(DomainDTO dto)
        {
            Result result = new Result();
            try
            {
                if (_context.Collection.Find(_ => _.DomainName == dto.DomainName).Any())
                {
                    result.Message = ConstMessages.DuplicateField;
                }
                else
                {

                    var equallentEntity = _mapper.Map<Entities.General.Domain.Domain>(dto);
                    //equallentEntity.DomainId = Guid.NewGuid().ToString();
                    foreach (var price in dto.Prices)
                    {
                        //price.PriceId = Guid.NewGuid().ToString();
                    }
                    #region Prices
                    equallentEntity.Prices = new List<Price>();
                    foreach (var price in dto.Prices.OrderBy(_ => _.SDate))
                    {
                        if (price.IsActive && string.IsNullOrWhiteSpace(price.EndDate))//price is valid from client
                        {
                            if (equallentEntity.Prices.Any(_ => _.CurrencyId == price.CurrencyId && _.EndDate != null && _.IsActive))
                            {
                                var exist = equallentEntity.Prices.FirstOrDefault(_ => _.CurrencyId == price.CurrencyId && _.EndDate != null && _.IsActive);
                                if (exist != null)
                                {
                                    exist.IsActive = false;
                                    exist.EndDate = DateTime.UtcNow;
                                }
                            }
                        }

                        var p = new Price()
                        {
                            PriceId = !string.IsNullOrWhiteSpace(price.PriceId) ? price.PriceId : Guid.NewGuid().ToString(),
                            CurrencyId = price.CurrencyId,
                            CurrencyName = price.CurrencyName,
                            IsActive = !string.IsNullOrWhiteSpace(price.PriceId) ? price.IsActive : true,
                            Prefix = price.Prefix,
                            PriceValue = price.PriceValue,
                            StartDate = DateHelper.ToEnglishDate(price.StartDate).ToUniversalTime(),
                            EndDate = !string.IsNullOrWhiteSpace(price.EndDate) ?
                            GeneralLibrary.Utilities.DateHelper.ToEnglishDate(price.EndDate).ToUniversalTime() : null
                        };
                        equallentEntity.Prices.Add(p);
                    }
                    #endregion

                    if (dto.IsShop)
                    {
                        equallentEntity.InvoiceNumberProcedure = (InvoiceNumberProcedure)Convert.ToInt32(dto.InvoiceNumberProcedure);
                    }




                    equallentEntity.Modifications = new List<Modification>();
                    equallentEntity.CreationDate = DateTime.Now;
                    equallentEntity.CreatorUserId = this.GetUserId();
                    equallentEntity.CreatorUserName = this.GetUserName();
                    equallentEntity.IsActive = true;



                    await _context.Collection.InsertOneAsync(equallentEntity);
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                }
            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.InternalServerErrorMessage;
            }
            return result;
        }

        public async Task<PagedItems<DomainViewModel>> AllDomainList(string queryString, ApplicationUser user)
        {

            PagedItems<DomainViewModel> result = new PagedItems<DomainViewModel>();
            try
            {
                NameValueCollection filter = HttpUtility.ParseQueryString(queryString);

                if (string.IsNullOrWhiteSpace(filter["page"]))
                {
                    filter.Set("page", "1");
                }

                if (string.IsNullOrWhiteSpace(filter["PageSize"]))
                {
                    filter.Set("PageSize", "20");
                }

                var page = Convert.ToInt32(filter["page"]);
                var pageSize = Convert.ToInt32(filter["PageSize"]);
                long totalCount = 0;
                var domainId = "";
                if(user.IsSystemAccount)
                {
                   totalCount = await _context.Collection.Find(c => true).CountDocumentsAsync();
                }
                else
                {
                    domainId = user.Domains.FirstOrDefault(a => a.IsOwner).DomainId;
                    totalCount = await _context.Collection.Find(c => c.AssociatedDomainId == domainId).CountDocumentsAsync();
                }
               
                var list = _context.Collection.AsQueryable().Where(_=> domainId == "" || _.AssociatedDomainId == domainId ).Skip((page - 1) * pageSize)
                   .Take(pageSize).Select(_ => new DomainViewModel()
                   {
                       DomainId = _.DomainId,
                       DomainName = _.DomainName,
                       Prices = _.Prices,
                       //DomainPrice  = _.Prices.FirstOrDefault(_=>_.IsActive),
                       OwnerUserId = _.OwnerUserId.ToString(),
                       OwnerUserName = _.OwnerUserName,
                       DefaultLanguageId = _.DefaultLanguageId,
                       DefaultLanguageName = _.DefaultLanguageName,
                       IsDefault = _.IsDefault,
                       DefaultCurrencyId = _.DefaultCurrencyId,
                       DefaultCurrencyName = _.DefaultCurrencyName

                   }).ToList();

                result.CurrentPage = page;
                result.Items = list;
                result.ItemsCount = totalCount;
                result.PageSize = pageSize;
                result.QueryString = queryString;

            }
            catch (Exception ex)
            {
                result.CurrentPage = 1;
                result.Items = new List<DomainViewModel>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<Result> DeleteDomain(string domainId, string modificationReason)
        {
            Result result = new Result();

            try
            {
                #region check object dependency 
                //??
                //checke all dependencies in other entities if any dependecy exist delete not allowed
                var allowDeletion = true;
                #endregion

                if (allowDeletion)
                {
                    var entity = _context.Collection.Find(_ => _.DomainId == domainId).FirstOrDefault();
                    if (entity != null)
                    {
                        entity.IsDeleted = true;

                        #region add modification
                        var mod = GetCurrentModification(modificationReason);
                        entity.Modifications.Add(mod);
                        #endregion

                        var updateResult = await _context.Collection.ReplaceOneAsync(_ => _.DomainId == domainId, entity);
                        if (updateResult.IsAcknowledged)
                        {
                            result.Message = ConstMessages.SuccessfullyDone;
                            result.Succeeded = true;
                        }
                        else
                        {
                            result.Message = ConstMessages.GeneralError;
                        }
                    }
                    else
                    {
                        result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
                    }

                }
                else
                {
                    result.Message = ConstMessages.DeletedNotAllowedForDependencies;
                }

            }
            catch (Exception e)
            {
                result.Message = ConstMessages.ExceptionOccured;
            }
            return result;
        }

        public async Task<Result> DomainChangePrice(DomainPrice dto)
        {
            var result = new Result();


            var dbEntity = _context.Collection.Find(_ => _.DomainId == dto.DomainId).FirstOrDefault();
            if (dbEntity != null)
            {
                dbEntity.Prices.FirstOrDefault(_ => _.IsActive).EndDate = DateTime.Now;
                dbEntity.Prices.FirstOrDefault(_ => _.IsActive).IsActive = false;
                dto.Price.IsActive = true;
                dto.Price.PriceId = Guid.NewGuid().ToString();
                dbEntity.Prices.Add(dto.Price);

                var mod = GetCurrentModification(dto.ModificationReason);
                dbEntity.Modifications.Add(mod);

                var updateResult = await _context.Collection
                       .ReplaceOneAsync(_ => _.DomainId == dto.DomainId, dbEntity);
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

            return result;
        }

        public Result<DomainDTO> FetchByName(string domainName, bool isDef)
        {
            Result<DomainDTO> result = new Result<DomainDTO>();
            try
            {

                var dbEntity = _context.Collection.Find(_ => _.DomainName == domainName).FirstOrDefault();
                if (dbEntity == null && isDef)
                {
                    dbEntity = _context.Collection.Find(_ => _.IsDefault).FirstOrDefault();
                }


                if (dbEntity != null)
                {
                    var dto = _mapper.Map<DomainDTO>(dbEntity);
                    dto.SupportedLangId = _basicDataContext.Collection.AsQueryable()
                   .Where(_ => _.AssociatedDomainId == dbEntity.DomainId && _.GroupKey == "SupportedCultures").Select(_ => _.Value).ToList();
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                    result.ReturnValue = dto;
                }
                else
                {
                    result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
                    result.ReturnValue = new DomainDTO();
                }

            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.ExceptionOccured;
            }

            return result;
        }

        public Result<DomainDTO> FetchDomain(string domainId)
        {
            Result<DomainDTO> result = new Result<DomainDTO>();

            try
            {
                var dbEntity = _context.Collection.Find(_ => _.DomainId == domainId).FirstOrDefault();
                if (dbEntity == null)
                {
                    result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
                }
                else
                {
                    var dto = _mapper.Map<DomainDTO>(dbEntity);
                    dto.SupportedLangId = _basicDataContext.Collection.AsQueryable()
                        .Where(_ => _.AssociatedDomainId == dbEntity.DomainId && _.GroupKey == "SupportedCultures").Select(_ => _.Value).ToList();
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                    result.ReturnValue = dto;
                }
            }
            catch (Exception)
            {
                result.Message = ConstMessages.ExceptionOccured;
            }

            return result;
        }

        public string FetchDomainTitle(string domainName)
        {
            var title = "";
            var dbEntity = _context.Collection.Find(_ => _.DomainName == domainName).FirstOrDefault();
            if (dbEntity != null && !string.IsNullOrWhiteSpace(dbEntity.Title))
            {
                title = dbEntity.Title;
            }
            if (string.IsNullOrWhiteSpace(title))
            {
                title = domainName;
            }
            return title;
        }

        public async Task<Result> Restore(string id)
        {
            var result = new Result();
            var entity = _context.Collection
              .Find(_ => _.DomainId == id).FirstOrDefault();
            if (entity != null)
            {
                entity.IsDeleted = false;
                var updateResult = await _context.Collection
                   .ReplaceOneAsync(_ => _.DomainId == id, entity);
                if (updateResult.IsAcknowledged)
                {
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = ConstMessages.ErrorInSaving;
                }
            }
            else
            {
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }

            return result;
        }

        public async Task InsertMany(List<Entities.General.Domain.Domain> domains)
        {
            await _context.Collection.InsertManyAsync(domains);
        }

        public bool HasAny()
        {
            return _context.Collection.AsQueryable().Any();
        }

        public async Task<Result> EditDomain(DomainDTO dto)
        {
            var result = new Result();
            var entity = _context.Collection
                .Find(_ => _.DomainId == dto.DomainId).FirstOrDefault();
            if (entity != null)
            {

                #region Add Modification
                var currentModifications = entity.Modifications;
                var mod = GetCurrentModification($"Change domain by userId={this.GetUserId()} and UserName={this.GetUserName()} at datetime={DateTime.Now.ToPersianDdate()}");
                currentModifications.Add(mod);
                #endregion

                entity.Modifications = currentModifications;
                entity.DomainName = dto.DomainName;
                entity.OwnerUserId = dto.OwnerUserId;
                entity.OwnerUserName = dto.OwnerUserName;
                //foreach (var price in dto.Prices)
                //{
                //    price.PriceId = Guid.NewGuid().ToString();
                //}

                #region Prices
                entity.Prices = new List<Price>();
                foreach (var price in dto.Prices.OrderBy(_ => _.StartDate))
                {
                    if (price.IsActive && string.IsNullOrWhiteSpace(price.EndDate))//price is valid from client
                    {
                        if (entity.Prices.Any(_ => _.CurrencyId == price.CurrencyId && _.EndDate != null && _.IsActive))
                        {
                            var exist = entity.Prices.FirstOrDefault(_ => _.CurrencyId == price.CurrencyId && _.EndDate != null && _.IsActive);
                            //entity.Prices.Remove(exist);
                            if (exist != null)
                            {
                                exist.IsActive = false;
                                exist.EndDate = DateTime.UtcNow;
                            }

                            //entity.Prices.Add(exist);

                        }
                    }

                    var p = new Price()
                    {
                        PriceId = !string.IsNullOrWhiteSpace(price.PriceId) ? price.PriceId : Guid.NewGuid().ToString(),
                        CurrencyId = price.CurrencyId,
                        CurrencyName = price.CurrencyName,
                        IsActive = !string.IsNullOrWhiteSpace(price.PriceId) ? price.IsActive : true,
                        Prefix = price.Prefix,
                        PriceValue = price.PriceValue,
                        StartDate = DateHelper.ToEnglishDate(price.StartDate).ToUniversalTime(),
                        EndDate = !string.IsNullOrWhiteSpace(price.EndDate) ?
                        GeneralLibrary.Utilities.DateHelper.ToEnglishDate(price.EndDate).ToUniversalTime() : null
                    };
                    entity.Prices.Add(p);
                }
                #endregion

                entity.InvoiceNumberProcedure = (InvoiceNumberProcedure)Convert.ToInt32(dto.InvoiceNumberProcedure);
                entity.DomainPaymentProviders = _mapper.Map<List<ProviderDetail>>(dto.DomainPaymentProviders);
                entity.DefaultShippingTypeId = dto.DefaultShippingTypeId;

                entity.HomePageDesign = dto.HomePageDesign;

                var updateResult = await _context.Collection.ReplaceOneAsync(_ => _.DomainId == dto.DomainId, entity);

                if (updateResult.IsAcknowledged)
                {
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = ConstMessages.ErrorInSaving;
                }
            }
            else
            {
                result.Succeeded = false;
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            return result;
        }

        public List<SelectListModel> GetAllActiveDomains()
        {
            var result = new List<SelectListModel>();
            result = _context.Collection.Find(_ => _.IsActive && !_.IsDeleted)
              .Project(_ => new SelectListModel()
              {
                  Text = _.DomainName,
                  Value = _.DomainId
              }).ToList();
            result.Insert(0, new SelectListModel()
            {
                Text = GeneralLibrary.Utilities.Language.GetString("Choose"),
                Value = ""
            });
            return result;
        }

        public Result<DomainDTO> GetDefaultDomain()
        {
            var result = new Result<DomainDTO>();
            var entity = _context.Collection.Find(_ => _.IsDefault).FirstOrDefault();
            if (entity != null)
            {
                result.Succeeded = true;
                result.ReturnValue = _mapper.Map<DomainDTO>(entity);
                result.Message = ConstMessages.SuccessfullyDone;
            }
            else
            {
                result.Succeeded = false;
                result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
            }
            return result;
        }

        public SMTP GetSMTPAccount(string domainName)
        {
            SMTP result = null;
            var domainEntity = _context.Collection.Find(_ => _.DomainName == domainName).FirstOrDefault();
            if (domainEntity != null && domainEntity.SMTPAccount != null)
            {
                return domainEntity.SMTPAccount;
            }
            return result;
        }

        public string GetDomainName()
        {
            return base.GetCurrentDomainName();
        }

        public List<SelectListModel> GetInvoiceNumberProcedureEnum()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(InvoiceNumberProcedure)))
            {
                string name = Enum.GetName(typeof(InvoiceNumberProcedure), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel() { Text = GeneralLibrary.Utilities.Language.GetString("Choose"), Value = "-1" });
            return result;
        }

        public List<SelectListModel> GetPspTypesEnum()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(PspType)))
            {
                string name = Enum.GetName(typeof(PspType), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel()
            {
                Text = GeneralLibrary.Utilities.Language.GetString("Choose"),
                Value = "-1"
            });
            return result;
        }

        public Entities.General.Domain.Domain FetchDomainByName(string domainName)
        {
            Entities.General.Domain.Domain result
                 = new Entities.General.Domain.Domain();
            try
            {
                result = _context.Collection.Find(_ => _.DomainName == domainName).FirstOrDefault();
                if (result == null)
                {
                    result = _context.Collection.Find(_ => _.IsDefault).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public List<SelectListModel> GetTwoColsTemplateWidthEnum()
        {
            var result = new List<SelectListModel>();

            foreach (int i in Enum.GetValues(typeof(TwoColsTemplateWidth)))
            {
                string name = Enum.GetName(typeof(TwoColsTemplateWidth), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel()
            {
                Text = GeneralLibrary.Utilities.Language.GetString("Choose"),
                Value = "-1"
            });
            return result;
        }

        public List<SelectListModel> GetThreeColsTemplateWidthEnum()
        {
            var result = new List<SelectListModel>();

            foreach (int i in Enum.GetValues(typeof(ThreeColsTemplateWidth)))
            {
                string name = Enum.GetName(typeof(ThreeColsTemplateWidth), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel()
            {
                Text = GeneralLibrary.Utilities.Language.GetString("Choose"),
                Value = "-1"
            });
            return result;
        }

        public List<SelectListModel> GetFourColsTemplateWidthEnum()
        {
            var result = new List<SelectListModel>();

            foreach (int i in Enum.GetValues(typeof(FourColsTemplateWidth)))
            {
                string name = Enum.GetName(typeof(FourColsTemplateWidth), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel()
            {
                Text = GeneralLibrary.Utilities.Language.GetString("Choose"),
                Value = "-1"
            });
            return result;
        }

        public List<SelectListModel> GetFiveColsTemplateWidthEnum()
        {
            var result = new List<SelectListModel>();

            foreach (int i in Enum.GetValues(typeof(FiveColsTemplateWidth)))
            {
                string name = Enum.GetName(typeof(FiveColsTemplateWidth), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel()
            {
                Text = GeneralLibrary.Utilities.Language.GetString("Choose"),
                Value = "-1"
            });
            return result;
        }

        public List<SelectListModel> GetSixColsTemplateWidthEnum()
        {
            var result = new List<SelectListModel>();

            foreach (int i in Enum.GetValues(typeof(SixColsTemplateWidth)))
            {
                string name = Enum.GetName(typeof(SixColsTemplateWidth), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel()
            {
                Text = GeneralLibrary.Utilities.Language.GetString("Choose"),
                Value = "-1"
            });
            return result;
        }

        public List<SelectListModel> GetOneColsTemplateWidthEnum()
        {
            var result = new List<SelectListModel>();

            foreach (int i in Enum.GetValues(typeof(OneColsTemplateWidth)))
            {
                string name = Enum.GetName(typeof(OneColsTemplateWidth), i);
                var obj = new SelectListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }
            result.Insert(0, new SelectListModel()
            {
                Text = GeneralLibrary.Utilities.Language.GetString("Choose"),
                Value = "-1"
            });
            return result;
        }

        public Result<DomainDTO> FetchDefaultDomain()
        {
            Result<DomainDTO> result = new Result<DomainDTO>();
            try
            {

                var dbEntity = _context.Collection.Find(_ => _.IsDefault).FirstOrDefault();


                if (dbEntity != null)
                {
                    var dto = _mapper.Map<DomainDTO>(dbEntity);
                    result.Succeeded = true;
                    result.Message = ConstMessages.SuccessfullyDone;
                    result.ReturnValue = dto;
                }
                else
                {
                    result.Message = GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_ObjectNotFound");
                    result.ReturnValue = new DomainDTO();
                }

            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.ExceptionOccured;
            }

            return result;
        }

        public void InsertOne(Entities.General.Domain.Domain entity)
        {
            _context.Collection.InsertOne(entity);
        }

        public bool HasDefaultDomain()
        {
            return _context.Collection.AsQueryable().Any(_=>_.IsDefault);
        }
    }
}
