namespace Quivi.Domain.Entities
{
    public interface IBaseEntity
    {

    }

    public interface IEntity : IBaseEntity
    {
        DateTime CreatedDate { get; set; }
        DateTime ModifiedDate { get; set; }
    }
}
