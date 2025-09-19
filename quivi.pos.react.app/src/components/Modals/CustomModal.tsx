import { Box, Breakpoint, Dialog, DialogActions, DialogContent, DialogTitle, IconButton } from "@mui/material";
import React, { ReactNode, useEffect } from "react";
import { CrossIcon } from "../../icons";

export enum ModalSize {
    Default="md",
    ExtraSmall="xs",
    Small="sm",
    Medium="md",
    Large="lg",
    ExtraLarge="xl",
    FullScreen="false",
}

export interface CustomModalProps {
    readonly isOpen: boolean;
    readonly onOpen?: () => any;
    readonly onClose?: () => any;
    readonly title: string | React.ReactNode;
    readonly children: React.ReactNode;
    readonly footer?: ReactNode;
    readonly disableCloseOutsideModal?: boolean;
    readonly hideClose?: boolean;
    readonly size?: ModalSize;
}

const CustomModal: React.FC<CustomModalProps> = ({
    isOpen,
    onOpen,
    onClose,
    title,
    children,
    footer,
    disableCloseOutsideModal,
    hideClose,
    size,
}) => {

    const getModalSizeValue = (key: ModalSize): Breakpoint | false => {
        switch(key)
        {
            case ModalSize.FullScreen: return false;
        }
        return key;
    }

    useEffect(() => {
        if(isOpen == false) {
            return;
        }
        onOpen?.();
    }, [isOpen])

    return (
        <Dialog
            sx={{
                '& .MuiDialogContent-root': {
                    padding: 1,
                },
                '& .MuiDialogActions-root': {
                    padding: 1,
                },
            }}
            maxWidth={getModalSizeValue(size ?? ModalSize.Small)}
            fullWidth
            fullScreen={size == ModalSize.FullScreen}
            open={isOpen}
            onClose={(_, reason) => {
                if (disableCloseOutsideModal && reason == "backdropClick") {
                    return;
                }
                onClose?.();
            }}
        >
            <DialogTitle
                sx={{
                    m: 0,
                    p: 2,
                }}
            >
            {
                typeof title === "string"
                ? 
                <Box
                    sx={{
                        display: "flex",
                        justifyContent: "center",
                    }}
                >
                    {title}
                </Box>
                :
                title
            }
            </DialogTitle>
            {
                hideClose != true &&
                <IconButton
                    aria-label="close"
                    onClick={onClose}
                    sx={{
                        position: 'absolute',
                        right: 8,
                        top: 8,
                        stroke: (theme) => theme.palette.grey[500],
                    }}
                >
                    <CrossIcon />
                </IconButton>
            }
            <DialogContent dividers>
                {children}
            </DialogContent>
            
            {
                footer != undefined &&
                <DialogActions>
                    {footer}
                </DialogActions>
            }
        </Dialog>
    );
}
export default CustomModal;