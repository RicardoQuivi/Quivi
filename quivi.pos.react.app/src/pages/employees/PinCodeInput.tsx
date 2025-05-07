import { useEffect, useRef, useState } from "react"
import Keyboard from "react-simple-keyboard";
import { CircularProgress } from "@mui/material";
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
    const [input, setInput] = useState(getPinFromValue(props.pin, totalInputs));
    const [currentDigit, setCurrentDigit] = useState<number | undefined>();
    const inputItems = Array.from({length: totalInputs}, () => useRef<HTMLInputElement>(null));

    useEffect(() => setInput(getPinFromValue(props.pin, totalInputs)), [props.pin])

    useEffect(() => ref.current?.focus(), [ref.current])
    useEffect(() => setCurrentDigit(input.trim().length), [input])
    useEffect(() => {
        const pin = input.trim();
        props.onChange(pin, pin.length == totalInputs);
    }, [input])

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
                setTimeout(() => element.type = "password", 350)
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
    }, [currentDigit, inputItems])

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
    }, [inputItems])

    const onKeyPadPress = (button: string) => {
        if(props.loading) {
            return;
        }
        
        if (button === "{clear}") {
            return setInput("");
        }

        if(button == "{bksp}" || button == "Backspace") {
            props.onDigitPress?.("Backspace");

            setInput(p => {    
                const index = currentDigit == undefined ? p.length - 1 : currentDigit - 1;
                if(index == -1) {
                    return p;
                }
                return p.substring(0, index) + ' ' + p.substring(index + 1);
            })
        }

        if(["1", "2", "3", "4", "5", "6", "7", "8", "9", "0"].findIndex(s => s == button) == -1) {
            return;
        }

        props.onDigitPress?.(button);
        setInput(p => {
            if(currentDigit == undefined || p.trim().length == totalInputs) {
                return p;
            }

            //The following trimStart is an attempt to solve an issue that is reproduced by (very) quickly spamming and
            //typing numbers and backspace. Eventually, somehow, the input string will be left with a space at the start
            //and thus leading into a bug where you can never fill the pin code.
            return (p.substring(0, currentDigit) + button + p.substring(currentDigit + 1)).trimStart();
        })
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

    <style>
        {`
  .custom-button-style {
    font-size: 20px; /* Ajuste o tamanho da fonte */
    font-weight: bold;
  }
`}
    </style>

    return <div onKeyDownCapture={(e) => onKeyPadPress(e.key)} tabIndex={-1} ref={ref} style={{outlineColor: "transparent"}}>
        <div style={{display: "flex", width: "100%", alignItems: "center", justifyContent: "center"}}>
            <div style={{width: "min-content", display: "grid", gridAutoFlow: "column", gridGap: "20px"}} className="mb-4">
                {
                    props.loading == true
                    ?
                        <CircularProgress style={{height: "4rem", width: "4rem" }}/>
                    :
                    getInputArray().map((c, i) => <input key={i} 
                                                            ref={inputItems[i]}
                                                            autoCapitalize="off" 
                                                            autoCorrect="off" 
                                                            autoComplete="off" 
                                                            inputMode="numeric" 
                                                            aria-required="true" 
                                                            value={c}
                                                            style={{
                                                                border: `2px solid ${currentDigit == i ? "#FF3F01" : "gray"}`,
                                                                fontSize: "2rem",
                                                                height: "4rem",
                                                                outline: "none",
                                                                textAlign: "center",
                                                                transitionDuration: "250ms",
                                                                transitionProperty: "color, border, box-shadow, transform",
                                                                width: "4rem",
                                                                boxShadow: currentDigit == i ? "0 0 0.25rem rgba(#FF3F01, 0.5)" : undefined
                                                            }}
                                                            disabled />)
                }
            </div>
        </div>
        <Keyboard
            layoutName="default"
            theme={"hg-theme-default hg-theme-numeric hg-layout-numeric numeric-theme hg-font-large"}
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
`}
        </style>
    </div>
}