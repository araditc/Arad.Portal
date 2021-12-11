using Arad.Portal.DataLayer.Contracts.General.MessageTemplate;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;

namespace Arad.Portal.DataLayer.Repositories.General.MessageTemplate.Mongo
{
    public class MessageTemplateRepository : BaseRepository, IMessageTemplateRepository
    {
        private readonly MessageTemplateContext _context;
        private readonly DomainContext _domainContext;
        private readonly IMapper _mapper;
        public MessageTemplateRepository(MessageTemplateContext context,
            IMapper mapper,
            DomainContext domainContext,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _domainContext = domainContext;
        }
        /// <summary>
        /// this method is used just in seedData
        /// </summary>
        /// <param name="entity"></param>
        public void InsertOne(Entities.General.MessageTemplate.MessageTemplate entity)
        {
            _context.Collection.InsertOne(entity);
        }

        public bool HasAny()
        {
            var result = false;
            if (_context.Collection.AsQueryable().Any())
            {
                result = true;
            }
            return result;
        }

        public  void InsertMany(List<Entities.General.MessageTemplate.MessageTemplate> templates)
        {
             _context.Collection.InsertMany(templates);
        }
        public Entities.General.MessageTemplate.MessageTemplate FetchTemplateByName(string templateName)
        {
            var res = new Entities.General.MessageTemplate.MessageTemplate();
            var entity = _context.Collection.Find(_ => _.TemplateName.ToLower() == templateName.ToLower()).FirstOrDefault();
            if(entity != null)
            {
                res = entity;
            }
            return res;
        }

        public async Task<List<Entities.General.MessageTemplate.MessageTemplate>> GetAllByName(string templateName)
        {
            //???
            //var domainEntity = _domainContext.Collection.Find(_ => _.DomainName == this.GetCurrentDomain()).FirstOrDefault();
            return await _context.Collection
                .Find(m => m.TemplateName.Equals(templateName) /*&& m.AssociatedDomainId == domainEntity.DomainId*/).ToListAsync();
        }
    }

    

}
