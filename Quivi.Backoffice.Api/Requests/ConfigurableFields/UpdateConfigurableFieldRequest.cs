using Quivi.Backoffice.Api.Dtos;
using Quivi.Domain.Entities;
using Quivi.Infrastructure.Apis;

namespace Quivi.Backoffice.Api.Requests.ConfigurableFields
{
    public class UpdateConfigurableFieldRequest
    {
        public string? Name { get; set; }
        public ConfigurableFieldType? Type { get; set; }
        public bool? IsRequired { get; set; }
        public bool? IsAutoFill { get; set; }
        public PrintedOn? PrintedOn { get; set; }
        public AssignedOn? AssignedOn { get; set; }

        public Optional<string> DefaultValue { get; init; }
        public required IReadOnlyDictionary<Language, UpdateConfigurableFieldTranslation> Translations { get; init; }
    }

    public class UpdateConfigurableFieldTranslation
    {
        public required string Name { get; init; }
    }
}