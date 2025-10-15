using Quivi.Domain.Repositories.EntityFramework.Models;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class SalesMapperHandler : IMapperHandler<Sales, Dtos.Sales>
    {
        public Dtos.Sales Map(Sales model)
        {
            return new Dtos.Sales
            {
                From = new DateTimeOffset(model.From, TimeSpan.Zero),
                To = new DateTimeOffset(model.To, TimeSpan.Zero),
                Total = model.Payment + model.Tip,
                Payment = model.Payment,
                Tip = model.Tip,
                TotalRefund = model.PaymentRefund + model.TipRefund,
                PaymentRefund = model.PaymentRefund,
                TipRefund = model.TipRefund,
            };
        }
    }
}