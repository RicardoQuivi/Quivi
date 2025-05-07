using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class EmployeeRestrictionsMapperHandler : IMapperHandler<EmployeeRestrictions, IEnumerable<Dtos.EmployeeRestriction>>,
                                                        IMapperHandler<IEnumerable<Dtos.EmployeeRestriction>, EmployeeRestrictions>
    {
        public IEnumerable<Dtos.EmployeeRestriction> Map(EmployeeRestrictions model)
        {
            foreach (EmployeeRestrictions restriction in Enum.GetValues(typeof(EmployeeRestrictions)))
            {
                if (restriction == EmployeeRestrictions.None)
                    continue;

                if (model.HasFlag(restriction))
                    yield return MapValue(restriction);
            }
        }

        private Dtos.EmployeeRestriction MapValue(EmployeeRestrictions model)
        {
            switch (model)
            {
                case EmployeeRestrictions.ApplyDiscounts: return Dtos.EmployeeRestriction.ApplyDiscounts;
                case EmployeeRestrictions.RemoveItems: return Dtos.EmployeeRestriction.RemoveItems;
                case EmployeeRestrictions.OnlyOwnTransactions: return Dtos.EmployeeRestriction.OnlyOwnTransactions;
                case EmployeeRestrictions.OnlyTransactionsOfLast24Hours: return Dtos.EmployeeRestriction.OnlyTransactionsOfLast24Hours;
                case EmployeeRestrictions.TransferingItems: return Dtos.EmployeeRestriction.TransferingItems;
                case EmployeeRestrictions.SessionsAccess: return Dtos.EmployeeRestriction.SessionsAccess;
                case EmployeeRestrictions.Refunds: return Dtos.EmployeeRestriction.Refunds;
            }
            throw new NotImplementedException();
        }

        public EmployeeRestrictions Map(IEnumerable<Dtos.EmployeeRestriction> model)
        {
            EmployeeRestrictions result = EmployeeRestrictions.None;
            foreach (var flag in model)
                result |= MapValue(flag);
            return result;
        }

        private EmployeeRestrictions MapValue(Dtos.EmployeeRestriction model)
        {
            switch (model)
            {
                case Dtos.EmployeeRestriction.ApplyDiscounts: return EmployeeRestrictions.ApplyDiscounts;
                case Dtos.EmployeeRestriction.RemoveItems: return EmployeeRestrictions.RemoveItems;
                case Dtos.EmployeeRestriction.OnlyOwnTransactions: return EmployeeRestrictions.OnlyOwnTransactions;
                case Dtos.EmployeeRestriction.OnlyTransactionsOfLast24Hours: return EmployeeRestrictions.OnlyTransactionsOfLast24Hours;
                case Dtos.EmployeeRestriction.TransferingItems: return EmployeeRestrictions.TransferingItems;
                case Dtos.EmployeeRestriction.SessionsAccess: return EmployeeRestrictions.SessionsAccess;
                case Dtos.EmployeeRestriction.Refunds: return EmployeeRestrictions.Refunds;
            }
            throw new NotImplementedException();
        }
    }
}
