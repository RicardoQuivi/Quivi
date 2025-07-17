import type { ICartItem } from "../context/cart/ICartItem";
import type { IBaseItem } from "../context/cart/item";
import BigNumber from "bignumber.js";
import type { SessionItem } from "../hooks/api/Dtos/sessions/SessionItem";

export class ItemsHelper {
    static getItemPrice = (item: IBaseItem | ICartItem): number => {
        let extraPrice = new BigNumber(0);

        const modifiers = 'modifiers' in item ? item.modifiers : undefined;
        for(const group of modifiers ?? []) {
            if(group.selectedOptions == undefined) {
                continue;
            }

            for(let option of group.selectedOptions) {
                extraPrice = new BigNumber(extraPrice).plus(new BigNumber(option.price).multipliedBy(option.quantity));
            }
        }

        const quantity = 'quantity' in item ? item.quantity : 1;
        const total = new BigNumber(quantity).multipliedBy(new BigNumber(item.price).plus(extraPrice));
        return total.toNumber();
    }

    static getItemsPrice = (items: IBaseItem[] | ICartItem[]): number => {
        let total = new BigNumber(0);
        items.forEach(item => {
            const itemPrice = new BigNumber(this.getItemPrice(item));
            total = total.plus(itemPrice);
        })
        return total.toNumber();
    }

    static getItemPriceGeneric = <T>(item: T, priceSelector: (item: T) => number, quantitySelector: (item: T) => number): number => {
        const price = priceSelector(item);
        const quantity = quantitySelector(item);

        const total = new BigNumber(quantity).multipliedBy(new BigNumber(price));
        return total.toNumber();
    }

    static getItemsPriceGeneric = <T>(items: T[], priceSelector: (item: T) => number, quantitySelector: (item: T) => number) => {
        let total = new BigNumber(0);
        items.forEach(item => {
            const itemPrice = new BigNumber(this.getItemPriceGeneric(item, priceSelector, quantitySelector));
            total = total.plus(itemPrice);
        })
        return total.toNumber();
    }

    static originalUnitPrice = (priceAfterDiscount: number, appliedDiscountPercentage: number): number => {
        return new BigNumber(priceAfterDiscount).dividedBy(new BigNumber(1).minus(new BigNumber(appliedDiscountPercentage).dividedBy(100))).toNumber();
    }

    static priceWithoutDiscount = (tableItem: SessionItem): BigNumber => {
        return BigNumber(tableItem.price).dividedBy(BigNumber(1).minus(BigNumber(tableItem.discountPercentage).dividedBy(100)));
    }

    static appliedDiscountTotal = (tableItem: SessionItem): BigNumber => {
        return this.priceWithoutDiscount(tableItem).minus(BigNumber(tableItem.price));
    }
}