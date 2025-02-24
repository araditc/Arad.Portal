using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.AspNetCore.StaticFiles;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Arad.Portal.Helpers.Shared;

public class ImageFunctions
{
    public static string ResizeImage(string filePath, int desiredHeight/*pixel*/)
    {
        byte[] byteArray;
        using (FileStream stream = File.OpenRead(filePath))
        {
            IImageFormat format = Image.DetectFormat(stream);

            if (format != null)
            {
                using (Image img = Image.Load(stream))
                {
                    IImageEncoder imageEncoder = Configuration.Default.ImageFormatsManager.GetEncoder(format);
                    double ratio = (double)desiredHeight / img.Height;
                    int newWidth = (int)(img.Width * ratio);
                    int newHeight = (int)(img.Height * ratio);
                    img.Mutate(x => x.Resize(newWidth, newHeight));

                    using (MemoryStream ms = new MemoryStream())
                    {
                        img.Save(ms, imageEncoder);
                        byteArray = ms.ToArray();
                    }
                }

                return Convert.ToBase64String(byteArray);
            }
            else
            {
                // Handle unsupported files
                Console.WriteLine("Unsupported image format.");
                // Or throw an exception
                throw new NotSupportedException("Unsupported image format.");
            }
        }
    }
    public static string ResizeImageBasedOnWidth(string filePath, int desiredWidth/*pixel*/)
    {
        byte[] byteArray;
        using (FileStream stream = File.OpenRead(filePath))
        {
            IImageFormat format = Image.DetectFormat(stream);

            if (format != null)
            {
                using (Image img = Image.Load(stream))
                {
                    IImageEncoder imageEncoder = Configuration.Default.ImageFormatsManager.GetEncoder(format);
                    double ratio = (double)desiredWidth / img.Width;
                    int newWidth = (int)(img.Width * ratio);
                    int newHeight = (int)(img.Height * ratio);
                    img.Mutate(x => x.Resize(newWidth, newHeight));

                    using (MemoryStream ms = new MemoryStream())
                    {
                        img.Save(ms, imageEncoder);
                        byteArray = ms.ToArray();
                    }
                    return Convert.ToBase64String(byteArray);
                }
            }
            else
            {
                // Handle unsupported files
                Console.WriteLine("Unsupported image format.");
                // Or throw an exception
                throw new NotSupportedException("Unsupported image format.");
            }
        }
    }
    public static byte[] GetResizedImage(string filePath, int desiredHeight/*pixel*/)
    {
        byte[] byteArray;
        using (FileStream stream = File.OpenRead(filePath))
        {
            IImageFormat format = Image.DetectFormat(stream);

            if (format != null)
            {
                using (Image img = Image.Load(stream))
                {
                    IImageEncoder imageEncoder = Configuration.Default.ImageFormatsManager.GetEncoder(format);
                    double ratio = (double)desiredHeight / img.Height;
                    int newWidth = (int)(img.Width * ratio);
                    int newHeight = (int)(img.Height * ratio);
                    img.Mutate(x => x.Resize(newWidth, newHeight));
                    using (MemoryStream ms = new())
                    {
                        img.Save(ms, imageEncoder);
                        byteArray = ms.ToArray();
                    }
                    return byteArray;
                }
            }
            else
            {
                // Handle unsupported files
                Console.WriteLine("Unsupported image format.");
                // Or throw an exception
                throw new NotSupportedException("Unsupported image format.");
            }
        }
    }
    public static byte[] GetResizedImageBasedOnWidth(string filePath, int desiredWidth/*pixel*/)
    {
        byte[] byteArray;
        if (filePath.EndsWith("NoImage.png"))
        {
            return null;
        }
        using (FileStream stream = File.OpenRead(filePath))
        {
            IImageFormat format = Image.DetectFormat(stream);

            if (format != null)
            {
                using (Image img = Image.Load(stream))
                {
                    IImageEncoder imageEncoder = Configuration.Default.ImageFormatsManager.GetEncoder(format);
                    double ratio = (double)desiredWidth / img.Width;
                    int newWidth = (int)(img.Width * ratio);
                    int newHeight = (int)(img.Height * ratio);
                    img.Mutate(x => x.Resize(newWidth, newHeight));
                    using (MemoryStream ms = new())
                    {
                        img.Save(ms, imageEncoder);
                        byteArray = ms.ToArray();
                    }
                    return byteArray;
                }
            }
            else
            {
                return null;
            }
        }
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
        image.Mutate(x => x.Resize(newWidth, newHeight));

        MemoryStream ms = new();
        image.SaveAsJpeg(ms);
        ms.Seek(0, SeekOrigin.Begin);

        return image;
    }

