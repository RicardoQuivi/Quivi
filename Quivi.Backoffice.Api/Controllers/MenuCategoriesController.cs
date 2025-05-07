using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.ItemCategories;
using Quivi.Application.Queries.ItemCategories;
using Quivi.Backoffice.Api.Requests.MenuCategories;
using Quivi.Backoffice.Api.Responses.MenuCategories;
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
    public class MenuCategoriesController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public MenuCategoriesController(IQueryProcessor queryProcessor,
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
        public async Task<GetMenuCategoriesResponse> Get([FromQuery] GetMenuCategoriesRequest request)
        {
            request ??= new GetMenuCategoriesRequest();

            var categoriesQuery = await queryProcessor.Execute(new GetItemCategoriesAsyncQuery
            {
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                IncludeTranslations = true,
                IsDeleted = false,

                PageSize = null,
            });

            return new GetMenuCategoriesResponse
            {
                Data = mapper.Map<Dtos.MenuCategory>(categoriesQuery),
                Page = categoriesQuery.CurrentPage,
                TotalItems = categoriesQuery.TotalItems,
                TotalPages = categoriesQuery.NumberOfPages,
            };
        }

        [HttpPost]
        public async Task<CreateMenuCategoryResponse> Create([FromBody] CreateMenuCategoryRequest request)
        {
            var category = await commandProcessor.Execute(new AddItemCategoryAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                Name = request.Name,
                ImageUrl = request.ImageUrl,
                Translations = request.Translations?.ToDictionary(s => s.Key, s => s.Value.Name),
            });

            return new CreateMenuCategoryResponse
            {
                Data = mapper.Map<Dtos.MenuCategory>(category),
            };
        }

        [HttpPatch("{id}")]
        public async Task<PatchMenuCategoryResponse> Patch(string id, [FromBody] PatchMenuCategoryRequest request)
        {
            var categories = await commandProcessor.Execute(new UpdateItemCategoryAsyncCommand
            {
                Criteria = new GetItemCategoriesCriteria
                {
                    Ids = [idConverter.FromPublicId(id)],
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    IncludeTranslations = true,
                    IsDeleted = false,
                },
                UpdateAction = c =>
                {
                    if(string.IsNullOrWhiteSpace(request.Name) == false)
                        c.Name = request.Name;

                    if (request.ImageUrl.IsSet)
                        c.ImageUrl = request.ImageUrl;

                    if (request.Translations != null)
                    {
                        foreach (var e in request.Translations)
                        {
                            if (string.IsNullOrWhiteSpace(e.Value?.Name))
                            {
                                c.Translations.Remove(e.Key);
                                continue;
                            }

                            c.Translations.Upsert(e.Key, translation =>
                            {
                                translation.Name = e.Value.Name;
                            });
                        }
                    }

                    return Task.CompletedTask;
                }
            });

            return new PatchMenuCategoryResponse
            {
                Data = mapper.Map<Dtos.MenuCategory>(categories.SingleOrDefault()),
            };
        }

        [HttpPut("sort")]
        public async Task<SortMenuCategoriesResponse> Put([FromBody] SortMenuCategoriesRequest request)
        {
            var sortIndexDictionary = request.Items.Select((item, index) => new
            {
                Id = idConverter.FromPublicId(item.Id),
                Index = index,
            }).ToDictionary(t => t.Id, t => t.Index);

            var categories = await commandProcessor.Execute(new UpdateItemCategoryAsyncCommand
            {
                Criteria = new GetItemCategoriesCriteria
                {
                    Ids = sortIndexDictionary.Keys,
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    IncludeTranslations = true,
                    IsDeleted = false,
                },
                UpdateAction = c =>
                {
                    if (sortIndexDictionary.TryGetValue(c.Id, out var index) == false)
                        return Task.CompletedTask;

                    c.SortIndex = index;
                    return Task.CompletedTask;
                }
            });

            return new SortMenuCategoriesResponse
            {
                Data = mapper.Map<Dtos.MenuCategory>(categories),
            };
        }

        [HttpDelete("{id}")]
        public async Task<DeleteMenuCategoryResponse> Delete(string id)
        {
            using var validator = new ModelStateValidator<string, ValidationError>(id);
            var result = await commandProcessor.Execute(new DeleteItemCategoryAsyncCommand
            {
                Criteria = new GetItemCategoriesCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                    PageIndex = 0,
                    PageSize = 1,
                },
                OnItemsAssociatedError = (e) => validator.AddError(id => id, ValidationError.UnableToDeleteDueToAssociatedEntities),
            });
            return new DeleteMenuCategoryResponse();
        }
    }
}
