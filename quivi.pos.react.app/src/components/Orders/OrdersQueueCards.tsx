import { Divider, Grid, useMediaQuery, useTheme } from "@mui/material"
import { useContext, useEffect, useState } from "react"
import { OrderState } from "../../hooks/api/Dtos/orders/OrderState";
import { Order } from "../../hooks/api/Dtos/orders/Order";
import { useTranslation } from "react-i18next";
import { useOrdersQuery } from "../../hooks/queries/implementations/useOrdersQuery";
import { SortDirection } from "../../hooks/api/Dtos/SortableRequest";
import { BackgroundJobPromise } from "../../hooks/signalR/promises/BackgroundJobPromise";
import { PaginationFooter } from "../Pagination/PaginationFooter";
import { useToast } from "../../context/ToastProvider";
import { OrderCard } from "./OrderCard";

interface Props {
    readonly states: OrderState[];
    readonly onOrderSelected: (order: Order) => any;
    readonly onOrderUpdated: (order: Order) => any;
}
export const OrdersQueueCards = (props: Props) => {
    const { t } = useTranslation();
    //const context = useContext(AppContext);
    const theme = useTheme();
    const xs = useMediaQuery(theme.breakpoints.only('xs'));
    // const posApi = usePoSApi();
    // const [webClient] = useWebEvents();
    // const jobsApi = useJobsApi();
    const toast = useToast();
    
    const [page, setPage] = useState(0);

    const ordersQuery = useOrdersQuery({
        page: page,
        pageSize: 50,
        states: props.states,
        sortDirection: SortDirection.Asc,
    });

    useEffect(() => setPage(0), [props.states])
    
    const updateOrder = async (o: Order, complete: boolean) => {
        // try {
        //     const response = await posApi.orders.UpdateToNextState({
        //         id: o.id,
        //         accessToken: context.merchant.token,
        //         completeOrder: complete,
        //     });
        //     await new BackgroundJobPromise(response.jobId, webClient, async (jobId) => {
        //         const response = await jobsApi.Get([jobId]);
        //         return response.data[0].state;
        //     });
        //     props.onOrderUpdated(o);
        // } catch {
        //     toast.error({ title: t('Resources.Error')!, message: t('Resources.UnexpectedErrorHasOccurred') });
        // }
    }

    return <div style={{display: "flex", flexDirection: "column", height: "100%", overflow: "hidden"}}>
        <div style={{flex: "1 1 auto", overflow: "auto"}}>
            <Grid container spacing={1} justifyContent={xs ? "center" : undefined}>
            {
                ordersQuery.isFirstLoading == false
                ?
                    ordersQuery.data.map(s => <Grid size="auto" style={{minWidth: 300}} key={s.id}>
                        <OrderCard 
                            order={s}
                            onNextStateClicked={o => updateOrder(o, false)}
                            onCompleteClicked={o => updateOrder(o, true)}
                            onCardClicked={(o) => props.onOrderSelected(o)}
                        />
                    </Grid>)
                :
                    [1, 2, 3, 4, 5].map(i => <Grid size="grow" key={`Loading-${i}`}>
                        <OrderCard />
                    </Grid>)
            }
            </Grid>
        </div>
        {
            ordersQuery.totalPages > 1 &&
            <div style={{flex: "0 0 auto"}}>
                <Divider />
                <PaginationFooter currentPage={page} numberOfPages={ordersQuery.totalPages} onPageChanged={(p) => setPage(p)} />
            </div>
        }
    </div>
}