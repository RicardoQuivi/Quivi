import React from "react";
import { default as MUILoadingButton } from '@mui/material/Button';

interface Props {
    readonly primaryButton?: boolean
    readonly className?: string;
    readonly onClick?: (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => void | Promise<void>;
    readonly children: React.ReactNode;
    readonly disabled?: boolean;
    readonly style?: React.CSSProperties;
    readonly isLoading?: boolean;
}

const LoadingButton = React.forwardRef(({
    primaryButton,
    className,
    onClick,
    children,
    disabled,
    style,
    isLoading,
}: Props, ref: React.Ref<HTMLButtonElement>) => {
    return (
        <MUILoadingButton 
            variant={primaryButton ? "contained" : "outlined"}
            className={className}
            loading={isLoading}
            disabled={disabled}
            onClick={onClick}
            style={style}
            ref={ref}
            sx={{
                "& .MuiLoadingButton-label": {
                    width: "100%",
                }
            }}
        >
            {children}
        </MUILoadingButton>
    )
})
export default LoadingButton;