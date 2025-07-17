using Microsoft.AspNetCore.Http;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Storage;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Infrastructure.Storage
{ 
    public class FileSystemStorage : IFileStorage
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IFileSystemStorageSettings localStorageSettings;
        private readonly IAppHostsSettings appHostsSettings;

        private readonly Lazy<string> storageDirectory;

        public FileSystemStorage(IHttpContextAccessor httpContextAccessor, IAppHostsSettings appHostsSettings, IFileSystemStorageSettings localStorageSettings)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.appHostsSettings = appHostsSettings;
            this.localStorageSettings = localStorageSettings;
            storageDirectory = new Lazy<string>(() => GetStorageDirectory());
        }

        private string _endpointDownload => "/api/Storage/";

        private string GetStorageDirectory()
        {
            var storageDirectory = localStorageSettings.Path;
            if (!Directory.Exists(storageDirectory))
                Directory.CreateDirectory(storageDirectory);
            return storageDirectory;
        }

        public Task<string> SaveFile(Stream file, string name, params string[] folderHierarchy)
        {
            var path = storageDirectory.Value.CombinePath(folderHierarchy);
            Directory.CreateDirectory(path);

            var originalFullPath = Path.Combine(path, name);
            using (Stream fileStream = File.Create(originalFullPath))
                file.CopyTo(fileStream);

            var url = _endpointDownload.CombinePath(folderHierarchy.Append(name).Select(Uri.EscapeDataString));
            var request = httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return Task.FromResult(url);

            var host = $"{request.Scheme}://{request.Host}";
            return Task.FromResult(host.CombineUrl(url));
        }

        public Task<Stream> GetFileAsync(string file)
        {
            if (IsMine(file) == false)
                throw new ArgumentException($"{file} is not a valid path");
            var fullPath = Path.Combine(storageDirectory.Value, file);
            if (File.Exists(fullPath))
                return Task.FromResult(File.OpenRead(fullPath) as Stream);

            return Task.FromException<Stream>(new FileNotFoundException());
        }

        public bool IsMine(string file) => file.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) == false &&
                                            file.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase) == false;

        public Task<bool> FileExists(string name, params string[] folderHierarchy)
        {
            var merchantPath = storageDirectory.Value.CombinePath(folderHierarchy);
            var fullPath = Path.Combine(merchantPath, name);
            return Task.FromResult(File.Exists(fullPath));
        }

        public Task<Stream> GetFile(string name, params string[] folderHierarchy)
        {
            var merchantPath = storageDirectory.Value.CombinePath(folderHierarchy);
            var fullPath = Path.Combine(merchantPath, name);
            if (File.Exists(fullPath))
                return Task.FromResult(File.OpenRead(fullPath) as Stream);

            return Task.FromException<Stream>(new FileNotFoundException());
        }

        public Task<IEnumerable<string>> GetFileUrls(params string[] folderHierarchy)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetFileUrl(string name, params string[] folderHierarchy)
        {
            var fileExists = await FileExists(name, folderHierarchy);
            if (fileExists == false)
                throw new FileNotFoundException();

            return appHostsSettings.BackofficeApi.CombineUrl(folderHierarchy).CombineUrl(name);
        }
    }
}
