import { ITableAction, ITableColumn, ResponsiveTableProps } from "./ResponsiveTable";
import { useMemo } from "react";
import Popover from "../ui/popover/Popover";
import { ThreeDotsVertical } from "../../icons";
import { Dropdown } from "../ui/dropdown/Dropdown";
import { DropdownItem } from "../ui/dropdown/DropdownItem";
import { Skeleton } from "../ui/skeleton/Skeleton";

const range = (count: number, startNumber: number = 1) => Array.from({length: count}, (_, i) => i + startNumber);

export const MobileCardsTable = <T,>(props: ResponsiveTableProps<T>) => {
    return <div className="grid grid-cols-1 gap-2 p-2">
    {
        props.isLoading
        ?
        range(props.loadingItemsCount ?? 5).map(i => <MobileCard
                key={`Loding-${i}`}
                item={undefined as (T | undefined)}
                actions={props.actions}
                columns={props.columns}
                name={props.name}
                onRowClick={props.onRowClick}
                rowClasses={props.rowClasses}
            />)
        :
        props.data.map(d => (
            <MobileCard
                key={props.getKey(d)}
                item={d}
                actions={props.actions}
                columns={props.columns}
                name={props.name}
                onRowClick={props.onRowClick}
                rowClasses={props.rowClasses}
            />
        ))
    }
    </div>
}

interface MobileCardProps<T> {
    readonly item: T | undefined;
    readonly name?: ITableColumn<T>;
    readonly columns?: ITableColumn<T>[];
    readonly actions?: ITableAction<T>[];
    readonly onRowClick?: (item: T) => any;
    readonly rowClasses?: (item: T) => string | undefined;
}
const MobileCard = <T,>(props: MobileCardProps<T>) => {
    const {
        onRowClick,
        rowClasses,
    } = useMemo(() => {
        if(props.item == undefined) {
            return {
                onRowClick: undefined,
                rowClasses: undefined,
            }
        }
        const item = props.item;
        return {
            onRowClick: () => props.onRowClick?.(item),
            rowClasses: props.rowClasses?.(item),
        }
    }, [props.item, props.onRowClick, props.rowClasses]);

    return <div
        className={`rounded-xl border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-white/[0.03] grid grid-cols-[1fr_auto] gap-2 ${rowClasses ?? ""}`}
        onClick={onRowClick}
    >
        <div
            className="grid grid-cols-1 gap-2"
        >
            {
                props.name != undefined &&
                <h4 className="mb-1 text-theme-xl font-medium text-gray-800 dark:text-white/90">
                {
                    props.item == undefined
                    ?
                    <Skeleton />
                    :
                    props.name.render(props.item)
                }
                </h4>
            }
            {
                props.columns != undefined &&
                <div
                    className="grid grid-cols-2 gap-5"
                >
                {
                    props.columns.map((c, i) => (
                        <div
                            key={c.key}
                            className={i % 2 === 0 ? 'pl-4 flex-col text-left justify-start' : 'pr-4 flex flex-col text-right justify-end'}
                        >
                            <p 
                                className="py-1 text-gray-500 text-theme-xs dark:text-gray-400 font-bold"
                            >
                                {c.label}
                            </p>
                            {
                                props.item == undefined
                                ?
                                <Skeleton />
                                :
                                <div className="text-sm text-gray-500 dark:text-gray-400">
                                    {c.render(props.item)}
                                </div>
                            }
                        </div>
                    ))
                }
                </div>
            }
        </div>
        <div>
            {
                props.actions != undefined &&
                <Popover
                    position="left"
                    trigger={<ThreeDotsVertical className="dark:fill-white" />}
                >
                    <Dropdown
                        className="top-full z-40 mt-2 w-auto rounded-2xl border border-gray-200 bg-white p-3 shadow-theme-lg dark:border-gray-800 dark:bg-[#1E2635]"
                        isOpen={true}
                        onClose={() => {}}
                    >
                        <ul className="flex flex-col gap-1">
                        {
                            props.actions.map(a => (
                            <li
                                key={a.key}
                            >
                                <DropdownItem
                                    onItemClick={() => props.item != undefined && a.onClick?.(props.item)}
                                    className={`flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50 dark:text-gray-300 dark:hover:bg-white/5 ${props.item == undefined ? "pointer-events-none" : ""}`}
                                >
                                    {
                                        props.item == undefined
                                        ?
                                        <Skeleton />
                                        :
                                        a.render(props.item)
                                    }
                                    {a.label}
                                </DropdownItem>
                            </li>
                            ))
                        }
                        </ul>
                    </Dropdown>
                </Popover>
            }
        </div>
    </div>
}