namespace Quivi.Application.Configurations
{
    public class AzureBlobStorageSettings
    {
        public required string ConnectionString { get; init; }
        public required string VirtualDirectory { get; init; }
    }
}