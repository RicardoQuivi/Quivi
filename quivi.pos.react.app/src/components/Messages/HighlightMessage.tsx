import * as React from 'react';
import Alert from '@mui/material/Alert';
import { SxProps } from '@mui/material';
import { Theme } from '@mui/system';

export enum MessageType {
    neutral = "info",
    information = "info",
    success = "success",
    warning = "warning",
    danger = "error",
}

interface Props {
    readonly messageType: MessageType,
    readonly className?: string;
    readonly children: React.ReactNode,
    readonly sx?: SxProps<Theme>;
}

const HighlightMessage: React.FC<Props> = (props: Props) => {
    return (
        <Alert className={props.className} severity={props.messageType} sx={props.sx}>
            {props.children}
        </Alert>
    );
}
export default HighlightMessage;