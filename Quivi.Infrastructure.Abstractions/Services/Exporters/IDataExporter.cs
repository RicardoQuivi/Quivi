namespace Quivi.Infrastructure.Abstractions.Services.Exporters
{
    public enum ExportType
    {
        Excel,
        Csv,
    }

    public interface IDataExporter
    {
        IExporterBuilder<T> Create<T>(IEnumerable<T> model);
        string GetExportName(ExportType type, string name);
    }

    public interface IExporterBuilder<T>
    {
        IExporterBuilder<T> AddColumn<Y>(string name, Func<T, Y> getter);
        Task<Stream> ExportAsync(ExportType type);
    }
}