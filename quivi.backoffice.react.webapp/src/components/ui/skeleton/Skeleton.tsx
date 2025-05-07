interface Props {
    readonly className?: string;
}
export const Skeleton = (props: Props) => {
    return (
    <div className={`flex animate-pulse space-x-4 ${props.className ?? ""}`}>
        <div className="h-auto rounded bg-gray-200 w-full">
            <p className="text-sm text-center text-gray-500 dark:text-gray-400 collapse">Loading...</p>
        </div>
    </div>
    )
}