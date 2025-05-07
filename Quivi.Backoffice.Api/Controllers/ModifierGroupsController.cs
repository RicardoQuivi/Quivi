using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.ItemsModifierGroups;
using Quivi.Application.Queries.ItemsModifierGroups;
using Quivi.Backoffice.Api.Requests.ModifierGroups;
using Quivi.Backoffice.Api.Responses.ModifierGroups;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    [RequireSubMerchant]
    public class ModifierGroupsController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public ModifierGroupsController(IQueryProcessor queryProcessor,
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
        public async Task<GetModifierGroupsResponse> Get([FromQuery] GetModifierGroupsRequest request)
        {
            request ??= new();
            var query = await queryProcessor.Execute(new GetItemsModifierGroupsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),

                IncludeTranslations = true,
                IncludeModifiers = true,
                IsDeleted = false,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetModifierGroupsResponse
            {
                Data = mapper.Map<Dtos.ModifierGroup>(query),
                Page = query.CurrentPage,
                TotalPages = query.NumberOfPages,
                TotalItems = query.TotalItems,
            };
        }

        [HttpPost]
        public async Task<CreateModifierGroupResponse> Create([FromBody] CreateModifierGroupRequest request)
        {
            var result = await commandProcessor.Execute(new AddItemsModifierGroupAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                Name = request.Name,
                MinSelection = request.MinSelection,
                MaxSelection = request.MaxSelection,
                Items = request.Items.ToDictionary(s => idConverter.FromPublicId(s.Key), s => new AddModifierItem
                { 
                    Price = s.Value.Price,
                    SortIndex = s.Value.SortIndex,
                }),
                Translations = request.Translations?.ToDictionary(t => t.Key, t => new AddItemsModifierGroupTranslation
                {
                    Name = t.Value.Name,
                }),
            });

            return new CreateModifierGroupResponse
            {
                Data = mapper.Map<Dtos.ModifierGroup>(result),
            };
        }

        [HttpPatch("{id}")]
        public async Task<PatchModifierGroupResponse> Patch(string id, [FromBody] PatchModifierGroupRequest request)
        {
            var result = await commandProcessor.Execute(new UpdateItemsModifierGroupAsyncCommand
            {
                Criteria = new GetItemsModifierGroupsCriteria
                {
                    Ids = [idConverter.FromPublicId(id)],
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],

                    IncludeTranslations = true,
                    IncludeModifiers = true,

                    IsDeleted = false,
                },
                UpdateAction = item =>
                {
                    if (string.IsNullOrWhiteSpace(request.Name) == false)
                        item.Name = request.Name;

                    if (request.MinSelection.HasValue)
                        item.MinSelection = request.MinSelection.Value;

                    if (request.MaxSelection.HasValue)
                        item.MaxSelection = request.MaxSelection.Value;

                    if (request.Translations != null)
                    {
                        foreach (var e in request.Translations)
                        {
                            if (string.IsNullOrWhiteSpace(e.Value?.Name))
                            {
                                item.Translations.Remove(e.Key);
                                continue;
                            }

                            item.Translations.Upsert(e.Key, translation =>
                            {
                                translation.Name = e.Value.Name;
                            });
                        }
                    }

                    if (request.Items != null)
                    {
                        IDictionary<int, PatchModifierItem> items = request.Items.ToDictionary(s => idConverter.FromPublicId(s.Key), s => s.Value);
                        foreach (var c in item.MenuItems.ToList())
                        {
                            if (items.ContainsKey(c.Id))
                                continue;

                            item.MenuItems.Remove(c.Id);
                        }

                        foreach (var (id, upsertingItem) in items)
                        {
                            item.MenuItems.Upsert(id, t =>
                            {
                                if (upsertingItem.Price.HasValue)
                                    t.Price = upsertingItem.Price.Value;

                                if (upsertingItem.SortIndex.HasValue)
                                    t.SortIndex = upsertingItem.SortIndex.Value;
                            });
                        }
                    }

                    return Task.CompletedTask;
                },
            });

            return new PatchModifierGroupResponse
            {
                Data = mapper.Map<Dtos.ModifierGroup?>(result.SingleOrDefault()),
            };
        }

        [HttpDelete("{id}")]
        public async Task<DeleteModifierGroupResponse> Delete(string id)
        {
            var result = await commandProcessor.Execute(new DeleteItemsModifierGroupAsyncCommand
            {
                Criteria = new GetItemsModifierGroupsCriteria
                {
                    Ids = [idConverter.FromPublicId(id)],
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    IsDeleted = false,
                },
            });
            return new DeleteModifierGroupResponse();
        }
    }
}