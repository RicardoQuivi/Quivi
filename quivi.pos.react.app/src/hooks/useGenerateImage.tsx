import { useMemo } from "react";
import { useGeneratedColor } from "./useGeneratedColor";

interface Props {
    readonly name?: string;
    readonly width: number;
    readonly height: number;
}
export const useGenerateImage = (props: Props) => {
    const color = useGeneratedColor(props.name ?? "");

    const imageUrl = useMemo(() => {
        if(props.name == undefined || color == undefined) {
            return undefined;
        }

        const nameSplit = props.name.split(' ');
        const initials = nameSplit.length > 1 ? `${nameSplit[0][0]}${nameSplit[1][0]}` : nameSplit[0][0];

        const canvas = document.createElement('canvas');
        canvas.width = props.width;
        canvas.height = props.height;
        const ctx = canvas.getContext('2d');

        if (ctx) {
            ctx.fillStyle = color;
            ctx.fillRect(0, 0, props.width, props.height);

            ctx.fillStyle = '#fff';
            ctx.font = 'bold 80px Arial';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText(initials, canvas.width / 2, canvas.height / 2);
        }

        return canvas.toDataURL('image/png');
    }, [props.name, props.height, props.width]);

    return imageUrl;
}