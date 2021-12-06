using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.MessageTemplate
{
    public interface IMessageTemplateRepository
    {
        //RepositoryOperationResult InsertOne(Entities.General.MessageTemplate.MessageTemplate entity);
        Entities.General.MessageTemplate.MessageTemplate FetchTemplateByName(string templateName);

        Task<List<Entities.General.MessageTemplate.MessageTemplate>> GetAllByName(string templateName);
    }
}
