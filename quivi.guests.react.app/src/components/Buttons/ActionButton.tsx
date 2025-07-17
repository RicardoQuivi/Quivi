import { useEffect, useRef, useState } from "react";
import LoadingButton from "./LoadingButton";

interface Props {
    readonly primaryButton?: boolean
    readonly className?: string;
    readonly onClick: () => Promise<void> | void;
    readonly children: React.ReactNode;
    readonly disabled?: boolean;
    readonly style?: React.CSSProperties;
}

const ActionButton: React.FC<Props> = ({
    primaryButton,
    className,
    onClick,
    children,
    disabled,
    style
}) => {
    const [isLoading, setIsLoading] = useState(false);
    const mounted = useRef(false);

    useEffect(() => {
        mounted.current = true;
        return () => {mounted.current = false;}
    }, []);

    const internalOnClick = async () => {
        setIsLoading(true);
        const clickResult = onClick();

        if (typeof clickResult?.finally === 'function') {
            clickResult.finally(() => mounted.current && setIsLoading(false));
        } else {
            mounted.current && setIsLoading(false);
        }
    }
    
    return (
        <LoadingButton isLoading={isLoading} primaryButton={primaryButton} className={className} onClick={internalOnClick} disabled={disabled} style={style}>
            {children}
        </LoadingButton>
    )
}
export default ActionButton;