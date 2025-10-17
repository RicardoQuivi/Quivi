import { useState } from "react";
import ComponentCard from "../../../../components/common/ComponentCard";
import { DeleteEntityModal } from "../../../../components/modals/DeleteEntityModal";
import Button from "../../../../components/ui/button/Button";
import { Skeleton } from "../../../../components/ui/skeleton/Skeleton";
import { Entity } from "../../../../hooks/EntitiesName";
import { PencilIcon, PlusIcon, TrashBinIcon } from "../../../../icons";
import { useNavigate } from "react-router";
import { useTranslation } from "react-i18next";
import { usePrinterWorkersQuery } from "../../../../hooks/queries/implementations/usePrinterWorkersQuery";
import { PrinterWorker } from "../../../../hooks/api/Dtos/printerWorkers/PrinterWorker";
import { usePrinterWorkerMutator } from "../../../../hooks/mutators/usePrinterWorkerMutator";

interface PrinterWorkersCardProps {
    readonly printerWorkerId: string | undefined;
    readonly onPrinterWorkerChanged: (printerWorkerId: string | undefined) => any;
}
export const PrinterWorkersCard = (props: PrinterWorkersCardProps) => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const mutator = usePrinterWorkerMutator();
    const printerWorkersQuery = usePrinterWorkersQuery({
        page: 0,
    });

    const [printerWorkerToDelete, setPrinterWorkerToDelete] = useState<PrinterWorker>();

    return (
    <ComponentCard
        title={t("common.entities.printerWorkers")}
        description={
            <Button
                size="md"
                variant="primary"
                startIcon={<PlusIcon />}
                onClick={() => navigate("/settings/printersmanagement/workers/add")}
                className="mt-4 w-full"
            >
                {
                    t("common.operations.new", {
                        name: t("common.entities.printerWorker")
                    })
                }
            </Button>
        }
        className="size-full"
    >
        <div className="overflow-x-auto pb-2 width-full [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb]:bg-gray-100 dark:[&::-webkit-scrollbar-thumb]:bg-gray-600 [&::-webkit-scrollbar-track]:bg-white dark:[&::-webkit-scrollbar-track]:bg-transparent [&::-webkit-scrollbar]:h-1.5">
            <nav className="flex flex-row w-full sm:flex-col sm:space-y-2">
                <button
                    className={`inline-flex items-center rounded-lg px-3 py-2 text-sm font-medium transition-colors duration-200 ease-in-out sm:p-3 ${
                        props.printerWorkerId === undefined
                        ? "text-brand-500 dark:bg-brand-400/20 dark:text-brand-400 bg-brand-50"
                        : "bg-transparent text-gray-500 border-transparent hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                    }`}
                    onClick={() => props.onPrinterWorkerChanged(undefined)}
                >
                    {t("pages.printers.all")}
                </button>

                {
                    printerWorkersQuery.isFirstLoading
                    ?
                    [1, 2, 3, 4, 5].map(c => (
                        <button
                            key={`Loading-${c}`}
                            className={`inline-flex items-center rounded-lg px-3 py-2.5 text-sm font-medium transition-colors duration-200 ease-in-out sm:p-3 ${"bg-transparent text-gray-500 border-transparent hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"}`}
                            disabled
                        >
                            <Skeleton className="w-full" />
                        </button>
                    ))
                    :
                    <>
                        {
                            printerWorkersQuery.isFirstLoading
                            ?
                            [1, 2, 3, 4, 5].map(i => (
                                <button
                                    key={`Loading-${i}`}
                                    className={`inline-flex items-center rounded-lg px-3 py-2.5 text-sm font-medium transition-colors duration-200 ease-in-out sm:p-3 ${"bg-transparent text-gray-500 border-transparent hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"}`}
                                    disabled
                                >
                                    <Skeleton className="w-full" />
                                </button>
                            ))
                            :
                            printerWorkersQuery.data.map(item => 
                                <button
                                    key={item.id}
                                    className={`inline-flex items-center rounded-lg px-3 py-2 text-sm font-medium transition-colors duration-200 ease-in-out sm:p-3 ${
                                        props.printerWorkerId === item.id
                                        ? "text-brand-500 dark:bg-brand-400/20 dark:text-brand-400 bg-brand-50"
                                        : "bg-transparent text-gray-500 border-transparent hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                                    }`}
                                    onClick={() => props.onPrinterWorkerChanged(item.id)}
                                >
                                    <div className="grid grid-cols-[1fr_auto_auto] gap-2 w-full">
                                        <div className="text-start flex items-center">
                                            {item.name}
                                        </div>
                                        <div
                                            className={`flex items-center justify-center text-gray-500 transition-colors border border-gray-200 rounded-lg max-w-10 hover:bg-gray-100 hover:text-gray-700 dark:border-gray-800 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white`}
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                navigate(`/settings/printersmanagement/workers/${item.id}/edit`)
                                            }}
                                        >
                                            <PencilIcon className="size-5 m-1" />
                                        </div>
                                        <div
                                            className={`flex items-center justify-center text-gray-500 transition-colors border border-gray-200 rounded-lg max-w-10 hover:bg-gray-100 hover:text-gray-700 dark:border-gray-800 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white`}
                                            onClick={(e) => {
                                                e.stopPropagation()
                                                setPrinterWorkerToDelete(item);
                                            }}
                                        >
                                            <TrashBinIcon className="size-5 m-1" />
                                        </div>
                                    </div>
                                </button>
                            )
                        }
                    </>
                }
            </nav>
        </div>
        <DeleteEntityModal
            model={printerWorkerToDelete}
            entity={Entity.PrinterWorkers}
            getName={m => m.identifier}
            action={async m => {
                await mutator.delete(m);
                if(m.id == props.printerWorkerId) {
                    props.onPrinterWorkerChanged(undefined);
                }
            }}
            onClose={() => setPrinterWorkerToDelete(undefined)}
        />
    </ComponentCard>
    )
}