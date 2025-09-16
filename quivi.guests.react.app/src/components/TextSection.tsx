import React, { useMemo, useState, type JSX } from "react";

const placeholder = "...";

interface Props {
    readonly element: JSX.Element;
    readonly text?: string;
    readonly maxLenght: number;
    readonly toggle?: (isCollapsed: boolean) => JSX.Element | undefined;
}
export const TextSection = (props: Props) => {
    const [isCollapsed, setIsCollapsed] = useState(true);

    const state = useMemo(() => ({
        isCollapseEnabled: (props.text?.length ?? 0) + placeholder.length > props.maxLenght,
        collapsedText: props.text == undefined ? undefined : props.text.substring(0, props.maxLenght) + placeholder,
    }), [props.text])

    const getChildren = (): React.ReactNode => {
        if(state.collapsedText == null || state.isCollapseEnabled == false) {
            return props.text;
        }

        const button = props.toggle?.(isCollapsed);
        if(button == undefined) {
            return state.collapsedText;
        }
        return <>
            {isCollapsed ? state.collapsedText : props.text}
            &nbsp;&nbsp;&nbsp;
            {React.cloneElement(button, {
                onClick: () => setIsCollapsed(p => !p)
            })}
        </>;
    }

    return React.cloneElement(props.element, {}, getChildren());
}