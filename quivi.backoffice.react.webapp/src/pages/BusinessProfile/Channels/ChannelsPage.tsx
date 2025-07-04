import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { Channel } from "../../../hooks/api/Dtos/channels/Channel";
import { useChannelsQuery } from "../../../hooks/queries/implementations/useChannelsQuery";
import { useChannelProfilesQuery } from "../../../hooks/queries/implementations/useChannelProfilesQuery";
import { ChannelProfile } from "../../../hooks/api/Dtos/channelProfiles/ChannelProfile";
import { DeleteEntityModal } from "../../../components/modals/DeleteEntityModal";
import { Entity } from "../../../hooks/EntitiesName";
import ComponentCard from "../../../components/common/ComponentCard";
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import Button from "../../../components/ui/button/Button";
import { PencilIcon, PlusIcon, PrinterIcon, TrashBinIcon } from "../../../icons";
import ResponsiveTable from "../../../components/tables/ResponsiveTable";
import { Skeleton } from "../../../components/ui/skeleton/Skeleton";
import { Tooltip } from "../../../components/ui/tooltip/Tooltip";
import { IconButton } from "../../../components/ui/button/IconButton";
import { QueryPagination } from "../../../components/pagination/QueryPagination";
import Checkbox from "../../../components/form/input/Checkbox";
import { AddChannelsModal } from "./AddChannelsModal";
import { useChannelMutator } from "../../../hooks/mutators/useChannelMutator";
import { CheckedItemsActions } from "../../../components/ui/notification/CheckedItemsActions";
import { DeleteEntitiesModal } from "../../../components/modals/DeleteEntitiesModal";
import { EditChannelsModal } from "./EditChannelsModal";
import { Divider } from "../../../components/dividers/Divider";

