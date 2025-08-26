namespace Quivi.Pos.Api.Dtos
{
    public class ConfigurableField
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? DefaultValue { get; set; }
        public bool ForPoSSessions { get; set; }
        public bool ForOrdering { get; set; }
        public ConfigurableFieldType Type { get; set; }
    }
}