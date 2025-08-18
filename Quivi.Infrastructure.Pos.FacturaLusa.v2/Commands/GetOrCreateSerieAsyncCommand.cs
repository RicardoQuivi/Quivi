using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.Series;
using FacturaLusa.v2.Exceptions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Commands
{
    public class GetOrCreateSerieAsyncCommand : AFacturaLusaAsyncCommand<Serie>
    {
        private string _serieName = string.Empty;
        public required string SerieName
        {
            get => _serieName;
            set => _serieName = value.Trim();
        }

        public int ExpirationYear { get; set; }
        public bool CommunicateIfNew { get; set; }
    }

    public class GetOrCreateSerieAsyncCommandHandler : ICommandHandler<GetOrCreateSerieAsyncCommand, Task<Serie>>
    {
        private readonly IFacturaLusaCacheProvider cacheProvider;
        private readonly ICommandProcessor commandProcessor;

        public GetOrCreateSerieAsyncCommandHandler(IFacturaLusaCacheProvider cacheProvider, ICommandProcessor commandProcessor)
        {
            this.cacheProvider = cacheProvider;
            this.commandProcessor = commandProcessor;
        }

        public Task<Serie> Handle(GetOrCreateSerieAsyncCommand command)
        {
            return cacheProvider.GetOrCreateSerie
            (
                command.Service.AccountUuid,
                command.SerieName,
                () => EntityFactory(command),
                TimeSpan.FromDays(7)
            );
        }

        private async Task<Serie> EntityFactory(GetOrCreateSerieAsyncCommand command)
        {
            var serie = await GetOrCreateSerieViaApi(command);

            if (command.CommunicateIfNew)
                await CommunicateSerie(command, serie.Id);

            return serie;
        }

        private async Task<Serie> GetOrCreateSerieViaApi(GetOrCreateSerieAsyncCommand command)
        {
            try
            {
                // Try to create Serie
                var response = await command.Service.CreateSerie(new CreateSerieRequest
                {
                    Description = command.SerieName,
                    ValidUntilYear = command.ExpirationYear,
                });
                return response;
            }
            catch (FacturaLusaException ex) when (new[] { ErrorType.FieldValueAlreadyExists, ErrorType.SerieNameAlreadyExists }.Contains(ex.ErrorType))
            {
                // Get existing Serie
                var existingResponse = await command.Service.SearchSerie(new SearchSerieRequest
                {
                    Value = command.SerieName,
                });
                return existingResponse;
            }
        }

        private Task CommunicateSerie(GetOrCreateSerieAsyncCommand command, long serieId)
        {
            return commandProcessor.Execute(new EnsureSerieCommunicatedAsyncCommand
            {
                Service = command.Service,
                SerieId = serieId,
                DocumentType = DocumentType.InvoiceReceipt, // Validate allways with the same doc type because the series are allways assigned to all types
            });
        }
    }
}