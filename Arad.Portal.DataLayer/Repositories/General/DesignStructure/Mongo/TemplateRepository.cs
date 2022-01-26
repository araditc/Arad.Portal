using Arad.Portal.DataLayer.Contracts.General.DesignStructure;
using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.General.DesignStructure.Mongo
{
    public class TemplateRepository: BaseRepository, ITemplateRepository
    {
        private readonly ModuleContext _context;
        public TemplateRepository(IHttpContextAccessor accessor,
            ModuleContext context):base(accessor)
        {
            _context = context; 
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

        public void InsertOne(Template template)
        {
           _context.TemplateCollection.InsertOne(template);
        }
    }
}
