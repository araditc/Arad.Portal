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
    public class ModuleRepository : BaseRepository, IModuleRepository
    {
        private readonly ModuleContext _context;
        public IMongoCollection<Module> Modules { get; set; }

        public IMongoCollection<Template> Templates { get; set; }
        public ModuleRepository(ModuleContext context, IHttpContextAccessor accessor):base(accessor)
        {
            _context = context;  
        }
        public bool HasAnyModule()
        {
            return _context.ModuleCollection.AsQueryable().Any();
        }

        public bool HasAnyTemplate()
        {
            return _context.TemplateCollection.AsQueryable().Any();
        }

        public void InsertOneModule(Module module)
        {
            _context.ModuleCollection.InsertOne(module);
        }

        public void InsertOneTemplate(Template template)
        {
            _context.TemplateCollection.InsertOne(template);
        }
    }
}
