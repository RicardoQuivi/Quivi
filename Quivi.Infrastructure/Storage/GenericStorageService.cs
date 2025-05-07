using Quivi.Infrastructure.Abstractions.Images;
using Quivi.Infrastructure.Abstractions.Storage;

namespace Quivi.Infrastructure.Storage
{
    public class GenericStorageService : IStorageService
    {
        private readonly IImageProcessor imageProcessor;
        private readonly IEnumerable<IFileStorage> fileStorages;
        private readonly IFileStorage defaultFileStorage;

        public GenericStorageService(IEnumerable<IFileStorage> fileStorages,
                                        IFileStorage defaultStorage,
                                        IImageProcessor imageProcessor)
        {
            this.fileStorages = fileStorages;
            this.defaultFileStorage = defaultStorage;
            this.imageProcessor = imageProcessor;
        }

        public Task<Stream> GetFileAsync(string fileNameAndExtention) => fileStorages.First(s => s.IsMine(fileNameAndExtention) == true).GetFileAsync(fileNameAndExtention);
        public async Task<Stream> GetFile(string name, params string[] folderHierarchy)
        {
            try
            {
                return await defaultFileStorage.GetFile(name, folderHierarchy);
            }
            catch (FileNotFoundException)
            {
                foreach (var storage in fileStorages)
                {
                    if (storage == defaultFileStorage)
                        continue;

                    try
                    {
                        return await storage.GetFile(name, folderHierarchy);
                    }
                    catch (FileNotFoundException)
                    {
                        continue;
                    }
                }
                throw new FileNotFoundException();
            }
        }

        public async Task<string> GetFileUrl(string name, params string[] folderHierarchy)
        {
            try
            {
                return await defaultFileStorage.GetFileUrl(name, folderHierarchy);
            }
            catch (FileNotFoundException)
            {
                foreach (var storage in fileStorages)
                {
                    if (storage == defaultFileStorage)
                        continue;

                    try
                    {
                        return await storage.GetFileUrl(name, folderHierarchy);
                    }
                    catch (FileNotFoundException)
                    {
                        continue;
                    }
                }
                throw new FileNotFoundException();
            }
        }

        public async Task<IEnumerable<string>> GetFileUrls(params string[] folderHierarchy)
        {
            try
            {
                return await defaultFileStorage.GetFileUrls(folderHierarchy);
            }
            catch (FileNotFoundException)
            {
                foreach (var storage in fileStorages)
                {
                    if (storage == defaultFileStorage)
                        continue;

                    try
                    {
                        return await storage.GetFileUrls(folderHierarchy);
                    }
                    catch (FileNotFoundException)
                    {
                        continue;
                    }
                }
                throw new FileNotFoundException();
            }
        }

        public Task<string> SaveFile(Stream file, string name, params string[] folderHierarchy) => defaultFileStorage.SaveFile(file, name, folderHierarchy);
        public async Task<string> SaveImage(Stream file, string filename)
        {
            var extension = Path.GetExtension(filename);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

            var guid = Guid.NewGuid().ToString();
            await SaveFile(file, filename, guid);

            await ResizeAndSave(file, nameWithoutExtension, extension, guid, ImageSize.Icon);
            await ResizeAndSave(file, nameWithoutExtension, extension, guid, ImageSize.Thumbnail);
            return await ResizeAndSave(file, nameWithoutExtension, extension, guid, ImageSize.Full);
        }

        private async Task<string> ResizeAndSave(Stream file, string fileNameWithoutExtension, string extension, string guid, ImageSize size)
        {
            var iconStream = ResizeImage(file, size);
            return await SaveFile(iconStream, $"{fileNameWithoutExtension}.{size}{extension}", guid);
        }

        private Stream ResizeImage(Stream file, ImageSize imageSize)
        {
            int actualSize = GetSize(imageSize);
            var result = imageProcessor.Compress(file, actualSize);
            return result;
        }

        private int GetSize(ImageSize imageSize)
        {
            switch (imageSize)
            {
                case ImageSize.Icon: return 60;
                case ImageSize.Thumbnail: return 300;
                case ImageSize.Full: return 800;
            }
            throw new NotImplementedException();
        }
    }
}
