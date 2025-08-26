import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { useState } from "react";
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import Button from "../../../components/ui/button/Button";
import { LinkIcon, PencilIcon, PlusIcon, TrashBinIcon } from "../../../icons";
import { ResponsiveTable } from "../../../components/tables/ResponsiveTable";
import { Divider } from "../../../components/dividers/Divider";
import { QueryPagination } from "../../../components/pagination/QueryPagination";
import { useConfigurableFieldsQuery } from "../../../hooks/queries/implementations/useConfigurableFieldsQuery";
import { ConfigurableField } from "../../../hooks/api/Dtos/configurableFields/ConfigurableField";
import { useConfigurableFieldMutator } from "../../../hooks/mutators/useConfigurableFieldMutator";
import { DeleteEntityModal } from "../../../components/modals/DeleteEntityModal";
import { Entity } from "../../../hooks/EntitiesName";
import { ConfigurableFieldType } from "../../../hooks/api/Dtos/configurableFields/ConfigurableFieldType";
import { PrintedOn } from "../../../hooks/api/Dtos/configurableFields/PrintedOn";
import Checkbox from "../../../components/form/input/Checkbox";
import { AssignedOn } from "../../../hooks/api/Dtos/configurableFields/AssignedOn";
import { LinkToChannelProfilesModal } from "./LinkToChannelProfilesModal";

export const ConfigurableFieldsPage = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const mutator = useConfigurableFieldMutator();
    
    const [state, setState] = useState({
        page: 0,
        pageSize: 25,
        deleteEntity: undefined as ConfigurableField | undefined,
        linkEntity: undefined as ConfigurableField | undefined,
    })
    const configurableFieldsQuery = useConfigurableFieldsQuery({
        page: state.page,
        pageSize: state.pageSize,
    })

    const getTypeName = (type: ConfigurableFieldType): string => {
        switch(type)
        {
            case ConfigurableFieldType.Text: return t("pages.configurableFields.configurableFieldType.text");
            case ConfigurableFieldType.LongText: return t("pages.configurableFields.configurableFieldType.longText");
            case ConfigurableFieldType.Check: return t("pages.configurableFields.configurableFieldType.check");
            case ConfigurableFieldType.Number: return t("pages.configurableFields.configurableFieldType.number");
        }
    }

    const getPrintedOnName = (type: PrintedOn): string => {
        switch(type)
        {
            case PrintedOn.None: return t("pages.configurableFields.printedOn.none");
            case PrintedOn.PreparationRequest: return t("pages.configurableFields.printedOn.preparationRequest");
            case PrintedOn.SessionBill: return t("pages.configurableFields.printedOn.sessionBill");
        }
    }


    return <>
        <PageMeta
            title={t("pages.configurableFields.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.configurableFields.title")}
        />

        <ComponentCard title={t("pages.configurableFields.title")}>
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
                                name: t("common.entities.configurableFields")
                            })
                        }
                    </Button>
                </div>

                <div className="max-w-full overflow-x-auto">
                    <ResponsiveTable
                        isLoading={configurableFieldsQuery.isFirstLoading}
                        name={{
                            key: "name",
                            render: (d) => d.name,
                            label: t("common.name"),
                        }}
                        columns={[
                            {
                                render: d => getTypeName(d.type),
                                label: t("common.type"),
                                key: "numbersOnly",
                            },
                            {
                                render: d => <Checkbox disabled checked={d.isRequired} />,
                                label: t("common.required"),
                                key: "required",
                            },
                            {
                                render: d => <Checkbox disabled checked={d.isAutoFill} />,
                                label: t("common.autoFill"),
                                key: "autofill",
                            },
                            {
                                render: d => d.defaultValue == undefined ? <b>{t("common.none")}</b> : d.defaultValue,
                                label: t("common.defaultValue"),
                                key: "defaultValue",
                            },
                            {
                                render: d => getPrintedOnName(d.printedOn),
                                label: t("pages.configurableFields.printable"),
                                key: "isPrintable",
                            },
                            {
                                render: d => <Checkbox disabled checked={(d.assignedOn & AssignedOn.PoSSessions) == AssignedOn.PoSSessions} />,
                                label: t("pages.configurableFields.forPosSessions"),
                                key: "forPoSSessions",
                            },
                            {
                                render: d => <Checkbox disabled checked={(d.assignedOn & AssignedOn.Ordering) == AssignedOn.Ordering} />,
                                label: t("pages.configurableFields.forOrdering"),
                                key: "forOrdering",
                            },
                        ]}
                        actions={[
                            {
                                render: () => <LinkIcon className="size-5" />,
                                key: "link",
                                label: t("common.linkEntities", {
                                    name: t("common.entities.channelProfiles")
                                }),
                                onClick: d => setState(s => ({ ...s, linkEntity: d}))
                            },
                            {
                                render: () => <PencilIcon className="size-5" />,
                                key: "edit",
                                label: t("common.edit"),
                                onClick: d => navigate(`/businessProfile/configurablefields/${d.id}/edit`)
                            },
                            {
                                render: () => <TrashBinIcon className="size-5" />,
                                key: "delete",
                                label: t("common.delete"),
                                onClick: d => setState(s => ({ ...s, deleteEntity: d}))
                            },
                        ]}
                        data={configurableFieldsQuery.data}
                        getKey={(d) => d.id}
                    />
                    <Divider />
                    <QueryPagination
                        query={configurableFieldsQuery}
                        onPageIndexChange={p => setState(s => ({ ...s, page: p }))}
                        pageSize={state.pageSize}
                    />
                </div>
            </div>
        </ComponentCard>
        <DeleteEntityModal
            model={state.deleteEntity}
            entity={Entity.ConfigurableFields}
            action={s => mutator.delete(s)}
            getName={s => s.name}
            onClose={() => setState(s => ({ ...s, deleteEntity: undefined}))}
        />
        <LinkToChannelProfilesModal
            model={state.linkEntity}
            onClose={() => setState(s => ({ ...s, linkEntity: undefined}))}
        />
    </>
}