using Quivi.Backoffice.Api.Dtos;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class AcquirerConfigurationMapperHandler : IMapperHandler<MerchantAcquirerConfiguration, Dtos.AcquirerConfiguration>
    {
        private readonly IIdConverter idConverter;

        public AcquirerConfigurationMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public AcquirerConfiguration Map(MerchantAcquirerConfiguration model)
        {
            if(model.ChargePartner == ChargePartner.Quivi)
            {
                if (model.ChargeMethod == ChargeMethod.Cash)
                     return MapToCash(model);

                throw new NotImplementedException();
            }

            return new AcquirerConfiguration
            {
                Id = idConverter.ToPublicId(model.Id),
                Method = model.ChargeMethod,
                Partner = model.ChargePartner,
                IsActive = model.DeletedDate.HasValue == false,
            };  
        }

        private AcquirerConfiguration MapToCash(MerchantAcquirerConfiguration model)
        {
            return new AcquirerConfiguration
            {
                Id = idConverter.ToPublicId(model.Id),
                Method = model.ChargeMethod,
                Partner = model.ChargePartner,
                IsActive = model.DeletedDate.HasValue == false,
            };
        }
    }
}