using Arad.Portal.DataLayer.Contracts.General.BasicData;
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


        public List<BasicDataModel> GetList(string groupKey, bool withChooseItem)
        {
            var result = new List<BasicDataModel>();
            var domainName = base.GetCurrentDomainName();
            var domainEntity = _domainContext.Collection.Find(_ => _.DomainName == domainName).Any() ?
            _domainContext.Collection.Find(_ => _.DomainName == domainName).First() :
            _domainContext.Collection.Find(_ => _.IsDefault).First();
            if (groupKey.ToLower() == "shippingType")
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
            //test uncommented
            var lst = _context.Collection
                .Find(_ => _.GroupKey.ToLower() == groupKey.ToLower() /*&& _.AssociatedDomainId == domainEntity.DomainId*/).ToList();

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
    }
}
