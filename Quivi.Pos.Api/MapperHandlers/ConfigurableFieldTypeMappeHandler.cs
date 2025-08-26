using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class ConfigurableFieldTypeMappeHandler
    {
    }

    public class ConfigurableFieldTypeMapperHandler : IMapperHandler<FieldType, Dtos.ConfigurableFieldType>
    {
        public Dtos.ConfigurableFieldType Map(FieldType model)
        {
            switch (model)
            {
                case FieldType.String: return Dtos.ConfigurableFieldType.Text;
                case FieldType.BigString: return Dtos.ConfigurableFieldType.LongText;
                case FieldType.Boolean: return Dtos.ConfigurableFieldType.Check;
                case FieldType.NumbersOnly: return Dtos.ConfigurableFieldType.Number;
            }

            throw new NotImplementedException();
        }

        public FieldType Map(Dtos.ConfigurableFieldType model)
        {
            switch (model)
            {
                case Dtos.ConfigurableFieldType.Text: return FieldType.String;
                case Dtos.ConfigurableFieldType.LongText: return FieldType.BigString;
                case Dtos.ConfigurableFieldType.Check: return FieldType.Boolean;
                case Dtos.ConfigurableFieldType.Number: return FieldType.NumbersOnly;
            }
            throw new NotImplementedException();
        }
    }
}