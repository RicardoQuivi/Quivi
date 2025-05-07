namespace Quivi.Domain.Entities
{
    public interface ITranslation : IDeletableEntity
    {
        /// <summary>
        /// ISO 3166-1 alpha-2.
        /// </summary>
        Language Language { get; set; }
    }
}
