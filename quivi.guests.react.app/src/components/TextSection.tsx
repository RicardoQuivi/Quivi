import React, { useEffect, useState, type JSX } from "react";

const placeholder = "...";

interface Props {
    readonly element: JSX.Element;
    readonly text?: string;
    readonly maxLenght: number;
    readonly toggle?: (isCollapsed: boolean) => JSX.Element | undefined;
}
export const TextSection = (props: Props) => {
    const [state, setState] = useState({
        isCollapsed: true,
        isCollapseEnabled: (props.text?.length ?? 0) + placeholder.length > props.maxLenght,
        collapsedText: props.text == undefined ? undefined : props.text.substring(0, props.maxLenght) + placeholder,
    })

    useEffect(() => setState({
        isCollapsed: true,
        isCollapseEnabled: (props.text?.length ?? 0) + placeholder.length > props.maxLenght,
        collapsedText: props.text == undefined ? undefined : props.text.substring(0, props.maxLenght) + placeholder,
    }), [props.text])

    const getChildren = (): React.ReactNode => {
        if(state.collapsedText == null || state.isCollapseEnabled == false) {
            return props.text;
        }

        const button = props.toggle?.(state.isCollapsed);
        if(button == undefined) {
            return state.collapsedText;
        }
        return <>
            {state.isCollapsed ? state.collapsedText : props.text}
            &nbsp;&nbsp;&nbsp;
            {React.cloneElement(button, {
                onClick: () => setState(p => ({...p, isCollapsed: !p.isCollapsed}))
            })}
        </>;
    }

    return React.cloneElement(props.element, {}, getChildren());
}