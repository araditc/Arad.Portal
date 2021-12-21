using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Helpers
{
    public class PSP
    {
        public static List<PaymentServiceProvider> GetAllPSP()
        {
            return new List<PaymentServiceProvider>()
            {
                GetPSP(Enums.PspType.IranKish),
                GetPSP(Enums.PspType.Parsian),
                GetPSP(Enums.PspType.Saman)
            };
        }

        public static PaymentServiceProvider GetPSP(Enums.PspType type)
        {
            switch (type)
            {
                case Enums.PspType.IranKish:
                    return new PaymentServiceProvider()
                    {
                        Type = Enums.PspType.IranKish,
                        Parameters = new List<Parameter>
                        {
                            new Parameter()
                            {
                                Key = "merchant Id"

                            },
                            new Parameter()
                            {
                                Key = "Sha1Key"

                            }

                        },
                        //IconAddress = "/PspImages/Irankish.jpg"
                    };

                case Enums.PspType.Parsian:
                    return new PaymentServiceProvider()
                    {
                        Type = Enums.PspType.Parsian,
                        Parameters = new List<Parameter>
                        {
                            new Parameter("Pin Code",null),
                            new Parameter("Terminal",null),
                        },

                        //IconAddress = "/PspImages/Parsian.jpg"
                    };

                case Enums.PspType.Saman:
                    return new PaymentServiceProvider()
                    {
                        Type = Enums.PspType.Saman,
                        Parameters = new List<Parameter>
                        {
                            new Parameter("Username",null),
                            new Parameter("Password",null),
                            new Parameter("Terminal Id",null),
                            new Parameter("Merchant Id",null),
                        },
                        //IconAddress = "/PspImages/Saman.jpg"
                    };
            }

            return null;
        }
    }
}
