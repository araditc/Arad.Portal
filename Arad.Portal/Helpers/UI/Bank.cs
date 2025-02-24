using Arad.Portal.DataLayer.Models.Shared;

using System.Collections.Generic;
using System.Linq;

namespace Arad.Portal.Helpers.UI;

public class Bank
{
    public static List<SelectListModel> GetBankList()
    {
        List<SelectListModel> list =
        [
            new() { Value = "01", Text = "سپه", },

            new() { Value = "02", Text = "ملی", },

            new() { Value = "03", Text = "ملت", },

            new() { Value = "04", Text = "پست بانک", },

            new() { Value = "05", Text = "توسعه تعاون", },

            new() { Value = "06", Text = "توسعه صادرات ایران", },

            new() { Value = "07", Text = "صنعت و معدن", },

            new() { Value = "08", Text = "مسکن", },

            new() { Value = "09", Text = "کشاورزی", },

            new() { Value = "10", Text = "آینده", },

            new() { Value = "11", Text = "اقتصادنوین", },

            new() { Value = "12", Text = "انصار" },

            new() { Value = "13", Text = "ایران زمین", },
            new() { Value = "14", Text = "پارسیان", },
            new() { Value = "15", Text = "ایران زمین", },
            new() { Value = "16", Text = "پاسارگاد", },
            new() { Value = "17", Text = "تجارت", },
            new() { Value = "18", Text = "حکمت ایرانیان", },
            new() { Value = "19", Text = "خاورميانه", },

            new() { Value = "20", Text = "رفاه کارگران", },
            new() { Value = "21", Text = "سامان", },
            new() { Value = "22", Text = "سرمایه", },
            new() { Value = "23", Text = "سینا", },
            new() { Value = "24", Text = "شهر", },
            new() { Value = "25", Text = "صادرات ايران", },
            new() { Value = "26", Text = "قوامين", },
            new() { Value = "27", Text = "گردشگری", },

            new() { Value = "28", Text = "کارآفرین" },


            new() { Value = "29", Text = "مرکزی", }
        ];

        return list.OrderBy(c => c.Text).ToList();
    }
}