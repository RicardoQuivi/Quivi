export class CurrencyHelper {
    static format = (args: {value: number, culture?: string, currencyIso?: string}): string => {
        if (!args.culture)
            args.culture = "pt-PT";
        if (!args.currencyIso)
            args.currencyIso = "EUR";

        return new Intl.NumberFormat(args.culture, { style: 'currency', currency: args.currencyIso }).format(args.value);
    }
}