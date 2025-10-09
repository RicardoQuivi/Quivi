export class EnumHelper {
    static getValues = <T extends object>(e: T): T[keyof T][] => {
        return Object.values(e).filter(v => typeof v === "number") as T[keyof T][];
    }
}