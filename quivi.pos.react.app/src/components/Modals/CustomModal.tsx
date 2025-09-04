import { Box, Breakpoint, Dialog, DialogActions, DialogContent, DialogTitle, IconButton, styled } from "@mui/material";
import React, { ReactNode, useEffect } from "react";
import { CloseIcon } from "../../icons";

export enum ModalSize {
    Default="md",
    ExtraSmall="xs",
    Small="sm",
    Medium="md",
    Large="lg",
    ExtraLarge="xl",
    FullScreen="false",
}

const BootstrapDialog = styled(Dialog)(({ }) => ({
    '& .MuiDialogContent-root': {
        padding: 2,
    },
    '& .MuiDialogActions-root': {
        padding: 1,
    },
}));

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
        <BootstrapDialog
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
                        color: (theme) => theme.palette.grey[500],
                    }}
                >
                    <CloseIcon />
                </IconButton>
            }
            <DialogContent dividers>
                <Box sx={{padding: 2}}>
                    {children}
                </Box>
            </DialogContent>
            
            {
                footer != undefined &&
                <DialogActions>
                    {footer}
                </DialogActions>
            }
        </BootstrapDialog>
    );
}
export default CustomModal;