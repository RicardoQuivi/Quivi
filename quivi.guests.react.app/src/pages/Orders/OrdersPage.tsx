import { useTranslation } from "react-i18next"
import { Page } from "../../layout/Page"
import { useQuiviTheme, type IColor } from "../../hooks/theme/useQuiviTheme";
import { useOrdersQuery } from "../../hooks/queries/implementations/useOrdersQuery";
import { useChannelContext } from "../../context/AppContextProvider";
import { useNavigate } from "react-router";
import { makeStyles } from "@mui/styles";
import type { Theme } from "@mui/material";
import { OrderRow } from "./OrderRow";
import { useSessionsQuery } from "../../hooks/queries/implementations/useSessionsQuery";

interface StyleProps {
    readonly primarycolor: IColor;
}

const useStyles = makeStyles<Theme, StyleProps>({
    transactionsTitle: {
        margin: "15px 0 0 0",
    },
    menuItemContainer: {
        margin: "15px 0",
        "-webkit-tap-highlight-color": "transparent",
    },
});

export const OrdersPage = () => {
    const channelContext = useChannelContext();

    const { t } = useTranslation();
    const theme = useQuiviTheme();
    const classes = useStyles({ primarycolor: theme.primaryColor });
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
                            <div 
                                key={o.id}
                                className={classes.menuItemContainer}
                                style={{cursor: "pointer"}}
                                onClick={() => navigate(`/c/${o.channelId}/orders/${o.id}/track`)}
                            >
                                <OrderRow model={o} />
                            </div>
                        ))
                    }
                </>
            )
        }
    </Page>
}