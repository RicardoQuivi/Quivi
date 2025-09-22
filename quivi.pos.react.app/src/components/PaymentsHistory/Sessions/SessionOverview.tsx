import { Box, Tab, Tabs } from "@mui/material";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { TransactionsTable } from "../TransactionsTable";
import { OrdersTable } from "../../Orders/OrdersTable";
import { SortDirection } from "../../../hooks/api/Dtos/SortableRequest";
import { SessionDetails } from "./SessionDetails";

interface Props {
    readonly sessionId: string;
}
export const SessionOverview = (props: Props) => {
    const { t } = useTranslation();

    const [tab, setCurrentTab] = useState(0);
    const [page, setPage] = useState(0);

    useEffect(() => setPage(0), [tab])

    return (
    <Box
        sx={{
            display: "flex",
            flexDirection: "row",
        }}
    >
        <Tabs
            orientation="vertical"
            variant="scrollable"
            value={tab}
            onChange={(_, v) => setCurrentTab(v)}
            sx={{ borderRight: 1, borderColor: 'divider', mr: "1rem" }}
        >
            <Tab label={t("details")} />
            <Tab label={t("orders")} />
            <Tab label={t("payments")} />
        </Tabs>
        <Box
            sx={{
                flex: "1 1 auto",
            }}
        >
            {
                tab == 0 &&
                <SessionDetails
                    sessionId={props.sessionId}
                />
            }
            {
                tab == 1 &&
                <OrdersTable
                    sessionIds={[props.sessionId]}
                    sortDirection={SortDirection.Desc}
                    onPageChanged={setPage}
                    page={page}
                />
            }
            {
                tab == 2 &&
                <TransactionsTable 
                    sessionIds={[props.sessionId]}
                    hideChannel
                />
            }
        </Box>
    </Box>
    )
}