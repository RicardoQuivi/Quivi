﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.PrinterNotificationMessages;
using Quivi.Application.Queries.PrinterMessageTargets;
using Quivi.Backoffice.Api.Requests.PrinterMessages;
using Quivi.Backoffice.Api.Responses.PrinterMessages;
using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Pos.EscPos;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequireSubMerchant]
    [Authorize]
    public class PrinterMessagesController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;
        private readonly IEscPosPrinterService escPosPrinterService;
        private readonly IDateTimeProvider dateTimeProvider;

        public PrinterMessagesController(IQueryProcessor queryProcessor,
                                    ICommandProcessor commandProcessor,
                                    IMapper mapper,
                                    IIdConverter idConverter,
                                    IEscPosPrinterService escPosPrinterService,
                                    IDateTimeProvider dateTimeProvider)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.mapper = mapper;
            this.idConverter = idConverter;
            this.escPosPrinterService = escPosPrinterService;
            this.dateTimeProvider = dateTimeProvider;
        }

        [HttpGet]
        public async Task<GetPrinterMessagesResponse> Get([FromQuery] GetPrinterMessagesRequest request)
        {
            var result = await queryProcessor.Execute(new GetPrinterMessageTargetsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                PrinterNotificationsContactIds = [idConverter.FromPublicId(request.PrinterId)],
                IncludePrinterNotificationMessage = true,
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetPrinterMessagesResponse
            {
                Data = mapper.Map<Dtos.PrinterMessage>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }

        [HttpPost]
        public async Task<CreatePrinterMessageResponse> TestPrinter(CreatePrinterMessageRequest request)
        {
            var document = escPosPrinterService.Get(new TestPrinterParameters
            {
                Title = request.Text,
                Message = "This is a test perfomed via Quivi Backoffice",
                PingOnly = request.PingOnly,
                Timestamp = request.Timestamp ?? dateTimeProvider.GetUtcNow(),
            });

            var result = await commandProcessor.Execute(new CreatePrinterNotificationMessageAsyncCommand
            {
                MessageType = NotificationMessageType.Test,
                GetContent = () => Task.FromResult<string?>(escPosPrinterService.Get(new TestPrinterParameters
                {
                    Title = request.Text,
                    Message = "This is a test perfomed via Quivi Backoffice",
                    PingOnly = request.PingOnly,
                    Timestamp = request.Timestamp ?? dateTimeProvider.GetUtcNow(),
                })),
                Criteria = new GetPrinterNotificationsContactsCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(request.PrinterId)],

                    IsDeleted = false,

                    PageIndex = 0,
                    PageSize = null,
                },
            });

            return new CreatePrinterMessageResponse
            {
                Data = mapper.Map<Dtos.PrinterMessage>(result.SingleOrDefault()?.PrinterMessageTargets!.SingleOrDefault()),
            };
        }
    }
}