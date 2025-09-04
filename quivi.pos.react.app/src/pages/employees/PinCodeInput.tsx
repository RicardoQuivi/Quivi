import { useEffect, useMemo, useRef, useState } from "react"
import Keyboard from "react-simple-keyboard";
import { Box, CircularProgress, Grid, LinearProgress } from "@mui/material";
import 'react-simple-keyboard/build/css/index.css';

interface Props {
    readonly pin: string;
    readonly loading?: boolean;
    readonly onChange: (pinCode: string, isComplete: boolean) => any;
    readonly onDigitPress?: (digit: string) => any;
}
const totalInputs = 4;

const getPinFromValue = (pin: string, totalInputs: number) => {
    const remainingDigits = totalInputs - pin.length;
    if(remainingDigits <= 0) {
        return pin.substring(0, totalInputs);
    }

    return `${pin}${Array(remainingDigits).join(" ")}`;
}
export const PinCodeInput = (props: Props) => {
    const ref = useRef<HTMLDivElement>(null);
    const [currentDigit, setCurrentDigit] = useState<number | undefined>();
    const inputItems = Array.from({length: totalInputs}, () => useRef<HTMLInputElement>(null));

    const input = useMemo(() => getPinFromValue(props.pin, totalInputs), [totalInputs, props.pin]);

    useEffect(() => ref.current?.focus(), [ref.current])
    useEffect(() => setCurrentDigit(input.trim().length), [input])

    useEffect(() => {
        if(currentDigit == undefined) {
            for(let i = 0; i < inputItems.length; ++i) {
            
                const element = inputItems[i].current;
                if(element == null) {
                    continue;
                }
    
                element.type = "password";
            }

            if(props.pin.length == 0 || props.pin.length > totalInputs) {
                return;
            }

            const element = inputItems[props.pin.length - 1].current;
            if(element == undefined) {
                return;
            }

            element.type = "text";
            const timeout = setTimeout(() => element.focus(), 0);
            return () => {
                clearTimeout(timeout);
                setTimeout(() => element.type = "password", 3500)
            }
        }

        if(currentDigit >= totalInputs) {
            return;
        }

        const element = inputItems[currentDigit].current;
        if(element == null) {
            return;
        }

        element.type = "text";
        const timeout = setTimeout(() => element.focus(), 0);
        return () => {
            clearTimeout(timeout);
            setTimeout(() => element.type = "password", 350)
        }
    }, [currentDigit, ...inputItems])

    useEffect(() => {
        if(props.loading != true) {
            return;
        }

        const aux = inputItems.map(i => ({
            input: i.current,
            type: i.current?.type ?? "text",
        }));

        for(const item of aux) {
            if(item.input == null) {
                continue;
            }
            item.input.type = "password";
        }

        return () => {
            for(const item of aux) {
                if(item.input == null) {
                    continue;
                }
                item.input.type = item.type;
            }
        }
    }, [props.loading])

    useEffect(() => {
        for(let i = 0; i < inputItems.length; ++i) {
            const element = inputItems[i].current;
            if(element == null) {
                continue;
            }

            const value = input[i]?.trim() ?? "";
            const oldValue = element.value;
            if(oldValue != value) {
                element.type = i == currentDigit ? "text" : "password";
            }
        }
    }, inputItems)

    const triggerOnPinChange = (input: string) => {
        const pin = input.trim();
        props.onChange(pin, pin.length == totalInputs);
    }

    const onKeyPadPress = (button: string) => {
        if(props.loading) {
            return;
        }
        
        if (button === "{clear}") {
            return triggerOnPinChange("");
        }

        if(button == "{bksp}" || button == "Backspace") {
            props.onDigitPress?.("Backspace");

            const index = currentDigit == undefined ? input.length - 1 : currentDigit - 1;
            if(index == -1) {
                return;
            }
            triggerOnPinChange(input.substring(0, index) + ' ' + input.substring(index + 1));
            return;
        }

        if(["1", "2", "3", "4", "5", "6", "7", "8", "9", "0"].findIndex(s => s == button) == -1) {
            return;
        }

        props.onDigitPress?.(button);
        if(currentDigit == undefined || input.trim().length == totalInputs) {
            return;
        }

        //The following trimStart is an attempt to solve an issue that is reproduced by (very) quickly spamming and
        //typing numbers and backspace. Eventually, somehow, the input string will be left with a space at the start
        //and thus leading into a bug where you can never fill the pin code.
        triggerOnPinChange((input.substring(0, currentDigit) + button + input.substring(currentDigit + 1)).trimStart());
    }

    const getInputArray = () => {
        const r: string[] = [];
        for(let i = 0; i < totalInputs; ++i) {
            if(i < input.length) {
                r.push(input[i].trim());
            } else {
                r.push("");
            }
        }
        return r;
    }

    const loading = props.loading;
    return <Box
        onKeyDownCapture={(e) => onKeyPadPress(e.key)}
        tabIndex={-1}
        ref={ref}
        sx={{
            outlineColor: "transparent",
            aspectRatio: 1,
        }}
    >
        <Box
            sx={{
                marginBottom: "1.5rem",
                paddingX: "1rem",
                display: "flex",
                flexDirection: "column",
            }}
            rowGap={1}
        >
            <Grid
                container
                gap={2}
            >
                {
                    getInputArray().map((c, i) => <Grid
                        key={i} 
                        size="grow"
                    >
                        <Box
                            sx={{
                                border: p => `2px solid ${currentDigit == i ? p.palette.primary.main : "gray"}`,
                                boxShadow: currentDigit == i ? p => `0 0 0.25rem rgba(${p.palette.primary.main}, 0.5)` : undefined,
                                transitionProperty: "color, border, box-shadow, transform",
                                position: "relative",
                            }}
                        >
                            <input
                                ref={inputItems[i]}
                                type={inputItems[i].current?.type ?? "text"}
                                autoCapitalize="off" 
                                autoCorrect="off" 
                                autoComplete="off" 
                                inputMode="numeric" 
                                aria-required="true" 
                                value={loading && !c ? " " : c}
                                style={{
                                    border: `none`,
                                    fontSize: "2rem",
                                    outline: "none",
                                    textAlign: "center",
                                    transitionDuration: "250ms",
                                    width: "100%",
                                    aspectRatio: 1,
                                    visibility: loading ? "collapse" : undefined,
                                }}
                                disabled
                            />
                        </Box>
                    </Grid>)
                }
            </Grid>

            <LinearProgress
                sx={{
                    visibility: loading ? "visible" : "hidden",
                    height: {
                        xs: 5,
                        sm: 5,
                        md: 10,
                        lg: 10,
                        xl: 10,
                    },
                    borderRadius: 1,
                }}
            />
        </Box>
        <Keyboard
            layoutName="default"
            theme="hg-theme-default hg-theme-numeric hg-layout-numeric numeric-theme hg-font-large"
            layout={{
                default: ["1 2 3", "4 5 6", "7 8 9", "{clear} 0 {bksp}"],
            }}
            display={{
                "{clear}": "Clear",
                "{bksp}": "←",
            }}
            onKeyReleased={onKeyPadPress}
        />

        <style>
        {`
            .hg-font-large .hg-button {
                font-size: 24px;
            }

            .hg-theme-default.hg-layout-numeric .hg-button {
                height: auto !important;
                aspect-ratio: 2 !important;
            }
        `}
        </style>
    </Box>
}