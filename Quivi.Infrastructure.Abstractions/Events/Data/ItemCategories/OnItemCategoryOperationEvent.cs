namespace Quivi.Infrastructure.Abstractions.Events.Data.ItemCategories
{
    public class OnItemCategoryOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
    }
}
