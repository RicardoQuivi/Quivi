import React, { useCallback } from "react";
import { Button as MUIButton, ButtonProps } from "@mui/material";

export function Button(props: ButtonProps) {
    const handleClick = useCallback((event: React.MouseEvent<HTMLButtonElement>) => {
        if ("vibrate" in navigator) {
            navigator.vibrate(20);
        }
        props.onClick?.(event);
    }, [props.onClick])

    return <MUIButton {...props} onClick={handleClick} />;
}
export default Button;