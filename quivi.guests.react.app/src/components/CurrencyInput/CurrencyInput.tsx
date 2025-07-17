import { NumberFormatBase, type NumberFormatValues } from 'react-number-format';

export function currencyFormatter(value: any) {
    let number = 0;
    if(typeof value === "string") {
        const num = Number(value);
        if (Number.isFinite(num)) {
            number = num;
        } else {
            return "";
        }
    } else if(typeof value !== "number") {
        return "";
    } else {
        number = value;
    }

    const amount = new Intl.NumberFormat('pt-PT', {
        style: 'currency',
        currency: 'EUR',
    }).format(number / 100);

    return `${amount}`;
}

interface CustomNumberFormatProps {
    readonly value: string | number | undefined | null;
    readonly onChange?: (v: number) => any;
    readonly placeholder?: number;
}
const InputCurrency = (props: CustomNumberFormatProps) => {
    const handleCustomTipChange = (values: NumberFormatValues) => props.onChange?.(values.floatValue ?? 0);

    return <NumberFormatBase
            value={props.value}
            onValueChange={handleCustomTipChange}
            format={currencyFormatter}
            placeholder={props.placeholder == undefined ? undefined : currencyFormatter(props.placeholder)}
            valueIsNumericString
        />
}
export default InputCurrency;