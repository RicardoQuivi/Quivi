using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Exceptions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.DocumentTypes;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Series;

namespace Quivi.Infrastructure.Pos.Facturalusa.Commands
{
    public class GetOrCreateSerieAsyncCommand : AFacturalusaAsyncCommand<ReadonlySerie>
    {
        public GetOrCreateSerieAsyncCommand(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }

        private string _serieName = string.Empty;
        public required string SerieName 
        {
            get => _serieName;
            set => _serieName = value.Trim();
        }

        public int ExpirationYear { get; set; }

        /// <summary>
        /// Communicates Serie to AT if Serie is new.
        /// </summary>
        public bool CommunicateIfNew { get; set; }
    }

    public class GetOrCreateSerieAsyncCommandHandler : ICommandHandler<GetOrCreateSerieAsyncCommand, Task<ReadonlySerie>>
    {
        private readonly IFacturalusaCacheProvider cacheProvider;
        private readonly ICommandProcessor commandProcessor;

        public GetOrCreateSerieAsyncCommandHandler(IFacturalusaCacheProvider cacheProvider, ICommandProcessor commandProcessor)
        {
            this.cacheProvider = cacheProvider;
            this.commandProcessor = commandProcessor;
        }

        public async Task<ReadonlySerie> Handle(GetOrCreateSerieAsyncCommand command)
        {
            return await cacheProvider.GetOrCreateSerie
            (
                command.FacturalusaService.AccountUuid,
                command.SerieName,
                () => EntityFactory(command),
                TimeSpan.FromDays(7)
            );
        }

        private async Task<ReadonlySerie> EntityFactory(GetOrCreateSerieAsyncCommand command)
        {
            var serie = await GetOrCreateSerieViaApi(command);

            if (command.CommunicateIfNew)
                await CommunicateSerie(command, serie.Id);

            return serie;
        }

        private async Task<ReadonlySerie> GetOrCreateSerieViaApi(GetOrCreateSerieAsyncCommand command)
        {
            try
            {
                // Try to create Serie
                var response = await command.FacturalusaService.CreateSerie(new CreateSerieRequest
                {
                    Name = command.SerieName,
                    ExpirationYear = command.ExpirationYear,
                    AssignToAllDocumentTypes = true,
                    IsActive = true,
                });
                return response.Data;
            }
            catch (FacturalusaApiException ex) when (new[] { Models.ErrorType.FieldValueAlreadyExists, Models.ErrorType.SerieNameAlreadyExists }.Contains(ex.ErrorType))
            {
                // Get existing Serie
                var existingResponse = await command.FacturalusaService.GetSeries(new GetSeriesRequest
                {
                    Name = command.SerieName,
                });
                return existingResponse.Data.First(s => string.Equals(s.Name, command.SerieName, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        private async Task CommunicateSerie(GetOrCreateSerieAsyncCommand command, long serieId)
        {
            await commandProcessor.Execute(new EnsureSerieCommunicatedAsyncCommand(command.FacturalusaService)
            {
                SerieId = serieId,
                DocumentType = DocumentType.InvoiceReceipt, // Validate allways with the same doc type because the series are allways assigned to all types
            });
        }
    }
}