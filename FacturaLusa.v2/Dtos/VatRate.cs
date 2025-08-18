namespace FacturaLusa.v2.Dtos
{
    public class VatRate
    {
        public long Id { get; init; }
        public required string Description { get; init; }
        public required TaxType Type { get; init; }
        public required decimal TaxPercentage { get; init; }
        public SaftRegion? SaftRegion { get; init; }
    }
}
