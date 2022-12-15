﻿using Arad.Portal.DataLayer.Contracts.General.BasicData;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities;
using Arad.Portal.GeneralLibrary.Utilities;

namespace Arad.Portal.DataLayer.Repositories.General.BasicData.Mongo
{
    public class BasicDataRepository : BaseRepository, IBasicDataRepository
    {
        private readonly IMapper _mapper;
        private readonly BasicDataContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DomainContext _domainContext;

        public BasicDataRepository(IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager,
            DomainContext domainContext,
            IMapper mapper, BasicDataContext basicDataContext) : base(httpContextAccessor, env)
        {
            _mapper = mapper;
            _context = basicDataContext;
            _userManager = userManager;
            _domainContext = domainContext;
        }

        public async Task<Result> DeleteGroupKeyListInDomain(string domainId, string groupKey)
        {
            var result = new Result();
            FilterDefinitionBuilder<Entities.General.BasicData.BasicData> _builder = new FilterDefinitionBuilder<Entities.General.BasicData.BasicData>();
            FilterDefinition<Entities.General.BasicData.BasicData> filterDef = _builder.Empty;
            filterDef = _builder.Eq(nameof(Entities.General.BasicData.BasicData.AssociatedDomainId), domainId);
            filterDef &= _builder.Eq(nameof(Entities.General.BasicData.BasicData.GroupKey), groupKey);

            var delResult = await _context.Collection.DeleteManyAsync(filterDef);
            if(delResult.IsAcknowledged)
            {
                result.Succeeded = true;
            }
            return result;
        }

        public List<BasicDataModel> GetList(string groupKey, bool withChooseItem, bool isDomain = true)
        {
            var result = new List<BasicDataModel>();
            var domainName = base.GetCurrentDomainName();
            var domainEntity = _domainContext.Collection.Find(_ => _.DomainName == domainName).Any() ?
            _domainContext.Collection.Find(_ => _.DomainName == domainName).First() :
            _domainContext.Collection.Find(_ => _.IsDefault).First();
            if (groupKey.ToLower() == "shippingtype")
            {
                var hasShippingType = HasShippingType();
                if (!hasShippingType)
                {
                    var post = new Entities.General.BasicData.BasicData
                    {
                        BasicDataId = Guid.NewGuid().ToString(),
                        GroupKey = "ShippingType",
                        Text = "Post",
                        Value = "1",
                        Order = 1,
                        AssociatedDomainId = domainEntity.DomainId
                    };
                    _context.Collection.InsertOne(post);


                    var courier = new Entities.General.BasicData.BasicData
                    {
                        BasicDataId = Guid.NewGuid().ToString(),
                        GroupKey = "ShippingType",
                        Text = "Courier",
                        Value = "2",
                        Order = 2,
                        AssociatedDomainId = domainEntity.DomainId

                    };
                    _context.Collection.InsertOne(courier);
                }
            }
            List<Entities.General.BasicData.BasicData> lst = null;
            if(!isDomain)
            {
               lst = _context.Collection
                .Find(_ => _.GroupKey.ToLower() == groupKey.ToLower() && _.AssociatedDomainId == null).ToList();
            }
            else
            {
             lst = _context.Collection
               .Find(_ => _.GroupKey.ToLower() == groupKey.ToLower() && _.AssociatedDomainId == domainEntity.DomainId).ToList();
            }
           

            result = _mapper.Map<List<BasicDataModel>>(lst);
            if (withChooseItem)
            {
                result.Insert(0, new BasicDataModel() { Text = GeneralLibrary.Utilities.Language.GetString("Choose"), Value = "-1" });
            }

            return result;
        }


        public bool HasLastID()
        {
            var result = false;
            var domainName = base.GetCurrentDomainName();
            var domainEntity = _domainContext.Collection.Find(_ => _.DomainName == domainName).Any() ?
                _domainContext.Collection.Find(_ => _.DomainName == domainName).First() :
                _domainContext.Collection.Find(_ => _.IsDefault).First();
            if (_context.Collection.Find(_ => _.GroupKey.ToLower() == "lastid" && _.AssociatedDomainId == domainEntity.DomainId).Any())
            {
                result = true;
            }
            return result;
        }

