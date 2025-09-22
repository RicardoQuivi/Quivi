import { useTranslation } from "react-i18next";
import { useMemo } from "react";
import { Grid, Skeleton, Stack, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from "@mui/material";
import { BaseSessionItem, SessionItem } from "../../../hooks/api/Dtos/sessions/SessionItem";
import { useDateHelper } from "../../../helpers/dateHelper";
import { useSessionsQuery } from "../../../hooks/queries/implementations/useSessionsQuery";
import { useSessionAdditionalInformationsQuery } from "../../../hooks/queries/implementations/useSessionAdditionalInformationsQuery";
import { useConfigurableFieldsQuery } from "../../../hooks/queries/implementations/useConfigurableFieldsQuery";
import { ConfigurableField } from "../../../hooks/api/Dtos/configurablefields/ConfigurableField";
import { SummaryBox } from "../../common/SummaryBox";
import CurrencySpan from "../../Currency/CurrencySpan";
import { Items } from "../../../helpers/itemsHelpers";
import { useMenuItemsQuery } from "../../../hooks/queries/implementations/useMenuItemsQuery";
import { CollectionFunctions } from "../../../helpers/collectionsHelper";
import { MenuItem } from "../../../hooks/api/Dtos/menuitems/MenuItem";

interface TransactionItem extends SessionItem {
    readonly unpaidQuantity: number;
}
interface Props {
    readonly sessionId: string;
}
export const SessionDetails = (props: Props) => {
    const { t } = useTranslation();
    const dateHelper = useDateHelper();

    const sessionQuery = useSessionsQuery(props.sessionId == undefined ? undefined : {
        ids: [props.sessionId],
        includeDeleted: true,
        page: 0,
    });
    const sessionAdditionalInfoQuery = useSessionAdditionalInformationsQuery({
        sessionId: props.sessionId,
    })
    const configurableFieldsQuery = useConfigurableFieldsQuery(sessionAdditionalInfoQuery.data.length == 0 ? undefined : {
        ids: sessionAdditionalInfoQuery.data.map(d => d.id),
        forPosSessions: true,
        page: 0,
    });

    const configurableFieldsMap = useMemo(() => {
        const map = new Map<string, ConfigurableField>();
        for(const f of configurableFieldsQuery.data) {
            map.set(f.id, f);
        }
        return map;
    }, [configurableFieldsQuery.data])

    const session = useMemo(() => sessionQuery.data.length == 0 ? undefined : sessionQuery.data[0], [sessionQuery.data])
    const itemIds = useMemo(() => {
        if(session == undefined) {
            return [];
        }

        const result = new Set<string>()
        for(const item of session.items) {
            result.add(item.menuItemId);
            for(const extra of item.extras) {
                result.add(extra.menuItemId);
            }
        }
        return Array.from(result);
    }, [session])
    const itemsQuery = useMenuItemsQuery(itemIds.length == 0 ? undefined : {
        ids: itemIds,
        page: 0,
    })
    const itemsMap = useMemo(() => CollectionFunctions.toMap(itemsQuery.data, m => m.id), [itemsQuery.data])

    const { total, totalPaid, items } = useMemo(() => {
        if(session == undefined) {
            return {
                total: 0,
                totalPaid: 0,
                items: [],
            }
        }

        const allItems = session.items.filter(i => i.quantity > 0);
        const paidItems = allItems.filter(i => i.isPaid);
        const paidTotal = Items.getTotalPrice(paidItems);
        const total = Items.getTotalPrice(allItems);

        const groupedItems = allItems.reduce((r, i) => {
            const itemsInGroup = r.get(i.id) ?? [];
            r.set(i.id, [...itemsInGroup, i]);
            return r;
        }, new Map<string, SessionItem[]>());

        const itemsResult: TransactionItem[] = [];

        for (const value of groupedItems.values()) {
            let result = {
                ...value[0],
                quantity: 0,
                unpaidQuantity: 0,
                isPaid: true,
                modifiers: [] as BaseSessionItem[],
            };

            for(let i = 0; i < value.length; ++i) {
                const item = value[i];

                for (const modifier of item.extras) {
                    result.modifiers.push(modifier);
                }

                result.quantity += item.quantity;
                if(item.isPaid == false) {
                    result.isPaid = true;
                    result.unpaidQuantity += item.quantity;
                }
            }

            itemsResult.push(result as TransactionItem);
        }
        
        return {
            total: total,
            totalPaid: paidTotal,
            items: itemsResult,
        }
    }, [session])

    return <>
        <SummaryBox
            isLoading={session == undefined}
            items={session == undefined ? [] : [
                {
                    label: t("total"),
                    content: <CurrencySpan value={total} />
                },
                {
                    label: t("unpaid"),
                    content: <CurrencySpan value={total - totalPaid} />
                },
                {
                    label: t("paid"),
                    content: <CurrencySpan value={totalPaid} />
                },
            ]} 
        />
        <Grid
            container
            rowSpacing={4}
        >
            <Grid
                size={{
                    xs: 6,
                }}
                flexDirection="column"
                alignContent="start"
                flexWrap="wrap"
                display="flex"
            >
                <InfoPanel
                    label={t("session.sessionStart")}
                    value={
                        session == undefined
                        ?
                        <Skeleton animation="wave" />
                        :
                        dateHelper.toLocalString(session.startDate, "YYYY-MM-DD HH:mm")
                    }
                />
            </Grid>
            <Grid
                size={{
                    xs: 6,
                }}
                flexDirection="column"
                alignContent="end"
                flexWrap="wrap"
                display="flex"
            >
                <InfoPanel
                    label={t("session.sessionEnd")}
                    value={
                        session?.closedDate == undefined
                        ?
                        <Skeleton animation="wave" />
                        :
                        dateHelper.toLocalString(session.closedDate, "YYYY-MM-DD HH:mm")
                    }
                />
            </Grid>
            {
                sessionAdditionalInfoQuery.data.map((a, i) => {
                    const field = configurableFieldsMap.get(a.id);
                    return (
                        <Grid
                            key={a.id}
                            size={{
                                xs: 6
                            }}
                            flexDirection="column"
                            alignContent={ i % 2 == 0 ? "start" : "end"}
                            flexWrap="wrap"
                            display="flex"
                        >
                            <InfoPanel
                                label={
                                    field == undefined
                                    ?
                                    <Skeleton animation="wave" />
                                    :
                                    field.name
                                }
                                value={a.value}
                            />
                        </Grid>
                    )
                })
            }
            {
                sessionAdditionalInfoQuery.data.length % 2 != 0 &&
                <Grid
                    size={{
                        xs: 6
                    }}
                    flexDirection="column"
                    alignContent="end"
                    flexWrap="wrap"
                    display="flex"
                />
            }
        </Grid>
        <TableContainer
            component="div"
        >
            <Table>
                <TableHead>
                    <TableRow>
                        <TableCell>{t("item")}</TableCell>
                        <TableCell align="right">{t("paymentHistory.unitPrice")}</TableCell>
                        <TableCell align="right">{t("quantity")}</TableCell>
                        <TableCell align="right">{t("discount")}</TableCell>
                        <TableCell align="right">{t("paid")}</TableCell>
                        <TableCell align="right">{t("total")}</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {
                        session == undefined
                        ?
                        [1, 2, 3, 4, 5].map(r => (
                            <SessionRow
                                key={`loading-${r}`}
                                row={undefined}
                                itemsMap={itemsMap}
                            />
                        ))
                        :
                        items.map(r => (
                            <SessionRow
                                key={r.id}
                                row={r}
                                itemsMap={itemsMap}
                            />
                        ))
                    }
                </TableBody>
            </Table>
        </TableContainer>
    </>
}

interface SessionRowProps {
    readonly row?: TransactionItem;
    readonly itemsMap: Map<string, MenuItem>;
}
const SessionRow = ({
    row,
    itemsMap,
}: SessionRowProps) => {

    const item = useMemo(() => row == undefined ? undefined : itemsMap.get(row.menuItemId), [row, itemsMap]);

    return (
    <TableRow>
        <TableCell component="th" scope="row">
        {
            item == undefined
            ?
                <Skeleton animation="wave" />
            :
                item.name
        }
        </TableCell>
        <TableCell component="th" scope="row" align="right">
        {
            row == undefined
            ?
                <Skeleton animation="wave" />
            :
                <CurrencySpan value={Items.getPrice(row, 1)} />
        }
        </TableCell>
        <TableCell component="th" scope="row" align="right">
        {
            row == undefined
            ?
                <Skeleton animation="wave" />
            :
                row.quantity
        }
        </TableCell>
        <TableCell component="th" scope="row" align="right">
        {
            row == undefined
            ?
                <Skeleton animation="wave" />
            :
                <>{row.discountPercentage} %</>
        }
        </TableCell>
        <TableCell component="th" scope="row" align="right">
        {
            row == undefined
            ?
                <Skeleton animation="wave" />
            :
                <CurrencySpan value={Items.getPrice(row, row.unpaidQuantity)} />
        }
        </TableCell>
        <TableCell component="th" scope="row" align="right">
        {
            row == undefined
            ?
                <Skeleton animation="wave" />
            :
                <CurrencySpan value={Items.getPrice(row)} />
        }
        </TableCell>
    </TableRow>
    )
}

interface InfoPanelProps {
    readonly label: React.ReactNode;
    readonly value: React.ReactNode;
}
const InfoPanel = (props: InfoPanelProps) => {
    return <Stack
        direction="column"
        gap={0}
    >
        <Typography variant="subtitle2" fontWeight="bold">{props.label}</Typography>
        <Typography variant="body1">{props.value}</Typography>
    </Stack>
} 