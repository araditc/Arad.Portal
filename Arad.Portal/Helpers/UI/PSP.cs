using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.Models.Shared.Transaction;
using System.Collections.Generic;

namespace Arad.Portal.Helpers.UI;

public class Psp
{
    public static List<PaymentServiceProvider> GetAllPsp()
    {
        return [GetPsp(Enums.PspType.Saman)];
    }

    public static PaymentServiceProvider GetPsp(Enums.PspType type)
    {
        switch (type)
        {
            //case Enums.PspType.IranKish:
            //    return new PaymentServiceProvider()
            //    {
            //        Type = Enums.PspType.IranKish,
            //        Parameters = new List<Parameter>
            //        {
            //            new Parameter()
            //            {
            //                Key = "merchant Id"

            //            },
            //            new Parameter()
            //            {
            //                Key = "Sha1Key"

            //            }

            //        },
            //        //IconAddress = "/PspImages/Irankish.jpg"
            //    };

            //case Enums.PspType.Parsian:
            //    return new PaymentServiceProvider()
            //    {
            //        Type = Enums.PspType.Parsian,
            //        Parameters = new List<Parameter>
            //        {
            //            new Parameter("Pin Code",null),
            //            new Parameter("Terminal",null),
            //        },

            //        //IconAddress = "/PspImages/Parsian.jpg"
            //    };

            case Enums.PspType.Saman:
                return new()
                       {
                           Type = Enums.PspType.Saman,
                           Parameters =
                           [
                               new("Username", null),
                               new("Password", null),
                               new("Terminal Id", null),
                               new("Merchant Id", null)
                           ],
                           //IconAddress = "/PspImages/Saman.jpg"
                       };
        }

        return null;
    }
}