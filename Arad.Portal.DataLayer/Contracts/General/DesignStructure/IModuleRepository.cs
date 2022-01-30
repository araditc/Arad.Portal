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
    }
}
