using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class PreparationGroupMapperHandler : IMapperHandler<PreparationGroup, Dtos.PreparationGroup>
    {
        private readonly IIdConverter _idConverter;
        private readonly IMapper _mapper;

        public PreparationGroupMapperHandler(IIdConverter idConverter, IMapper mapper)
        {
            _idConverter = idConverter;
            _mapper = mapper;
        }

        public Dtos.PreparationGroup Map(PreparationGroup model)
        {
            var items = _mapper.Map<Dtos.PreparationGroupItem>(model.PreparationGroupItems!.Where(p => p.ParentPreparationGroupItemId.HasValue == false));

            if (model.State != PreparationGroupState.Committed)
                items = items.Select(s => s with
                {
                    Extras = s.Extras.Where(r => r.RemainingQuantity != 0),
                }).Where(p => p.RemainingQuantity != 0 || p.Extras.Any(e => e.RemainingQuantity != 0));

            return new Dtos.PreparationGroup
            {
                Id = _idConverter.ToPublicId(model.Id),
                SessionId = _idConverter.ToPublicId(model.SessionId),
                OrderIds = model.Orders?.Select(s => _idConverter.ToPublicId(s.Id)) ?? [],
                Items = items,
                IsCommited = model.State == PreparationGroupState.Committed,
                Note = string.IsNullOrWhiteSpace(model.AdditionalNote) ? null : model.AdditionalNote,
                CreatedDate = new DateTimeOffset(model.CreatedDate, TimeSpan.Zero),
                LastModified = new DateTimeOffset(model.ModifiedDate, TimeSpan.Zero),
            };
        }
    }
}