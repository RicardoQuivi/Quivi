import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { useState } from "react";
import Button from "../../../components/ui/button/Button";
import { PencilIcon, PlusIcon, TrashBinIcon } from "../../../icons";
import ResponsiveTable from "../../../components/tables/ResponsiveTable";
import { QueryPagination } from "../../../components/pagination/QueryPagination";
import { IconButton } from "../../../components/ui/button/IconButton";
import { Tooltip } from "../../../components/ui/tooltip/Tooltip";
import { Divider } from "../../../components/dividers/Divider";
import { useAcquirerConfigurationsQuery } from "../../../hooks/queries/implementations/useAcquirerConfigurationsQuery";
import { AcquirerConfiguration } from "../../../hooks/api/Dtos/acquirerconfigurations/AcquirerConfiguration";
import Avatar from "../../../components/ui/avatar/Avatar";
import { ChargePartner } from "../../../hooks/api/Dtos/acquirerconfigurations/ChargePartner";
import { ChargeMethod } from "../../../hooks/api/Dtos/ChargeMethod";
import { PartnerIcon } from "../../../icons/PartnerIcon";
import { ChargeMethodIcon } from "../../../icons/ChargeMethodIcon";

export const AcquirerConfigurationsPage = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    //const mutator = useAcquirerConfigurationMutator();

    const [state, setState] = useState({
        page: 0,
        pageSize: 25,
        deleteEntity: undefined as AcquirerConfiguration | undefined,
    })
    const acquirersQuery = useAcquirerConfigurationsQuery({
        page: state.page,
        pageSize: state.pageSize,
    })

    const rowAction = (evt: React.MouseEvent<HTMLElement, MouseEvent>, action: () => any) => {
        evt.stopPropagation();
        action();
    }

    const getPartnerName = (partner: ChargePartner) => {
        switch (partner) {
            case ChargePartner.Quivi: return "Quivi";
            case ChargePartner.TicketRestaurant: return "Ticket Restaurant";
            case ChargePartner.SibsPaymentGateway: return "Sibs";
            case ChargePartner.Stripe: return "Stripe";
            case ChargePartner.Checkout: return "Checkout";
            case ChargePartner.Paybyrd: return "Paybyrd";
        }
    }

    const getMethodName = (method: ChargeMethod) => t(`common.chargeMethod.${ChargeMethod[method]}`);

    return <>
        <PageMeta
            title={t("pages.acquirerConfigurations.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb pageTitle={t("pages.acquirerConfigurations.title")} />

        <ComponentCard title={t("pages.acquirerConfigurations.title")}>
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
                                name: t("common.entities.acquirerConfiguration")
                            })
                        }
                    </Button>
                </div>
                <div className="max-w-full overflow-x-auto">
                    <ResponsiveTable
                        isLoading={acquirersQuery.isFirstLoading}
                        columns={[
                            {
                                key: "partner",
                                label: t("common.partner"),
                                render: item => {
                                    const name = getPartnerName(item.partner)
                                    return (
                                        <div className="flex items-center gap-3">
                                            <Avatar
                                                src={<PartnerIcon partner={item.partner} className="size-full" />}
                                                alt={name}
                                                size="large"
                                            />
                                            <div>
                                                <span className="block font-medium text-gray-800 text-theme-sm dark:text-white/90">
                                                    {name}
                                                </span>
                                            </div>
                                        </div>
                                    )
                                }
                            },
                            {
                                key: "name",
                                label: t("common.name"),
                                render: item => {
                                    const name = getMethodName(item.method);
                                    return (
                                        <div className="flex items-center gap-3">
                                            <Avatar
                                                src={<ChargeMethodIcon chargeMethod={item.method} className="size-full" />}
                                                alt={name}
                                                size="large"
                                            />
                                            <div>
                                                <span className="block font-medium text-gray-800 text-theme-sm dark:text-white/90">
                                                    {name}
                                                </span>
                                            </div>
                                        </div>
                                    )
                                }
                            },
                            {
                                render: d => <>
                                    <Tooltip message={t("common.edit")}>
                                        <IconButton
                                            onClick={(e) => rowAction(e, () => navigate(`/admin/acquirerConfigurations/${d.id}/edit`))}
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
                        data={acquirersQuery.data}
                        getKey={d => d.id}
                    />
                    <Divider />
                    <QueryPagination
                        query={acquirersQuery}
                        onPageIndexChange={p => setState(s => ({ ...s, page: p }))}
                        pageSize={state.pageSize}
                    />
                </div>
            </div>
        </ComponentCard>
        {/* <DeleteEntityModal
            model={state.deleteEntity}
            entity={Entity.AcquirerConfigurations}
            action={s => mutator.delete(s)}
            getName={s => s.name}
            onClose={() => setState(s => ({ ...s, deleteEntity: undefined}))}
        /> */}
    </>
}