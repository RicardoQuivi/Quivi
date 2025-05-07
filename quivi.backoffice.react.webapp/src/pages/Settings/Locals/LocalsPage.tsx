import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { useState } from "react";
import { useLocalsQuery } from "../../../hooks/queries/implementations/useLocalsQuery";
import { Local } from "../../../hooks/api/Dtos/locals/Local";
import { DeleteEntityModal } from "../../../components/modals/DeleteEntityModal";
import { useLocalMutator } from "../../../hooks/mutators/useLocalMutator";
import { Entity } from "../../../hooks/EntitiesName";
import Button from "../../../components/ui/button/Button";
import { PencilIcon, PlusIcon, TrashBinIcon } from "../../../icons";
import ResponsiveTable from "../../../components/tables/ResponsiveTable";
import { QueryPagination } from "../../../components/pagination/QueryPagination";
import { IconButton } from "../../../components/ui/button/IconButton";
import { Tooltip } from "../../../components/ui/tooltip/Tooltip";
import { Divider } from "../../../components/dividers/Divider";

export const LocalsPage = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const mutator = useLocalMutator();

    const [state, setState] = useState({
        page: 0,
        pageSize: 25,
        deleteEntity: undefined as Local | undefined,
    })
    const localsQuery = useLocalsQuery({
        page: state.page,
        pageSize: state.pageSize,
    })

    const rowAction = (evt: React.MouseEvent<HTMLElement, MouseEvent>, action: () => any) => {
        evt.stopPropagation();
        action();
    }

    return <>
        <PageMeta
            title={t("pages.locals.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb pageTitle={t("pages.locals.title")} />

        <ComponentCard title={t("pages.locals.title")}>
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
                                name: t("common.entities.local")
                            })
                        }
                    </Button>
                </div>
                <div className="max-w-full overflow-x-auto">
                    <ResponsiveTable
                        isLoading={localsQuery.isFirstLoading}
                        columns={[
                            {
                                key: "name",
                                render: (d) => d.name,
                                label: t("common.name"),
                            },
                            {
                                render: d => <>
                                    <Tooltip message={t("common.edit")}>
                                        <IconButton
                                            onClick={(e) => rowAction(e, () => navigate(`/settings/locals/${d.id}/edit`))}
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
                        data={localsQuery.data}
                        getKey={d => d.id}
                    />
                    <Divider />
                    <QueryPagination
                        query={localsQuery}
                        onPageIndexChange={p => setState(s => ({ ...s, page: p }))}
                        pageSize={state.pageSize}
                    />
                </div>
            </div>
        </ComponentCard>
        <DeleteEntityModal
            model={state.deleteEntity}
            entity={Entity.Locals}
            action={s => mutator.delete(s)}
            getName={s => s.name}
            onClose={() => setState(s => ({ ...s, deleteEntity: undefined}))}
        />
    </>
}