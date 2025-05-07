namespace Quivi.Infrastructure.Abstractions.Storage
{
    public interface IFileStorage
    {
        bool IsMine(string file);

        /// <summary>
        /// Stores a file with the specified name in the specified folderHierarchy
        /// </summary>
        Task<string> GetFileUrl(string name, params string[] folderHierarchy);

        /// <summary>
        /// Checks if a file with the specified name in the specified folderHierarchy exists
        /// </summary>
        Task<bool> FileExists(string name, params string[] folderHierarchy);

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

        Task<IEnumerable<string>> GetFileUrls(params string[] folderHierarchy);
    }
}
