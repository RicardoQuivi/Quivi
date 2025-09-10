import { Button, ButtonGroup, TextField } from "@mui/material"
import React from "react"
import { MinusIcon, PlusIcon } from "../../icons";

interface Props {
    readonly name?: string;

    readonly value: number;
    readonly onChange: (n: number) => any; 
    readonly onBlur?: React.FocusEventHandler<HTMLButtonElement | HTMLInputElement | HTMLTextAreaElement> | undefined;
    readonly errorMessage?: string;
    readonly label?: string;

    readonly minValue?: number;
    readonly maxValue?: number;

    readonly decrementDisabled?: boolean;
    readonly incrementDisabled?: boolean;
}
export const NumberInputField = (props: Props) => {
    return (
        <ButtonGroup
            variant="outlined"
            sx={{
                width: "100%",
                display: "flex",
            }}
        >
            <Button
                disabled={(props.minValue == undefined ? false : props.value <= props.minValue) || props.decrementDisabled == true}
                onClick={() => props.onChange(+props.value - 1)}
                onBlur={props.onBlur}
                name={props.name}
            >
                <MinusIcon height={15} width={15} />
            </Button>
            <TextField
                error={!!props.errorMessage}
                helperText={props.errorMessage}
                label={props.label}
                type="number"
                value={props.value}
                onBlur={props.onBlur}
                name={props.name}
                sx={{ 
                    flex: 1,
                    pointerEvents: "none",

                    "& input": {
                        textAlign: "center",
                        pointerEvents: "none", 
                    }
                }}
                slotProps={{
                    input: {
                        readOnly: true,
                    }
                }}
            />
            <Button
                disabled={(props.maxValue == undefined ? false : props.value >= props.maxValue) || props.incrementDisabled == true}
                onClick={() => props.onChange(+props.value + 1)}
                onBlur={props.onBlur}
                name={props.name}
            >
                <PlusIcon height={15} width={15} />
            </Button>
        </ButtonGroup>
    )
}