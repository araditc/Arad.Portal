using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.DesignStructure
{
    public interface IModuleRepository
    {
        void InsertOne(Entities.General.DesignStructure.Module module);
        bool HasAny();
    }
}
