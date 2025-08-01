import { memo } from "react";

interface Props {
    readonly url: string;
}
const Paybyrd3DSFrame: React.FC<Props> = ({
    url
}) => {
    if(!url) {
        return <></>
    }
    return (
        <iframe src={url} style={{width: "-webkit-fill-available", height: "-webkit-fill-available", border: "0 none transparent", position: "absolute", top: 0, bottom: 0, left: 0, right: 0}}/>
    )
}
export default memo(Paybyrd3DSFrame);