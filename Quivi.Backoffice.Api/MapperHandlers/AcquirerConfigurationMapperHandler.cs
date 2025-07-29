using Quivi.Backoffice.Api.Dtos;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using System.Security.Principal;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class AcquirerConfigurationMapperHandler : IMapperHandler<MerchantAcquirerConfiguration, Dtos.AcquirerConfiguration>
    {
        private readonly IIdConverter idConverter;
        private readonly IPrincipal principal;

        public AcquirerConfigurationMapperHandler(IIdConverter idConverter, IPrincipal principal)
        {
            this.idConverter = idConverter;
            this.principal = principal;
        }

        public AcquirerConfiguration Map(MerchantAcquirerConfiguration model)
        {
            return new AcquirerConfiguration
            {
                Id = idConverter.ToPublicId(model.Id),
                Method = model.ChargeMethod,
                Partner = model.ChargePartner,
                IsActive = model.DeletedDate.HasValue == false,
                Settings = GetSettings(model),
            };
        }

        private IReadOnlyDictionary<ChargePartner, IReadOnlyDictionary<ChargeMethod, object>> GetSettings(MerchantAcquirerConfiguration model)
        {
            var result = new Dictionary<ChargePartner, IReadOnlyDictionary<ChargeMethod, object>>();
            if (principal.IsAdmin() == false)
                return result;

            switch (model.ChargePartner)
            {
                case ChargePartner.Quivi:
                    result.Add(ChargePartner.Quivi, new Dictionary<ChargeMethod, object>());
                    break;
                case ChargePartner.Paybyrd:
                    {
                        var state = new Dictionary<ChargeMethod, object>
                        {
                            {
                                model.ChargeMethod,
                                new
                                {
                                    ApiKey = model.ApiKey,
                                }
                            }
                        };
                        result.Add(ChargePartner.Paybyrd, state);
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }
            return result;
        }
    }
}