using Quivi.Application.Extensions.Pos;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class PosChargeInvoiceItemMapperHandler : IMapperHandler<IEnumerable<PosChargeInvoiceItem>, IEnumerable<Dtos.TransactionItem>>
    {
        private readonly IIdConverter idConverter;

        public PosChargeInvoiceItemMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public IEnumerable<Dtos.TransactionItem> Map(IEnumerable<PosChargeInvoiceItem> model)
        {
            var sessionItems = model.AsConvertedSessionItems();
            return sessionItems.Select(g =>
            {
                var first = g.Source.First();

                if (first.OrderMenuItem == null)
                    throw new Exception($"Entity {nameof(OrderMenuItem)} was not included into {nameof(PosChargeInvoiceItem)}.");

                return new Dtos.TransactionItem
                {
                    Id = idConverter.ToPublicId(first.Id),
                    Name = first.OrderMenuItem.Name,
                    Quantity = g.Quantity,
                    FinalPrice = g.Price,
                    OriginalPrice = first.OrderMenuItem.OriginalPrice,
                    Modifiers = g.Extras.Select(e =>
                    {
                        var firstExtra = e.Source.First();

                        if (firstExtra.OrderMenuItem == null)
                            throw new Exception($"Entity {nameof(OrderMenuItem)} was not included into {nameof(PosChargeInvoiceItem)}.");

                        return new Dtos.BaseTransactionItem
                        {
                            Id = idConverter.ToPublicId(firstExtra.Id),
                            Name = firstExtra.OrderMenuItem.Name,
                            Quantity = e.Quantity * g.Quantity,
                            FinalPrice = e.Price,
                            OriginalPrice = firstExtra.OrderMenuItem.OriginalPrice,
                        };
                    })
                };
            }).ToList();
        }
    }
}