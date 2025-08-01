using System.Runtime.Serialization;

namespace Paybyrd.Api.Models
{
    public enum CredentialType
    {
        [EnumMember(Value = "api-key")]
        ApiKey,
        [EnumMember(Value = "basic")]
        Basic,
    }
}