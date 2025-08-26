using Quivi.Domain.Entities;

namespace Quivi.Backoffice.Api.Dtos
{
    public class ConfigurableField
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public bool IsRequired { get; init; }
        public bool IsAutoFill { get; init; }
        public PrintedOn PrintedOn { get; init; }
        public AssignedOn AssignedOn { get; init; }
        public string? DefaultValue { get; init; }
        public ConfigurableFieldType Type { get; init; }
        public required IReadOnlyDictionary<Language, ConfigurableFieldTranslation> Translations { get; init; }
    }

    public class ConfigurableFieldTranslation
    {
        public required string Name { get; init; }
    }

    public enum ConfigurableFieldType
    {
        Text = 0,
        LongText = 1,
        Check = 2,
        Number = 3,
    }

    [Flags]
    public enum PrintedOn
    {
        None = 0,
        PreparationRequest = 1 << 1,
        TableBill = 1 << 2,
    }

    [Flags]
    public enum AssignedOn
    {
        None = 0,
        PoSSessions = 1 << 1,
        Ordering = 1 << 2,
    }
}