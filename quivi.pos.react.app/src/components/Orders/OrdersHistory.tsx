import { Paper } from "@mui/material"
import { useContext, useEffect, useState } from "react"
import { OrderState } from "../../hooks/api/Dtos/orders/OrderState";
import { Order } from "../../hooks/api/Dtos/orders/Order";
import { OrdersTable } from "./OrdersTable";
import { SortDirection } from "../../hooks/api/Dtos/SortableRequest";

interface Props {
    readonly states: OrderState[];
    onOrderSelected: (order: Order) => any;
}
export const OrdersHistory = (props: Props) => {
    const [page, setPage] = useState(0);
    
    return (
    <Paper
        elevation={16}
        sx={{
            height: "100%",
            width: "100%",
            display: "flex",
            flexDirection: "column",
        }}
    >
        <OrdersTable
            page={page}
            pageSize={19}
            states={props.states}
            onOrderSelected={props.onOrderSelected}
            sortDirection={SortDirection.Asc}
            onPageChanged={setPage}
        />
    </Paper>
    )
}