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
        //void InsertOneTemplate(Entities.General.DesignStructure.Template template);
        bool HasAnyModule();
        //bool HasAnyTemplate();
        //List<SelectListModel> GetAllTemplate();
        List<SelectListModel> GetAllModules();

        List<Entities.General.DesignStructure.Module> GetAllModuleList();

        List<SelectListModel> GetAllProductOrContentTypes();

        List<SelectListModel> GetAllTransactionType();

        List<SelectListModel> GetAllLoadAnimationType();

        List<SelectListModel> GetAllSelectionType();

        List<SelectImageListModel> GetAllContentTemplateDesign();

        List<SelectImageListModel> GetAllProductTemplateDesign();

        List<SelectImageListModel> GetAllAdvertisementTemplateDesign();

        List<SelectImageListModel> GetAllImageSliderTemplateDesign();

        Entities.General.DesignStructure.Module FetchById(string moduleId);

         Entities.General.DesignStructure.Module FetchModuleByName(string moduleName);

    }
}
