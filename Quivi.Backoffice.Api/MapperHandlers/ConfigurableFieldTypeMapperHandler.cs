using Quivi.Backoffice.Api.Dtos;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class ConfigurableFieldTypeMapperHandler : IMapperHandler<FieldType, ConfigurableFieldType>,
                                                        IMapperHandler<ConfigurableFieldType, FieldType>
    {
        public ConfigurableFieldType Map(FieldType model)
        {
            switch (model)
            {
                case FieldType.String: return ConfigurableFieldType.Text;
                case FieldType.BigString: return ConfigurableFieldType.LongText;
                case FieldType.Boolean: return ConfigurableFieldType.Check;
                case FieldType.NumbersOnly: return ConfigurableFieldType.Number;
            }

            throw new NotImplementedException();
        }

        public FieldType Map(ConfigurableFieldType model)
        {
            switch (model)
            {
                case ConfigurableFieldType.Text: return FieldType.String;
                case ConfigurableFieldType.LongText: return FieldType.BigString;
                case ConfigurableFieldType.Check: return FieldType.Boolean;
                case ConfigurableFieldType.Number: return FieldType.NumbersOnly;
            }
            throw new NotImplementedException();
        }
    }
}