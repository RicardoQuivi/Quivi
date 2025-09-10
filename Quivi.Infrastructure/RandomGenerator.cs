using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Quivi.Infrastructure.Abstractions;
using System.Security.Cryptography;

namespace Quivi.Infrastructure
{
    public class RandomGenerator : IRandomGenerator
    {
        private readonly IdentityOptions identityOptions;
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";

        public RandomGenerator(IOptions<IdentityOptions> identityOptions)
        {
            this.identityOptions = identityOptions?.Value ?? throw new ArgumentNullException(nameof(identityOptions));
        }

        public Guid Guid() => System.Guid.NewGuid();

        public string Password(int minLength) => GeneratePassword(identityOptions.Password, Math.Max(minLength, 1));

        public string String(int length)
        {
            if (length < 0)
                throw new ArgumentException("Length must be non-negative.", nameof(length));

            var bytes = RandomNumberGenerator.GetBytes(length);
            var result = new char[length];

            for (int i = 0; i < length; i++)
                result[i] = Chars[bytes[i] % Chars.Length];

            return new string(result);
        }

        private static string GeneratePassword(PasswordOptions options, int minLength)
        {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string nonAlphanumeric = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            List<char> charPool = new();
            List<char> passwordChars = new();

            if (options.RequireLowercase)
            {
                charPool.AddRange(lowercase);
                passwordChars.Add(lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)]);
            }

            if (options.RequireUppercase)
            {
                charPool.AddRange(uppercase);
                passwordChars.Add(uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)]);
            }

            if (options.RequireDigit)
            {
                charPool.AddRange(digits);
                passwordChars.Add(digits[RandomNumberGenerator.GetInt32(digits.Length)]);
            }

            if (options.RequireNonAlphanumeric)
            {
                charPool.AddRange(nonAlphanumeric);
                passwordChars.Add(nonAlphanumeric[RandomNumberGenerator.GetInt32(nonAlphanumeric.Length)]);
            }

            if (!charPool.Any())
            {
                charPool.AddRange(lowercase);
                charPool.AddRange(uppercase);
                charPool.AddRange(digits);
                charPool.AddRange(nonAlphanumeric);
            }

            int remainingLength = Math.Max(minLength, options.RequiredLength) - passwordChars.Count;

            if (remainingLength < 0)
                throw new ArgumentException($"The required length ({options.RequiredLength}) is too short to satisfy the character requirements.");

            for (int i = 0; i < remainingLength; i++)
                passwordChars.Add(charPool[RandomNumberGenerator.GetInt32(charPool.Count)]);

            while (passwordChars.Distinct().Count() < options.RequiredUniqueChars)
            {
                int index = RandomNumberGenerator.GetInt32(passwordChars.Count);
                passwordChars[index] = charPool[RandomNumberGenerator.GetInt32(charPool.Count)];
            }

            for (int i = passwordChars.Count - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                char temp = passwordChars[i];
                passwordChars[i] = passwordChars[j];
                passwordChars[j] = temp;
            }

            return new string(passwordChars.ToArray());
        }
    }
}
