﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Quivi.Infrastructure.Abstractions.Storage;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Infrastructure.Storage.Azure
{
    public class AzureBlobStorage : IFileStorage
    {
        public required string ConnectionString { get; init; }
        public required string VirtualDirectory { get; init; }

        public async Task<bool> FileExists(string name, params string[] folderHierarchy)
        {
            var blobClient = await GetBlobClient(name, folderHierarchy);
            return await blobClient.ExistsAsync();
        }

        public async Task<Stream> GetFile(string name, params string[] folderHierarchy)
        {
            var blobClient = await GetBlobClient(name, folderHierarchy);
            if (await blobClient.ExistsAsync() == false)
                throw new FileNotFoundException();

            BlobDownloadInfo download = await blobClient.DownloadAsync();
            return download.Content;
        }

        public async Task<Stream> GetFileAsync(string file)
        {
            if (IsMine(file) == false)
                throw new ArgumentException($"{file} is not a valid path");

            var blobClient = await GetBlobClient(file);
            if (await blobClient.ExistsAsync() == false)
                throw new FileNotFoundException();

            BlobDownloadInfo download = await blobClient.DownloadAsync();
            return download.Content;
        }

        public async Task<string> GetFileUrl(string name, params string[] folderHierarchy)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(ConnectionString);

            BlobContainerClient merchantContainerClient = blobServiceClient.GetBlobContainerClient(VirtualDirectory);
            await merchantContainerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);
            if (await merchantContainerClient.ExistsAsync() == false)
                throw new FileNotFoundException();

            var fullName = string.Join("/", (folderHierarchy ?? Enumerable.Empty<string>()).Append(name));
            var blobClient = merchantContainerClient.GetBlobsAsync(prefix: fullName);

            var enumerator = blobClient.AsPages().GetAsyncEnumerator();
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    var result = enumerator.Current.Values.SingleOrDefault();
                    if (result == null)
                        throw new FileNotFoundException();
                    return merchantContainerClient.Uri.AbsoluteUri.CombineUrl(result.Name);
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }
            throw new FileNotFoundException();
        }

        public async Task<IEnumerable<string>> GetFileUrls(params string[] folderHierarchy)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(ConnectionString);

            BlobContainerClient merchantContainerClient = blobServiceClient.GetBlobContainerClient(VirtualDirectory);
            await merchantContainerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);
            if (await merchantContainerClient.ExistsAsync() == false)
                return Enumerable.Empty<string>();

            var fullName = string.Join("/", (folderHierarchy ?? Enumerable.Empty<string>()));
            var blobClient = merchantContainerClient.GetBlobsAsync(prefix: fullName);

            var enumerator = blobClient.AsPages().GetAsyncEnumerator();
            var result = new List<(string, DateTimeOffset?)>();
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    var page = enumerator.Current;
                    result.AddRange(page.Values.Select(v => (merchantContainerClient.Uri.AbsoluteUri.CombineUrl(v.Name), v.Properties.CreatedOn)));
                }
                return result.OrderBy(r => r.Item2 ?? DateTimeOffset.MinValue).Select(r => r.Item1).ToList();
            }
            finally
            {
                await enumerator.DisposeAsync();
            }
        }

        public bool IsMine(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                return false;

            BlobServiceClient blobServiceClient = new BlobServiceClient(ConnectionString);
            var baseUri = blobServiceClient.Uri.AbsoluteUri;
            return file.StartsWith(baseUri);
        }

        public async Task<string> SaveFile(Stream file, string name, params string[] folderHierarchy)
        {
            var blobClient = await GetBlobClient(name, folderHierarchy);
            await blobClient.UploadAsync(file);
            return blobClient.Uri.AbsoluteUri.ToString();
        }

        private async Task<BlobClient> GetBlobClient(string name, params string[] folderHierarchy)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(ConnectionString);

            BlobContainerClient merchantContainerClient = blobServiceClient.GetBlobContainerClient(VirtualDirectory);
            await merchantContainerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);

            var fullName = string.Join("/", (folderHierarchy ?? []).Append(name.Replace("/", "")));
            return merchantContainerClient.GetBlobClient(fullName);
        }
    }
}
