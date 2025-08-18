namespace FacturaLusa.v2.Exceptions
{
    public enum ErrorType
    {
        GenericError = 0,

        Unauthorized,

        #region Required
        MissingRequiredFields,
        MissingFieldValue,
        MissingFieldValueOrNotPreDefined,
        #endregion

        #region Not Exists
        CustomerNotExists,
        ItemNotExists,
        ReferenceEntityNotExists,
        EntityXNotExists,
        #endregion

        FieldValueAlreadyExists,
        SerieNameAlreadyExists,
        InvalidVatNumberForCountry,
        InvalidOrInactiveDocumentType,
        SelectAtLeastOneItem,
        InvalidDocumentStatus,
        ExpiredSerie,
        InvalidItemsAtLineX,
        VatRateMustBe0WhenExemption,
        AllItemsVatRateMustBe0WhenIsDoNothingType,
        DiscountMustBeLessOrEqualThanTotalPrice,
        InvalidCountry,
        MissingATConfigurations,
        SerieAlreadyCommunicated,
        MissingSerieCommunicationForDocumentType,
        MissingSerieATComunication,
    }
}