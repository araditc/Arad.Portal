using Minio;
using Minio.DataModel;
using Minio.Exceptions;

using Serilog;
using Minio.DataModel.Args;
using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Arad.Portal.Helpers.Shared;

public class MinioHelper(IMinioClient client, IConfiguration configuration)
{
    public async Task<bool> MakeBucket(string bucketName)
    {
        try
        {
            BucketExistsArgs beArgs = new BucketExistsArgs().WithBucket(bucketName);
            bool found = await client.BucketExistsAsync(beArgs);

            if (found)
            {
                return true;
            }

            MakeBucketArgs mbArgs = new MakeBucketArgs().WithBucket(bucketName);
            await client.MakeBucketAsync(mbArgs);

            SetBucketLifecycleArgs args = new SetBucketLifecycleArgs().WithBucket(bucketName);
            await client.SetBucketLifecycleAsync(args);

            return true;
        }
        catch (MinioException e)
        {
            Console.WriteLine("MinioException: {0}", e.Message);
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e.Message);
            return false;
        }
    }

    public async Task<bool> Upload(string bucketName, string objectName, string filePath, string contentType)
    {
        try
        {
            PutObjectArgs putObjectArgs = new PutObjectArgs()
                                          .WithBucket(bucketName)
                                          .WithObject(objectName)
                                          .WithFileName(filePath)
                                          .WithContentType(contentType);
            await client.PutObjectAsync(putObjectArgs);

            StatObjectArgs statObjectArgs = new StatObjectArgs()
                                            .WithBucket(bucketName)
                                            .WithObject(objectName);
            ObjectStat objectStat = await client.StatObjectAsync(statObjectArgs);

            return objectStat is { Size: > 0 };
        }
        catch (MinioException e)
        {
            Console.WriteLine("File Upload Error: {0}", e.Message);
            return false;
        }
    }

    public async Task<(bool, string)> GetObjectUrl(string bucketName, string objectName, int expiration)
    {
        try
        {
            PresignedGetObjectArgs args = new PresignedGetObjectArgs()
                                          .WithBucket(bucketName)
                                          .WithObject(objectName)
                                          .WithExpiry(expiration);
            string url = await client.PresignedGetObjectAsync(args);

            return (true, url);
        }
        catch (MinioException e)
        {
            Log.Error("Error occurred: " + e);
            return (false, "");
        }
    }

    public async Task<(bool, byte[])> GetObject(string bucketName, string objectName)
    {
        try
        {
            MemoryStream memoryStream = new();
            GetObjectArgs getObjectArgs = new GetObjectArgs()
                                          .WithBucket(bucketName)
                                          .WithObject(objectName)
                                          .WithCallbackStream(stream =>
                                                              {
                                                                  stream.CopyTo(memoryStream);
                                                                  stream.Dispose();
                                                              });
            await client.GetObjectAsync(getObjectArgs).ConfigureAwait(false);
            return (true, memoryStream.ToArray());
        }
        catch (Exception e)
        {
            Log.Error($"[Bucket(Name:{bucketName})] Exception: {e}");
            return (false, null);
        }
    }

    public async Task<(bool, string)> GetObjectUrl2(string bucketName, string objectName)
    {
        if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(objectName))
        {
            return (false, "");
        }

        return (true, $"{configuration["minioConsole"]}/{bucketName}/{objectName}");
    }

    public async Task<bool> RemoveObject(string bucketName, string objectName)
    {
        try
        {
            StatObjectArgs statObjectArgs = new StatObjectArgs()
                                            .WithBucket(bucketName)
                                            .WithObject(objectName);

            ObjectStat result = await client.StatObjectAsync(statObjectArgs).ConfigureAwait(false);

            if (result is { Size: > 0 })
            {
                RemoveObjectArgs removeObjectArgs = new RemoveObjectArgs()
                                                    .WithBucket(bucketName)
                                                    .WithObject(objectName);

                await client.RemoveObjectAsync(removeObjectArgs).ConfigureAwait(false);
            }

            return true;
        }
        catch (Exception e)
        {
            Log.Error($"[Bucket(Name:{bucketName})] Exception: {e}");
            return false;
        }
    }

    public async Task<bool> Upload(string bucketName, string objectName, Stream stream, string contentType, long objectSize)
    {
        try
        {
            PutObjectArgs putObjectArgs = new PutObjectArgs()
                                          .WithBucket(bucketName)
                                          .WithObject(objectName)
                                          .WithStreamData(stream)
                                          .WithObjectSize(objectSize)
                                          .WithContentType(contentType);
            await client.PutObjectAsync(putObjectArgs);

            StatObjectArgs statObjectArgs = new StatObjectArgs()
                                            .WithBucket(bucketName)
                                            .WithObject(objectName);
            ObjectStat objectStat = await client.StatObjectAsync(statObjectArgs);

            return objectStat is { Size: > 0 };
        }
        catch (MinioException e)
        {
            Log.Error("File Stream Upload Error: {0}", e.Message);
            return false;
        }
    }
    public async Task<bool> Upload(IFormFile file, string bucketName, string objectName)
    {
        if (file == null || file.Length == 0)
        {
            return false;
        }

        // Ensure the bucket exists or create it
        bool bucketExists = await MakeBucket(bucketName);
        if (!bucketExists)
        {
            return false;
        }

        try
        {
            using var stream = file.OpenReadStream();
            string contentType = file.ContentType;
            long fileSize = file.Length;

            // Upload the file stream to MinIO
            return await Upload(bucketName, objectName, stream, contentType, fileSize);
        }
        catch (Exception e)
        {
            Log.Error("Error uploading file to MinIO: " + e);
            return false;
        }
    }
}