import { useTranslation } from "react-i18next"
import { Page } from "../../layout/Page"
import { useOrdersQuery } from "../../hooks/queries/implementations/useOrdersQuery";
import { useChannelContext } from "../../context/AppContextProvider";
import { useNavigate } from "react-router";
import { Box } from "@mui/material";
import { OrderRow } from "./OrderRow";
import { useSessionsQuery } from "../../hooks/queries/implementations/useSessionsQuery";

export const OrdersPage = () => {
    const channelContext = useChannelContext();

    const { t } = useTranslation();
    const navigate = useNavigate();

    const sessionQuery = useSessionsQuery({
        channelId: channelContext.channelId,
    })
    const ordersQuery = useOrdersQuery(sessionQuery.data == undefined ? undefined : {
        channelIds: [channelContext.channelId],
        sessionId: sessionQuery.data.id,
        page: 0,
    });
    
    return <Page title={t("orders.title")}>
        {
            ordersQuery.isFirstLoading
            ?
                [1, 2, 3, 4, 5].map(i => <OrderRow key={`loading-${i}`} />)
            :
            (
                ordersQuery.data.length == 0
                ?
                    <img src="/assets/illustrations/transactions-empty-state.png" alt="Cards" />
                :
                <>
                    {
                        ordersQuery.data.map(o => (
                            <Box 
                                key={o.id}
                                sx={{
                                    cursor: "pointer",
                                    margin: "15px 0",
                                    "-webkit-tap-highlight-color": "transparent",
                                }}
                                onClick={() => navigate(`/c/${o.channelId}/orders/${o.id}/track`)}
                            >
                                <OrderRow model={o} />
                            </Box>
                        ))
                    }
                </>
            )
        }
    </Page>
}