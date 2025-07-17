import { CircularProgress, styled } from "@mui/material";
import { useEffect, useRef } from "react";
import { useQuiviTheme, type IColor } from "../../hooks/theme/useQuiviTheme";

interface StyledCircularProgressProps {
    readonly primarycolor: IColor;
}
 
const StyledCircularProgress = styled(CircularProgress)(({
    primarycolor,
}: StyledCircularProgressProps) => ({
    margin: "auto",
    position: "absolute",
    top: 0,
    bottom: 0,
    left: 0,
    right: 0,

    "& .MuiCircularProgress-svg": {
        color: primarycolor.hex,
    }
}));

type Props = {
    primaryButton?: boolean
    className?: string;
    onClick: () => void | Promise<void>;
    children: React.ReactNode;
    disabled?: boolean;
    style?: React.CSSProperties;
    isLoading: boolean;
}

const LoadingButton: React.FC<Props> = ({
    primaryButton,
    className,
    onClick,
    children,
    disabled,
    style,
    isLoading
}) => {
    const theme = useQuiviTheme();
    const spanRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if(spanRef.current == null) {
            return;
        }
    }, [spanRef]);
    
    const isDisabled = isLoading || (disabled === true);
    return (
        <button 
            disabled={isDisabled} 
            type="button" 
            className={`${(primaryButton ?? true) ? `primary-button ${isDisabled ? "primary-button--inactive" : ""}` : `secondary-button ${isDisabled ? "secondary-button--inactive" : ""}`} ${className || ""}`} 
            onClick={() => onClick()}
            style={style}>
            <div style={{position: "relative", width: "100%", height: "100%"}}>
                <div style={{visibility: isLoading ? "hidden" : "visible"}} ref={spanRef}>
                    {children}
                </div>
                {
                    isLoading &&
                    <StyledCircularProgress primarycolor={theme.primaryColor} size={`${spanRef.current?.offsetHeight}px`}/>
                }
            </div>
        </button>
    )
}
export default LoadingButton;