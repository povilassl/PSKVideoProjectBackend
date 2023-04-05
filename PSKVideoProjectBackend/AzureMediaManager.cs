using DemoVideoApplication;
using Azure.ResourceManager.Media;
using Azure.ResourceManager.Media.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;
using System.IO;
using System.ComponentModel;

namespace PSKVideoProjectBackend
{
    public static class AzureMediaManager
    {
        //public static IAzureMediaServicesClient _client = null;
        public static AzConfigWrapper? _config = null;

        static BlobServiceClient _thumbnailBlobClient;
        static BlobContainerClient _thumbnailContainerClient;

        static string _thumbnailBlobUriPrefix = "https://mediastorageaccount1312.blob.core.windows.net/thumbnail-images/";


        public static async Task InitManager()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();
            string connectionString = configuration.GetConnectionString("ThumbnailBlobStorageConnectionString")!;

            _thumbnailBlobClient = new BlobServiceClient(connectionString);
            _thumbnailContainerClient = _thumbnailBlobClient.GetBlobContainerClient("thumbnail-images");

        }

        public static async Task<string> UploadThumbnailImage(IFormFile image)
        {
            Azure.Response<BlobContentInfo> response;

            var originalFileName = Path.GetFileNameWithoutExtension(image.FileName);
            var extension = Path.GetExtension(image.FileName);
            var newFileName = $"{originalFileName}_{Guid.NewGuid()}{extension}";

            using (var memoryStream = new MemoryStream())
            {
                try
                {
                    image.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    BlobClient blobClient = _thumbnailContainerClient.GetBlobClient(newFileName);

                    response = await blobClient.UploadAsync(memoryStream, new BlobHttpHeaders { ContentType = image.ContentType });
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    return null!;
                }

            }

            var rawResponse = response.GetRawResponse();

            //returning Blob URI
            return rawResponse.ReasonPhrase == "Created" ? _thumbnailBlobUriPrefix + newFileName : "";
        }

        //public static async Task InitManager()
        //{
        //    Console.WriteLine("Dir:" + Directory.GetCurrentDirectory());

        //    AzConfigWrapper config = new AzConfigWrapper(new ConfigurationBuilder()
        //        .SetBasePath(Directory.GetCurrentDirectory())
        //        .AddJsonFile("configappsettings.json", optional: true, reloadOnChange: true)
        //        .AddEnvironmentVariables()
        //        .Build());

        //    try
        //    {
        //        IAzureMediaServicesClient client = await CreateMediaServicesClientAsync(config);

        //        Console.WriteLine("connected");

        //        _client = client;
        //        _config = config;
        //    }
        //    catch (Exception exception)
        //    {
        //        if (exception.Source.Contains("ActiveDirectory"))
        //        {
        //            Console.Error.WriteLine("TIP: Make sure that you have filled out the appsettings.json file before running this sample.");
        //        }

        //        Console.Error.WriteLine($"{exception.Message}");


        //        if (exception.GetBaseException() is ErrorResponseException apiException)
        //        {
        //            Console.Error.WriteLine(
        //                $"ERROR: API call failed with error code '{apiException.Body.Error.Code}' and message '{apiException.Body.Error.Message}'.");
        //        }
        //    }
        //}
    }
}
