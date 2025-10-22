import { Skeleton } from "../../../components/ui/skeleton/Skeleton";

interface MetricWidgetProps {
    readonly title: string;
    readonly children: React.ReactNode;
    readonly percentage?: number;
    readonly isLoading?: boolean;
}
export const MetricWidget = (props: MetricWidgetProps) => {
    return (
        <div
            className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]"
        >
            <div className="flex items-start justify-center">
                <p className="text-gray-500 text-theme-sm dark:text-gray-400">
                    {props.title}
                </p>
            </div>

            <div className="flex items-end justify-center mt-3 gap-2">
                <div className="flex items-start justify-between flex-col">
                    <h4 className="text-2xl font-bold text-gray-800 dark:text-white/90">
                        {
                            props.isLoading == true
                            ?
                            <Skeleton />
                            :
                            props.children
                        }
                    </h4>
                </div>
            </div>
        </div>
    )
}