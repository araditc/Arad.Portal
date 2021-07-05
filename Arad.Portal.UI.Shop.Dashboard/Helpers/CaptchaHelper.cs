using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{

    public static class CaptchaHelper
    {
        public static FileContentResult GenerateCaptchaImage(this ISession session, int lifeTime, bool noisy = true)
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            //generate new question
            int a = rand.Next(10, 99);
            int b = rand.Next(0, 9);
            var captcha = $"{a} + {b} = ?";



            //image stream
            FileContentResult img = null;

            using (var mem = new MemoryStream())
            using (var bmp = new Bitmap(130, 30))
            using (var gfx = Graphics.FromImage((Image)bmp))
            {
                gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));

                //add noise
                if (noisy)
                {
                    int i, r, x, y;
                    var pen = new Pen(Color.Yellow);
                    for (i = 1; i < 10; i++)
                    {
                        pen.Color = Color.FromArgb(
                            (rand.Next(0, 255)),
                            (rand.Next(0, 255)),
                            (rand.Next(0, 255)));

                        r = rand.Next(0, (130 / 3));
                        x = rand.Next(0, 130);
                        y = rand.Next(0, 30);

                        gfx.DrawEllipse(pen, x - r, y - r, r, r);
                    }
                }

                //add question
                gfx.DrawString(captcha, new Font("Tahoma", 15), Brushes.Gray, 2, 3);

                //render as Jpeg
                bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);
                img = new FileContentResult(mem.GetBuffer(), "image/Jpeg");
            }


            var captModel = new CaptchaModel()
            {
                Code = (a + b).ToString(),
                ExpirationDate = DateTime.Now.AddMinutes(lifeTime)
            };

            var serializedData = JsonConvert.SerializeObject(captModel);
            session.SetString("SystemCaptcha", serializedData);

            return img;
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
