import { ComponentType, SVGProps, useMemo } from "react";
import { CheckLineIcon } from "../../icons";
import { Spinner } from "../spinners/Spinner";

interface MultiSelectionZoneProps<T> {
    readonly options: T[];
    readonly selected: T[];
    readonly onChange?: (value: T[], diff: T) => any;
    readonly getId: (o: T) => string;
    readonly render: (o: T) => React.ReactNode;
    readonly checkIcon?: ComponentType<SVGProps<SVGSVGElement>>;
    readonly isLoading?: boolean;
}
export const MultiSelectionZone = <T,>(props: MultiSelectionZoneProps<T>) => {
    const selectedIds = useMemo(() => {
        const result = new Set<string>();
        for(const s of props.selected) {
            result.add(props.getId(s));
        }
        return result;
    }, [props.selected, props.getId])

    const unselectedBorder = "border border-gray-200 dark:border-gray-800";
    const selectedBorder = "border-1";
    return (
        <div
            className="grid grid-cols-2 xs:grid-cols-2 sm:grid-cols-2 md:grid-cols-2 lg:sm:grid-cols-2 xl:grid-cols-3 gap-4 select-none"
        >
            {
                props.options.map(o => {
                    const id = props.getId(o);
                    const selected = selectedIds.has(id);
                    return (
                    <div
                        key={id}
                        className={`${selected ? selectedBorder : unselectedBorder} flex ${props.isLoading == true ? "pointer-events-none" : "cursor-pointer"} items-center gap-3 rounded-lg p-3 hover:bg-gray-100 dark:hover:bg-white/[0.03] cursor-pointer`}
                        onClick={() => {
                            props.onChange?.(props.options.filter(o => {
                                const thisId = props.getId(o);
                                if(thisId == id) {
                                    return !selected;
                                }

                                return selectedIds.has(thisId);
                            }), o);
                        }}
                    >
                        <div className="relative h-7 flex-none rounded-full text-success-500 p-1">
                            {
                                props.isLoading == true
                                ?
                                <Spinner
                                    className="object-cover object-center size-full rounded-full"
                                />
                                :
                                (
                                    props.checkIcon == undefined
                                    ?
                                    <CheckLineIcon
                                        className={`object-cover object-center size-full rounded-full ${selected ? "" : "collapse"}`}
                                    />
                                    :
                                    <props.checkIcon
                                        className={`object-cover object-center size-full rounded-full ${selected ? "" : "collapse"}`}
                                    />
                                )
                            }
                        </div>
                        <div className="w-full flex-1">
                            <div className="flex items-start justify-between">
                                {props.render(o)}
                            </div>
                        </div>
                    </div>
                    );
                })
            }
        </div>
    )
}