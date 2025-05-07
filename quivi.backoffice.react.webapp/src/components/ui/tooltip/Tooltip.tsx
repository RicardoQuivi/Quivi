import { useMemo } from "react";

export enum Placement {
    Top,
    Right,
    Left,
    Bottom,
}
interface Props {
    readonly message: string | React.ReactNode;
    readonly children: React.ReactNode;
    readonly placement?: Placement;
}

const topClasses = {
    container: "bottom-full left-1/2 mb-2.5 -translate-x-1/2",
    arrow: "-bottom-1 left-1/2 -translate-x-1/2",
}

const rightClasses = {
    container: "left-full top-1/2 ml-2.5 -translate-y-1/2",
    arrow: "-left-1.5 top-1/2 -translate-y-1/2",
}

const leftClasses = {
    container: "right-full top-1/2 mr-2.5 -translate-y-1/2",
    arrow: "-right-1.5 top-1/2 -translate-y-1/2",
}

const bottomClasses = {
    container: "top-full left-1/2 mb-2.5 -translate-x-1/2",
    arrow: "-top-1 left-1/2 -translate-x-1/2",
}

export const Tooltip = (props: Props) => {

    const classes = useMemo(() => {
        if(props.placement == undefined) {
            return topClasses;
        }

        switch(props.placement)
        {
            case Placement.Top: return topClasses;
            case Placement.Right: return rightClasses;
            case Placement.Left: return leftClasses;
            case Placement.Bottom: return bottomClasses;
        }
        return topClasses;
    }, [props.placement])

    return (
        <div className="relative inline-block group">
            {props.children}
            <div className={`invisible absolute ${classes.container} opacity-0 transition-opacity duration-300 group-hover:visible group-hover:opacity-100`}>
                <div className="relative">
                    <div className="drop-shadow-4xl whitespace-nowrap rounded-lg bg-[#1E2634] px-3 py-2 text-xs font-medium text-white">
                        {props.message}
                    </div>
                    <div className={`absolute ${classes.arrow} h-3 w-4 rotate-45 bg-[#1E2634]`}></div>
                </div>
            </div>
        </div>
    )
}