        public bool HasShippingType()
        {
            var result = false;
            var domainName = base.GetCurrentDomainName();
            var domainEntity = _domainContext.Collection.Find(_ => _.DomainName == domainName).Any() ?
               _domainContext.Collection.Find(_ => _.DomainName == domainName).First() :
               _domainContext.Collection.Find(_ => _.IsDefault).First();
            if (_context.Collection.Find(_ => _.GroupKey.ToLower() == "shippingtype" && _.AssociatedDomainId == domainEntity.DomainId).Any())
            {
                result = true;
            }
            return result;
        }

        public async Task<Result> InsertNewRecord(Entities.General.BasicData.BasicData model)
        {
            Result result = new Result();
            try
            {
                await _context.Collection.InsertOneAsync(model);
                result.Succeeded = true;
                result.Message = ConstMessages.SuccessfullyDone;
            }
            catch (Exception ex)
            {
                result.Message = ConstMessages.InternalServerErrorMessage;
            }
            return result;
        }

        public void InsertOne(Entities.General.BasicData.BasicData entity)
        {
            try
            {
                _context.Collection.InsertOne(entity);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.ToString());
            }

        }

        public bool SaveLastId(long id)
        {
            var result = false;
            var domainName = base.GetCurrentDomainName();
            var domainEntity = _domainContext.Collection.Find(_ => _.DomainName == domainName).Any() ?
              _domainContext.Collection.Find(_ => _.DomainName == domainName).First() :
              _domainContext.Collection.Find(_ => _.IsDefault).First();
            var hasLastId = this.HasLastID();
            if (!hasLastId)
            {
                var def = new Entities.General.BasicData.BasicData
                {
                    BasicDataId = Guid.NewGuid().ToString(),
                    GroupKey = "lastid",
                    Text = "0",
                    Value = "0",
                    Order = 1,
                    AssociatedDomainId = domainEntity.DomainId
                };
                _context.Collection.InsertOne(def);
            }
            var entity = _context.Collection.Find(_ => _.GroupKey.ToLower() == "lastid" && _.AssociatedDomainId == domainEntity.DomainId).FirstOrDefault();
            entity.Text = id.ToString();
            entity.Value = id.ToString();
            var updateResult = _context.Collection.ReplaceOne(_ => _.BasicDataId == entity.BasicDataId, entity);
            if (updateResult.IsAcknowledged)
            {
                result = true;
            }
            return result;

        }

        public async Task<Result> UpdateDefaultDomainLastId(long lastId)
        {
            var res = new Result();
            var defaultDomain = _domainContext.Collection.Find(_ => _.IsDefault).FirstOrDefault();
            if(_context.Collection.Find(_=>_.GroupKey == "LastId" && _.AssociatedDomainId == defaultDomain.DomainId).Any())
            {
                var basic = _context.Collection.Find(_ => _.GroupKey == "LastId" && _.AssociatedDomainId == defaultDomain.DomainId).FirstOrDefault();
                basic.Text = lastId.ToString();
                basic.Value = lastId.ToString();
                var updateResult = await _context.Collection.ReplaceOneAsync(_ => _.BasicDataId == basic.BasicDataId, basic);
                if(updateResult.IsAcknowledged)
                {
                    res.Succeeded = true;
                }
            }else
            {
                var basic = new Entities.General.BasicData.BasicData()
                {
                    BasicDataId = Guid.NewGuid().ToString(),
                    Text = lastId.ToString(),
                    Value = lastId.ToString(),
                    GroupKey = "LastId",
                    AssociatedDomainId = defaultDomain.DomainId
                };
                try
                {
                    _context.Collection.InsertOne(basic);
                    res.Succeeded = true;
                }
                catch (Exception ex)
                {
                    res.Succeeded = false;
                }
            }

            return res;
        }
    }
}
