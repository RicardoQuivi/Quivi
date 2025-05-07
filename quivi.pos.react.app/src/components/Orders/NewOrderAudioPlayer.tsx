import { useEffect, useState } from "react"
import { useOrdersQuery } from "../../hooks/queries/implementations/useOrdersQuery";
import { OrderState } from "../../hooks/api/Dtos/orders/OrderState";
import { SortDirection } from "../../hooks/api/Dtos/SortableRequest";
import { useDateHelper } from "../../helpers/dateHelper";

const newOrderAudio = new Audio("/Content/sounds/New.wav");

export const NewOrderAudioPlayer = () => {  
    const dateHelper = useDateHelper();
    const requestedOrdersQuery = useOrdersQuery({
        page: 0,
        pageSize: 1,
        states: [OrderState.Requested],
        sortDirection: SortDirection.Desc,
    });

    const [lastRequestedOrderDate, setLastRequestedOrderDate] = useState<Date>();

    useEffect(() => {
        const newDate = requestedOrdersQuery.data.length == 0 ? undefined : dateHelper.toDate(requestedOrdersQuery.data[0].lastModified);
        if(newDate == undefined) {
            return;
        }
        if(lastRequestedOrderDate == undefined || newDate > lastRequestedOrderDate) {
            if(lastRequestedOrderDate != undefined) {
                newOrderAudio.muted = false;
                newOrderAudio.play();
            }
            setLastRequestedOrderDate(newDate)
        }
    }, [dateHelper, lastRequestedOrderDate, requestedOrdersQuery.data])
    
    return <>
    </>
}