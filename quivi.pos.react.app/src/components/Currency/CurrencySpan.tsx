import { useTranslation } from "react-i18next";

const toCurrencyFormat = (args: {value: number, culture?: string, currencyIso?: string}): string => {
    if (!args.culture)
        args.culture = "pt-PT";
    if (!args.currencyIso)
        args.currencyIso = "EUR";

    return new Intl.NumberFormat(args.culture, { style: 'currency', currency: args.currencyIso }).format(args.value);
}

interface Props {
    readonly value: number;
    readonly currency?: string;
    readonly lineThrough?: boolean;
}
const CurrencySpan: React.FC<Props> = ({
    value,
    currency,
}) => {
    const { i18n } = useTranslation();

    const formatCurrency = (): string => toCurrencyFormat({ value: value, culture: i18n.language, currencyIso: currency });

    return (<>{formatCurrency()}</>);
}
export default CurrencySpan;