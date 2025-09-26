import type { Order } from "../hooks/api/Dtos/orders/Order";
import BigNumber from "bignumber.js";

export class OrdersHelper {
    static getTotal = (order: Order) => {
        let total = BigNumber(0);
        
        for(const item of order.items) {
            let modifiersPrices = BigNumber(0);

            for(const modifier of item.modifiers ?? []) {
                for(const o of modifier.selectedOptions) {
                    modifiersPrices = modifiersPrices.plus(BigNumber(o.amount).multipliedBy(BigNumber(o.quantity)));
                }
            }
            
            total = total.plus(BigNumber(item.amount).plus(modifiersPrices).multipliedBy(item.quantity));
        }

        total = total.plus(OrdersHelper.getExtraCosts(order));
        return total.toNumber();
    }

    static getExtraCosts = (order: Order) => {
        let total = BigNumber(0);

        for(const item of order.extraCosts) {
            total = total.plus(item.amount);
        }

        return total.toNumber();
    }
}