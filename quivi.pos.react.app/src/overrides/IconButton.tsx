import React, { useCallback } from "react";
import { IconButton as MUIIconButton, IconButtonProps } from "@mui/material";

export function IconButton(props: IconButtonProps) {
    const handleClick = useCallback((event: React.MouseEvent<HTMLButtonElement>) => {
        if ("vibrate" in navigator) {
            navigator.vibrate(20);
        }
        props.onClick?.(event);
    }, [props.onClick])

    return <MUIIconButton {...props} onClick={handleClick} />;
}
export default IconButton;