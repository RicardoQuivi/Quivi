import BigNumber from 'bignumber.js';

export class Enumerable {
    static toDictionary = <TItem, TKey, TValue>(items: TItem[], keySelector: (item: TItem) => TKey, valueSelector: (item: TItem) => TValue) => {
        return items.reduce((result, item) => result.set(keySelector(item), valueSelector(item)), new Map<TKey, TValue>());
    }

    static joinArrays = <TItem1, TItem2, TKey>(array1: TItem1[], array2: TItem2[], keySelector1: (item: TItem1) => TKey, keySelector2: (item: TItem2) => TKey): { item1: TItem1, item2: TItem2 }[] => {
        let result: { item1: TItem1, item2: TItem2 }[] = [];
        
        if (!array1?.length || !array2?.length)
            return [];

        var array2Dictionary = Enumerable.toDictionary(array2, keySelector2, item => item);
        array1.forEach(item1 => {
            const item2 = array2Dictionary.get(keySelector1(item1));
            if (item2 !== undefined)
                result.push({item1, item2});
        });

        return result;
    }

    static sumArray = <TItem>(items: TItem[], numberSelector: (item: TItem) => number): number => {
        return items.reduce((result, item) => BigNumber(result).plus(numberSelector(item)).toNumber(), 0);
    }

    static areEquivalentArrays = <TItem1, TItem2>(array1: TItem1[], array2: TItem2[], compareFunc: (item1: TItem1, item2: TItem2) => boolean) => {
        if (array1?.length !== array2?.length)
            return false;
        
        array1.forEach(item1 => {
            const someEqual = array2.some(item2 => compareFunc(item1, item2));
            if (!someEqual)
                return false;
        });

        return true;
    }

    static areEquivalentArraysOptimized = <TItem1, TItem2, TKey>(array1: TItem1[], array2: TItem2[], keySelector1: (item: TItem1) => TKey, keySelector2: (item: TItem2) => TKey, compareFunc: (item1: TItem1, item2: TItem2) => boolean) => {
        if (array1.length !== array2.length)
            return false;
        
        const zippedArrays = Enumerable.joinArrays(array1, array2, keySelector1, keySelector2);

        if (zippedArrays.length !== array1.length)
            return false;

        return zippedArrays.every(pair => compareFunc(pair.item1, pair.item2));
    }

    static range = (count: number, startNumber: number = 1) => Array.from({length: count}, (_, i) => i + startNumber);
}