using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class PreparationGroupItemMapperHandler : IMapperHandler<PreparationGroupItem, Dtos.PreparationGroupItem>
    {
        private readonly IIdConverter _idConverter;

        public PreparationGroupItemMapperHandler(IIdConverter idConverter)
        {
            _idConverter = idConverter;
        }

        public Dtos.PreparationGroupItem Map(PreparationGroupItem model)
        {
            var parentLocationId = model.LocationId.HasValue ? _idConverter.ToPublicId(model.LocationId.Value) : null;
            var result = new Dtos.PreparationGroupItem
            {
                Id = _idConverter.ToPublicId(model.Id),
                MenuItemId = _idConverter.ToPublicId(model.MenuItemId),
                Quantity = model.OriginalQuantity,
                RemainingQuantity = model.RemainingQuantity,
                LocationId = parentLocationId,
                Extras = model.Extras?.Select(s => new Dtos.BasePreparationGroupItem
                {
                    Id = _idConverter.ToPublicId(s.Id),
                    MenuItemId = _idConverter.ToPublicId(s.MenuItemId),
                    Quantity = s.OriginalQuantity,
                    LocationId = s.LocationId.HasValue ? _idConverter.ToPublicId(s.LocationId.Value) : parentLocationId,
                    RemainingQuantity = s.RemainingQuantity,
                }) ?? [],
            };
            return result;
        }
    }
}
