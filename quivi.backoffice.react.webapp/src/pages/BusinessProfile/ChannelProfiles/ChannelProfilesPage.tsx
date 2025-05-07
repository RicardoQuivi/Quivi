import { useTranslation } from "react-i18next";
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { useChannelProfilesQuery } from "../../../hooks/queries/implementations/useChannelProfilesQuery";
import { useMemo, useState } from "react";
import ResponsiveTable from "../../../components/tables/ResponsiveTable";
import CurrencySpan from "../../../components/currency/CurrencySpan";
import { usePosIntegrationsQuery } from "../../../hooks/queries/implementations/usePosIntegrationsQuery";
import { PosIntegration } from "../../../hooks/api/Dtos/posIntegrations/PosIntegration";
import { Skeleton } from "../../../components/ui/skeleton/Skeleton";
import Button from "../../../components/ui/button/Button";
import { PencilIcon, PlusIcon, TrashBinIcon } from "../../../icons";
import { useNavigate } from "react-router";
import { useIntegrationHelper } from "../../../utilities/useIntegrationHelper";
import { ChannelModeName } from "../../../components/channels/ChannelModeName";
import { Tooltip } from "../../../components/ui/tooltip/Tooltip";
import { IconButton } from "../../../components/ui/button/IconButton";
import Badge from "../../../components/ui/badge/Badge";
import { DeleteEntityModal } from "../../../components/modals/DeleteEntityModal";
import { Entity } from "../../../hooks/EntitiesName";
import { ChannelProfile } from "../../../hooks/api/Dtos/channelProfiles/ChannelProfile";
import { useChannelProfileMutator } from "../../../hooks/mutators/useChannelProfileMutator";
import { QueryPagination } from "../../../components/pagination/QueryPagination";
import { Divider } from "../../../components/dividers/Divider";

export const ChannelProfilesPage = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const integrationHelper = useIntegrationHelper();
    const mutator = useChannelProfileMutator();
    
    const [state, setState] = useState({
        page: 0,
        pageSize: 25,
        deleteEntity: undefined as ChannelProfile | undefined,
    })
    const profilesQuery = useChannelProfilesQuery({
        page: state.page,
        pageSize: state.pageSize,
    })
    const integrationIds = useMemo(() => profilesQuery.data.map(d => d.posIntegrationId), [profilesQuery.data])

    const integrationsQuery = usePosIntegrationsQuery(integrationIds.length == 0 ? undefined : {
        ids: integrationIds,
        page: 0,
        pageSize: undefined,
    });
    const integrationsMap = useMemo(() => {
        const result = new Map<string, PosIntegration>();
        for(const integration of integrationsQuery.data) {
            result.set(integration.id, integration);
        }
        return result;
    }, [integrationsQuery.data]);

    const rowAction = (evt: React.MouseEvent<HTMLElement, MouseEvent>, action: () => any) => {
        evt.stopPropagation();
        action();
    }

    return <>
        <PageMeta
            title={t("pages.channelProfiles.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb pageTitle={t("pages.channelProfiles.title")} />

        <ComponentCard title={t("pages.channelProfiles.title")}>
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
                                name: t("common.entities.channelProfile")
                            })
                        }
                    </Button>
                </div>

                <div className="max-w-full overflow-x-auto">
                    <ResponsiveTable
                        isLoading={profilesQuery.isFirstLoading}
                        columns={[
                            {
                                key: "name",
                                render: (d) => d.name,
                                label: t("common.name"),
                            },
                            {
                                render: d => <ChannelModeName features={d.features} />,
                                label: t("common.mode"),
                                key: "mode",
                            },
                            {
                                key: "integration",
                                render: d => {
                                    if(integrationsQuery.isFirstLoading) {
                                        return <Skeleton className="w-24" />
                                    }
                                    const integration = integrationsMap.get(d.posIntegrationId);
                                    if(integration == undefined) {
                                        return "";
                                    }

                                    return integrationHelper.getStrategyName(integration.type);
                                },
                                label: t("pages.channelProfiles.integration"),
                            },
                            {
                                key: "minAmount",
                                render: d => <CurrencySpan value={d.minimumPrePaidOrderAmount} />,
                                label: t("pages.channelProfiles.minimumPrePaidOrderAmount"),
                            },
                            {
                                key: "required",
                                render: d => (
                                    <Badge
                                        variant="light"
                                        color={d.sendToPreparationTimer != undefined ? "success" : "light" }
                                        size="sm"
                                    >
                                        {t(`common.${d.sendToPreparationTimer == undefined ? "inactive" : "active"}`)}
                                    </Badge>
                                ),
                                label: t("pages.channelProfiles.preparationTimer"),
                            },
                            {
                                render: d => <>
                                    <Tooltip message={t("common.edit")}>
                                        <IconButton
                                            onClick={(e) => rowAction(e, () => navigate(`/businessProfile/channels/profiles/${d.id}/edit`))}
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
                        data={profilesQuery.data}
                        getKey={(d) => d.id}
                    />
                    <Divider />
                    <QueryPagination
                        query={profilesQuery}
                        onPageIndexChange={p => setState(s => ({ ...s, page: p }))}
                        pageSize={state.pageSize}
                    />
                </div>
            </div>
        </ComponentCard>
        <DeleteEntityModal
            model={state.deleteEntity}
            entity={Entity.ChannelProfiles}
            action={s => mutator.delete(s)}
            getName={s => s.name}
            onClose={() => setState(s => ({ ...s, deleteEntity: undefined}))}
        />
    </>
}