
interface DividerProps {
    readonly children?: React.ReactNode;
    readonly className?: string;
}

export const Divider: React.FC<DividerProps> = (props: DividerProps) => {
    return (
        <div className={`flex items-center w-full my-2 ${props.className ?? ""}`}>
            <div className="flex-1 h-px bg-gray-300" />
            {
                props.children != undefined &&
                <span className="px-2 text-gray-300 text-sm whitespace-nowrap">
                    {props.children}
                </span>
            }
            {props.children != undefined && <div className="flex-1 h-px bg-gray-300" />}
        </div>
    );
}