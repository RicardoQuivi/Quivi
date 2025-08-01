import { Trans, useTranslation } from "react-i18next";
import { Skeleton } from "../ui/skeleton/Skeleton";

interface Props {
    readonly isLoading: boolean;
    readonly pageIndex: number;
    readonly totalItems: number;
    readonly pageSize: number;
    readonly onPageIndexChange?: (page: number) => void;
}
export const Pagination = (props: Props) => {
    const { t } = useTranslation();

    const totalPages = Math.ceil(props.totalItems / props.pageSize);
    const currentPage = props.pageIndex + 1;
    const startIndex = (currentPage - 1) * props.pageSize;
    const endIndex = Math.min(startIndex + props.pageSize, props.totalItems);
    
    const handlePageChange = (page: number) => {
        if (page < 1 || page > totalPages) {
            return;
        }
        
        props.onPageIndexChange?.(page - 1);
    };

    const renderPageNumbers = () => {
        const pagesToShow = 5; // Show 5 pages at a time
        const startPage = props.isLoading ? 1 : Math.max(1, currentPage - Math.floor(pagesToShow / 2));
        const endPage = props.isLoading ? 5 : Math.min(totalPages, startPage + pagesToShow - 1);

        const pages = [];
        for (let i = startPage; i <= endPage; i++) {
            pages.push(<li key={i}>{renderPageButton(i)}</li>);
        }

        if (startPage > 1) {
            pages.unshift(<li key="ellipsis-start">{renderEllipsis()}</li>);
        }
        if (endPage < totalPages) {
            pages.push(<li key="ellipsis-end">{renderEllipsis()}</li>);
        }

        return pages;
    };

    const renderPageButton = (page: number) => {
        return (
            <button
                onClick={() => handlePageChange(page)}
                className={`px-4 py-2 rounded ${currentPage === page
                        ? "bg-brand-500 text-white"
                        : "text-gray-700 dark:text-gray-400"
                    } flex w-10 items-center justify-center h-10 rounded-lg text-sm font-medium hover:bg-blue-500/[0.08] hover:text-brand-500 dark:hover:text-brand-500`}
                disabled={props.isLoading}
            >
                {page}
            </button>
        );
    };

    const renderEllipsis = () => {
        return (
            <span className="flex items-center justify-center w-10 h-10 text-sm font-medium text-gray-700 dark:text-gray-400">
                ...
            </span>
        );
    };

    return (
        <div className="border border-t-0 rounded-b-xl border-gray-100 py-3 pl-[18px] pr-4 dark:border-white/[0.05]">
            <div className="flex flex-col xl:flex-row xl:items-center xl:justify-between gap-2">
                {/* Left side: Showing entries */}
                {
                    (props.isLoading || props.totalItems > 0) &&
                    <div className="flex items-center justify-center gap-4 xl:justify-start">
                        <button
                            onClick={() => handlePageChange(currentPage - 1)}
                            disabled={currentPage === 1 || props.isLoading}
                            className="flex h-10 items-center gap-2 rounded-lg border border-gray-300 bg-white p-2 sm:p-2.5 text-gray-700 shadow-theme-xs hover:bg-gray-50 hover:text-gray-800 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-400 dark:hover:bg-white/[0.03] dark:hover:text-gray-200 disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            {t("common.previous")}
                        </button>

                        <ul className="flex items-center gap-1">{renderPageNumbers()}</ul>

                        <button
                            onClick={() => handlePageChange(currentPage + 1)}
                            disabled={currentPage === totalPages || props.isLoading}
                            className="flex h-10 items-center gap-2 rounded-lg border border-gray-300 bg-white p-2 sm:p-2.5 text-gray-700 shadow-theme-xs hover:bg-gray-50 hover:text-gray-800 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-400 dark:hover:bg-white/[0.03] dark:hover:text-gray-200 disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            {t("common.next")}
                        </button>
                    </div>
                }
                <div className="flex-1 overflow-hidden">
                    {
                        props.isLoading
                        ?
                        <Skeleton className="w-full min-w-md"/>
                        :
                        <p className="pt-3 text-sm font-medium text-center text-gray-500 border-t border-gray-100 dark:border-gray-800 dark:text-gray-400 xl:border-t-0 xl:pt-0 xl:text-left">
                        {
                            props.totalItems == 0
                            ?
                            t("common.paginationNoData")
                            :
                            <Trans
                                i18nKey={"common.paginationDescription"}
                                values={{
                                    start: startIndex + 1,
                                    end: endIndex,
                                    total: props.totalItems,
                                }}
                                components={{
                                    b: <b />,
                                }}
                            />
                        }
                        </p>
                    }
                </div>
            </div>
        </div>
    )
}