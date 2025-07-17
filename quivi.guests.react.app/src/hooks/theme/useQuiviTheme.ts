export interface IColor  {
    readonly r: number;
    readonly g: number;
    readonly b: number;
    readonly hex: string;
}

export const fromHex = (hex: string): IColor => {
    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);

    return {
        hex: hex,
        r: result ? parseInt(result[1], 16) : 0,
        g: result ? parseInt(result[2], 16) : 0,
        b: result ? parseInt(result[3], 16) : 0,
    }
}

export const useQuiviTheme = () => {
    return ({
        primaryColor: fromHex("#FF3F01"),
    });
};