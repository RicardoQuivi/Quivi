import { useMemo } from "react";
import Label from "../form/Label";
import { useTranslation } from "react-i18next";
import CurrencyInput from 'react-currency-input-field';

interface CurrencyFieldProps {
    readonly label?: string;
    readonly value?: number;
    readonly placeholder?: string;
    readonly onChange?: (v: number) => any;
    readonly errorMessage?: string;
    readonly name?: string;
    readonly disabled?: boolean;
    readonly autoComplete?: string;
    readonly onKeyUp?: (e: React.KeyboardEvent<HTMLInputElement>) => void;
    readonly className?: string;
    readonly startElement?: React.ReactNode;
    readonly endElement?: React.ReactNode;
    readonly decimalPlaces?: number;
    readonly minValue?: number;
    readonly maxValue?: number;
}
export const CurrencyField = (props: CurrencyFieldProps) => {
    const { i18n } = useTranslation();

    let borderClasses = 'rounded-lg border';
    let inputClasses = `h-11 appearance-none px-4 py-2.5 text-sm shadow-theme-xs placeholder:text-gray-400 focus:outline-hidden focus:ring-3 dark:bg-gray-900 dark:text-white/90 dark:placeholder:text-white/30`;

    if (props.disabled) {
        borderClasses += ' border-gray-300 dark:border-gray-700'
        inputClasses += ` text-gray-500 opacity-40 bg-gray-100 cursor-not-allowed dark:bg-gray-800 dark:text-gray-400 opacity-40`;
    } else if (props.errorMessage != undefined) {
        borderClasses += ' border-error-500 focus:border-error-300 dark:border-error-500 dark:focus:border-error-800';
        inputClasses += ` focus:ring-error-500/20 dark:text-error-400`;
    } else {
        borderClasses += ' border-gray-300 focus:border-brand-300 dark:border-gray-700 dark:focus:border-brand-800';
        inputClasses += ` bg-transparent text-gray-800 focus:ring-brand-500/20 dark:text-white/90`;
    }

    inputClasses += borderClasses;
    if(props.startElement != undefined) {
        inputClasses += ' rounded-l-none border-l-0';
    }

    if(props.endElement != undefined) {
        inputClasses += ' rounded-r-none border-r-0';
    }

    const step = useMemo(() => props.decimalPlaces == undefined ? undefined : Math.pow(10, -props.decimalPlaces), [props.decimalPlaces])
    const value = useMemo(() => (props.value ?? 0).toFixed(2), [props.value]);

    return (
    <div className={`flex flex-col ${props.className ?? ""}`}>
        {
            props.label != undefined &&
            <Label>{props.label}</Label>
        }
        <div className="relative flex flex-col">
            <div className={`gap-0 flex flex-row`}>
                {
                    props.startElement != undefined &&
                    <div
                        className={`${borderClasses} rounded-r-none box-border h-11 flex-none`}
                    >
                        {props.startElement}
                    </div>
                }
                <CurrencyInput
                    name={props.name}
                    value={value}
                    onValueChange={(_value, _name, values) => {
                        const newValue = values?.float ?? 0;

                        if(props.minValue != undefined){
                            if(newValue < props.minValue) {
                                return;
                            }
                        }

                        if(props.maxValue != undefined){
                            if(newValue > props.maxValue) {
                                return;
                            }
                        }
                        props.onChange?.(values?.float ?? 0);
                    }}
                    disabled={props.disabled}
                    autoComplete={props.autoComplete}
                    onKeyUp={props.onKeyUp}
                    placeholder={props.placeholder}
                    className={`${inputClasses} grow min-w-0 text-right`}
                    decimalsLimit={props.decimalPlaces}
                    fixedDecimalLength={props.decimalPlaces}
                    decimalScale={props.decimalPlaces}
                    intlConfig={{
                        locale: i18n.language,
                        currency: 'EUR',
                    }}
                    step={step}
                    allowDecimals
                    suffix={""}
                    prefix={" "}
                />
                {
                    props.endElement != undefined &&
                    <div
                        className={`${borderClasses} rounded-l-none box-border h-11 flex-none`}
                    >
                        {props.endElement}
                    </div>
                }
            </div>
            {
                props.errorMessage != undefined && 
                <p
                    className='mt-1.5 text-xs text-error-500'
                >
                    {props.errorMessage}
                </p>
            }
        </div>
    </div>
    )
}