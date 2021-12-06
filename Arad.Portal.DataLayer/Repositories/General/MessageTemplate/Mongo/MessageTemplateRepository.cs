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

namespace Arad.Portal.DataLayer.Repositories.General.MessageTemplate.Mongo
{
    public class MessageTemplateRepository : BaseRepository, IMessageTemplateRepository
    {
        private readonly ErrorLogContext _context;
        private readonly IMapper _mapper;
        public MessageTemplateRepository(ErrorLogContext context, IMapper mapper,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
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
            return await _context.Collection.Find(m => m.TemplateName.Equals(templateName)).ToListAsync();
        }
    }

    

}
