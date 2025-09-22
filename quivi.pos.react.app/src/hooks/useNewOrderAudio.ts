import { useEffect, useState } from "react"
import { useDateHelper } from "../helpers/dateHelper";
import { useOrdersQuery } from "./queries/implementations/useOrdersQuery";
import { SortDirection } from "./api/Dtos/SortableRequest";
import { OrderState } from "./api/Dtos/orders/OrderState";

const newOrderAudio = new Audio("/sounds/New.wav");
newOrderAudio.muted = false;

export const useNewOrderAudio = () => {  
    const dateHelper = useDateHelper();
    const requestedOrdersQuery = useOrdersQuery({
        page: 0,
        pageSize: 1,
        states: [OrderState.PendingApproval],
        sortDirection: SortDirection.Desc,
    });

    const [lastRequestedOrderDate, setLastRequestedOrderDate] = useState<Date | undefined | null>(undefined);

    useEffect(() => {
        if(requestedOrdersQuery.isFirstLoading) {
            return;
        }
        
        const newDate = requestedOrdersQuery.data.length == 0 ? undefined : dateHelper.toDate(requestedOrdersQuery.data[0].lastModified);
        if(lastRequestedOrderDate === undefined) {
            setLastRequestedOrderDate(newDate == undefined ? null : newDate);
            return;
        }

        if(newDate == undefined) {
            return;
        }

        if(lastRequestedOrderDate === null || newDate > lastRequestedOrderDate) {
            newOrderAudio.play();
            setLastRequestedOrderDate(newDate)
        }
    }, [dateHelper, lastRequestedOrderDate, requestedOrdersQuery.data, requestedOrdersQuery.isFirstLoading])
}