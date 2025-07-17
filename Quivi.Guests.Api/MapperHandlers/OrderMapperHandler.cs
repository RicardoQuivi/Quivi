using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class OrderMapperHandler : IMapperHandler<Order, Dtos.Order>
    {
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public OrderMapperHandler(IIdConverter idConverter, IMapper mapper)
        {
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        public Dtos.Order Map(Order model)
        {
            var changes = mapper.Map<Dtos.OrderChangeLog>(model.OrderChangeLogs ?? []);

            IEnumerable<PosCharge> capturedCharges = model.OrderMenuItems!.SelectMany(p => p.PosChargeInvoiceItems ?? [])
                                                        .Select(p => p.PosCharge!)
                                                        .Where(ecd => ecd.CaptureDate.HasValue)
                                                        .Distinct();

            var aggregate = capturedCharges.Aggregate((Surcharge: 0.0m, Tip: 0.0m), (r, c) =>
            {
                r.Surcharge += c.SurchargeFeeAmount;
                r.Tip += c.Tip;
                return r;
            });
            return new Dtos.Order
            {
                Id = idConverter.ToPublicId(model.Id),
                MerchantId = idConverter.ToPublicId(model.MerchantId),
                SequenceNumber = model.OrderSequence?.SequenceNumber.ToString() ?? idConverter.ToPublicId(model.Id),
                ChannelId = idConverter.ToPublicId(model.ChannelId),
                State = mapper.Map<Dtos.OrderState>(model.State),
                Items = mapper.Map<Dtos.OrderItem>(GroupModifiers(model.OrderMenuItems?.Where(o => o.Quantity > 0.0m) ?? [])),
                ScheduledTo = model.ScheduledTo.HasValue ? new DateTimeOffset(model.ScheduledTo.Value, TimeSpan.Zero) : null,
                LastModified = new DateTimeOffset(model.ModifiedDate, TimeSpan.Zero),
                Type = mapper.Map<Dtos.OrderType>(model.OrderType),
                ExtraCosts = new[]
                {
                    new Dtos.OrderExtraCost
                    {
                        Type = Dtos.ExtraCostType.SurchargeFee,
                        Amount = aggregate.Surcharge,
                    },
                    new Dtos.OrderExtraCost
                    {
                        Type = Dtos.ExtraCostType.Tip,
                        Amount = aggregate.Tip,
                    },
                }.Where(x => x.Amount != 0),
                Changes = changes.OrderByDescending(c => c.LastModified),
                Fields = model.OrderAdditionalInfos?.Select(f => new Dtos.OrderFieldValue
                {
                    Id = idConverter.ToPublicId(f.OrderConfigurableFieldId),
                    Value = f.Value,
                }) ?? [],
            };
        }

        private IEnumerable<OrderMenuItem> GroupModifiers(IEnumerable<OrderMenuItem> orderMenuItems)
        {
            var modifiersDictionary = orderMenuItems.Where(i => i.ParentOrderMenuItemId.HasValue)
                                                        .GroupBy(i => i.ParentOrderMenuItemId!.Value)
                                                        .ToDictionary(i => i.Key, i => i.ToList());
            foreach (var menuItem in orderMenuItems.Where(i => i.ParentOrderMenuItemId.HasValue == false))
            {
                if (modifiersDictionary.TryGetValue(menuItem.Id, out var modifiers))
                    menuItem.Modifiers = modifiers;
                yield return menuItem;
            }
        }
    }
}