    public static string SaveBackgroundImage(string base64Content, string pathToSave, string staticFileStorageURL)
    {
        string finalUrl;
        string fileName = Guid.NewGuid().ToString();
        string path = Path.Combine(staticFileStorageURL, pathToSave);
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            finalUrl = Path.Combine(pathToSave, $"{fileName}.png").Replace("\\", "/");
            byte[] bytes = Convert.FromBase64String(base64Content.Replace("data:image/jpeg;base64,", ""));
            Image image = Image.Load(bytes);
            image.Save(Path.Combine(path, $"{fileName}.png"), new JpegEncoder() { Quality = 100 });
        }
        catch (Exception ex)
        {
            finalUrl = "";
        }
        return finalUrl;
    }

    public static (byte[], string) GetImageWithActualSize(string path, string localStaticFileStorage)
    {

        string finalPath;
        if (!string.IsNullOrWhiteSpace(path))
        {
            if (path.StartsWith("/"))
                path = path[1..];
            finalPath = Path.Combine(localStaticFileStorage, path).Replace("\\", "/");

            if (!File.Exists(finalPath))
            {
                finalPath = "/imgs/NoImage.png";
            }
            string fileName = Path.GetFileName(finalPath);
            string mimeType = GetMimeType(fileName);
            byte[] fileContent = File.ReadAllBytes(finalPath);
            return (fileContent, mimeType);
        }
        else
        {
            finalPath = "/imgs/NoImage.png";
            string fileName = Path.GetFileName(finalPath);
            string mimeType = GetMimeType(fileName);
            byte[] fileContent = File.ReadAllBytes(finalPath);
            return (fileContent, mimeType);
        }
    }


    public static KeyValuePair<string, string> SaveImageModel(DataLayer.Models.Shared.Image picture, string pathToSave,
                                                              string staticFileStorageURL, string webRootPath)
    {
        KeyValuePair<string, string> res;
        if (string.IsNullOrWhiteSpace(staticFileStorageURL))
        {
            staticFileStorageURL = webRootPath;
        }
        picture.ImageId = Guid.NewGuid().ToString();
        string path = Path.Combine(staticFileStorageURL, pathToSave);
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            picture.Url = Path.Combine(pathToSave, $"{picture.ImageId}.jpg");
            byte[] bytes = Convert.FromBase64String(picture.Content.Replace("data:image/jpeg;base64,", ""));
            Image image = Image.Load(bytes);
            image.Save(Path.Combine(path, $"{picture.ImageId}.jpg"), new JpegEncoder() { Quality = 100 });
            res = new(picture.ImageId, picture.Url);
        }
        catch (Exception ex)
        {
            res = new(Guid.Empty.ToString(), "");
        }
        return res;
    }

    public static KeyValuePair<string, string> SaveImageModel(DataLayer.Models.Shared.Image picture, string pathToSave, string localStaticFileStorageURL)
    {
        KeyValuePair<string, string> res;

        picture.ImageId = Guid.NewGuid().ToString();
        string path = Path.Combine(localStaticFileStorageURL, pathToSave).Replace("\\", "/");
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            picture.Url = Path.Combine(path, $"{picture.ImageId}.jpg").Replace("\\", "/");
            byte[] bytes = Convert.FromBase64String(picture.Content.Replace("data:image/jpeg;base64,", ""));
            Image image = Image.Load(bytes);
            image.Save(Path.Combine(path, $"{picture.ImageId}.jpg").Replace("\\", "/"), new JpegEncoder() { Quality = 100 });


            res = new(picture.ImageId, picture.Url);
        }
        catch (Exception ex)
        {
            res = new(Guid.Empty.ToString(), "");
        }
        return res;
    }

    public static string GetMimeType(string fileName)
    {
        FileExtensionContentTypeProvider provider =
            new FileExtensionContentTypeProvider();
        string contentType;
        if (!provider.TryGetContentType(fileName, out contentType))
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }

}