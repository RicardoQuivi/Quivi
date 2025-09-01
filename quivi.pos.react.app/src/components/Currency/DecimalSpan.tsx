import React from "react";
import { useTranslation } from "react-i18next";


const toDecimalFormat = (args: {value: number, culture?: string, maxDecimalPlaces?: number, minDecimalPlaces?: number}): string => {
    if (!args.culture)
        args.culture = "pt-PT";
    if (args.maxDecimalPlaces == undefined)
        args.maxDecimalPlaces = 2;
    if (args.minDecimalPlaces == undefined)
        args.minDecimalPlaces = 0;

    return new Intl.NumberFormat(args.culture, { maximumFractionDigits: args.maxDecimalPlaces, minimumFractionDigits: args.minDecimalPlaces }).format(args.value);
}

type Props =  {
    value: number;
}

const DecimalSpan: React.FC<Props> = ({
    value,
}) => {
    const { i18n } = useTranslation();

    const formatDecimal = (): string => toDecimalFormat({ value: value, culture: i18n.language, maxDecimalPlaces: 2, minDecimalPlaces: 0 });

    return (<>{formatDecimal()}</>);
}
export default DecimalSpan;