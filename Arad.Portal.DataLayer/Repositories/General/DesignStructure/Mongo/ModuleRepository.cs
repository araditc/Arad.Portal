﻿using Arad.Portal.DataLayer.Contracts.General.DesignStructure;
using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using Arad.Portal.GeneralLibrary.Utilities;
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
            Modules = _context.ModuleCollection;
            Templates = _context.TemplateCollection;
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

        public List<SelectListModel> GetAllTemplate()
        {
            FilterDefinitionBuilder<Template> builder = new();
            FilterDefinition<Template> filterDef;
            filterDef = builder.Empty;
            var lst = _context.TemplateCollection
                .Find(filterDef)
                .Project(_=> new SelectListModel() 
                {
                    Value = _.TemplateId, 
                    Text = _.TemplateName
                }).ToList();
            return lst;
        }

        public List<SelectListModel> GetAllModules()
        {
            FilterDefinitionBuilder<Module> builder = new();
            FilterDefinition<Module> filterDef;
            filterDef = builder.Empty;
            var lst = _context.ModuleCollection
                .Find(filterDef)
                .Project(_ => new SelectListModel()
                {
                    Value = _.ModuleId,
                    Text = _.ModuleName
                }).ToList();
            lst.Insert(0, new SelectListModel() { Text = GeneralLibrary.Utilities.Language.GetString("Choose"), Value = "-1" });
            return lst;
        }

        public Template FetchTemplateByName(string templateName)
        {
            var tem = _context.TemplateCollection
                .Find(_ => _.TemplateName.ToLower() == templateName.ToLower()).FirstOrDefault();
            return tem;
        }

        public Module FetchModuleByName(string moduleName)
        {
            var module = _context.ModuleCollection
                         .Find(_ => _.ModuleName == moduleName).FirstOrDefault();
            return module;
        }

        public List<SelectListModel> GetAllProductOrContentTypes()
        {
            var result = new List<SelectListModel>();
            foreach (int i in Enum.GetValues(typeof(ProductOrContentType)))
            {
                string name = Enum.GetName(typeof(ProductOrContentType), i);
                var obj = new SelectListModel()
                {
                    Text = GeneralLibrary.Utilities.Language.GetString($"Enum_{name}"),
                    Value = name
                };
                result.Add(obj);
            }
            
            return result;
        }

        public List<SelectImageListModel> GetAllContentTemplateDesign()
        {
            var result = new List<SelectImageListModel>();
            foreach (int i in Enum.GetValues(typeof(ContentTemplateDesign)))
            {
                string name = Enum.GetName(typeof(ContentTemplateDesign), i);
                var obj = new SelectImageListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }

            return result;
        }

        public List<SelectImageListModel> GetAllProductTemplateDesign()
        {
            var result = new List<SelectImageListModel>();
            foreach (int i in Enum.GetValues(typeof(ProductTemplateDesign)))
            {
                string name = Enum.GetName(typeof(ProductTemplateDesign), i);
                var obj = new SelectImageListModel()
                {
                    Text = name,
                    Value = name,
                    ImageUrl = $"template/{name}.jpg"
                };
                
                result.Add(obj);
            }

            return result;
        }

        public List<SelectImageListModel> GetAllAdvertisementTemplateDesign()
        {
            var result = new List<SelectImageListModel>();
            foreach (int i in Enum.GetValues(typeof(AdvertisementTemplateDesign)))
            {
                string name = Enum.GetName(typeof(AdvertisementTemplateDesign), i);
                var obj = new SelectImageListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }

            return result;
        }

        public List<SelectImageListModel> GetAllImageSliderTemplateDesign()
        {
            var result = new List<SelectImageListModel>();
            foreach (int i in Enum.GetValues(typeof(ImageSliderTemplateDesign)))
            {
                string name = Enum.GetName(typeof(ImageSliderTemplateDesign), i);
                var obj = new SelectImageListModel()
                {
                    Text = name,
                    Value = i.ToString()
                };
                result.Add(obj);
            }

            return result;
        }

        public Template FetchTemplateById(string templateId)
        {
            var tem = _context.TemplateCollection
                .Find(_ => _.TemplateId == templateId).FirstOrDefault();
            return tem;
        }

        public Module FetchById(string moduleId)
        {
            var module = _context.ModuleCollection
                   .Find(_ => _.ModuleId == moduleId).FirstOrDefault();
            return module;
        }

        public List<Module> GetAllModuleList()
        {
            var moduleList = _context.ModuleCollection.Find(_ => _.IsActive).ToList();
            return moduleList;
        }
    }
}