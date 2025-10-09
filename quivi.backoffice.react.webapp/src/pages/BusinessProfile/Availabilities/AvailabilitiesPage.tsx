import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { useState } from "react";
import Button from "../../../components/ui/button/Button";
import { PencilIcon, PlusIcon, TrashBinIcon } from "../../../icons";
import { QueryPagination } from "../../../components/pagination/QueryPagination";
import { Divider } from "../../../components/dividers/Divider";
import { ResponsiveTable } from "../../../components/tables/ResponsiveTable";
import { useAvailabilitiesQuery } from "../../../hooks/queries/implementations/useAvailabilitiesQuery";
import { Availability } from "../../../hooks/api/Dtos/availabilities/Availability";
import { LinkToMenuItemsModal } from "./LinkToMenuItemsModal";
import { LinkToChannelProfilesModal } from "./LinkToChannelProfilesModal";
import { DeleteEntityModal } from "../../../components/modals/DeleteEntityModal";
import { Entity } from "../../../hooks/EntitiesName";
import { useAvailabilityMutator } from "../../../hooks/mutators/useAvailabilityMutator";

export const AvailabilitiesPage = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const mutator = useAvailabilityMutator();

    const [state, setState] = useState({
        page: 0,
        pageSize: 25,
        deleteEntity: undefined as Availability | undefined,
        linkMenuItemsEntity: undefined as Availability | undefined,
        linkChannelProfilesEntity: undefined as Availability | undefined,
    })
    const availabilitiesQuery = useAvailabilitiesQuery({
        page: state.page,
        pageSize: state.pageSize,
    })

    return <>
        <PageMeta
            title={t("pages.availabilities.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb pageTitle={t("pages.availabilities.title")} />

        <ComponentCard title={t("pages.availabilities.title")}>
            <div className="overflow-hidden rounded-2xl border border-gray-200 bg-white pt-4 dark:border-white/[0.05] dark:bg-white/[0.03]">
                <div className="flex flex-col gap-4 px-6 mb-4 sm:flex-row sm:items-center sm:justify-between">
                    <Button
                        size="md"
                        variant="primary"
                        startIcon={<PlusIcon />}
                        onClick={() => navigate("add")}
                    >
                        {
                            t("common.operations.new", {
                                name: t("common.entities.availability")
                            })
                        }
                    </Button>
                </div>
                <div className="max-w-full overflow-x-auto">
                    <ResponsiveTable
                        isLoading={availabilitiesQuery.isFirstLoading}
                        name={{
                            key: "name",
                            render: (d) => d.name,
                            label: t("common.name"),
                        }}
                        columns={[
                            {
                                key: "items",
                                label: t("pages.availabilities.associatedMenuItems"),
                                render: d => <span 
                                    className="text-sm font-normal text-brand-500 cursor-pointer"
                                    onClick={() => setState(s => ({ ...s, linkMenuItemsEntity: d}))}
                                >
                                    {t("pages.availabilities.linkToMenuItems")}
                                </span>,
                            },
                            {
                                key: "profiles",
                                label: t("pages.availabilities.associatedChannelProfiles"),
                                render: d => <span 
                                    className="text-sm font-normal text-brand-500 cursor-pointer"
                                    onClick={() => setState(s => ({ ...s, linkChannelProfilesEntity: d}))}
                                >
                                    {t("pages.availabilities.linkToChannelProfiles")}
                                </span>,
                            }
                        ]}
                        actions={[
                            {
                                render: () => <PencilIcon className="size-5" />,
                                key: "edit",
                                label: t("common.edit"),
                                onClick: d => navigate(`/businessProfile/availabilities/${d.id}/edit`),
                            },
                            {
                                render: () => <TrashBinIcon className="size-5" />,
                                key: "delete",
                                label: t("common.delete"),
                                onClick: d => setState(s => ({ ...s, deleteEntity: d}))
                            },
                        ]}
                        data={availabilitiesQuery.data}
                        getKey={d => d.id}
                    />
                    <Divider />
                    <QueryPagination
                        query={availabilitiesQuery}
                        onPageIndexChange={p => setState(s => ({ ...s, page: p }))}
                        pageSize={state.pageSize}
                    />
                </div>
            </div>
        </ComponentCard>
        <DeleteEntityModal
            model={state.deleteEntity}
            entity={Entity.Availabilities}
            action={s => mutator.delete(s)}
            getName={s => s.name}
            onClose={() => setState(s => ({ ...s, deleteEntity: undefined}))}
        />
        <LinkToMenuItemsModal
            model={state.linkMenuItemsEntity}
            onClose={() => setState(s => ({ ...s, linkMenuItemsEntity: undefined}))}
        />
        <LinkToChannelProfilesModal
            model={state.linkChannelProfilesEntity}
            onClose={() => setState(s => ({ ...s, linkChannelProfilesEntity: undefined}))}
        />
    </>
}