import type { DetailedHTMLProps, HTMLAttributes } from "react";

interface Props extends DetailedHTMLProps<HTMLAttributes<HTMLDivElement>, HTMLDivElement> {
    readonly children: React.ReactNode;
}
export const TextDivider = ({
    children, 
    ...props
}: Props) => {
    return (
    <div {...props} style={{display: "flex", flexDirection: "row", ...(props.style ?? {})}}>
        <hr style={{flexGrow: 1, alignSelf: "center"}} />
        <p style={{display: "inline-block", padding: "0 10px", alignSelf: "center" }}>{children}</p>
        <hr style={{flexGrow: 1, alignSelf: "center"}} />
    </div>)
}