using Microsoft.AspNetCore.Identity;

namespace Quivi.Application.Extensions
{
    public static class IdentityErrorExtensions
    {
        public static Dictionary<string, object> ToErrorContext(this IEnumerable<IdentityError> errors, PasswordOptions passwordOptions)
        {
            return errors.Select<IdentityError, (string, object)?>(e =>
            {
                if (e.Code == "PasswordTooShort")
                    return (nameof(passwordOptions.RequiredLength), passwordOptions.RequiredLength);
                else if (e.Code == "PasswordRequiresNonAlphanumeric")
                    return (nameof(passwordOptions.RequireNonAlphanumeric), passwordOptions.RequireNonAlphanumeric);
                else if (e.Code == "PasswordRequiresDigit")
                    return (nameof(passwordOptions.RequireDigit), passwordOptions.RequireDigit);
                else if (e.Code == "PasswordRequiresLower")
                    return (nameof(passwordOptions.RequireLowercase), passwordOptions.RequireLowercase);
                else if (e.Code == "PasswordRequiresUpper")
                    return (nameof(passwordOptions.RequireUppercase), passwordOptions.RequireUppercase);
                else if (e.Code == "PasswordRequiresUniqueChars")
                    return (nameof(passwordOptions.RequiredUniqueChars), passwordOptions.RequiredUniqueChars);

                return null;
            }).Where(e => e.HasValue).ToDictionary(e => e!.Value.Item1, e => e!.Value.Item2);
        }
    }
}