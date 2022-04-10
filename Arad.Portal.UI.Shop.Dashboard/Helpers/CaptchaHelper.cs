using System;
using System.IO;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{

    public static class CaptchaHelper
    {
        public static FileContentResult GenerateCaptchaImage(this ISession session, int lifeTime, bool noisy = true)
        {
            byte[] byteArray;
            var rand = new Random((int)DateTime.Now.Ticks);
            //generate new question
            int a = rand.Next(10, 99);
            int b = rand.Next(0, 9);
            var captcha = $"{a} + {b} = ?";

            //image stream
            FileContentResult fileResult = null;
            var img = new Image<Rgba32>(130, 30);
            img.Mutate(_ => _.BackgroundColor(Color.White));
            
            var font = SystemFonts.CreateFont("Tahoma", 17, FontStyle.Italic);
            var location = new PointF(10, 5);
            img.Mutate(_=> _.DrawText(captcha, font, Color.DimGray, location));

            if (noisy)
            {
                int i, r, x, y;
                Color clr;
                clr = Color.Yellow;
                for (i = 1; i < 10; i++)
                {
                    clr = new Color(new Bgr24(
                         (byte)rand.Next(0, 255),
                         (byte)rand.Next(0, 255),
                         (byte)rand.Next(0, 255)));

                    r = rand.Next(1, (130 / 3));
                    x = rand.Next(0, 130);
                    y = rand.Next(0, 30);
                    IPath polygon = new EllipsePolygon(new PointF(x-1, y-1), r);
                    img.Mutate(x => x.Draw(clr,1f, polygon));
                }
            }
            using (MemoryStream ms = new())
            {
                img.SaveAsJpeg(ms);
                byteArray = ms.ToArray();
            }
            fileResult = new FileContentResult(byteArray, "image/Jpeg");
            img.Dispose();

            var captModel = new CaptchaModel()
            {
                Code = (a + b).ToString(),
                ExpirationDate = DateTime.Now.AddMinutes(lifeTime)
            };

            var serializedData = JsonConvert.SerializeObject(captModel);
            session.SetString("SystemCaptcha", serializedData);
            return fileResult;
        }

        public static string GenerateCaptchaImageString(this ISession session, int lifeTime, bool noisy = true)
        {
            byte[] byteArray;
            var rand = new Random((int)DateTime.Now.Ticks);
            //generate new question
            int a = rand.Next(10, 99);
            int b = rand.Next(0, 9);
            var captcha = $"{a} + {b} = ?";

            //image stream
            string base64String = "";
            var img = new Image<Rgba32>(130, 30);
            img.Mutate(_ => _.BackgroundColor(Color.White));

            var font = SystemFonts.CreateFont("Tahoma", 17, FontStyle.Italic);
            var location = new PointF(10, 5);
            img.Mutate(_ => _.DrawText(captcha, font, Color.DimGray, location));

            if (noisy)
            {
                int i, r, x, y;
                Color clr;
                clr = Color.Yellow;
                for (i = 1; i < 10; i++)
                {
                    clr = new Color(new Bgr24(
                         (byte)rand.Next(0, 255),
                         (byte)rand.Next(0, 255),
                         (byte)rand.Next(0, 255)));

                    r = rand.Next(1, (130 / 3));
                    x = rand.Next(0, 130);
                    y = rand.Next(0, 30);
                    IPath polygon = new EllipsePolygon(new PointF(x-1, y-1), r);
                    img.Mutate(x => x.Draw(clr, 1f, polygon));
                }
            }
            using (MemoryStream ms = new())
            {
                img.SaveAsJpeg(ms);
                byteArray = ms.ToArray();
            }
            base64String = Convert.ToBase64String(byteArray);
            img.Dispose();

            var captModel = new CaptchaModel()
            {
                Code = (a + b).ToString(),
                ExpirationDate = DateTime.Now.AddMinutes(lifeTime)
            };

            var serializedData = JsonConvert.SerializeObject(captModel);
            session.SetString("SystemCaptcha", serializedData);
            return base64String;

        }

        public static bool ValidateCaptcha(this ISession session, string code)
        {

            var data = session.GetString("SystemCaptcha");
            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }

            try
            {
                var capt = JsonConvert.DeserializeObject<CaptchaModel>(data);
                if (capt.Code == code && capt.ExpirationDate > DateTime.Now)
                {
                    session.Remove("SystemCaptcha");
                    return true;
                }

                session.Remove("SystemCaptcha");
                return false;
            }
            catch (Exception e)
            {
                session.Remove("SystemCaptcha");
                return false;
            }
        }
    }
}
