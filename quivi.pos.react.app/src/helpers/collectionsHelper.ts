export class CollectionFunctions {
    static toMap = <T,>(data: T[], getId: (row: T) => string) => {
        const result = new Map<string, T>();
        for(const d of data) {
            result.set(getId(d), d);
        }
        return result;
    }

    static toSet = <T,>(data: T[], getId: (row: T) => string) => {
        const result = new Set<string>();
        for(const d of data) {
            result.add(getId(d));
        }
        return result;
    }

    static uniqueIds = <T,>(data: T[], getId: (row: T) => string) => Array.from(this.toSet(data, getId));
}