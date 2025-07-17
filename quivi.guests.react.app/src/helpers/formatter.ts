import { Calculations } from "./calculations";

export class Formatter {
    // Formats total currency as user types
    static formatTotal = (valueNumber: number) => {
        let value = valueNumber.toFixed(2);
        const clean = value.replace(/\./g, "").replace(/^0+/, "");
        value = clean;

        if (value.length === 2) value = "0" + value;
        if (value.length === 1) value = "00" + value;

        let formatted = "";
        for (let i = 0; i < value.length; i++) {
            let sep = "";
            if (i === 2) sep = ",";
            if (i > 3 && (i + 1) % 3 === 0) sep = " ";
            formatted =
                value.substring(value.length - 1 - i, value.length - i) +
                sep +
                formatted;
        }
        return formatted;
    };

    // Formats a number as a string with 2 decimals and comma
    static amount = (value: number) => {
        return value.toFixed(2).replace(".", ",");
    }

    // Correctly floats decimals
    static floatify = (number: number) => {
        return parseFloat((number).toFixed(2));
    }

    static formatUrl = (url: string) => {
        if (!url?.includes("http")) {
            return `https://${url}`
        } else {
            return url;
        }
    }

    static price = (value: number, currencySymbol: string = "€"): string => {
        return `${this.amount(Calculations.roundUp(value))} ${currencySymbol}`;
    }

    static number = (n: number, language: string) => new Intl.NumberFormat(language).format(n)

    static currency = (price: number, language: string) =>{
        const currencyIso = "EUR";
        return new Intl.NumberFormat(language, { style: 'currency', currency: currencyIso }).format(price);
    }

    static cleanString = (str: string) => {
        return str.replaceAll(".", "").replaceAll(" ", "");
    }
}