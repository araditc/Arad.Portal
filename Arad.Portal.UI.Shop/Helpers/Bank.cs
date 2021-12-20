using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Helpers
{
    public  class Bank
    {
        public static List<SelectListModel> GetBankList()
        {
            List<SelectListModel> list = new List<SelectListModel>()
            {
                new SelectListModel()
                {
                Value = "01",
                Text = "سپه",
                },
            new SelectListModel()
            {
                Value = "02",
                Text = "ملی",
            },
            new SelectListModel()
            {
                Value = "03",
                Text = "ملت",
            },
            new SelectListModel()
            {
                Value = "04",
                Text = "پست بانک",
            },
            new SelectListModel()
            {
                Value = "05",
                Text = "توسعه تعاون",
            },
            new SelectListModel()
            {
                Value = "06",
                Text = "توسعه صادرات ایران",
            },
            new SelectListModel()
            {
                Value = "07",
                Text = "صنعت و معدن",
            },
            new SelectListModel()
            {
                Value = "08",
                Text = "مسکن",
            },
            new SelectListModel()
            {
                Value = "09",
                Text = "کشاورزی",
            },
             new SelectListModel()
            {
                Value = "10",
                Text = "آینده",
            },
             new SelectListModel()
            {
                Value = "11",
                Text = "اقتصادنوین",
            },
             new SelectListModel()
            {
                Value = "12",
                Text = "انصار"
            },
             new SelectListModel()
            {
                Value = "13",
                Text = "ایران زمین",
            }, new SelectListModel()
            {
                Value = "14",
                Text = "پارسیان",
            }, new SelectListModel()
            {
                Value = "15",
                Text = "ایران زمین",
            }, new SelectListModel()
            {
                Value = "16",
                Text = "پاسارگاد",
            }, new SelectListModel()
            {
                Value = "17",
                Text = "تجارت",
            }, new SelectListModel()
            {
                Value = "18",
                Text = "حکمت ایرانیان",
            }, new SelectListModel()
            {
                Value = "19",
                Text = "خاورميانه",
            },
             new SelectListModel()
            {
                Value = "20",
                Text = "رفاه کارگران",
            },  new SelectListModel()
            {
                Value = "21",
                Text = "سامان",
            },  new SelectListModel()
            {
                Value = "22",
                Text = "سرمایه",
            },  new SelectListModel()
            {
                Value = "23",
                Text = "سینا",
            },  new SelectListModel()
            {
                Value = "24",
                Text = "شهر",
            },new SelectListModel()
            {
                Value = "25",
                Text = "صادرات ايران",
            },new SelectListModel()
            {
                Value = "26",
                Text = "قوامين",
            },new SelectListModel()
            {
                Value = "27",
                Text = "گردشگری",
            },
             new SelectListModel()
             {
                 Value = "28",
                 Text = "کارآفرین"
             },

             new SelectListModel()
             {
             Value = "29",
             Text = "مرکزی",
             }

        };

            return list.OrderBy(c => c.Text).ToList();
        }
    }
}
