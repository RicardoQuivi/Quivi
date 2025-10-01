import { useRef, type CSSProperties } from "react";
import { styled } from '@mui/styles';
import type { Theme } from "@mui/material/styles";
import { type IColor } from "../../hooks/theme/useQuiviTheme";
import { CircularProgress } from "@mui/material";
import { Colors } from "../../helpers/colors";

interface StyledCircularProgressProps {
    readonly primarycolor: IColor;
}
 
const StyledCircularProgress = styled(CircularProgress)<Theme, StyledCircularProgressProps>(({
    primarycolor,
}) => ({
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

export interface SquareButtonProps {
    readonly color: IColor;
    readonly showShadow?: boolean;
    readonly borderRadius?: string;
    readonly className?: string; 
    readonly onClick?: () => void;
    readonly disabled?: boolean;
    readonly isLoading?: boolean;
    readonly style?: CSSProperties;
    readonly children: React.ReactNode;
}
export const SquareButton: React.FC<SquareButtonProps> = (props) => {
    const showShadow = props.showShadow ?? false;
    const borderRadius = props.borderRadius ?? undefined;
    const customClass = props.className ?? "";
    
    const spanRef = useRef<HTMLDivElement>(null);

    return (
        <div
            className={customClass} 
            onClick={() => props.disabled != true && props.onClick?.()}
            style={{
                ...(props.style ?? {}),

                cursor: props.disabled ? undefined : "pointer",
                padding: "1rem",
                textAlign: "center",
                backgroundColor: props.color.hex,
                boxShadow: showShadow ? `0px 4px 24px rgba(${props.color.r}, ${props.color.g}, ${props.color.b}, 0.4)` : "",
                borderRadius: borderRadius != undefined ? borderRadius : "0",
                opacity: props.disabled ? 0.3 : undefined,
            }}
        >
            <div style={{position: "relative", width: "100%", height: "100%"}}>
                <div style={{visibility: props.isLoading == true ? "hidden" : "visible"}} ref={spanRef}>
                    {props.children}
                </div>
                {
                    props.isLoading == true &&
                    <StyledCircularProgress primarycolor={Colors.fromHex("#FFFFFF")} size={`${spanRef.current?.offsetHeight}px`}/>
                }
            </div>
        </div>
    );
}