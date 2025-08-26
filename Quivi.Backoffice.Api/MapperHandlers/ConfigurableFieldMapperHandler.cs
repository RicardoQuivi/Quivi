using Quivi.Backoffice.Api.Dtos;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class ConfigurableFieldMapperHandler : IMapperHandler<OrderConfigurableField, ConfigurableField>
    {
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public ConfigurableFieldMapperHandler(IMapper mapper, IIdConverter idConverter)
        {
            this.mapper = mapper;
            this.idConverter = idConverter;
        }

        public ConfigurableField Map(OrderConfigurableField model)
        {
            return new ConfigurableField
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                IsRequired = model.IsRequired,
                IsAutoFill = model.IsAutoFill,
                DefaultValue = model.DefaultValue,
                PrintedOn = mapper.Map<Dtos.PrintedOn>(model.PrintedOn),
                AssignedOn = mapper.Map<Dtos.AssignedOn>(model.AssignedOn),
                Type = mapper.Map<ConfigurableFieldType>(model.Type),
                Translations = model.Translations?.ToDictionary(s => s.Language, s => new ConfigurableFieldTranslation
                {
                    Name = s.Name,
                }) ?? [],
            };
        }
    }
}