namespace Quivi.Domain.Entities
{
    public interface IDeletableEntity : IEntity
    {
        DateTime? DeletedDate { get; set; }
    }
}
