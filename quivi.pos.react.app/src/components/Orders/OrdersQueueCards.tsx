import { Divider, Grid, useMediaQuery, useTheme } from "@mui/material"
import { useEffect, useState } from "react"
import { OrderState } from "../../hooks/api/Dtos/orders/OrderState";
import { Order } from "../../hooks/api/Dtos/orders/Order";
import { useTranslation } from "react-i18next";
import { useOrdersQuery } from "../../hooks/queries/implementations/useOrdersQuery";
import { SortDirection } from "../../hooks/api/Dtos/SortableRequest";
import { PaginationFooter } from "../Pagination/PaginationFooter";
import { useToast } from "../../context/ToastProvider";
import { OrderCard } from "./OrderCard";
import { useOrderMutator } from "../../hooks/mutators/useOrderMutator";
import { useActionAwaiter } from "../../hooks/useActionAwaiter";

interface Props {
    readonly states: OrderState[];
    readonly onOrderSelected: (order: Order) => any;
    readonly onOrderUpdated: (order: Order) => any;
}
export const OrdersQueueCards = (props: Props) => {
    const { t } = useTranslation();
    const theme = useTheme();
    const xs = useMediaQuery(theme.breakpoints.only('xs'));
    const toast = useToast();
    const awaiter = useActionAwaiter();
    const [page, setPage] = useState(0);

    const ordersQuery = useOrdersQuery({
        page: page,
        pageSize: 50,
        states: props.states,
        sortDirection: SortDirection.Asc,
    });
    const orderMutator = useOrderMutator();

    useEffect(() => setPage(0), [props.states])
    
    const updateOrder = async (o: Order, complete: boolean) => {
        try {
            const jobId = await orderMutator.process(o, {
                completeOrder: complete,
            })
            await awaiter.job(jobId);
            props.onOrderUpdated(o);
        } catch {
            toast.error(t('unexpectedErrorHasOccurred'));
        }
    }

    return <div style={{display: "flex", flexDirection: "column", height: "100%", overflow: "hidden"}}>
        <div style={{flex: "1 1 auto", overflow: "auto"}}>
            <Grid container spacing={1} justifyContent={xs ? "center" : undefined}>
            {
                ordersQuery.isFirstLoading == false
                ?
                    ordersQuery.data.map(s => <Grid
                        size="auto"
                        sx={{
                            minWidth: 300,
                            maxWidth: "100%",
                        }}
                        key={s.id}
                    >
                        <OrderCard 
                            order={s}
                            onNextStateClicked={o => updateOrder(o, false)}
                            onCompleteClicked={o => updateOrder(o, true)}
                            onCardClicked={props.onOrderSelected}
                        />
                    </Grid>)
                :
                    [1, 2, 3, 4, 5].map(i => <Grid
                        size="auto"
                        sx={{
                            minWidth: 300,
                            maxWidth: "100%",
                        }}
                        key={`Loading-${i}`}
                    >
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