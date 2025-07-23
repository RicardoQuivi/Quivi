import { Box, Checkbox, Divider, Grid, useMediaQuery, useTheme } from "@mui/material"
import { useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next";
import { usePreparationGroupsQuery } from "../../../hooks/queries/implementations/usePreparationGroupsQuery";
import { Order } from "../../../hooks/api/Dtos/orders/Order";
import { useLocalsQuery } from "../../../hooks/queries/implementations/useLocalsQuery";
import { Local } from "../../../hooks/api/Dtos/locals/Local";
import { PaginationFooter } from "../../Pagination/PaginationFooter";
import { BasePreparationGroupItem, PreparationGroup } from "../../../hooks/api/Dtos/preparationgroups/PreparationGroup";
import { useToast } from "../../../context/ToastProvider";
import LoadingButton from "../../Buttons/LoadingButton";
import { GenericPreparationGroupCard } from "./GenericPreparationGroupCard";
import ConfirmButton from "../../Buttons/ConfirmButton";
import { useWebEvents } from "../../../hooks/signalR/useWebEvents";
import { useBackgroundJobsApi } from "../../../hooks/api/useBackgroundJobsApi";
import { PreparationGroupDetailModal } from "./PreparationGroupDetailModal";
import { usePreparationGroupMutator } from "../../../hooks/mutators/usePreparationGroupMutator";
import { BackgroundJobPromise } from "../../../hooks/signalR/promises/BackgroundJobPromise";

interface Props {
    readonly locationId: string | undefined;
    readonly onOrderSelected: (order: Order) => any;
}
export const PreparationGroupsQueueCards = (props: Props) => {
    const theme = useTheme();
    const xs = useMediaQuery(theme.breakpoints.only('xs'));
    const [page, setPage] = useState(0);

    const groupsQuery = usePreparationGroupsQuery({
        locationId: props.locationId,
        isCommited: false,
        page: page,
        pageSize: 50,
    });

    const locationsQuery = useLocalsQuery({})

    const locationsMap = useMemo(() => locationsQuery.data.reduce((r, l) => {
        r.set(l.id, l);
        return r;
    }, new Map<string, Local>()), [locationsQuery.data])

    return <Box style={{display: "flex", flexDirection: "column", height: "100%", overflow: "hidden"}}>
        <Box style={{flex: "1 1 auto", overflow: "auto"}}>
            <Grid container spacing={1} justifyContent={xs ? "center" : undefined}>
            {
                groupsQuery.isFirstLoading == false
                ?
                    groupsQuery.data.map(s => <Grid size="auto" style={{minWidth: 300}} key={s.id}>
                        <PreparationGroupCard group={s} 
                                                onOrderClicked={props.onOrderSelected}
                                                locationId={props.locationId}
                                                locationsMap={locationsMap}
                        />
                    </Grid>)
                :
                    [1, 2, 3, 4, 5].map(i => <Grid size="grow" key={`Loading-${i}`}>
                        <GenericPreparationGroupCard 
                            locationId={props.locationId}
                            locationsMap={locationsMap}
                            isCheckedMap={{}}
                        />
                    </Grid>)
            }
            </Grid>
        </Box>
        {
            groupsQuery.totalPages > 1 &&
            <Box style={{flex: "0 0 auto"}}>
                <Divider />
                <PaginationFooter currentPage={page} numberOfPages={groupsQuery.totalPages} onPageChanged={setPage} />
            </Box>
        }
    </Box>
}

interface PreparationGroupCardProps {
    readonly group: PreparationGroup;
    readonly locationId: string | undefined;
    readonly onOrderClicked?: (order: Order) => any;
    readonly locationsMap: Map<string, Local>;
}
const PreparationGroupCard = (props: PreparationGroupCardProps) => {
    const { t } = useTranslation();
    const toast = useToast();
    const webEvents = useWebEvents();
    const mutator = usePreparationGroupMutator();
    const jobsApi = useBackgroundJobsApi();

    const [itemsCheck, setItemsCheck] = useState(() => {
        const result = {} as Record<string, boolean>;
        for(const item of props.group.items) {
            result[item.id] = true;
            for(const extra of item.extras) {
                result[extra.id] = true;
            }
        }
        return result;
    });

    const [isPrepareModalOpen, setIsPrepareModalOpen] = useState(false);

    const totalItemChecked = useMemo(() => {
        let result = 0;
        for(const key in itemsCheck) {
            if(itemsCheck[key]) {
                ++result;
            }
        }
        return result;
    }, [itemsCheck])
    
    useEffect(() => setItemsCheck(s => {
        const keysToDelete = new Set(Object.keys(s));
        let hasChanged = false;
        for(const item of props.group.items.reduce((r, item) => {
            r.push(item);
            for(const extra of item.extras) {
                r.push(extra);
            }
            return r;
        }, [] as BasePreparationGroupItem[])) {
            if(item.id in s) {
                keysToDelete.delete(item.id);
                continue;
            }

            hasChanged = true;
            s[item.id] = true;
        }

        if(hasChanged == false) {
            return s;
        }

        for(const key of keysToDelete) {
            delete s[key];
        }

        return {...s};
    }), [props.group])

    const commitPreparationGroup = async (g: PreparationGroup, asComplete: boolean) => {
        try {
            const jobId = await mutator.commit(g, {
                locationId: props.locationId,
                itemsToCommit: g.items.reduce((r, item) => {
                    if(itemsCheck[item.id] == true) {
                        r[item.id] = item.remainingQuantity;
                    }

                    for(const extra of item.extras) {
                        if(itemsCheck[extra.id] == true) {
                            r[extra.id] = extra.remainingQuantity;
                        }
                    }
                    return r;
                }, {} as Record<string, number>),
                isPrepared: asComplete,
            })

            await new BackgroundJobPromise(jobId, webEvents.client, async (jobId) => {
                const response = await jobsApi.get({
                    ids: [jobId],
                });
                return response.data[0].state;
            })
        } catch {
            toast.error(t('unexpectedErrorHasOccurred'));
        }
    }

    const onAllChecked = () => {
        if(totalItemChecked == Object.keys(itemsCheck).length) {
            setItemsCheck({});
            return;
        }

        const result = {} as Record<string, boolean>;
        for(const item of props.group.items) {
            result[item.id] = true;
            for(const extra of item.extras) {
                result[extra.id] = true;
            }
        }
        setItemsCheck(result);
    }

    return <GenericPreparationGroupCard
        {...props}
        onCardClicked={() => setIsPrepareModalOpen(true)}
        isCheckedMap={itemsCheck}
        onItemsChecked={(_g, _c, r) => setItemsCheck(r)}
        headerAction={
            <Checkbox
                edge="end"
                size="small"
                checked={totalItemChecked == Object.keys(itemsCheck).length}
                sx={{margin: "0 0.5rem"}}
                onClick={(evt) => {evt.stopPropagation(); onAllChecked()}}
            />
        }
        footerButtons={[
            <>
                {
                    props.locationId == undefined
                    ?
                    <LoadingButton
                        disabled={totalItemChecked == 0}
                        primaryButton
                        style={{width: "100%", height: "100%"}}
                        onClick={() => setIsPrepareModalOpen(true)}
                    >
                        {t("ordersTab.sendToPreparation")}
                    </LoadingButton>
                    :
                    <ConfirmButton
                        disabled={totalItemChecked == 0}
                        primaryButton
                        confirmText={`${t("confirm")}?`} 
                        style={{width: "100%", height: "100%"}}
                        onAction={() => commitPreparationGroup(props.group, false)}
                    >
                        { t("ordersTab.sendToPreparation")}
                    </ConfirmButton>
                }
                <PreparationGroupDetailModal 
                    group={isPrepareModalOpen ? props.group : undefined} 
                    onClose={() => setIsPrepareModalOpen(false)}
                    localsMap={props.locationsMap}

                    checkedItems={itemsCheck}
                    onCheckedItemsChanged={setItemsCheck}
                />  
            </>
            ,
            <ConfirmButton
                disabled={totalItemChecked == 0}
                confirmText={`${t("confirm")}?`} 
                style={{width: "100%", height: "100%"}} 
                onAction={() => commitPreparationGroup(props.group, true)}
            >
                {t("ordersTab.skipPreparation")}
            </ConfirmButton>
        ]}
    />
}