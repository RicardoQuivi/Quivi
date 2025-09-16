import { useEffect } from "react";
import React from "react";
import { IconButton } from "@mui/material";
import { Modal } from "./Modal";
import Drawer from "./Drawer";
import { PageMode, usePageMode } from "../../hooks/usePageMode";
import { CloseIcon } from "../../icons";

export interface DialogProps {
    readonly isOpen: boolean;
    readonly onClose: () => void;
    readonly children?: React.ReactNode;
    readonly style?: React.CSSProperties;
    readonly disableClosing?: boolean;
    readonly animationDurationMilliseconds?: number; 
    readonly className?: string;
    readonly showCloseButton?: boolean;
}

const Dialog = (props: DialogProps) => {
    const pageMode = usePageMode();

    //#region Effects
    useEffect(() => {
        document.body.style.overflow = props.isOpen ? 'hidden' : 'auto';
    }, [props.isOpen])
    //#endregion
    
    const onClose = () => {
        document.body.style.overflow = 'auto';
        props.onClose?.();
    }
    
    return <>
        {
            props.isOpen && props.showCloseButton === true &&
            <IconButton
                aria-label="close"
                onClick={onClose}
                sx={{
                    zIndex: 999999,
                    position: 'absolute',
                    right: 20,
                    top: 20,
                }}
            >
                <CloseIcon />
            </IconButton>
        }
        {
            pageMode == PageMode.Kiosk 
            ? 
                <Modal {...props} onClose={onClose}/>
            :
                <Drawer {...props} onClose={onClose}/>
        }
    </>
};
export default Dialog;