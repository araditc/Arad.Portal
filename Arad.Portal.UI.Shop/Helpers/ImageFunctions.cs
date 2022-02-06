using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats;

namespace Arad.Portal.UI.Shop.Helpers
{
    public class ImageFunctions
    {
        public static string ResizeImage(string filePath, int desiredHeight/*pixel*/)
        {
            byte[] byteArray;
            //Image img;
            IImageFormat format;
            Image img = Image.Load(filePath, out format);
            var imageEncoder = Configuration.Default.ImageFormatsManager.FindEncoder(format);
            double ratio = (double)desiredHeight / img.Height;
            int newWidth = (int)(img.Width * ratio);
            int newHeight = (int)(img.Height * ratio);
            img.Mutate(x => x.Resize(newHeight, newWidth));

            using (var ms = new MemoryStream())
            {
                img.Save(ms, imageEncoder);
                byteArray = ms.ToArray();
            }
                        
            return Convert.ToBase64String(byteArray);
        }
        public static string ResizeImageBasedOnWidth(string filePath, int desiredWidth/*pixel*/)
        {
            byte[] byteArray;
            //Image img;
            IImageFormat format;
            Image img = Image.Load(filePath, out format);
            var imageEncoder = Configuration.Default.ImageFormatsManager.FindEncoder(format);
            double ratio = (double)desiredWidth / img.Width;
            int newWidth = (int)(img.Width * ratio);
            int newHeight = (int)(img.Height * ratio);
            img.Mutate(x => x.Resize(newHeight, newWidth));

            using (var ms = new MemoryStream())
            {
                img.Save(ms, imageEncoder);
                byteArray = ms.ToArray();
            }
            return Convert.ToBase64String(byteArray);
        }

        public static byte[] GetResizedImage(string filePath, int desiredHeight/*pixel*/)
        {
            byte[] byteArray;
            //Image img;
            IImageFormat format;
            Image img = Image.Load(filePath, out format);
           
            var imageEncoder = Configuration.Default.ImageFormatsManager.FindEncoder(format);
            double ratio = (double)desiredHeight / img.Height;
            int newWidth = (int)(img.Width * ratio);
            int newHeight = (int)(img.Height * ratio);
            img.Mutate(x => x.Resize(newHeight, newWidth));
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, imageEncoder);
                byteArray = ms.ToArray();
            }
            return byteArray;
        }

        public static byte[] GetResizedImageBasedOnWidth(string filePath, int desiredWidth/*pixel*/)
        {
            byte[] byteArray;
            //Image img;
            IImageFormat format;
            Image img = Image.Load(filePath, out format);

            var imageEncoder = Configuration.Default.ImageFormatsManager.FindEncoder(format);
            double ratio = (double)desiredWidth / img.Width;
            int newWidth = (int)(img.Width * ratio);
            int newHeight = (int)(img.Height * ratio);
            img.Mutate(x => x.Resize(newHeight, newWidth));
            using (MemoryStream ms = new())
            {
                img.Save(ms, imageEncoder);
                byteArray = ms.ToArray();
            }
            return byteArray;
        }

        public static Image ScaleImage(Image image, int desiredHeight)
        {
            double ratio = (double)desiredHeight / image.Height;
            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);
            image.Mutate(x => x.Resize(newHeight, newWidth));

            return image;
        }

        public static Image ScaleImageBasedOnWidth(Image image, int desiredWidth)
        {
            double ratio = (double)desiredWidth / image.Width;
            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);
            image.Mutate(x => x.Resize(newHeight, newWidth));

            return image;
        }
        public static KeyValuePair<string, string> SaveImageModel(DataLayer.Models.Shared.Image picture, string pathToSave, string staticFileStorageURL, string webRootPath)
        {
            KeyValuePair<string, string> res;
            if (string.IsNullOrWhiteSpace(staticFileStorageURL))
            {
                staticFileStorageURL = webRootPath;
            }
            picture.ImageId = Guid.NewGuid().ToString();
            var path = Path.Combine(staticFileStorageURL, pathToSave);
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                picture.Url = Path.Combine(pathToSave, $"{picture.ImageId}.jpg");
                byte[] bytes = Convert.FromBase64String(picture.Content.Replace("data:image/jpeg;base64,", ""));
                Image image = Image.Load(bytes);
                image.Save(Path.Combine(path, $"{picture.ImageId}.jpg"), new JpegEncoder() { Quality = 100});
                res = new KeyValuePair<string, string>(picture.ImageId, picture.Url);
            }
            catch (Exception ex)
            {
                res = new KeyValuePair<string, string>(Guid.Empty.ToString(), "");
            }
            return res;
        }

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
                Image image = Image.Load(bytes);
                image.Save(Path.Combine(path, $"{picture.ImageId}.jpg").Replace("\\", "/"), new JpegEncoder() { Quality = 100 });
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

    }

}
