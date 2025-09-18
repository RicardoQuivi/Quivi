import { useEffect, useState } from "react"
import { useOrdersQuery } from "../../hooks/queries/implementations/useOrdersQuery";
import { OrderState } from "../../hooks/api/Dtos/orders/OrderState";
import { SortDirection } from "../../hooks/api/Dtos/SortableRequest";
import { useDateHelper } from "../../helpers/dateHelper";

const newOrderAudio = new Audio("/sounds/New.wav");

export const NewOrderAudioPlayer = () => {  
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
            newOrderAudio.muted = false;
            newOrderAudio.play();
            setLastRequestedOrderDate(newDate)
        }
    }, [dateHelper, lastRequestedOrderDate, requestedOrdersQuery.data, requestedOrdersQuery.isFirstLoading])
    
    return <>
    </>
}