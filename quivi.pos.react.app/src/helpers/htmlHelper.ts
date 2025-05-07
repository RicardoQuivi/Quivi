export class Html {
    static getValueFromMetaTag = <T,>(key: string, defaultValue: T): string | T => {
        const metas = document.getElementsByTagName('meta');
        for (let i = 0; i < metas.length; ++i) {
            const name = metas[i].getAttribute('name')
            if (name != key)
                continue;

            return metas[i].getAttribute('value') ?? defaultValue;
        }
        return defaultValue;
    }

    static onFocusSetCursorToEnd = (event: React.FocusEvent<HTMLInputElement>) => {
        const input = event.target;
        if (input) {
            // Set the cursor position to the end of the input
            input.selectionStart = input.value.length;
            input.selectionEnd = input.value.length;
        }
    }

    static hexToRgbaColor = (hex: string, alpha?: number) => {
        let props: number[] = [
            parseInt(hex.slice(1, 3), 16), // R
            parseInt(hex.slice(3, 5), 16), // G
            parseInt(hex.slice(5, 7), 16), // B
        ];

        if (alpha != undefined)
            props.push(alpha);

        return `rgba(${props.join(",")})`;
    }
}