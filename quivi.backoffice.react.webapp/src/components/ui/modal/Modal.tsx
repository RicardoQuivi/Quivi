import { useRef, useEffect, useMemo, forwardRef } from "react";
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
    Auto,
}

const fromSize = (size: ModalSize | undefined): string => {
    if (size == undefined) {
        size = ModalSize.Default;
    }

    switch (size) {
        case ModalSize.Default: return "max-w-[700px]";
        case ModalSize.ExtraSmall: return "max-w-[700px]";
        case ModalSize.Small: return "max-w-[700px]";
        case ModalSize.Medium: return "max-w-[700px]";
        case ModalSize.Large: return "max-w-[700px]";
        case ModalSize.ExtraLarge: return "max-w-[700px]";
        case ModalSize.FullScreen: return "full";
        case ModalSize.Auto: return "container";
    }
}

interface ModalProps {
    readonly isOpen: boolean;
    readonly onOpen?: () => any;
    readonly onClose?: () => any;
    readonly title?: string | React.ReactNode;
    readonly children: React.ReactNode;
    readonly footer?: React.ReactNode;
    readonly hideCloseButton?: boolean;
    readonly size?: ModalSize;
    readonly className?: string;
}

export const Modal = forwardRef<HTMLDivElement, ModalProps>((props, ref) => {
    const modalRef = useRef<HTMLDivElement>(null);

    const size = useMemo(() => fromSize(props.size), [props.size]);

    useEffect(() => {
        const handleEscape = (event: KeyboardEvent) => {
            if (event.key === "Escape") {
                props.onClose?.();
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
        if (props.isOpen === false) {
            return;
        }
        props.onOpen?.();
    }, [props.isOpen]);

    if (!props.isOpen) {
        return null;
    }

    const contentClasses = props.size === ModalSize.FullScreen ? "w-full h-full" : "relative w-full rounded-3xl bg-white dark:bg-gray-900 max-h-full";

    // Attach the forwarded ref to the modal container
    const modalContainerRef = ref as React.RefObject<HTMLDivElement> || modalRef;

    return (
        <div className="fixed inset-0 flex items-center justify-center overflow-y-auto modal z-99999">
            {
                props.size !== ModalSize.FullScreen &&
                <div
                    className="fixed inset-0 h-full w-full bg-gray-400/50 backdrop-blur-[10px]"
                    onClick={props.onClose}
                />
            }
            <div
                ref={modalContainerRef} // Attach ref here
                className={`${contentClasses} ${size} ${props.className ?? ""}`}
                onClick={(e) => e.stopPropagation()}
            >
                <div className="relative no-scrollbar relative w-full overflow-y-auto rounded-3xl bg-white p-4 dark:bg-gray-900 lg:p-8">
                    <div className="flex items-center justify-between p-4 md:p-5 border-b rounded-t dark:border-gray-600 border-gray-200">
                        <h4 className="text-2xl font-semibold text-gray-800 dark:text-white/90">
                            {props.title}
                        </h4>
                        {
                            props.hideCloseButton != true &&
                            <IconButton onClick={props.onClose}>
                                <CloseIcon className="size-8" />
                            </IconButton>
                        }
                    </div>

                    <div className="p-4 md:p-5 space-y-4">
                        {props.children}
                    </div>

                    {
                        props.footer != undefined &&
                        <div className="flex items-center p-4 md:p-5 border-t border-gray-200 rounded-b dark:border-gray-600">
                            {props.footer}
                        </div>
                    }
                </div>
            </div>
        </div>
    );
});
Modal.displayName = "Modal";