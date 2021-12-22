﻿using Arad.Portal.DataLayer.Contracts.General.Services;
using Arad.Portal.DataLayer.Entities.General.Service;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.General.Service.Mongo
{
    public class ProviderRepository : BaseRepository, IProviderRepository
    {

        private readonly ProviderContext _context;
        public ProviderRepository(IHttpContextAccessor accessor,
            ProviderContext context):base(accessor)
        {
            _context = context;
        }
        public List<SelectListModel> GetProvidersPerType(ProviderType type)
        {
            var res = new List<SelectListModel>();
            var lst = _context.Collection.Find(_ => _.ProviderType == type).ToList();

            res = lst.Select(_ => new SelectListModel()
            {
                Text = _.ProviderName,
                Value = _.ProviderId
            }).ToList();

            return res;
        }

        public void InsertOne(Entities.General.Service.Provider entity)
        {
            _context.Collection.InsertOne(entity);
        }
    }
}
