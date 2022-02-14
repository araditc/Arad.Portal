using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Helpers
{
    public class Utilities
    {
        public static EntityRate ConvertPopularityRate(long totalScore, int scoredCount)
        {
            var res = new EntityRate();
            float[] arr = { 0.0f, 0.5f, 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.0f, 4.5f, 5.0f };
            //float[] intNumbers = { 0.0f, 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
            float[] halfNumbers = { 0.5f, 1.5f, 2.5f, 3.5f, 4.5f };
            float popularityRate = 0;

            if (scoredCount != 0)
                popularityRate = (float)totalScore / scoredCount;

            var rounded = Math.Round(popularityRate, 1, MidpointRounding.AwayFromZero);
            float tmp = 0;
            bool hasHalf = false;
            for (int i = 0; i <= 9; i++)
            {
                if (rounded > arr[i] && rounded <= arr[i + 1])
                {
                    tmp = arr[i + 1];
                    if (halfNumbers.Contains(tmp))
                        hasHalf = true;
                    else
                        hasHalf = false;
                    break;
                }
            }
            res.LikeRate = Convert.ToInt32(Math.Floor(tmp));

            if (hasHalf)
                res.DisikeRate = 5 - (Convert.ToInt32(Math.Floor(tmp)) + 1);
            else
                res.DisikeRate = 5 - Convert.ToInt32(Math.Floor(tmp));

            res.HalfLikeRate = hasHalf;

            return res;
        }
        public static string GenerateOtp()
        {
            Random generator = new();
            string generateOtp = generator.Next(0, 999999).ToString("D6");

            return generateOtp;
        }
    }
}
