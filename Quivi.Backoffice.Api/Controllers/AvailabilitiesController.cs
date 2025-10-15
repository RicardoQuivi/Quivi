using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.Availabilities;
using Quivi.Application.Commands.AvailabilityGroups;
using Quivi.Application.Queries.AvailabilityGroups;
using Quivi.Backoffice.Api.Requests.Availabilities;
using Quivi.Backoffice.Api.Responses.Availabilities;
using Quivi.Backoffice.Api.Validations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Infrastructure.Validations;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [RequireSubMerchant]
    [ApiController]
    public class AvailabilitiesController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public AvailabilitiesController(IIdConverter idConverter,
                                        ICommandProcessor commandProcessor,
                                        IQueryProcessor queryProcessor,
                                        IMapper mapper)
        {
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<GetAvailabilitiesResponse> Get([FromQuery] GetAvailabilitiesRequest request)
        {
            request ??= new();

            var availabilitiesQuery = await queryProcessor.Execute(new GetAvailabilityGroupsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),

                IncludeWeeklyAvailabilities = true,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetAvailabilitiesResponse
            {
                Data = mapper.Map<Dtos.Availability>(availabilitiesQuery),
                Page = availabilitiesQuery.CurrentPage,
                TotalItems = availabilitiesQuery.TotalItems,
                TotalPages = availabilitiesQuery.NumberOfPages,
            };
        }

        [HttpPost]
        public async Task<CreateAvailabilityResponse> Create([FromBody] CreateAvailabilityRequest request)
        {
            using var validator = new ModelStateValidator<CreateAvailabilityRequest, ValidationError>(request);
            var result = await commandProcessor.Execute(new AddAvailabilityGroupAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                OnInvalidName = () => validator.AddError(m => m.Name, ValidationError.InvalidValue),
                OnCreate = group =>
                {
                    group.Name = request.Name;
                    group.AutoAddNewMenuItems = request.AutoAddNewMenuItems;
                    group.AutoAddNewChannelProfiles = request.AutoAddNewChannelProfiles;

                    if (request.WeeklyAvailabilities != null)
                        foreach (var entry in request.WeeklyAvailabilities)
                            group.WeekdayAvailabilities.AddAvailability(entry.StartAt, entry.EndAt);

                    return Task.CompletedTask;
                }
            });
            if (result == null)
                throw validator.Exception;

            return new CreateAvailabilityResponse
            {
                Data = mapper.Map<Dtos.Availability>(result),
            };
        }

        [HttpPatch("{id}")]
        public async Task<PatchAvailabilityResponse> Patch(string id, [FromBody] PatchAvailabilityRequest request)
        {
            using var validator = new ModelStateValidator<PatchAvailabilityRequest, ValidationError>(request);
            var result = await commandProcessor.Execute(new UpdateAvailabilityGroupsAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetAvailabilityGroupsCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                },
                UpdateAction = group =>
                {
                    if (string.IsNullOrWhiteSpace(request.Name) == false)
                        group.Name = request.Name;

                    if (request.AutoAddNewMenuItems.HasValue)
                        group.AutoAddNewMenuItems = request.AutoAddNewMenuItems.Value;

                    if (request.AutoAddNewChannelProfiles.HasValue)
                        group.AutoAddNewChannelProfiles = request.AutoAddNewChannelProfiles.Value;

                    if (request.WeeklyAvailabilities != null)
                    {
                        TimeSpan previous = TimeSpan.Zero;
                        foreach (var entry in request.WeeklyAvailabilities)
                        {
                            if (entry.StartAt != previous)
                                group.WeekdayAvailabilities.RemoveAvailability(previous, entry.StartAt);

                            group.WeekdayAvailabilities.AddAvailability(entry.StartAt, entry.EndAt);
                            previous = entry.EndAt;
                        }

                        if (previous != TimeSpan.FromDays(7))
                            group.WeekdayAvailabilities.RemoveAvailability(previous, TimeSpan.FromDays(7));
                    }

                    return Task.CompletedTask;
                },
                OnInvalidName = local => validator.AddError(m => m.Name, ValidationError.InvalidValue),
            });

            return new PatchAvailabilityResponse
            {
                Data = mapper.Map<Dtos.Availability?>(result.SingleOrDefault()),
            };
        }

        [HttpDelete("{id}")]
        public async Task<DeleteAvailabilityResponse> Delete(string id)
        {
            var result = await commandProcessor.Execute(new DeleteAvailabilityGroupsAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetAvailabilityGroupsCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                },
            });

            return new DeleteAvailabilityResponse
            {
            };
        }
    }
}