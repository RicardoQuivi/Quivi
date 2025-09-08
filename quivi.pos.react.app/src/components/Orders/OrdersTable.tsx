import { useMemo } from "react"
import { useTranslation } from "react-i18next";
import { Box, Divider, Skeleton } from "@mui/material";
import { GetOrdersRequest } from "../../hooks/api/Dtos/orders/GetOrdersRequest";
import { Order } from "../../hooks/api/Dtos/orders/Order";
import { useOrdersQuery } from "../../hooks/queries/implementations/useOrdersQuery";
import { useChannelsQuery } from "../../hooks/queries/implementations/useChannelsQuery";
import { useEmployeesQuery } from "../../hooks/queries/implementations/useEmployeesQuery";
import { useChannelProfilesQuery } from "../../hooks/queries/implementations/useChannelProfilesQuery";
import { PaginationFooter } from "../Pagination/PaginationFooter";
import { ResponsiveTable } from "../Tables/ResponsiveTable";
import { useDateHelper } from "../../helpers/dateHelper";
import CurrencySpan from "../Currency/CurrencySpan";
import { useOrderHelper } from "../../helpers/useOrderHelper";
import { MapFunctions } from "../../helpers/mapHelper";

interface Props extends GetOrdersRequest {
    readonly onOrderSelected?: (order: Order) => any;
    readonly onPageChanged: (p: number) => any;
}
export const OrdersTable = ({
    onOrderSelected,
    pageSize,
    onPageChanged,
    ...baseRequest
}: Props) => {
    const { t } = useTranslation();
    const dateHelper = useDateHelper();
    const orderHelper = useOrderHelper();
    
    const ordersQuery = useOrdersQuery({
        ...baseRequest,
        pageSize: pageSize ?? 10,
    });
    const employeeIds = useMemo(() => {
        const set = new Set<string>();
        for(const o of ordersQuery.data) {
            if(o.employeeId == undefined) {
                continue;
            }

            set.add(o.employeeId)
        }
        return Array.from(set.values());
    }, [ordersQuery.data]);

    const channelsQuery = useChannelsQuery(ordersQuery.isFirstLoading ? undefined : {
        ids: ordersQuery.data.map(o => o.channelId),
        page: 0,
        allowsSessionsOnly: false,
        includeDeleted: true,
    })
    const channelsMap = useMemo(() => MapFunctions.toMap(channelsQuery.data, q => q.id), [channelsQuery.data])
    const profileIds = useMemo(() => {
        const set = new Set<string>();
        for(const c of channelsQuery.data) {
            set.add(c.channelProfileId)
        }
        return Array.from(set.values());
    }, [channelsQuery.data]);

    const profilesQuery = useChannelProfilesQuery(profileIds.length == 0 ? undefined : {
        ids: profileIds,
        page: 0,
    })
    const profilesMap = useMemo(() => MapFunctions.toMap(profilesQuery.data, q => q.id), [profilesQuery.data])

    const employeesQuery = useEmployeesQuery(employeeIds.length == 0 ? undefined : {
        ids: employeeIds,
        includeDeleted: true,
        page: 0,
    });
    const employeesMap = useMemo(() => MapFunctions.toMap(employeesQuery.data, q => q.id), [employeesQuery.data])
            
    return <>
        <Box
            sx={{
                flex: 1,
                display: "flex",
                overflow: "auto",
            }}
        >
            <ResponsiveTable
                placeholderText={t("ordersTab.noOrdersAvailable")}
                isLoading={ordersQuery.isFirstLoading}
                data={ordersQuery.data}
                columns={[
                    {
                        key: "channel",
                        label: t("channel"),
                        render: o => {
                            const channel = channelsMap.get(o.channelId);
                            const profile = channel == undefined ? undefined : profilesMap.get(channel.channelProfileId);
                            if(channel == undefined || profile == undefined) {
                                return <Skeleton animation="wave"/>;
                            }

                            return <>{profile.name} {channel.name} {o.scheduledTo != undefined && `(${t("scheduled")})`}</>;
                        }
                    },
                    {
                        key: "order",
                        label: t("order"),
                        render: o => {
                            if(o.employeeId != undefined) {
                                const employee = employeesMap.get(o.employeeId);
                                return <>{t("viaPos")} ({employee == undefined ? <Skeleton animation="wave" /> : employee.name})</>
                            }
                            return o.sequenceNumber;
                        }
                    },
                    {
                        key: "date",
                        label: t("date"),
                        render: o => dateHelper.toLocalString(o.createdDate, "DD MMMM YYYY | HH:mm:ss"),
                    },
                    {
                        key: "amount",
                        label: t("amount"),
                        render: o => <CurrencySpan value={orderHelper.getTotal(o)}/>
                    },
                    {
                        key: "total",
                        label: t("total"),
                        render: o => <CurrencySpan value={orderHelper.getTotal(o)}/>
                    },
                    {
                        key: "items",
                        label: t("items"),
                        render: o => orderHelper.getItemsCount(o),
                    },
                ]}
                getKey={d => d.id}
                onRowClick={onOrderSelected}
            />
        </Box>
        {
            ordersQuery.totalPages > 1 &&
            <Box
                sx={{
                    flex: 0,
                }}
            >
                <Divider />
                <PaginationFooter currentPage={baseRequest.page} numberOfPages={ordersQuery.totalPages} onPageChanged={onPageChanged} />
            </Box>
        }
    </>
}