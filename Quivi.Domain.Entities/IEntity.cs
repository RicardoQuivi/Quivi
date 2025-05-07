namespace Quivi.Domain.Entities
{
    public interface IEntity
    {
        DateTime CreatedDate { get; set; }
        DateTime ModifiedDate { get; set; }
    }
}
