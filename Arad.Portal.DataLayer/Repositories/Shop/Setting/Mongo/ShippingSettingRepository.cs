﻿using Arad.Portal.DataLayer.Contracts.Shop.Setting;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Setting;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.General.BasicData.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Arad.Portal.GeneralLibrary.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Entities.Shop.Setting;
using Arad.Portal.DataLayer.Repositories.General.Currency.Mongo;

namespace Arad.Portal.DataLayer.Repositories.Shop.Setting.Mongo
{
    public class ShippingSettingRepository: BaseRepository, IShippingSettingRepository
    {

        private readonly ShippingSettingContext _context;
        private readonly CurrencyContext _currencyContext;
        private readonly DomainContext _domainContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public ShippingSettingRepository(IHttpContextAccessor accessor,
            ShippingSettingContext context,
            IMapper mapper,
            CurrencyContext currencyContext,
            IWebHostEnvironment env,
            DomainContext  domainContext,
            UserManager<ApplicationUser> userManager):base(accessor, env)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _domainContext = domainContext;
            _currencyContext = currencyContext;
        }

        public async Task<Result> AddShippingSetting(ShippingSettingDTO dto)
        {
            Result result = new Result();
            try
            {
                var equallentEntity = _mapper.Map<Entities.Shop.Setting.ShippingSetting>(dto);
                  
                equallentEntity.CreationDate = DateTime.Now;
                equallentEntity.CreatorUserId = this.GetUserId();
                equallentEntity.CreatorUserName = this.GetUserName();
                equallentEntity.IsActive = true;

                await _context.Collection.InsertOneAsync(equallentEntity);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.InternalServerErrorMessage;
            }
            return result;
        }

        public async Task<Result> Delete(string id)
        {
            var result = new Result();
            var entity = _context.Collection
              .Find(_ => _.ShippingSettingId == id).FirstOrDefault();
            if (entity != null)
            {
                entity.IsDeleted = true;
                var updateResult = await _context.Collection
                   .ReplaceOneAsync(_ => _.ShippingSettingId == id, entity);
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

        public async Task<Result> EditShippingSetting(ShippingSettingDTO dto)
        {
            var result = new Result();
            var entity = _context.Collection
                .Find(_ => _.ShippingSettingId == dto.ShippingSettingId).FirstOrDefault();
            if (entity != null)
            {

                #region Add Modification
                var currentModifications = entity.Modifications;
                var mod = GetCurrentModification($"Change ShippingSetting by userId={this.GetUserId()} and UserName={this.GetUserName()} at datetime={DateTime.Now.ToPersianDdate()}");
                currentModifications.Add(mod);
                #endregion
                var model = _mapper.Map<Entities.Shop.Setting.ShippingSetting>(dto);
               
                var updateResult = await _context.Collection
               .ReplaceOneAsync(_ => _.ShippingSettingId == dto.ShippingSettingId, model);
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

        public ShippingSettingDTO FetchById(string shippingSettingId)
        {
            var result = new ShippingSettingDTO();
            var entity = _context.Collection
                .Find(_ => _.ShippingSettingId == shippingSettingId).FirstOrDefault();

            result = _mapper.Map<ShippingSettingDTO>(entity);
            if(entity.ShippingCoupon != null)
            {
                result.ShippingCoupon.PersianStartDate = DateHelper.ToPersianDdate(entity.ShippingCoupon.StartDate);
                if(entity.ShippingCoupon.EndDate != null)
                {
                    result.ShippingCoupon.PersianEndDate = DateHelper.ToPersianDdate(entity.ShippingCoupon.EndDate.Value);
                }
            }
            return result;
        }

        public ShippingSetting FetchShippingSettingOfDomain(string domainId)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedItems<ShippingSettingDTO>> List(string queryString, ApplicationUser user)
        {
            PagedItems<ShippingSettingDTO> result = new PagedItems<ShippingSettingDTO>();
            var list = new List<ShippingSettingDTO>();

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
                if(user.IsSystemAccount)
                {
                    totalCount = await _context.Collection.Find(c => true).CountDocumentsAsync();
                    var lst = _context.Collection.AsQueryable().Skip((page - 1) * pageSize)
                      .Take(pageSize).ToList();
                   
                    list = _mapper.Map<List<ShippingSettingDTO>>(lst);
                }
                else
                {
                    totalCount = await _context.Collection
                        .Find(_ => _.AssociatedDomainId == user.Domains.FirstOrDefault(a=> a.IsOwner).DomainId).CountDocumentsAsync();
                    var lst = _context.Collection.AsQueryable()
                        .Where(_ => _.AssociatedDomainId == user.Domains.FirstOrDefault(a => a.IsOwner).DomainId).Skip((page - 1) * pageSize)
                        .Take(pageSize).ToList();
                   
                    list = _mapper.Map<List<ShippingSettingDTO>>(lst);
                    foreach (var item in list)
                    {
                        item.CurrencySymbol = _currencyContext.Collection.Find(_ => _.CurrencyId == item.CurrencyId).FirstOrDefault().Symbol;
                    }
                }

                result.CurrentPage = page;
                result.Items = list;
                result.ItemsCount = totalCount;
                result.PageSize = pageSize;
                result.QueryString = queryString;

            }
            catch (Exception ex)
            {
                result.CurrentPage = 1;
                result.Items = new List<ShippingSettingDTO>();
                result.ItemsCount = 0;
                result.PageSize = 10;
                result.QueryString = queryString;
            }
            return result;
        }

        public async Task<Result> Restore(string id)
        {
            var result = new Result();
            var entity = _context.Collection
              .Find(_ => _.ShippingSettingId == id).FirstOrDefault();
            if (entity != null)
            {
                entity.IsDeleted = false;
                var updateResult = await _context.Collection
                   .ReplaceOneAsync(_ => _.ShippingSettingId == id, entity);
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
    }
}
