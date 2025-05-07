using System.Text;
using System.Text.RegularExpressions;

namespace Quivi.Infrastructure.Validations
{
    public static class StringValidationsExtensions
    {
        public static bool IsValidNif(this string nif)
        {
            if (string.IsNullOrWhiteSpace(nif) || !Regex.IsMatch(nif, "^[0-9]+$") || nif.Length != 9)
                return false;

            char c = nif[0];
            int checkDigit = (Convert.ToInt32(c.ToString()) * 9);
            for (int i = 2; i <= 8; i++)
            {
                checkDigit += Convert.ToInt32(nif[i - 1].ToString()) * (10 - i);
            }
            checkDigit = 11 - (checkDigit % 11);
            if (checkDigit >= 10)
                checkDigit = 0;

            if (checkDigit.ToString() != nif[8].ToString())
                return false;

            return true;
        }

        public static bool IsValidIban(this string iban)
        {
            if (string.IsNullOrEmpty(iban))
                return false;

            if (Regex.IsMatch(iban, "^[A-Z0-9]"))
            {
                iban = iban.ToUpper();
                iban = iban.Replace(" ", String.Empty);
                string bank =
                iban.Substring(4, iban.Length - 4) + iban.Substring(0, 4);
                int asciiShift = 55;
                StringBuilder sb = new StringBuilder();
                foreach (char c in bank)
                {
                    int v;
                    if (char.IsLetter(c)) v = c - asciiShift;
                    else v = int.Parse(c.ToString());
                    sb.Append(v);
                }
                string checkSumString = sb.ToString();
                int checksum = int.Parse(checkSumString.Substring(0, 1));
                for (int i = 1; i < checkSumString.Length; i++)
                {
                    int v = int.Parse(checkSumString.Substring(i, 1));
                    checksum *= 10;
                    checksum += v;
                    checksum %= 97;
                }
                return checksum == 1;
            }

            return false;
        }
    }
}
