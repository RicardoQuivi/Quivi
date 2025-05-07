import React, { useMemo } from "react";
import { FormControl, InputLabel, MenuItem, Select } from "@mui/material";

export const SingleSelect = <T,>(props: {
    style?: React.CSSProperties,
    label?: React.ReactNode,
    disabled?: boolean,
    value: T,
    options: T[],
    getId: (e: T) => string;
    render: (e: T) => React.ReactNode;
    onChange: (value: T) => void,
}) => {
    const itemsMap = useMemo(() => props.options.reduce((r, o) => {
        r.set(props.getId(o), o);
        return r;
    }, new Map<string, T>), [props.options, props.getId])
    
    return (
        <FormControl fullWidth>
            {!!props.label && <InputLabel>{props.label}</InputLabel> }
            <Select
                value={props.getId(props.value)}
                label={props.label}
                onChange={e => props.onChange(itemsMap.get(e.target.value.toString())!)}
                MenuProps={{
                    style: {
                        zIndex: 9999999,
                    }
                }}
                style={props.style}
                disabled={props.disabled}
            >
                {
                    props.options.map(option => {
                        const id = props.getId(option);
                        return <MenuItem key={id} value={id}>
                            {props.render(option)}
                        </MenuItem>
                    })
                }
            </Select>
        </FormControl>
    );
}