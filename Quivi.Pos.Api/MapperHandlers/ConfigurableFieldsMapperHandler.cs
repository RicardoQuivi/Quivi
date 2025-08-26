using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class ConfigurableFieldsMapperHandler : IMapperHandler<OrderConfigurableField, Dtos.ConfigurableField>
    {
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public ConfigurableFieldsMapperHandler(IMapper mapper, IIdConverter idConverter)
        {
            this.mapper = mapper;
            this.idConverter = idConverter;
        }

        public Dtos.ConfigurableField Map(OrderConfigurableField model)
        {
            return new Dtos.ConfigurableField
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                DefaultValue = model.DefaultValue,
                ForPoSSessions = model.AssignedOn.HasFlag(AssignedOn.PoSSessions),
                ForOrdering = model.AssignedOn.HasFlag(AssignedOn.Ordering),
                Type = mapper.Map<Dtos.ConfigurableFieldType>(model.Type),
            };
        }
    }
}