import { useTranslation } from "react-i18next";

interface CheckedItemsActionsProps {
    readonly totalItems: number;
    readonly itemsPerPage: number;
    readonly totalCheckedItems: number;
    readonly areAllItemsChecked: boolean;
    readonly onAllItemsChecked: () => void;
    readonly actions: React.ReactNode[];
}

export const CheckedItemsActions: React.FC<CheckedItemsActionsProps> = (props: CheckedItemsActionsProps) => {
    const { t } = useTranslation();

    return (
        <div className="relative w-full rounded-xl border border-gray-200 bg-white p-6 dark:border-gray-800 dark:bg-[#1E2634]">
            {/* Message */}
            <p className="pr-4 mb-6 text-sm text-gray-700 dark:text-gray-400">
                {
                    props.areAllItemsChecked
                    ? 
                    t("common.selection.all")
                    :
                    <>
                        {
                            t("common.selection.someItems", {
                                total: props.totalCheckedItems,
                            })
                        }
                        {
                            !props.areAllItemsChecked && props.itemsPerPage == props.totalCheckedItems &&
                            <>
                                <br/>
                                <a 
                                    href="javascript:void(0);"
                                    onClick={props.onAllItemsChecked}
                                >
                                        {` ${t("common.selection.selectAll")}`}
                                </a>
                            </>
                        }
                    </>
                }
            </p>

            
            <div className="flex flex-col justify-end gap-6 sm:flex-row sm:items-center sm:gap-4">
                <div className="flex gap-3 sm:flex-nowrap flex-wrap">
                    {
                        props.actions.map((node, index) => !!node && (
                            <div
                                key={index}
                                className="flex-1 shrink-0 sm:w-full"
                            >
                                {node}
                            </div>
                        ))
                    }
                </div>
            </div>
        </div>
    );
};