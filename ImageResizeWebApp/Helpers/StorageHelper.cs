using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using ImageResizeWebApp.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace ImageResizeWebApp.Helpers
{
    public static class StorageHelper
    {

        public static bool IsSupportedFile(IFormFile file)
        {
            //TODO: for now I accept all files
            // if (file.ContentType.Contains("image"))
            // {
            //     return true;
            // }

            // string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg", ".pptx",".zip",".pdf",".xlsx" };

            // return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
            return true;
        }

        public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName,
                                                            AzureStorageConfig _storageConfig)
        {
            // Create a URI to the blob
            Uri blobUri = new Uri("https://" +
                                  _storageConfig.AccountName +
                                  ".blob.core.windows.net/" +
                                  _storageConfig.ImageContainer +
                                  "/" + fileName);

            // Create StorageSharedKeyCredentials object by reading
            // the values from the configuration (appsettings.json)
            StorageSharedKeyCredential storageCredentials =
                new StorageSharedKeyCredential(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create the blob client.
            BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

            // Upload the file
            await blobClient.UploadAsync(fileStream);

            return await Task.FromResult(true);
        }

        public static async Task<List<string>> GetLinksUrls(AzureStorageConfig _storageConfig)
        {
            List<string> linksUrls = new List<string>();
            var storageConnString = "DefaultEndpointsProtocol=https;AccountName=stfilequick;AccountKey=jQ7NeHMwYMnSDwdxGtRjPQzWC/iWND12nwVZuJ0SL/f8zolNZDLu47u4EeQDIpW6gfLc3oEv5gaLzbKBNzCgWA==;EndpointSuffix=core.windows.net";
            var table = TableService.GetTableReference(storageConnString,"ShortLinks");
            // Find books published before 1950 and return the first 5 sorted by author name.
            var query = new TableQuery<LinkEntity>()
                .OrderBy(nameof(LinkEntity.ShortLink))
                .Take(5);

            var queryResults = table.ExecuteQuery(query);
            foreach (var result in queryResults)
            {
                linksUrls.Add(result.ToString());
                Console.WriteLine(result.ToString());
            }
            return await Task.FromResult(linksUrls);

        }
        public static async Task<List<string>> GetThumbNailUrls(AzureStorageConfig _storageConfig)
        {
            
            List<string> thumbnailUrls = new List<string>();

            // Create a URI to the storage account
            Uri accountUri = new Uri("https://" + _storageConfig.AccountName + ".blob.core.windows.net/");

            // Create BlobServiceClient from the account URI
            BlobServiceClient blobServiceClient = new BlobServiceClient(accountUri);

            // Get reference to the container
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(_storageConfig.ThumbnailContainer);

            if (container.Exists())
            {
                foreach (BlobItem blobItem in container.GetBlobs())
                {
                    thumbnailUrls.Add(container.Uri + "/" + blobItem.Name);
                }
            }

            return await Task.FromResult(thumbnailUrls);
        }
    }
}
