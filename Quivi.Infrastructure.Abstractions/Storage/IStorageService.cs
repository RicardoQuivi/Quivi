namespace Quivi.Infrastructure.Abstractions.Storage
{
    public interface IStorageService
    {
        /// <summary>
        /// Stores a file with the specified name in the specified folderHierarchy
        /// </summary>
        Task<string> GetFileUrl(string name, params string[] folderHierarchy);

        /// <summary>
        /// Stores a file with the specified name in the specified folderHierarchy
        /// </summary>
        Task<Stream> GetFile(string name, params string[] folderHierarchy);

        /// <summary>
        /// Stores a file with the specified name in the specified folderHierarchy
        /// </summary>
        Task<string> SaveFile(Stream file, string name, params string[] folderHierarchy);

        /// <summary>
        /// Returns a file that was previously stored on the server
        /// </summary>
        Task<Stream> GetFileAsync(string fileNameAndExtention);

        Task<string> SaveImage(Stream file, string filename);

        Task<IEnumerable<string>> GetFileUrls(params string[] folderHierarchy);
    }
}
