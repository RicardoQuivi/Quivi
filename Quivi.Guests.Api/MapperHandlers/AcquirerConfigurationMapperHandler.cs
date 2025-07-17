using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class AcquirerConfigurationMapperHandler : IMapperHandler<MerchantAcquirerConfiguration, Dtos.PaymentMethod>
    {
        private readonly IIdConverter idConverter;

        public AcquirerConfigurationMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.PaymentMethod Map(MerchantAcquirerConfiguration model)
        {
            return new Dtos.PaymentMethod
            {
                Id = idConverter.ToPublicId(model.Id),
                Method = model.ChargeMethod,
            };
        }
    }
}