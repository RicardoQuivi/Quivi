import { type IColor } from "../hooks/theme/useQuiviTheme";

export class Colors {
    static shadeColor = (color: IColor, decimal: number): IColor => {   
        let r = color.r;
        let g = color.g;
        let b = color.b;

        r = Math.round(r / decimal);
        g = Math.round(g / decimal);
        b = Math.round(b / decimal);

        r = (r < 255)? r : 255;
        g = (g < 255)? g : 255;
        b = (b < 255)? b : 255;

        return this.fromRgb(r, g, b);
    }

    static fromRgb = (r: number, g: number, b: number): IColor => {
        const format = (c: number) => c.toString(16);

        return {
            hex: `#${format(r)}${format(g)}${format(b)}`,
            r: r,
            g: g,
            b: b,
        }
    }

    static fromHex = (hex: string): IColor => {
        var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);

        return {
            hex: hex,
            r: result ? parseInt(result[1], 16) : 0,
            g: result ? parseInt(result[2], 16) : 0,
            b: result ? parseInt(result[3], 16) : 0,
        }
    }
}