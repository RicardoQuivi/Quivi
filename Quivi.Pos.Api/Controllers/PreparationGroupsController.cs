using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Commands.PreparationGroups;
using Quivi.Application.Queries.PreparationGroups;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Extensions;
using Quivi.Pos.Api.Dtos.Requests.PreparationGroups;
using Quivi.Pos.Api.Dtos.Responses.PreparationGroups;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreparationGroupsController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public PreparationGroupsController(IQueryProcessor queryProcessor,
                                            ICommandProcessor commandProcessor,
                                            IIdConverter idConverter,
                                            IMapper mapper)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<GetPreparationsGroupsResponse> Get([FromQuery] GetPreparationGroupsRequest request)
        {
            int? locationId = string.IsNullOrWhiteSpace(request.LocationId) ? (int?)null : idConverter.FromPublicId(request.LocationId);
            var query = await queryProcessor.Execute(new GetPreparationGroupsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                LocationIds = locationId.HasValue ? [locationId.Value] : null,
                SessionIds = request.SessionIds?.Select(idConverter.FromPublicId),
                States = request.IsCommited.HasValue ? [request.IsCommited.Value ? PreparationGroupState.Committed : PreparationGroupState.Draft] : null,
                Completed = false,
                IncludeOrders = true,
                IncludePreparationGroupItems = true,
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetPreparationsGroupsResponse
            {
                Page = query.CurrentPage,
                TotalPages = query.NumberOfPages,
                TotalItems = query.TotalItems,
                Data = mapper.Map<Dtos.PreparationGroup>(query),
            };
        }

        [HttpPut("{id}")]
        public async Task<CommitPreparationGroupResponse> Commit(string id, [FromBody] CommitPreparationGroupRequest request)
        {
            request = request ?? new CommitPreparationGroupRequest();

            var result = await commandProcessor.Execute(new AddCommitedPreparationGroupAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                PreparationGroupId = idConverter.FromPublicId(id),
                Note = request.Note,
                IsPrepared = request.IsPrepared,
                PreparationGroupItemsQuantities = request.ItemsToCommit?.ToDictionary(e => idConverter.FromPublicId(e.Key), e => e.Value),
                SourceLocationId = string.IsNullOrWhiteSpace(request.LocationId) ? (int?)null : idConverter.FromPublicId(request.LocationId),
            });

            return new CommitPreparationGroupResponse
            {
                JobId = result,
            };
        }

        [HttpPost("{id}/print")]
        public async Task<PrintPreparationGroupResponse> Print(string id, [FromBody] PrintPreparationGroupRequest request)
        {
            request = request ?? new PrintPreparationGroupRequest();

            await commandProcessor.Execute(new PrintPreparationGroupAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                PreparationGroupId = idConverter.FromPublicId(id),
                LocationId = string.IsNullOrWhiteSpace(request.LocationId) ? null : idConverter.FromPublicId(request.LocationId),
            });
            return new PrintPreparationGroupResponse();
        }

        [HttpPatch("{id}")]
        public async Task<PatchPreparationGroupResponse> Patch(string id, [FromBody] PatchPreparationGroupRequest request)
        {
            var items = request.Items.ToDictionary(s => idConverter.FromPublicId(s.Key), s => s.Value);
            var result = await commandProcessor.Execute(new UpdatePreparationGroupsAsyncCommand
            {
                Criteria = new GetPreparationGroupsCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                    States = [PreparationGroupState.Committed],
                    IncludePreparationGroupItems = true,
                },
                UpdateAction = (entity) =>
                {
                    foreach (var e in items)
                    {
                        var preparationItem = entity.Items[e.Key];
                        preparationItem.RemainingQuantity = preparationItem.OriginalQuantity - e.Value;
                    }
                    return Task.CompletedTask;
                },
            });

            return new PatchPreparationGroupResponse
            {
                Data = mapper.Map<Dtos.PreparationGroup>(result.SingleOrDefault()),
            };
        }
    }
}