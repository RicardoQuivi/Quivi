import { DialogContent, Dialog as MyDialog, Slide, styled, type SlideProps } from "@mui/material";
import React, { useEffect, useState } from "react";
import type { DialogProps } from "./Dialog";

const Transition = React.forwardRef((props: SlideProps, ref: React.Ref<unknown>) => <Slide direction="up" ref={ref} {...props} />);

const StyledDialog = styled(MyDialog)(({
    style
}) => ({
    "& .MuiDialog-paper": {
        ...(style ?? {}),

        "& .MuiDialogContent-root": {
            overflowY: "hidden",
            display: "flex",
            flexDirection: "column",
        }
    },
}));

export const Modal = (props: DialogProps) => {
    const animationDuration = (props.animationDurationMilliseconds || 350) / 1000.0;

    const [isOpen, setIsOpen] = useState(props.isOpen);
    
    useEffect(() => {
        setIsOpen(props.isOpen);
    }, [props.isOpen])

    return (
        <StyledDialog
            open={isOpen}
            slots={{
                transition: Transition,
            }}
            slotProps={{
                transition: {
                    timeout: animationDuration * 1000,
                    onExited: () => props.onClose(),
                }
            }}
            keepMounted
            fullWidth
            maxWidth="md"
            onClose={() => setIsOpen(false)}
            className={props.className}
            style={props.style}>
                <DialogContent>
                    {props.children}
                </DialogContent>
        </StyledDialog>
    )
}