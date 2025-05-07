import React, { CSSProperties, useState } from "react";
import { useEffect, } from "react";
import { default as MUILoadingButton } from '@mui/material/Button';
import { Box } from "@mui/material";

interface Props {
    readonly primaryButton?: boolean
    readonly className?: string;
    readonly overrideClassName?: boolean;
    readonly onClick?: (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => void | Promise<void>;
    readonly children: React.ReactNode;
    readonly disabled?: boolean;
    readonly style?: React.CSSProperties;
    readonly isLoading?: boolean;
}

const LoadingButton = React.forwardRef(({
    primaryButton,
    className,
    overrideClassName,
    onClick,
    children,
    disabled,
    style,
    isLoading,
}: Props, ref: React.Ref<HTMLButtonElement>) => {
    const [currentClassName, setCurrentClassName] = useState("");
    const [isPrimary, setIsPrimary] = useState(false);

    const isDisabled = isLoading == true || (disabled === true);

    useEffect(() => {
        setIsPrimary(primaryButton ?? true);
    }, [primaryButton]);

    useEffect(() => {
        let classNameTemp = isPrimary ? "main-btn" : "main-btn-outline";
        if (isDisabled) {
            classNameTemp = `${classNameTemp} ${classNameTemp}--inactive`;
        }
        if (className) {
            classNameTemp = overrideClassName ? className : `${classNameTemp} ${className}`;
        }
        setCurrentClassName(classNameTemp);
    }, [className, overrideClassName, isDisabled, primaryButton, isPrimary]);

    const getStyle = (): CSSProperties => {
        let result: CSSProperties = {pointerEvents: isDisabled ? "none" : "auto", ...style};
        //rgba(0, 0, 0, 0.12)
        if(isDisabled) {
            result.color = "rgba(0, 0, 0, 0.26)"
            result.backgroundColor = "rgba(0, 0, 0, 0.12)"
        }
        return result;
    }

    //This is temporary. This property should be discontinued, so we should go thorugh all places using it
    //and remove it
    if(overrideClassName != true) {
        return (
            <MUILoadingButton 
                variant={primaryButton ? "contained" : "outlined"}
                className={className}
                loading={isLoading}
                disabled={disabled}
                onClick={(e) => onClick?.(e)}
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
    }
    return (
        <button 
            disabled={isDisabled} 
            type="button" 
            className={currentClassName}
            onClick={(e) => onClick?.(e)}
            ref={ref}
            style={getStyle()}>
            <Box style={{position: "relative", width: "100%", height: "100%", alignContent: "center"}}>
                <Box style={{visibility: isLoading == true ? "hidden" : "visible"}}>
                    {children}
                </Box>

                <Box style={{visibility: isLoading == true ? "visible" : "hidden", position: "absolute", top: 0, left: 0, right: 0, bottom: 0}}>
                    <Box style={{display: "flex", height: "100%", width: "100%", justifyContent: "center"}}>
                        <Box className="spinner-border" role="status" style={{width: "auto", height: "100%", aspectRatio: 1}}>
                            <span className="sr-only">Loading...</span>
                        </Box>
                    </Box>
                </Box>
            </Box>
        </button>
    )
})
export default LoadingButton;