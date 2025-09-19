import React, { useEffect, useRef, useState } from "react";
import LoadingButton from "./LoadingButton";

interface Props {
    readonly disabled?: boolean;
    readonly onAction: () => Promise<void>;
    readonly children?: React.ReactNode;
    readonly confirmText: string;
    readonly timeoutMillis?: number;
    readonly primaryButton?: boolean;
    readonly className?: string;
    readonly overrideClassName?: boolean;
    readonly style?: React.CSSProperties;
    readonly stopPropagation?: boolean;
}

const ConfirmButton: React.FC<Props> = ({
    disabled,
    onAction,
    children,
    confirmText,
    timeoutMillis,
    primaryButton,
    className,
    style,
}) => {
    const ref = useRef<HTMLButtonElement>(null);
    const [clicked, setClicked] = useState(false);
    const [timer, setTimer] = useState<NodeJS.Timeout>();
    const [isLoading, setIsLoading] = useState(false);

    const onClickHandle = async (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
        e.stopPropagation();
        if(clicked) {
            setIsLoading(true);
            return;
        }
        setClicked(true);
    }

    useEffect(() => {
        if(clicked) {
            const interval = setTimeout(() => {
                setClicked(false);
            }, timeoutMillis ?? 5000);
            setTimer(interval);

            const eventHandler = () => {
                ref.current && removeEventListener("focusout", eventHandler);
                clearTimeout(timer);
            }; 
            ref.current && ref.current.addEventListener("focusout", eventHandler);
            return () => clearTimeout(interval);
        }
    }, [clicked])

    useEffect(() => {
        if(isLoading == false) {
            setClicked(false);
            return;
        }

        onAction().finally(() => setIsLoading(false));
    }, [isLoading])

    return (
        <LoadingButton 
            disabled={disabled}
            style={style}
            className={className}
            onClick={onClickHandle} 
            ref={ref}
            isLoading={isLoading}
            primaryButton={primaryButton}
        >
            {clicked ? confirmText : children}
        </LoadingButton>
    )
}
export default ConfirmButton;