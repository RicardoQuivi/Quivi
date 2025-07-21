using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.PrinterNotificationMessages;
using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Pos.EscPos;
using Quivi.Infrastructure.Extensions;
using Quivi.Pos.Api.Dtos.Requests.Pos;
using Quivi.Pos.Api.Dtos.Responses.Pos;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [RequireEmployee]
    [ApiController]
    public class PosController : ControllerBase
    {
        private readonly ICommandProcessor commandProcessor;
        private readonly IIdConverter idConverter;
        private readonly IEscPosPrinterService escPosPrinterService;
        private readonly IPosSyncService posSyncService;

        public PosController(ICommandProcessor commandProcessor,
                                IIdConverter idConverter,
                                IEscPosPrinterService escPosPrinterService,
                                IPosSyncService posSyncService)
        {
            this.commandProcessor = commandProcessor;
            this.idConverter = idConverter;
            this.escPosPrinterService = escPosPrinterService;
            this.posSyncService = posSyncService;
        }

        [HttpPost("cashDrawer")]
        public async Task<OpenCashDrawerResponse> OpenCashDrawer([FromBody] OpenCashDrawerRequest request)
        {
            await commandProcessor.Execute(new CreatePrinterNotificationMessageAsyncCommand
            {
                MessageType = NotificationMessageType.OpenCashDrawer,
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetPrinterNotificationsContactsCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    MessageTypes = [NotificationMessageType.OpenCashDrawer],
                    LocationIds = string.IsNullOrWhiteSpace(request.LocationId) ? null : [idConverter.FromPublicId(request.LocationId)],
                    IsDeleted = false,

                    PageIndex = 0,
                    PageSize = null,
                },
                GetContent = () => Task.FromResult<string?>(escPosPrinterService.Get(new OpenCashDrawerParameters())),
            });
            return new OpenCashDrawerResponse
            {

            };
        }

        [HttpPost("bill")]
        public async Task<PrintConsumerBillResponse> PrintConsumerBill([FromBody] PrintConsumerBillRequest request)
        {
            await posSyncService.NewConsumerBill(idConverter.FromPublicId(request.SessionId), string.IsNullOrWhiteSpace(request.LocationId) ? null : idConverter.FromPublicId(request.LocationId));
            return new PrintConsumerBillResponse
            {

            };
        }
    }
}