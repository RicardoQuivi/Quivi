import { useRef, useEffect, useMemo } from "react";
import { IconButton } from "../button/IconButton";
import { CloseIcon } from "../../../icons";

export enum ModalSize {
    Default,
    ExtraSmall,
    Small,
    Medium,
    Large,
    ExtraLarge,
    FullScreen,
}

const fromSize = (size: ModalSize | undefined): string => {
    if(size == undefined) {
        size = ModalSize.Default;
    }

    switch(size)
    {
        case ModalSize.Default: return "[700px]";
        case ModalSize.ExtraSmall: return "[700px]";
        case ModalSize.Small: return "[700px]";
        case ModalSize.Medium: return "[700px]";
        case ModalSize.Large: return "[700px]";
        case ModalSize.ExtraLarge: return "[700px]";
        case ModalSize.FullScreen: return "full";
    }
}

interface ModalProps {
    readonly isOpen: boolean;
    readonly onOpen?: () => any;
    readonly onClose: () => any;
    readonly title?: string | React.ReactNode;
    readonly children: React.ReactNode;
    readonly footer?: React.ReactNode;
    readonly showCloseButton?: boolean;
    readonly size?: ModalSize;

    readonly className?: string;
}

export const Modal = (props: ModalProps) => {
    const modalRef = useRef<HTMLDivElement>(null);

    const size = useMemo(() => fromSize(props.size), [props.size])

    useEffect(() => {
        const handleEscape = (event: KeyboardEvent) => {
            if (event.key === "Escape") {
                props.onClose();
            }
        };

        if (props.isOpen) {
            document.addEventListener("keydown", handleEscape);
        }

        return () => {
            document.removeEventListener("keydown", handleEscape);
        };
    }, [props.isOpen, props.onClose]);

    useEffect(() => {
        if (props.isOpen) {
            document.body.style.overflow = "hidden";
        } else {
            document.body.style.overflow = "unset";
        }

        return () => {
            document.body.style.overflow = "unset";
        };
    }, [props.isOpen]);

    useEffect(() => {
        if(props.isOpen== false) {
            return;
        }
        props.onOpen?.();
    }, [props.isOpen])

    if (!props.isOpen) {
        return null;
    }

    const contentClasses = props.size == ModalSize.FullScreen ? "w-full h-full" : "relative w-full rounded-3xl bg-white dark:bg-gray-900";

    return (
        <div className="fixed inset-0 flex items-center justify-center overflow-y-auto modal z-99999">
            {
                props.size != ModalSize.FullScreen &&
                <div
                    className="fixed inset-0 h-full w-full bg-gray-400/50 backdrop-blur-[10px]"
                    onClick={props.onClose}
                />
            }
            <div
                ref={modalRef}
                className={`${contentClasses} max-w-${size} ${props.className ?? ""}`}
                onClick={(e) => e.stopPropagation()}
            >
                {
                    props.showCloseButton &&
                    <button
                        onClick={props.onClose}
                        className="absolute right-3 top-3 z-999 flex h-9.5 w-9.5 items-center justify-center rounded-full bg-gray-100 text-gray-400 transition-colors hover:bg-gray-200 hover:text-gray-700 dark:bg-gray-800 dark:text-gray-400 dark:hover:bg-gray-700 dark:hover:text-white sm:right-6 sm:top-6 sm:h-11 sm:w-11"
                    >
                        <svg
                            width="24"
                            height="24"
                            viewBox="0 0 24 24"
                            fill="none"
                            xmlns="http://www.w3.org/2000/svg"
                        >
                            <path
                                fillRule="evenodd"
                                clipRule="evenodd"
                                d="M6.04289 16.5413C5.65237 16.9318 5.65237 17.565 6.04289 17.9555C6.43342 18.346 7.06658 18.346 7.45711 17.9555L11.9987 13.4139L16.5408 17.956C16.9313 18.3466 17.5645 18.3466 17.955 17.956C18.3455 17.5655 18.3455 16.9323 17.955 16.5418L13.4129 11.9997L17.955 7.4576C18.3455 7.06707 18.3455 6.43391 17.955 6.04338C17.5645 5.65286 16.9313 5.65286 16.5408 6.04338L11.9987 10.5855L7.45711 6.0439C7.06658 5.65338 6.43342 5.65338 6.04289 6.0439C5.65237 6.43442 5.65237 7.06759 6.04289 7.45811L10.5845 11.9997L6.04289 16.5413Z"
                                fill="currentColor"
                            />
                        </svg>
                    </button>
                }

                {/* Modal content */}
                <div className={`relative no-scrollbar relative w-full overflow-y-auto rounded-3xl bg-white p-4 dark:bg-gray-900 lg:p-8`}>
                    {/* Modal header */}
                    <div className="flex items-center justify-between p-4 md:p-5 border-b rounded-t dark:border-gray-600 border-gray-200">
                        <h4 className="text-2xl font-semibold text-gray-800 dark:text-white/90">
                            {props.title}
                        </h4>
                        <IconButton
                            onClick={props.onClose}
                        >
                            <CloseIcon className="size-8" />
                        </IconButton>
                    </div>

                    {/*Modal body */}
                    <div className="p-4 md:p-5 space-y-4">
                        {props.children}
                    </div>
                    
                    {/* Modal footer */}
                    {
                        props.footer != undefined &&
                        <div className="flex items-center p-4 md:p-5 border-t border-gray-200 rounded-b dark:border-gray-600">
                            {props.footer}
                            {/* <button data-modal-hide="default-modal" type="button" className="text-white bg-blue-700 hover:bg-blue-800 focus:ring-4 focus:outline-none focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5 text-center dark:bg-blue-600 dark:hover:bg-blue-700 dark:focus:ring-blue-800">I accept</button>
                            <button data-modal-hide="default-modal" type="button" className="py-2.5 px-5 ms-3 text-sm font-medium text-gray-900 focus:outline-none bg-white rounded-lg border border-gray-200 hover:bg-gray-100 hover:text-blue-700 focus:z-10 focus:ring-4 focus:ring-gray-100 dark:focus:ring-gray-700 dark:bg-gray-800 dark:text-gray-400 dark:border-gray-600 dark:hover:text-white dark:hover:bg-gray-700">Decline</button> */}
                        </div>
                    }
                </div>
            </div>
        </div>
    );
};
