import { useMemo } from "react";

export const useGeneratedColor = (name?: string) => {
    const color = useMemo(() => {
        if(name == undefined) {
            return undefined;
        }

        // Generate color based on name
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
    }, [name]);

    return color;
}