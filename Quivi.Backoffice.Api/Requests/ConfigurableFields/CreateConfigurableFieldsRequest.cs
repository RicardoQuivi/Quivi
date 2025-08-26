using Quivi.Backoffice.Api.Dtos;
using Quivi.Domain.Entities;

namespace Quivi.Backoffice.Api.Requests.ConfigurableFields
{
    public class CreateConfigurableFieldsRequest
    {
        public string Name { get; init; } = string.Empty;
        public ConfigurableFieldType Type { get; init; }
        public bool IsRequired { get; init; }
        public bool IsAutoFill { get; init; }
        public PrintedOn PrintedOn { get; init; }
        public AssignedOn AssignedOn { get; init; }
        public string? DefaultValue { get; init; }
        public required IReadOnlyDictionary<Language, CreateConfigurableFieldTranslation> Translations { get; init; }
    }

    public class CreateConfigurableFieldTranslation
    {
        public required string Name { get; init; }
    }
}