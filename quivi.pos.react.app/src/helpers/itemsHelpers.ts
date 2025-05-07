import BigNumber from "bignumber.js";
import { BaseSessionItem, SessionItem } from "../hooks/api/Dtos/sessions/SessionItem";

type Item = SessionItem | BaseSessionItem;

const getItemPriceBig = (item: Item, quantity?: number) => {
    let extras: BaseSessionItem[] = [];
    if("extras" in item) {
        extras = item.extras ?? [];
    }

    const unitPrice = new BigNumber(item.price)
                .plus(extras.reduce((p, i) => p.plus(new BigNumber(i.price).times(i.quantity)), new BigNumber(0)));

    if(quantity == undefined) {
        return unitPrice;
    }

    return unitPrice.times(quantity);
}

export class Items {
    static getPrice = (item: Item, quantity?: number) => {
        return getItemPriceBig(item, quantity).decimalPlaces(2).toNumber();
    }

    static getTotalPrice = (items: Item[]) => {
        const preResult = items.reduce((p, i) => p.plus(getItemPriceBig(i, i.quantity)), new BigNumber(0));
        return preResult.decimalPlaces(2).toNumber();
    }
}