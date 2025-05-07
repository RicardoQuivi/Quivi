import { useState } from "react";
import { TimeUnit, useDateHelper } from "./dateHelper";
import { Order } from "../hooks/api/Dtos/orders/Order";

export const useOrderHelper = () => {
    const dateHelper = useDateHelper();

    const isOrderDelayed = (date: string) => {
        const now = new Date()
        const timeTaken = dateHelper.getTimeTaken(date, now, TimeUnit.Minutes);
        return timeTaken > 15;
    }

    const getTotal = (o: Order) => {
        let total = 0;
        o.items.forEach(i => total += (i.price + i.extras.reduce((r, m) => r + m.price * m.quantity, 0)) * i.quantity);
        return total;
    }

    const getItemsCount = (o: Order) => {
        let total = 0;
        o.items.forEach(i => total += i.quantity);
        return total;
    }

    const [helper] = useState({
        isOrderDelayed,
        getTotal,
        getItemsCount,
    });

    return helper;
}