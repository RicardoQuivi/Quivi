import React, { useEffect, useState } from "react";
import LoadingButton from "./LoadingButton";

interface Props {
    readonly primaryButton?: boolean
    readonly className?: string;
    readonly overrideClassName?: boolean;
    readonly children: React.ReactNode;
    readonly disabled?: boolean;
    readonly style?: React.CSSProperties;
    readonly onAction: () => Promise<any>;
}
export const ActionButton = (props: Props) => {
    const {
        onAction,
        ...otherProps
    } = props;

    const [isLoading, setIsLoading] = useState(false);

    const onClickHandle = async (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
        e.stopPropagation();
        setIsLoading(true);
    }

    useEffect(() => {
        if(isLoading == false) {
            return;
        }

        onAction().finally(() => setIsLoading(false));
    }, [isLoading])

    return <LoadingButton
            {...otherProps}
            onClick={onClickHandle}
            isLoading={isLoading}
        />
}