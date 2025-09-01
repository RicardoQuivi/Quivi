using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class PosChargeInvoiceItemMapperHandler : IMapperHandler<PosChargeInvoiceItem, Dtos.TransactionItem>
    {
        private readonly IIdConverter idConverter;

        public PosChargeInvoiceItemMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.TransactionItem Map(PosChargeInvoiceItem model)
        {
            return new Dtos.TransactionItem
            {
                Id = idConverter.ToPublicId(model.Id),
                TransactionId = idConverter.ToPublicId(model.PosChargeId),
                Quantity = model.Quantity,
                Name = model.OrderMenuItem!.Name,
                Price = model.OrderMenuItem!.FinalPrice,
                OriginalPrice = model.OrderMenuItem!.OriginalPrice,
                AppliedDiscountPercentage = PriceHelper.CalculateDiscountPercentage(model.OrderMenuItem!.OriginalPrice, model.OrderMenuItem!.FinalPrice),
                CreatedDate = new DateTimeOffset(model.CreatedDate, TimeSpan.Zero),
                LastModified = new DateTimeOffset(model.ModifiedDate, TimeSpan.Zero),
                Modifiers = model.ChildrenPosChargeInvoiceItems?.Select(MapToBase) ?? [],
            };
        }

        private Dtos.BaseTransactionItem MapToBase(PosChargeInvoiceItem model, int parentQuantity) => new Dtos.BaseTransactionItem
        {
            Id = idConverter.ToPublicId(model.Id),
            TransactionId = idConverter.ToPublicId(model.PosChargeId),
            Quantity = model.Quantity / parentQuantity,
            Name = model.OrderMenuItem!.Name,
            Price = model.OrderMenuItem!.FinalPrice,
            OriginalPrice = model.OrderMenuItem!.OriginalPrice,
            AppliedDiscountPercentage = PriceHelper.CalculateDiscountPercentage(model.OrderMenuItem!.OriginalPrice, model.OrderMenuItem!.FinalPrice),
            CreatedDate = new DateTimeOffset(model.CreatedDate, TimeSpan.Zero),
            LastModified = new DateTimeOffset(model.ModifiedDate, TimeSpan.Zero),
        };
    }
}