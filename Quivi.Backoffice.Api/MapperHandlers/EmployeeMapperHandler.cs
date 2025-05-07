using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class EmployeeMapperHandler : IMapperHandler<Employee, Dtos.Employee>
    {
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public EmployeeMapperHandler(IIdConverter idConverter, IMapper mapper)
        {
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        public Dtos.Employee Map(Employee model)
        {
            return new Dtos.Employee
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                HasPinCode = string.IsNullOrWhiteSpace(model.PinCodeHash) == false,
                InactivityLogoutTimeout = model.LogoutInactivity,
                CreatedDate = new DateTimeOffset(model.CreatedDate, TimeSpan.Zero),
                Restrictions = mapper.Map<IEnumerable<Dtos.EmployeeRestriction>>(model.Restrictions),
            };
        }
    }
}