export const ChannelsPage = () => {
    const { t } = useTranslation();
    const mutator = useChannelMutator();

    const [allItemsChecked, setAllItemsChecked] = useState(false);
    const [allPageItemsChecked, setAllPageItemsChecked] = useState(false);
    const [checkedItems, setCheckedItems] = useState<Set<Channel>>(new Set<Channel>());

    const [channelsToDelete, setChannelsToDelete] = useState<Channel[]>([]);
    const [isOpenDeleteConfirmModal, setIsOpenDeleteConfirmModal] = useState(false);

    const [channelsToEdit, setChannelsToEdit] = useState<Channel[]>([]);

    const [addChannelsModalOpen, setAddChannelsModalOpen] = useState(false);
    const [state, setState] = useState({
        page: 0,
        pageSize: 25,
        deleteEntity: undefined as Channel | undefined,
    })

    const channelsQuery = useChannelsQuery({
        page: state.page,
        pageSize: state.pageSize,
    })
    const profileIds = useMemo(() => Array.from(new Set(channelsQuery.data.map(d => d.channelProfileId))), [channelsQuery.data])

    const profilesQuery = useChannelProfilesQuery(profileIds.length == 0 ? undefined : {
        ids: profileIds,
        page: 0,
        pageSize: undefined,
    });
    const profilesMap = useMemo(() => {
        const result = new Map<string, ChannelProfile>();
        for(const profile of profilesQuery.data) {
            result.set(profile.id, profile);
        }
        return result;
    }, [profilesQuery.data]);

    
    const onCheckAllPageItems = (isChecked: boolean) => setCheckedItems(new Set<Channel>(isChecked ? channelsQuery.data : []));

    const onCheckItem = (item: Channel, isChecked: boolean) => setCheckedItems(p => {
        let result = new Set(p);
        if(isChecked) {
            result.add(item);
        } else {
            result.delete(item);
        }
        return result;
    })

    const onEditSelectedItems = () => {
        const checkedQrCodes = Array.from(checkedItems.values());
        setChannelsToEdit(checkedQrCodes);
    }

    const onDeleteSelectedItems = () => {
        setChannelsToDelete(allItemsChecked ? [] : Array.from(checkedItems.values()));
        setIsOpenDeleteConfirmModal(true);
    }

    const rowAction = (evt: React.MouseEvent<HTMLElement, MouseEvent>, action: () => any) => {
        evt.stopPropagation();
        action();
    }

    const onEditChannelModalClose = () => {
        setChannelsToEdit([]);
        setCheckedItems(new Set<Channel>());
    }

    useEffect(() => {
        const allPageChecked = checkedItems.size > 0 && checkedItems.size == channelsQuery.data.length;
        setAllPageItemsChecked(allPageChecked);

        if (!allItemsChecked) {
            setAllItemsChecked(allPageChecked && channelsQuery.totalPages == 1);
        }
    }, [checkedItems]);

    useEffect(() => {
        if (!allPageItemsChecked) {
            setAllItemsChecked(false);
        }
    }, [allPageItemsChecked]);

    
    useEffect(() => {
        if (allItemsChecked) {
            onCheckAllPageItems(true);
        }
    }, [allItemsChecked]);

    return <>
        <PageMeta
            title={t("pages.channels.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb pageTitle={t("pages.channels.title")} />

        <ComponentCard title={t("pages.channels.title")}>
            {
                checkedItems.size > 0 &&
                    <CheckedItemsActions 
                        itemsPerPage={state.pageSize} 
                        totalCheckedItems={checkedItems.size} 
                        areAllItemsChecked={allItemsChecked} 
                        totalItems={channelsQuery.totalItems} 
                        onAllItemsChecked={() => setAllItemsChecked(true)} 
                        actions={[
                            <Button
                                variant="outline"
                                onClick={onEditSelectedItems}
                                startIcon={<PencilIcon className="size-5" />}
                            >
                                {t("common.edit")}
                            </Button>,
                            <Button
                                variant="outline" 
                                onClick={onDeleteSelectedItems}
                                startIcon={<TrashBinIcon className="size-5" />}
                            >
                                    {t("common.delete")}
                            </Button>,
                        ]} 
                    />
            }
            <div className="overflow-hidden rounded-2xl border border-gray-200 bg-white pt-4 dark:border-white/[0.05] dark:bg-white/[0.03]">
                <div className="flex flex-col gap-4 px-6 mb-4 sm:flex-row sm:items-center sm:justify-between">
                    <Button
                        size="md"
                        variant="primary"
                        startIcon={<PlusIcon />}
                        onClick={() => setAddChannelsModalOpen(true)}
                    >
                        {
                            t("common.operations.new", {
                                name: t("common.entities.channels")
                            })
                        }
                    </Button>
                </div>
                <div className="max-w-full overflow-x-auto">
                    <ResponsiveTable
                        isLoading={channelsQuery.isFirstLoading}
                        columns={[
                            {
                                render: d => <Checkbox
                                    checked={checkedItems.has(d) || false}
                                    onChange={(e) => onCheckItem(d, e)}
                                />,
                                label: <Checkbox 
                                    checked={allPageItemsChecked}
                                    onChange={(e) => onCheckAllPageItems(e)}
                                />,
                                key: "checks"
                            },
                            {
                                key: "identifier",
                                render: (d) => d.name,
                                label: t("common.identifier"),
                            },
                            {
                                key: "profile",
                                render: d => {
                                    if(profilesQuery.isFirstLoading) {
                                        return <Skeleton className="w-24" />
                                    }
                                    const profile = profilesMap.get(d.channelProfileId);
                                    return profile?.name ?? <Skeleton className="w-24" />;
                                },
                                label: t("common.entities.channelProfile"),
                            },
                            {
                                render: d => <>
                                    <Tooltip message={t("common.edit")}>
                                        <IconButton
                                            onClick={() => {}}
                                            className="!text-gray-700 hover:!text-error-500 dark:!text-gray-400 dark:!hover:text-error-500"
                                        >
                                            <PrinterIcon className="size-5" />
                                        </IconButton>
                                    </Tooltip>
                                    <Tooltip message={t("common.edit")}>
                                        <IconButton
                                            onClick={(e) => rowAction(e, () => setChannelsToEdit([d]))}
                                            className="!text-gray-700 hover:!text-error-500 dark:!text-gray-400 dark:!hover:text-error-500"
                                        >
                                            <PencilIcon className="size-5" />
                                        </IconButton>
                                    </Tooltip>
                                    <Tooltip message={t("common.delete")}>
                                        <IconButton
                                            onClick={() => setState(s => ({ ...s, deleteEntity: d}))}
                                            className="!text-gray-700 hover:!text-error-500 dark:!text-gray-400 dark:!hover:text-error-500"
                                        >
                                            <TrashBinIcon className="size-5" />
                                        </IconButton>
                                    </Tooltip>
                                </>,
                                key: "actions",
                                label: "",
                                isActions: true,
                            },
                        ]}
                        data={channelsQuery.data}
                        getKey={(d) => d.id}
                    />
                    <Divider />
                    <QueryPagination
                        query={channelsQuery}
                        onPageIndexChange={p => setState(s => ({ ...s, page: p }))}
                        pageSize={state.pageSize}
                    />
                </div>
            </div>
        </ComponentCard>
        <AddChannelsModal
            isOpen={addChannelsModalOpen}
            onClose={() => setAddChannelsModalOpen(false)}
        />
        <DeleteEntityModal
            model={state.deleteEntity}
            entity={Entity.Channels}
            action={s => mutator.delete([s])}
            getName={s => s.name}
            onClose={() => setState(s => ({ ...s, deleteEntity: undefined}))}
        />
        <DeleteEntitiesModal
            isOpen={isOpenDeleteConfirmModal}
            model={channelsToDelete}
            entity={Entity.Channels}
            action={async s => {
                await mutator.delete(s);
                setCheckedItems(new Set<Channel>());
            }}
            getName={s => s.name}
            onClose={() => setIsOpenDeleteConfirmModal(false)}
        />
        <EditChannelsModal
            applyToAll={allItemsChecked}
            channelIds={allItemsChecked ? [] : channelsToEdit.map(q => q.id)}
            isOpen={channelsToEdit.length > 0}
            channelProfileId={channelsToEdit.length > 0 ? channelsToEdit[0].channelProfileId : undefined}
            onClose={onEditChannelModalClose}
        />
    </>
}