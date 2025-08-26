namespace Quivi.Backoffice.Api.Requests.ConfigurableFieldAssociations
{
    public class UpdateConfigurableFieldAssociationsRequest : ARequest
    {
        public required IEnumerable<UpdateConfigurableFieldAssociation> Associations { get; init; }
    }

    public class UpdateConfigurableFieldAssociation
    {
        public required string Id { get; init; }
        public bool Active { get; init; }
    }
}