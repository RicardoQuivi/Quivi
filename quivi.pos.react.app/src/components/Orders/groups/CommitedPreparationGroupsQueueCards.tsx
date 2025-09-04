import { Box, Divider, Grid, useMediaQuery, useTheme } from "@mui/material"
import { useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next";
import { Order } from "../../../hooks/api/Dtos/orders/Order";
import { usePreparationGroupsQuery } from "../../../hooks/queries/implementations/usePreparationGroupsQuery";
import { useLocalsQuery } from "../../../hooks/queries/implementations/useLocalsQuery";
import { Local } from "../../../hooks/api/Dtos/locals/Local";
import { GenericPreparationGroupCard } from "./GenericPreparationGroupCard";
import { PaginationFooter } from "../../Pagination/PaginationFooter";
import { BasePreparationGroupItem, PreparationGroup } from "../../../hooks/api/Dtos/preparationgroups/PreparationGroup";
import { useToast } from "../../../context/ToastProvider";
import { usePreparationGroupMutator } from "../../../hooks/mutators/usePreparationGroupMutator";
import ConfirmButton from "../../Buttons/ConfirmButton";
import { PreparationGroupDetailModal } from "./PreparationGroupDetailModal";

interface Props {
    readonly locationId: string | undefined;
    readonly onOrderSelected: (order: Order) => any;
}
export const CommitedPreparationGroupsQueueCards = (props: Props) => {
    const theme = useTheme();
    const xs = useMediaQuery(theme.breakpoints.only('xs'));
    const [page, setPage] = useState(0);
    const groupsQuery = usePreparationGroupsQuery({
        locationId: props.locationId,
        isCommited: true,
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
                    groupsQuery.data.map(s => <Grid 
                        size="auto"
                        sx={{
                            minWidth: 300,
                            maxWidth: "100%",
                        }}
                        key={s.id}
                    >
                        <CommitedPreparationGroupCard group={s} 
                                                        onOrderClicked={props.onOrderSelected}
                                                        locationId={props.locationId}
                                                        locationsMap={locationsMap}
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

interface CommitedPreparationGroupCardProps {
    readonly group: PreparationGroup;
    readonly locationId: string | undefined;
    readonly onOrderClicked?: (order: Order) => any;
    readonly locationsMap: Map<string, Local>;
}
const CommitedPreparationGroupCard = (props: CommitedPreparationGroupCardProps) => {
    const { t } = useTranslation();
    const toast = useToast();
    const mutator = usePreparationGroupMutator();    

    const [patchGroupFunc, setPatchGroupFunc] = useState<() => any>();
    const [isPrepareModalOpen, setIsPrepareModalOpen] = useState(false);
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

    const isCheckedMap = useMemo(() => {
        const result = {} as Record<string, any>;

        for(const item of props.group.items) {
            const preparedQuantity = item.quantity - item.remainingQuantity;
            result[item.id] = preparedQuantity == item.quantity;
            for(const extra of item.extras) {
                const extraPreparedQuantity = extra.quantity - extra.remainingQuantity;
                result[extra.id] = extraPreparedQuantity == extra.quantity;
            }
        }
        return result;
    },  [props.group])

    useEffect(() => {
        if(patchGroupFunc == undefined) {
            return;
        }

        const timeout = setTimeout(async () => {
            await patchGroupFunc();
            setPatchGroupFunc(undefined);
        }, 1500);
        return () => clearTimeout(timeout);
    }, [patchGroupFunc])

    const getAllItems = (group: PreparationGroup) => {
        return group.items.reduce((r, item) => {
            r.push(item);
            for(const extra of item.extras) {
                r.push(extra);
            }
            return r;
        }, [] as BasePreparationGroupItem[])
    } 

    const scheduleUpdate = (group: PreparationGroup, changes: Record<string, boolean>) => {
        if(Object.keys(changes).length == 0) {
            return;
        }

        setPatchGroupFunc(_ => {
            return async () => {
                await mutator.patch(group, {
                    items: getAllItems(group).filter(i => i.id in changes).reduce((r, item) => {
                        r[item.id] = item.remainingQuantity;
                        return r;
                    }, {} as Record<string, number>),
                })
            }
        })
    }

    const completePreparationGroup = async (o: PreparationGroup, itemdIds: string[]) => {
        try {
            await mutator.patch(o, {
                items: getAllItems(o).filter(i => itemdIds.includes(i.id)).reduce((r, item) => {
                    r[item.id] = item.quantity;
                    return r;
                }, {} as Record<string, number>),
            })
        } catch(e) {
            toast.error(t('unexpectedErrorHasOccurred'));
        }
    }

    return <GenericPreparationGroupCard 
        group={props.group} 
        onCardClicked={() => setIsPrepareModalOpen(true)} 
        onOrderClicked={props.onOrderClicked}
        locationId={props.locationId}
        locationsMap={props.locationsMap}
        isCheckedMap={isCheckedMap}
        onItemsChecked={scheduleUpdate}
        footerButtons={[
            <>
                <ConfirmButton
                    primaryButton
                    confirmText={`${t("confirm")}?`} 
                    style={{width: "100%", height: "100%"}} 
                    onAction={async () => {
                        const itemIds = props.group.items.map(i => [i.id, ...i.extras.map(e => e.id)]).reduce((r, ids) => {
                            for(const id of ids) {
                                r.push(id);
                            }
                            return r;
                        }, [] as string[])
                        await completePreparationGroup(props.group, itemIds);
                    }}
                >
                    {t("ready")}
                </ConfirmButton>
                <PreparationGroupDetailModal
                    group={isPrepareModalOpen ? props.group : undefined}
                    note={props.group.note}
                    onClose={() => setIsPrepareModalOpen(false)}
                    localsMap={props.locationsMap}
                    currentLocalId={props.locationId} 
                    checkedItems={itemsCheck}
                    onCheckedItemsChanged={setItemsCheck}                
                />
            </>
        ]}
    />
}