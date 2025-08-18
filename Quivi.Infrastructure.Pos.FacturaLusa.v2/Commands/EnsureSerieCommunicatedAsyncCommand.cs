using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.Series;
using FacturaLusa.v2.Exceptions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Commands
{
    public class EnsureSerieCommunicatedAsyncCommand : AFacturaLusaAsyncCommand
    {
        public long SerieId { get; set; }
        public DocumentType DocumentType { get; set; }
    }

    public class EnsureSerieCommunicatedAsyncCommandHandler : ICommandHandler<EnsureSerieCommunicatedAsyncCommand, Task>
    {
        private readonly IFacturaLusaCacheProvider cacheProvider;

        public EnsureSerieCommunicatedAsyncCommandHandler(IFacturaLusaCacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public async Task Handle(EnsureSerieCommunicatedAsyncCommand command)
        {
            // Get in cache
            var cacheResult = await cacheProvider.GetIsCommunicatedSerie(command.Service.AccountUuid, command.SerieId, command.DocumentType);
            if (cacheResult.Exists)
                return;

            var isCommunicated = await ValidateSerieIsCommunicatedViaApi(command);
            if (isCommunicated)
                await cacheProvider.CreateIsCommunicatedSerie(command.Service.AccountUuid, command.SerieId, command.DocumentType, isCommunicated, TimeSpan.FromDays(7));
            else
                await cacheProvider.CreateIsCommunicatedSerie(command.Service.AccountUuid, command.SerieId, command.DocumentType, isCommunicated, TimeSpan.FromMinutes(5));
        }

        private async Task<bool> ValidateSerieIsCommunicatedViaApi(EnsureSerieCommunicatedAsyncCommand command)
        {
            try
            {
                bool isAlreadyCommunicated = await CheckSerieCommunication(command);
                if (isAlreadyCommunicated)
                    return true;

                await command.Service.CommunicateSerie(new CommunicateSerieRequest
                {
                    Id = command.SerieId,
                });
                return true;
            }
            catch (FacturaLusaException ex) when (ex.ErrorType == ErrorType.MissingATConfigurations)
            {
                throw;
            }
            catch (FacturaLusaException ex) when (ex.ErrorType == ErrorType.SerieAlreadyCommunicated)
            {
                return true;
            }
        }

        private async Task<bool> CheckSerieCommunication(EnsureSerieCommunicatedAsyncCommand command)
        {
            try
            {
                var alreadyCommunicatedResponse = await command.Service.CheckSerieCommunication(new CheckSerieCommunicationRequest
                {
                    Id = command.SerieId,
                    DocumentType = command.DocumentType,
                });
                return alreadyCommunicatedResponse.Status;
            }
            catch (FacturaLusaException ex) when (ex.ErrorType == ErrorType.MissingSerieCommunicationForDocumentType)
            {
                return false;
            }
        }
    }
}