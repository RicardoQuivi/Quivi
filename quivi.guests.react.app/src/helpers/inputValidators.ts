import { Formatter } from "./formatter";

const vatValidationRegex = new RegExp(
    "^((AT)?U[0-9]{8}|(BE)?0[0-9]{9}|(BG)?[0-9]{9,10}|(CY)?[0-9]{8}L|(CZ)?[0-9]{8,10}|(DE)?[0-9]{9}|(DK)?[0-9]{8}|(EE)?[0-9]{9}|(EL|GR)?[0-9]{9}|(ES)?[0-9A-Z][0-9]{7}[0-9A-Z]|(FI)?[0-9]{8}|(FR)?[0-9A-Z]{2}[0-9]{9}|(GB)?([0-9]{9}([0-9]{3})?|[A-Z]{2}[0-9]{3})|(HU)?[0-9]{8}|(IE)?[0-9]S[0-9]{5}L|(IT)?[0-9]{11}|(LT)?([0-9]{9}|[0-9]{12})|(LU)?[0-9]{8}|(LV)?[0-9]{11}|(MT)?[0-9]{8}|(NL)?[0-9]{9}B[0-9]{2}|(PL)?[0-9]{10}|(PT)?[0-9]{9}|(RO)?[0-9]{2,10}|(SE)?[0-9]{12}|(SI)?[0-9]{8}|(SK)?[0-9]{10})$"
);

export const nifValidation = (nif: string) => {
  const validationSets = {
    one: ["1", "2", "3", "5", "6", "8"],
    two: [
      "45",
      "70",
      "71",
      "72",
      "74",
      "75",
      "77",
      "79",
      "90",
      "91",
      "98",
      "99"
    ]
  };
  if (nif.length !== 9) return false;
  if (
    !validationSets.one.includes(nif.substr(0, 1)) &&
    !validationSets.two.includes(nif.substr(0, 2))
  )
    return false;
  const total =
    Number(nif[0]) * 9 +
    Number(nif[1]) * 8 +
    Number(nif[2]) * 7 +
    Number(nif[3]) * 6 +
    Number(nif[4]) * 5 +
    Number(nif[5]) * 4 +
    Number(nif[6]) * 3 +
    Number(nif[7]) * 2;
  const modulo11 = total - Math.trunc(total / 11) * 11;
  const checkDigit = modulo11 < 2 ? 0 : 11 - modulo11;
  return checkDigit === Number(nif[8]);
};

export const emailValidation = (email: string) => {
  const emailFormat = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
  if (email.match(emailFormat)) return true;
  else return false;
};

export const vatValidation = (vat: string) => {
    const cleanVat = Formatter.cleanString(vat);
    return vatValidationRegex.test(cleanVat);
};