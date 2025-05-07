import { ReactNode } from "react";

interface Props {
    readonly children: ReactNode;
    readonly onClick?: (evt: React.MouseEvent<HTMLElement, MouseEvent>) => void;
    readonly disabled?: boolean;
    readonly className?: string;
}
export const IconButton = (props: Props) => {
    return (
        <button
            type="button"
            className={`fill-white dark:fill-gray-800 text-gray-400 bg-transparent hover:bg-gray-200 hover:text-gray-900 rounded-lg text-sm w-8 h-8 inline-flex justify-center items-center dark:hover:bg-gray-600 dark:hover:text-white ${props.className ?? ""}`}
            onClick={props.onClick}
            disabled={props.disabled}
        >
            {props.children}
        </button>
    )
}