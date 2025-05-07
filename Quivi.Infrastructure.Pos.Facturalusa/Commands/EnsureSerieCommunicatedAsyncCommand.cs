using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Exceptions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.DocumentTypes;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Series;

namespace Quivi.Infrastructure.Pos.Facturalusa.Commands
{
    public class EnsureSerieCommunicatedAsyncCommand : AFacturalusaAsyncCommand
    {
        public EnsureSerieCommunicatedAsyncCommand(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }

        public long SerieId { get; set; }
        public DocumentType DocumentType { get; set; }
    }

    public class EnsureSerieCommunicatedAsyncCommandHandler : ICommandHandler<EnsureSerieCommunicatedAsyncCommand, Task>
    {
        private readonly IFacturalusaCacheProvider _cacheProvider;

        public EnsureSerieCommunicatedAsyncCommandHandler(IFacturalusaCacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        public async Task Handle(EnsureSerieCommunicatedAsyncCommand command)
        {
            // Get in cache
            var cacheResult = await _cacheProvider.GetIsCommunicatedSerie(command.FacturalusaService.AccountUuid, command.SerieId, command.DocumentType);
            if (cacheResult.Exists)
                return;

            var isCommunicated = await ValidateSerieIsCommunicatedViaApi(command);
            if (isCommunicated)
                await _cacheProvider.CreateIsCommunicatedSerie(command.FacturalusaService.AccountUuid, command.SerieId, command.DocumentType, isCommunicated, TimeSpan.FromDays(7));
            else
                await _cacheProvider.CreateIsCommunicatedSerie(command.FacturalusaService.AccountUuid, command.SerieId, command.DocumentType, isCommunicated, TimeSpan.FromMinutes(5));
        }

        private async Task<bool> ValidateSerieIsCommunicatedViaApi(EnsureSerieCommunicatedAsyncCommand command)
        {
            try
            {
                bool isAlreadyCommunicated = await CheckSerieCommunication(command);
                if (isAlreadyCommunicated)
                    return true;

                await command.FacturalusaService.CommunicateSerie(command.SerieId);
                return true;
            }
            catch (FacturalusaApiException ex) when (ex.ErrorType == Models.ErrorType.MissingATConfigurations)
            {
                throw;
            }
            catch (FacturalusaApiException ex) when (ex.ErrorType == Models.ErrorType.SerieAlreadyCommunicated)
            {
                return true;
            }
        }

        private async Task<bool> CheckSerieCommunication(EnsureSerieCommunicatedAsyncCommand command)
        {
            try
            {
                var alreadyCommunicatedResponse = await command.FacturalusaService.CheckCommunicateSerie(command.SerieId, new CheckCommunicateSerieRequest
                {
                    DocumentType = command.DocumentType,
                });
                return alreadyCommunicatedResponse.AlreadyCommunicated;
            }
            catch (FacturalusaApiException ex) when (ex.ErrorType == Models.ErrorType.MissingSerieCommunicationForDocumentType)
            {
                return false;
            }
        }
    }
}