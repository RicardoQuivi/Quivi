import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { useMemo, useState } from "react";
import { useLocalsQuery } from "../../../../hooks/queries/implementations/useLocalsQuery";
import { Local } from "../../../../hooks/api/Dtos/locals/Local";
import ComponentCard from "../../../../components/common/ComponentCard";
import Button from "../../../../components/ui/button/Button";
import { PencilIcon, PlusIcon, TrashBinIcon } from "../../../../icons";
import ResponsiveTable from "../../../../components/tables/ResponsiveTable";
import { Skeleton } from "../../../../components/ui/skeleton/Skeleton";
import { IconButton } from "../../../../components/ui/button/IconButton";
import { Tooltip } from "../../../../components/ui/tooltip/Tooltip";
import { DeleteEntityModal } from "../../../../components/modals/DeleteEntityModal";
import { Entity } from "../../../../hooks/EntitiesName";
import { usePrintersQuery } from "../../../../hooks/queries/implementations/usePrintersQuery";
import { Printer } from "../../../../hooks/api/Dtos/printers/Printer";
import { usePrinterMutator } from "../../../../hooks/mutators/usePrinterMutator";
import { usePrinterWorkersQuery } from "../../../../hooks/queries/implementations/usePrinterWorkersQuery";
import { PrinterWorker } from "../../../../hooks/api/Dtos/printerWorkers/PrinterWorker";
import Badge from "../../../../components/ui/badge/Badge";

interface PrintersCardProps {
    readonly printerWorkerId?: string;   
}
export const PrintersCard = (props: PrintersCardProps) => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const mutator = usePrinterMutator();

    const [printerToDelete, setPrinterToDelete] = useState<Printer>();

    const printersQuery = usePrintersQuery({
        printerWorkerId: props.printerWorkerId ?? undefined,
        page: 0,
    });

    const workersQuery = usePrinterWorkersQuery({
        page: 0,
    })
    const workersMap = useMemo(() => {
        const map = new Map<string, PrinterWorker>();
        for(const l of workersQuery.data) {
            map.set(l.id, l);
        }
        return map;
    }, [workersQuery.data])

    const localIds = useMemo(() => {
        const set = new Set<string>();
        for(const item of printersQuery.data) {
            if(item.locationId == undefined) {
                continue;
            }

            set.add(item.locationId);
        }
        return Array.from(set.values());
    }, [printersQuery.data])
    const localsQuery = useLocalsQuery({
        ids: localIds,
        page: 0,
    })
    const localsMap = useMemo(() => {
        const map = new Map<string, Local>();
        for(const l of localsQuery.data) {
            map.set(l.id, l);
        }
        return map;
    }, [localsQuery.data])

    const rowAction = (evt: React.MouseEvent<HTMLElement, MouseEvent>, action: () => any) => {
        evt.stopPropagation();
        action();
    }

    return <ComponentCard
        title={t("common.entities.printers")}
        className="size-full"
    >

        <div className="flex flex-col gap-2 px-4 py-4 border border-b-0 border-gray-100 dark:border-white/[0.05] rounded-t-xl sm:flex-row sm:items-center sm:justify-between">
            <div className="flex items-center gap-3">
                <Button
                    variant="primary"
                    size="sm"
                    startIcon={<PlusIcon />}
                    onClick={() => navigate(`/settings/printersmanagement/printers/add?${props.printerWorkerId == undefined ? "" : `printerWorkerId=${props.printerWorkerId}`}`)}
                    disabled={workersQuery.data.length == 0}
                >
                    {
                        t("common.operations.new", {
                            name: t("common.entities.printer")
                        })
                    }
                </Button>
            </div>
        </div>
        <div className="max-w-full overflow-x-auto">
            <ResponsiveTable
                isLoading={printersQuery.isFirstLoading}
                columns={[
                    {
                        key: "name",
                        label: t("common.name"),
                        render: item => <>
                            <span className="block font-medium text-gray-800 text-theme-sm dark:text-white/90">
                                {item.name}
                            </span>
                        </>
                    },
                    {
                        key: "address",
                        label: t("common.hardwareAddress"),
                        render: item => <>
                            <span className="block font-medium text-gray-800 text-theme-sm dark:text-white/90">
                                {item.address}
                            </span>
                        </>
                    },
                    {
                        key: "worker",
                        label: t("common.entities.printerWorkers"),
                        render: item => {
                            const worker = workersMap.get(item.printerWorkerId);
                            if(worker == undefined) {
                                return <Skeleton className="w-md"/>;
                            }

                            const isSelectedWorker = item.printerWorkerId === props.printerWorkerId;
                            return <Badge 
                                variant={isSelectedWorker ? "solid" : "light"}
                                color={isSelectedWorker ? "primary" : "light"}
                            >
                                {worker.name}
                            </Badge>;
                        }
                    },
                    {
                        key: "location",
                        label: t("common.entities.local"),
                        render: item => {
                            if(item.locationId == undefined) {
                                return t("common.noLocation");
                            }

                            const local = localsMap.get(item.locationId);
                            if(local == undefined) {
                                return <Skeleton className="w-md"/>;
                            }

                            return <Badge 
                                variant="light"
                                color="light"
                            >
                                {local.name}
                            </Badge>;
                        }
                    },
                    {
                        key: "actions",
                        render: d => <>
                            <Tooltip message={t("common.edit")}>
                                <IconButton
                                    onClick={(e) => rowAction(e, () => navigate(`/settings/printersmanagement/printers/${d.id}/edit`, ))}
                                    className="!text-gray-700 hover:!text-error-500 dark:!text-gray-400 dark:!hover:text-error-500"
                                >
                                    <PencilIcon className="size-5" />
                                </IconButton>
                            </Tooltip>
                            <Tooltip message={t("common.delete")}>
                                <IconButton
                                    onClick={() => setPrinterToDelete(d)}
                                    className="!text-gray-700 hover:!text-error-500 dark:!text-gray-400 dark:!hover:text-error-500"
                                >
                                    <TrashBinIcon className="size-5" />
                                </IconButton>
                            </Tooltip>
                        </>,
                        label: "",
                        isActions: true,
                    },
                ]}
                data={printersQuery.data}
                getKey={d => d.id}
            />
        </div>
        <DeleteEntityModal
            model={printerToDelete}
            entity={Entity.Printers}
            getName={m => m.name}
            action={mutator.delete}
            onClose={() => setPrinterToDelete(undefined)}
        />
    </ComponentCard>
}