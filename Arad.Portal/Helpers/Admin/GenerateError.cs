using Arad.Portal.Models.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Arad.Portal.Helpers.Admin;

public static class GenerateError
{
    public static List<ClientValidationErrorModel> Generate(this ModelStateDictionary model)
    {
        List<ClientValidationErrorModel> errors = new();

        foreach (string modelStateKey in model.Keys)
        {
            ModelStateEntry modelStateVal = model[modelStateKey];
            foreach (ModelError error in modelStateVal.Errors)
            {
                ClientValidationErrorModel obj = new ClientValidationErrorModel
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