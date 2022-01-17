using Microsoft.AspNetCore.Http;
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

        //public static KeyValuePair<string, string> SaveImageModel(DataLayer.Models.Shared.Image picture, string pathToSave, string staticFileStorageURL, string webRootPath)
        //{
        //    KeyValuePair<string, string> res;
        //    if (string.IsNullOrWhiteSpace(staticFileStorageURL))
        //    {
        //        staticFileStorageURL = webRootPath;
        //    }
        //    picture.ImageId = Guid.NewGuid().ToString();
        //    var path = Path.Combine(staticFileStorageURL, pathToSave);
        //    try
        //    {
        //        if (!Directory.Exists(path))
        //        {
        //            Directory.CreateDirectory(path);
        //        }
        //        picture.Url = Path.Combine(pathToSave, $"{picture.ImageId}.jpg");
        //        byte[] bytes = Convert.FromBase64String(picture.Content.Replace("data:image/jpeg;base64,", ""));
        //        Image image;
        //        using MemoryStream ms = new MemoryStream(bytes);
        //        image = Image.FromStream(ms);
        //        image.Save(Path.Combine(path, $"{picture.ImageId}.jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
        //        res = new KeyValuePair<string, string>(picture.ImageId, picture.Url);
        //    }
        //    catch (Exception ex)
        //    {
        //        res = new KeyValuePair<string, string>(Guid.Empty.ToString(), "");
        //    }
        //    return res;
        //}

        public static KeyValuePair<string, string> SaveImageModel(DataLayer.Models.Shared.Image picture, string pathToSave, string localStaticFileStorageURL)
        {
            KeyValuePair<string, string> res;
           
            picture.ImageId = Guid.NewGuid().ToString();
            var path = Path.Combine(localStaticFileStorageURL, pathToSave).Replace("\\", "/");
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                picture.Url = Path.Combine("/Images", pathToSave, $"{picture.ImageId}.jpg").Replace("\\", "/");
                byte[] bytes = Convert.FromBase64String(picture.Content.Replace("data:image/jpeg;base64,", ""));
                Image image;
                using MemoryStream ms = new MemoryStream(bytes);
                image = Image.FromStream(ms);
                image.Save(Path.Combine(path, $"{picture.ImageId}.jpg").Replace("\\", "/"), System.Drawing.Imaging.ImageFormat.Jpeg);
                res = new KeyValuePair<string, string>(picture.ImageId, picture.Url);
            }
            catch (Exception ex)
            {
                res = new KeyValuePair<string, string>(Guid.Empty.ToString(), "");
            }
            return res;
        }

        public static string GetMIMEType(string fileName)
        {
            var provider =
                new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(fileName, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
        public static byte[] GetResizedImage(string filePath, int desiredHeight)
        {
            byte[] byteArray = File.ReadAllBytes(filePath);
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
            return byteImage;
        }
        public static Image ScaleImage(Image image, int height)
        {
            double ratio = (double)height / image.Height;
            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);
            Bitmap newImage = new(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            image.Dispose();
            return newImage;
        }
        //public static bool ThumbnailCallback()
        //{
        //    return false;
        //}

        //public static Image GetReducedImage(int width, int height, Image resourceImage)
        //{
        //    try
        //    {
        //        Image ReducedImage;

        //        Image.GetThumbnailImageAbort callb = ThumbnailCallback;

        //        ReducedImage = resourceImage.GetThumbnailImage(width, height, callb, IntPtr.Zero);

        //        return ReducedImage;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        return null;
        //    }
        //}
        
    }

    public static class ImageValidator
    {
        public static bool IsImage(this IFormFile file)
        {
            try
            {
                var img = Image.FromStream(file.OpenReadStream());
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
