import { Avatar } from "@mui/material";
import type { CSSProperties } from "react";

const getColor = (name: string) => {
    let hash = 0;
    /* eslint-disable no-bitwise */
    for (let i = 0; i < name.length; i += 1) {
        hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
  
    let color = '#';
    for (let i = 0; i < 3; i += 1) {
        const value = (hash >> (i * 8)) & 0xff;
        color += `00${value.toString(16)}`.slice(-2);
    }
    /* eslint-enable no-bitwise */
    return color;
}

interface Props extends React.HTMLProps<HTMLElement> {
    readonly name?: string;
}
export const AvatarImage = ({
    src,
    name,
    ref,
    style,
    ...props
}: Props) => {
    const hasPhoto = !!src;
    const hasName = !!name;
    const sx: CSSProperties  = {
        backgroundColor: !hasPhoto && hasName ? getColor(name) : undefined,
        transform: "translateZ(0)", //Weirdly, this solves an issue causing artifacts on iOS
        ...{ ...(style ?? {})}
    };

    return (
    <Avatar alt={name} src={hasPhoto ? src : undefined} {...props} style={sx}>
        {!hasPhoto && hasName && name.charAt(0)}
    </Avatar>
    )
}