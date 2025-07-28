using Quivi.Backoffice.Api.Dtos;
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

        public IEnumerable<TransactionItem> Map(IEnumerable<PosChargeInvoiceItem> model)
        {
            return model.GroupBy(g =>
            {
                if (g.OrderMenuItem == null)
                    throw new Exception($"Entity {nameof(OrderMenuItem)} was not included into {nameof(PosChargeInvoiceItem)}.");

                return new
                {
                    g.OrderMenuItem.MenuItemId,
                    g.OrderMenuItem.Name,
                    g.OrderMenuItem!.FinalPrice,
                    g.OrderMenuItem!.OriginalPrice,
                };
            }).Select(g =>
            {
                var first = g.First();

                if (first.OrderMenuItem == null)
                    throw new Exception($"Entity {nameof(OrderMenuItem)} was not included into {nameof(PosChargeInvoiceItem)}.");

                return new TransactionItem
                {
                    Id = idConverter.ToPublicId(first.Id),
                    Name = first.OrderMenuItem.Name,
                    Quantity = g.Sum(i => i.Quantity),
                    FinalPrice = first.OrderMenuItem.FinalPrice,
                    OriginalPrice = first.OrderMenuItem.OriginalPrice,
                };
            }).ToList();
        }
    }
}