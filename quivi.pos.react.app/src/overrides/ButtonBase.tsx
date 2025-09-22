import React, { useCallback } from "react";
import { ButtonBase as MUIButtonBase, ButtonBaseProps } from "@mui/material";

export function ButtonBase(props: ButtonBaseProps) {
    const handleClick = useCallback((event: React.MouseEvent<HTMLButtonElement>) => {
        if ("vibrate" in navigator) {
            navigator.vibrate(20);
        }
        props.onClick?.(event);
    }, [props.onClick])

    return <MUIButtonBase {...props} onClick={handleClick} />;
}
export default ButtonBase;