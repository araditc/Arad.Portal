using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.DesignStructure
{
    public interface ITemplateRepository
    {
        void InsertOne(Entities.General.DesignStructure.Template template);

        bool HasAny();
    }
}
