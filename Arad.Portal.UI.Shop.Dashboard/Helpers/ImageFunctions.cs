using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{
    public class ImageFunctions
    {
        public static string ResizeImage(string filePath, int desiredHeight)
        {
            var base64String = File.ReadAllText(filePath);
            byte[] byteArray = Convert.FromBase64String(base64String);
            Image img;
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                img = Image.FromStream(ms);
            }

            double ratio = (double)desiredHeight / img.Height;
            int newWidth = (int)(img.Width * ratio);
            int newHeight = (int)(img.Height * ratio);
            Bitmap bitMapImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(bitMapImage))
            {
                g.DrawImage(img, 0, 0, newWidth, newHeight);
            }
            img.Dispose();

            byte[] byteImage;
            using (MemoryStream ms = new MemoryStream())
            {
                bitMapImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byteImage = ms.ToArray();
            }
            return Convert.ToBase64String(byteImage);
        }
        public static KeyValuePair<string, string> SaveImageModel(DataLayer.Models.Shared.Image picture, string pathToSave, string staticFileStorageURL, string webRootPath)
        {
            KeyValuePair<string, string> res;
            if (string.IsNullOrWhiteSpace(staticFileStorageURL))
            {
                staticFileStorageURL = webRootPath;
            }
            picture.ImageId = Guid.NewGuid().ToString();
            var path = $"{staticFileStorageURL}/{pathToSave}";
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                picture.Url = $"/{pathToSave}/{picture.ImageId}.jpg";
                byte[] bytes = Convert.FromBase64String(picture.Content.Replace("data:image/jpeg;base64,", ""));
                Image image;
                using MemoryStream ms = new MemoryStream(bytes);
                image = Image.FromStream(ms);
                image.Save(picture.Url, System.Drawing.Imaging.ImageFormat.Jpeg);
                res = new KeyValuePair<string, string>(picture.ImageId, picture.Url);
            }
            catch (Exception ex)
            {
                res = new KeyValuePair<string, string>(Guid.Empty.ToString(), "");
            }
            return res;
        }
    }
}
