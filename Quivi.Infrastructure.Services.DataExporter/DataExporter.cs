using MiniExcelLibs;
using Quivi.Infrastructure.Abstractions.Services.Exporters;
using System.Data;
using System.Text;

namespace Quivi.Infrastructure.Services.DataExporter
{
    public class DataExporter : IDataExporter
    {
        public IExporterBuilder<T> Create<T>(IEnumerable<T> model) => new ExporterBuilder<T>(model);

        public string GetExportName(ExportType type, string name)
        {
            switch (type)
            {
                case ExportType.Excel: return $"{name}.xlsx";
                case ExportType.Csv: return $"{name}.csv";
            }
            throw new NotImplementedException();
        }

        private class ExporterBuilder<T> : IExporterBuilder<T>
        {
            private readonly IEnumerable<T> model;

            private readonly List<(string Header, Func<T, string> ValueGetter)> exportSettings = [];

            public ExporterBuilder(IEnumerable<T> model)
            {
                this.model = model;
            }

            public IExporterBuilder<T> AddColumn<Y>(string name, Func<T, Y> getter)
            {
                exportSettings.Add((name, (T m) => getter(m)?.ToString() ?? string.Empty));
                return this;
            }

            public async Task<Stream> ExportAsync(ExportType type)
            {
                switch (type)
                {
                    case ExportType.Excel: return await ExportToExcelAsync();
                    case ExportType.Csv: return ExportToCsvAsync();
                }
                throw new NotImplementedException();
            }

            private async Task<Stream> ExportToExcelAsync()
            {
                var table = new DataTable();

                foreach (var (name, _) in exportSettings)
                    table.Columns.Add(name, typeof(string));

                foreach (var item in model)
                {
                    var dataRow = exportSettings.Select((i) => i.ValueGetter(item)).ToArray();
                    table.Rows.Add(dataRow);
                }
                var memoryStream = new MemoryStream();
                await memoryStream.SaveAsAsync(table);
                memoryStream.Position = 0;

                return memoryStream;
            }

            private Stream ExportToCsvAsync()
            {
                var headers = string.Join(",", exportSettings.Select(e => e.Header));
                var csv = model.Select(s => string.Join(",", exportSettings.Select(e => e.ValueGetter(s)).Select(EscapeCsvValue)));
                var bytes = Encoding.UTF8.GetBytes(string.Join("\n", csv.Prepend(headers)));
                return new MemoryStream(bytes);
            }

            private string? EscapeCsvValue(string? rawValue)
            {
                if (rawValue == null)
                    return rawValue;

                if (rawValue.Contains('"'))
                    rawValue = rawValue.Replace("\"", "\"\"");

                if (rawValue.Contains(','))
                    rawValue = $"\"{rawValue}\"";

                return rawValue;
            }
        }
    }
}