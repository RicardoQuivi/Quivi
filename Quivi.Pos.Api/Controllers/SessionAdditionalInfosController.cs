using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.OrderAdditionalInfos;
using Quivi.Application.Commands.Orders;
using Quivi.Application.Queries.OrderAdditionalInfos;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Extensions;
using Quivi.Pos.Api.Dtos.Requests.SessionAdditionalInfos;
using Quivi.Pos.Api.Dtos.Responses.SessionAdditionalInfos;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireSubMerchant]
    [Authorize]
    [ApiController]
    public class SessionAdditionalInfosController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public SessionAdditionalInfosController(IQueryProcessor queryProcessor, ICommandProcessor commandProcessor, IIdConverter idConverter, IMapper mapper)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        [HttpGet("/api/sessions/{sessionId}/additionalinfo")]
        public async Task<GetSessionAdditionalInfosResponse> Get(string sessionId)
        {
            var query = await queryProcessor.Execute(new GetOrderAdditionalInfosAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                SessionIds = [idConverter.FromPublicId(sessionId)],
                IsAutoFill = false,
                AssignedOn = [AssignedOn.PoSSessions],

                PageIndex = 0,
                PageSize = null,
            });

            return new GetSessionAdditionalInfosResponse
            {
                Data = mapper.Map<Dtos.SessionAdditionalInfo>(query),
            };
        }

        [HttpPost("/api/sessions/{sessionId}/additionalinfo")]
        public async Task<UpsertSessionAdditionalInfoResponse> Put(string sessionId, [FromBody] UpsertSessionAdditionalInfoRequest request)
        {
            var requestDictionary = request.Fields.ToDictionary(t => idConverter.FromPublicId(t.Key), t => t.Value);

            var result = new HashSet<OrderAdditionalInfo>();

            var updateResult = await commandProcessor.Execute(new UpdateOrderAdditionalInfosAsyncCommand
            {
                Criteria = new GetOrderAdditionalInfosCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    SessionIds = [idConverter.FromPublicId(sessionId)],
                    OrderConfigurableFieldIds = requestDictionary.Keys,
                    AssignedOn = [AssignedOn.PoSSessions],
                },
                UpdateAction = t =>
                {
                    requestDictionary.Remove(t.OrderConfigurableFieldId, out var value);
                    if (value != null)
                        t.Value = value;
                    return Task.CompletedTask;
                }
            });

            foreach (var e in updateResult)
                result.Add(e);

            if (requestDictionary.Count != 0)
            {
                var orders = await commandProcessor.Execute(new UpdateOrdersAsyncCommand
                {
                    Criteria = new GetOrdersCriteria
                    {
                        MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                        SessionIds = [idConverter.FromPublicId(sessionId)],
                        PageSize = 1,
                    },
                    UpdateAction = (IUpdatableOrder o) =>
                    {
                        foreach (var field in requestDictionary)
                            o.Fields.Upsert(field.Key, t => t.Value = field.Value);
                        return Task.CompletedTask;
                    },
                });

                foreach (var e in orders.SelectMany(o => o.OrderAdditionalInfos!))
                    result.Add(e);
            }

            return new UpsertSessionAdditionalInfoResponse
            {
                Data = mapper.Map<Dtos.SessionAdditionalInfo>(result),
            };
        }
    }
}