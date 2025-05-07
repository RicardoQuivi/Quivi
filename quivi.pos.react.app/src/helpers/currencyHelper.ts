export class Currency {
    static toDecimalFormat = (args: {value: number, culture?: string, maxDecimalPlaces?: number, minDecimalPlaces?: number}): string => {
        if (!args.culture)
            args.culture = "pt-PT";
        if (args.maxDecimalPlaces == undefined)
            args.maxDecimalPlaces = 2;
        if (args.minDecimalPlaces == undefined)
            args.minDecimalPlaces = 0;

        return new Intl.NumberFormat(args.culture, { maximumFractionDigits: args.maxDecimalPlaces, minimumFractionDigits: args.minDecimalPlaces }).format(args.value);
    }

    static toCurrencyFormat = (args: {value: number, culture?: string, currencyIso?: string}): string => {
        if (!args.culture)
            args.culture = "pt-PT";
        if (!args.currencyIso)
            args.currencyIso = "EUR";

        return new Intl.NumberFormat(args.culture, { style: 'currency', currency: args.currencyIso }).format(args.value);
    }
}