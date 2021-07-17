using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arad.Portal.GeneralLibrary.Utilities.Language;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public static class ConstMessages
    {
        public static string IdRequired = GetString("AlertAndMessage_IdRequired");
        public static string ObjectNotFound = GetString("AlertAndMessage_ObjectNotFound");
        public static string InternalServerErrorMessage = GetString("AlertAndMessage_InternalServerErrorMessage");
        public static string ModelHasIdError = GetString("AlertAndMessage_ModelHasIdError");
        public static string DuplicateClientIdInsertError = GetString("AlertAndMessage_DuplicateClientIdInsertError");
        public static string InProperIdFormat = GetString("AlertAndMessage_InProperIdFormat");
        public static string ExceptionOccured = GetString("AlertAndMessage_ExceptionOccured");
        public static string ParameterError = GetString("AlertAndMessage_ParameterError");
        public static string SuccessfullyDone = GetString("AlertAndMessage_SuccessfullyDone");
        public static string DuplicateError = GetString("AlertAndMessage_DuplicateError");
        public static string GeneralError = GetString("AlertAndMessage_GeneralError");
        public static string ErrorInSaving = GetString("AlertAndMessage_ErrorInSaving");
        public static string ModelError = GetString("AlertAndMessage_ModelError");
        public static string InvalidPassword = GetString("AlertAndMessage_InvalidPassword");
        public static string NoServerResponseError = GetString("AlertAndMessage_NoServerResponseError");
        public static string BadRequestError = GetString("AlertAndMessage_BadRequestError");
        public static string RangeLimitExceed = GetString("AlertAndMessage_RangeLimitExceed");
        public static string DuplicateField = GetString("AlerAndMessage_DuplicateField");
        public static string DeletedNotAllowedForDependencies = GetString("AlerAndMessage_DeletedNotAllowedForDependencies");
        public static string LackOfInventoryOfProduct = GetString("AlertAndMessage_LackOfInventoryOfProduct");
        
    }
}
