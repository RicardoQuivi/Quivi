using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.MenuItems;
using Quivi.Application.Queries.MenuItems;
using Quivi.Backoffice.Api.Requests.MenuItems;
using Quivi.Backoffice.Api.Responses.MenuItems;
using Quivi.Backoffice.Api.Validations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Extensions;
using Quivi.Infrastructure.Validations;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [RequireSubMerchant]
    public class MenuItemsController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public MenuItemsController(IQueryProcessor queryProcessor,
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
        public async Task<GetMenuItemsResponse> Get([FromQuery] GetMenuItemsRequest request)
        {
            request ??= new GetMenuItemsRequest();

            var query = await queryProcessor.Execute(new GetMenuItemsAsyncQuery
            {
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                ItemCategoryIds = request.ItemCategoryId == null ? null : [idConverter.FromPublicId(request.ItemCategoryId)],
                HasCategory = request.HasCategory,
                Search = request.Search,

                IncludeTranslations = true,
                IncludeMenuItemCategoryAssociations = true,
                IncludeModifierGroupsAssociations = true,

                IsDeleted = false,
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetMenuItemsResponse
            {
                Data = mapper.Map<Dtos.MenuItem>(query),
                Page = query.CurrentPage,
                TotalPages = query.NumberOfPages,
                TotalItems = query.TotalItems,
            };
        }

        [HttpPost]
        public async Task<CreateMenuItemResponse> Create([FromBody] CreateMenuItemRequest request)
        {
            var result = await commandProcessor.Execute(new AddMenuItemAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                Name = request.Name,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                Price = request.Price,
                PriceType = request.PriceType,
                VatRate = request.VatRate,
                Translations = request.Translations?.ToDictionary(t => t.Key, t => new AddMenuItemTranslation
                {
                    Name = t.Value.Name,
                    Description = t.Value.Description,
                }),
                MenuItemCategoryIds = request.MenuCategoryIds?.Select(idConverter.FromPublicId) ?? [],
                ModifierGroupIds = request.ModifierGroupIds?.Select(idConverter.FromPublicId) ?? [],
                LocationId = string.IsNullOrWhiteSpace(request.LocationId) ? null : idConverter.FromPublicId(request.LocationId),
            });

            return new CreateMenuItemResponse
            {
                Data = mapper.Map<Dtos.MenuItem>(result),
            };
        }

        [HttpPatch("{id}")]
        public async Task<PatchMenuItemResponse> Patch(string id, [FromBody] PatchMenuItemRequest request)
        {
            var result = await commandProcessor.Execute(new UpdateMenuItemsAsyncCommand
            {
                Criteria = new GetMenuItemsCriteria
                {
                    Ids = [idConverter.FromPublicId(id)],
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    IsDeleted = false,
                },
                UpdateAction = item =>
                {
                    if (string.IsNullOrWhiteSpace(request.Name) == false)
                        item.Name = request.Name;

                    if (request.Description.IsSet)
                        item.Description = request.Description;

                    if (request.Price.HasValue)
                        item.Price = request.Price.Value;

                    if (request.PriceType.HasValue)
                        item.PriceType = request.PriceType.Value;

                    if (request.VatRate.HasValue)
                        item.VatRate = request.VatRate.Value;

                    if (request.LocationId.IsSet)
                        item.LocationId = request.LocationId.Value == null ? null : idConverter.FromPublicId(request.LocationId.Value);

                    if (request.ImageUrl.IsSet)
                        item.ImageUrl = request.ImageUrl;

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

                                if (e.Value.Description.IsSet)
                                    translation.Description = e.Value.Description.Value;
                            });
                        }
                    }

                    if (request.MenuCategoryIds != null)
                    {
                        HashSet<int> categories = request.MenuCategoryIds.Select(idConverter.FromPublicId).ToHashSet();
                        foreach (var c in item.Categories.ToList())
                        {
                            if (categories.Contains(c.Id))
                            {
                                categories.Remove(c.Id);
                                continue;
                            }

                            item.Categories.Remove(c.Id);
                        }

                        foreach (var newCategory in categories)
                        {
                            item.Categories.Upsert(newCategory, t =>
                            {
                                //Nothing to update
                            });
                        }
                    }

                    if (request.ModifierGroupIds != null)
                    {
                        HashSet<int> modifierGroups = request.ModifierGroupIds.Select(idConverter.FromPublicId).ToHashSet();
                        foreach (var c in item.ModifierGroups.ToList())
                        {
                            if (modifierGroups.Contains(c.Id))
                            {
                                modifierGroups.Remove(c.Id);
                                continue;
                            }

                            item.ModifierGroups.Remove(c.Id);
                        }

                        foreach (var newModifierGroup in modifierGroups)
                        {
                            item.ModifierGroups.Upsert(newModifierGroup, t =>
                            {
                                //Nothing to update
                            });
                        }
                    }

                    return Task.CompletedTask;
                },
            });

            return new PatchMenuItemResponse
            {
                Data = mapper.Map<Dtos.MenuItem?>(result.SingleOrDefault()),
            };
        }

        [HttpDelete("{id}")]
        public async Task<DeleteMenuItemResponse> Delete(string id)
        {
            using var validator = new ModelStateValidator<string, ValidationError>(id);
            await commandProcessor.Execute(new DeleteMenuItemsAsyncCommand
            {
                Criteria = new GetMenuItemsCriteria
                {
                    Ids = [idConverter.FromPublicId(id)],
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    IsDeleted = false,
                },
                OnItemsAssociatedWithModifiersError = (e) => validator.AddError(id => id, ValidationError.UnableToDeleteDueToAssociatedEntities),
            });

            return new DeleteMenuItemResponse();
        }
    }
}