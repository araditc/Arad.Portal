using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{
    public static class GenerateError
    {
        public static List<ClientValidationErrorModel> Generate(this ModelStateDictionary model)
        {
            List<ClientValidationErrorModel> errors = new List<ClientValidationErrorModel>();

            foreach (var modelStateKey in model.Keys)
            {
                var modelStateVal = model[modelStateKey];
                foreach (var error in modelStateVal.Errors)
                {
                    var obj = new ClientValidationErrorModel
                    {
                        Key = modelStateKey,
                        ErrorMessage = error.ErrorMessage,
                    };

                    errors.Add(obj);
                }
            }

            return errors;
        }
    }
}
