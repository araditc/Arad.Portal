using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.DesignStructure
{
    public interface IModuleRepository
    {
        void InsertOneModule(Entities.General.DesignStructure.Module module);
        void InsertOneTemplate(Entities.General.DesignStructure.Template template);
        bool HasAnyModule();
        bool HasAnyTemplate();
        List<SelectListModel> GetAllTemplate();
        List<SelectListModel> GetAllModules();

        List<SelectListModel> GetAllProductOrContentTypes();

        List<SelectImageListModel> GetAllContentTemplateDesign();

        List<SelectImageListModel> GetAllProductTemplateDesign();

        List<SelectImageListModel> GetAllAdvertisementTemplateDesign();

        List<SelectImageListModel> GetAllImageSliderTemplateDesign();
        
        Entities.General.DesignStructure.Template FetchTemplateByName(string templateName);

        Entities.General.DesignStructure.Module FetchModuleByName(string moduleName);

    }
}
