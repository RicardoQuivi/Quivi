using Quivi.Infrastructure.Abstractions.Configurations;

namespace Quivi.Infrastructure.Configurations
{
    public class FileSystemStorageSettings : IFileSystemStorageSettings
    {
        public required string Path { get; set; }
    }
}
