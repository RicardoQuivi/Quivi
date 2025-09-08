export class MapFunctions {
    static toMap = <T,>(data: T[], getId: (row: T) => string) => {
        const result = new Map<string, T>();
        for(const d of data) {
            result.set(getId(d), d);
        }
        return result;
    }
}