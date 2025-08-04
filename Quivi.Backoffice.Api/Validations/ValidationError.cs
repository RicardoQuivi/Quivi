namespace Quivi.Backoffice.Api.Validations
{
    public enum ValidationError
    {
        Required,
        InvalidEmail,
        InvalidPassword,
        InvalidValue,
        Expired,
        InvalidCredentials,
        Duplicate,
        UnableToDeleteDueToAssociatedEntities,
        NoBalance,
    }
}
