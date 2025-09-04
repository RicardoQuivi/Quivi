import { useMemo } from "react";

interface Props {
    readonly name?: string;
    readonly width: number;
    readonly height: number;
}
export const useGenerateImage = (props: Props) => {
    const imageUrl = useMemo(() => {
        if(props.name == undefined) {
            return "";
        }

        // Generate initials
        const nameSplit = props.name.split(' ');
        const initials = nameSplit.length > 1 ? `${nameSplit[0][0]}${nameSplit[1][0]}` : nameSplit[0][0];

        // Generate color based on name
        let hash = 0;
        /* eslint-disable no-bitwise */
        for (let i = 0; i < props.name.length; i += 1) {
            hash = props.name.charCodeAt(i) + ((hash << 5) - hash);
        }
        let color = '#';
        for (let i = 0; i < 3; i += 1) {
            const value = (hash >> (i * 8)) & 0xff;
            color += `00${value.toString(16)}`.slice(-2);
        }
        /* eslint-enable no-bitwise */

        // Create canvas
        const canvas = document.createElement('canvas');
        canvas.width = props.width; // Match CardMedia height
        canvas.height = props.height;
        const ctx = canvas.getContext('2d');

        if (ctx) {
            // Fill background
            ctx.fillStyle = color;
            ctx.fillRect(0, 0, props.width, props.height);

            // Draw initials
            ctx.fillStyle = '#fff';
            ctx.font = 'bold 80px Arial';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText(initials, canvas.width / 2, canvas.height / 2); // Center text dynamically
        }

        // Convert canvas to image URL
        return canvas.toDataURL('image/png');
    }, [props.name, props.height, props.width]);

    return imageUrl;
}