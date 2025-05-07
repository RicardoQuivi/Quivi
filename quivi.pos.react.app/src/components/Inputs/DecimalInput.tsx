import * as React from 'react';
import { useEffect, useState, useRef } from 'react';
import { InputAdornment, TextField, TextFieldProps, TextFieldVariants } from '@mui/material';
import { Currency } from '../../helpers/currencyHelper';
import { useTranslation } from 'react-i18next';

interface Props {
    readonly endAdornment?: React.ReactNode;
    readonly errorMsg?: string | null;
    readonly value: number;
    readonly label?: React.ReactNode;
    readonly style?: React.CSSProperties;
    readonly onChange?: (newValue: number) => void;
    readonly textFieldProps?: {
        variant?: TextFieldVariants;
    } & Omit<TextFieldProps, 'variant'>
}

const DecimalInput: React.FC<Props> = (props: Props) => {
    const { i18n } = useTranslation();

    const convertToNumber = (value: string) => Number(value.replace(",", "."));
    const convertToString = (value: number) => Currency.toDecimalFormat({ value: value, culture: i18n.language, minDecimalPlaces: 2, maxDecimalPlaces: 2 });
    
    const [value, setValue] = useState(convertToString(props.value));
    const inputRef = useRef(null);

    const handleChange = (event: any) => {
        let inputValue = event.target.value;
        inputValue = inputValue.replace(/[^0-9]/g, ''); // remove non-digits
        while (inputValue.length < 3) {
          inputValue = '0' + inputValue; // pad with zeros
        }
        inputValue = inputValue.slice(0, -2) + ',' + inputValue.slice(-2);
        if (inputValue.startsWith('00')) {
          inputValue = inputValue.slice(1);
        }
        if (inputValue.startsWith('0') && inputValue.charAt(1) !== ',') {
          inputValue = inputValue.slice(1);
        }
    
        setValue(inputValue);
        props.onChange?.(convertToNumber(inputValue));
    };

    const handleKeyDown = (event: any) => {
        if (event.key === 'Backspace' || event.key === 'Delete') {
            let inputValue = value.replace(',', ''); // remove comma
            inputValue = inputValue.slice(0, -1); // remove last digit
            while (inputValue.length < 3) {
                inputValue = '0' + inputValue; // pad with zeros
            }
            inputValue = inputValue.slice(0, -2) + ',' + inputValue.slice(-2);
            if (inputValue.startsWith('00')) {
                inputValue = inputValue.slice(1);
            }
            if (inputValue.startsWith('0') && inputValue.charAt(1) !== ',') {
                inputValue = inputValue.slice(1);
            }

            setValue(inputValue);
            event.preventDefault(); // prevent the default backspace behavior
            props.onChange?.(convertToNumber(inputValue));
        }
    };

    const handleCursor = () => {
        const inputEl = inputRef.current as any;
        inputEl.setSelectionRange(inputEl.value.length, inputEl.value.length);
    };

    useEffect(() => {
        setValue(convertToString(props.value));
    }, [props.value]);
    
    return (
        <TextField
            {...props.textFieldProps} 
            variant={props.textFieldProps?.variant ?? "outlined"}
            inputRef={inputRef}
            label={props.label}
            error={!!props.errorMsg}
            helperText={props.errorMsg}
            value={value}
            onChange={handleChange}
            onKeyDown={handleKeyDown}
            onMouseDown={handleCursor}
            onMouseUp={handleCursor}
            onFocus={handleCursor}
            onClick={handleCursor}
            slotProps={{
                htmlInput: {
                    min: 0, 
                    step: "0.01", 
                    style: { textAlign: "right", marginRight: "1rem", ...props.style },
                },
                input: !!props.endAdornment ? {
                    endAdornment: <InputAdornment position="start">{props.endAdornment}</InputAdornment>,
                } : undefined
            }}
        />
    );
}
export default DecimalInput;