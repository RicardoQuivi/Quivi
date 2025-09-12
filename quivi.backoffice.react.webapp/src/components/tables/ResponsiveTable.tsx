import React, { useCallback, useMemo, useState } from "react";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "../ui/table";
import { useTranslation } from "react-i18next";
import { Skeleton } from "../ui/skeleton/Skeleton";
import { ThreeDotsVertical } from "../../icons";
import { DropdownItem } from "../ui/dropdown/DropdownItem";
import { Dropdown } from "../ui/dropdown/Dropdown";
import { Tooltip } from "../ui/tooltip/Tooltip";
import Popover from "../ui/popover/Popover";
import { IconButton } from "../ui/button/IconButton";

const range = (count: number, startNumber: number = 1) => Array.from({length: count}, (_, i) => i + startNumber);

export interface ITableAction<T> {
    readonly render: (row: T) => React.ReactNode;
    readonly label: React.ReactNode;
    readonly key: React.Key;
    readonly onClick?: (row: T) => any;
}

export interface ITableColumn<T> {
    readonly render: (row: T) => React.ReactNode;
    readonly label: React.ReactNode;
    readonly key: React.Key;
}

interface Props<T> {
    readonly name?: ITableColumn<T>;
    readonly columns?: ITableColumn<T>[];
    readonly actions?: ITableAction<T>[];
    readonly data: T[];
    
    readonly getKey: (row: T) => React.Key;
    readonly getChildren?: (row: T) => T[];
    readonly hasInnerRows?: (item: T) => boolean;
    readonly onRowClick?: (item: T) => any;
    readonly rowClasses?: (item: T) => string | undefined;

    readonly isLoading?: boolean;
    readonly loadingItemsCount?: number;
}

export const ResponsiveTable = <T,>(props: Props<T>) => {
    const { t } = useTranslation();

    const hasInnerRows = useCallback(() => {
        if(props.getChildren == undefined) {
            return false;
        }
        for(const item of props.data) {
            const children = props.getChildren(item);
            if(children.length > 0) {
                return true;
            }
        }
        return false;
    }, [props.data, props.hasInnerRows]) ;

    const isClickableRow = (row: T) => props.onRowClick != undefined || props.hasInnerRows?.(row) == true;

    return <>
        {/* Mobile View */}
        <div className="block sm:hidden grid grid-cols-1 gap-2 p-2">
        {
            props.data.map(d => {
                return (
                <React.Fragment
                    key={props.getKey(d)}
                >
                    <div
                        className={`rounded-xl border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-white/[0.03] grid grid-cols-[1fr_auto] gap-2 ${props.rowClasses?.(d) ?? ""}`}
                        onClick={() => props.onRowClick?.(d)}
                    >
                        <div
                            className="grid grid-cols-1 gap-2"
                        >
                            {
                                props.name != undefined &&
                                <h4 className="mb-1 text-theme-xl font-medium text-gray-800 dark:text-white/90">
                                    {props.name.render(d)}
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
                                            <p className="text-sm text-gray-500 dark:text-gray-400">
                                                {c.render(d)}
                                            </p>
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
                                                    onItemClick={() => a.onClick?.(d)}
                                                    className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50 dark:text-gray-300 dark:hover:bg-white/5"
                                                >
                                                    {a.render(d)}
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
                </React.Fragment>
                )
            })
        }
        </div>

        {/* Other Devices View */}
        <div
            className="hidden sm:block"
        >
            {
                hasInnerRows() == true
                ?
                <Table>
                    <TableHeader className="border-b border-gray-100 dark:border-white/[0.05]">
                        <TableRow>
                            {
                                props.name != undefined &&
                                <TableCell
                                    isHeader
                                    className="px-5 py-2 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                                >
                                    {props.name.label}
                                </TableCell>
                            }
                            {
                                props.columns?.map(header => (
                                    <TableCell
                                        key={header.key}
                                        isHeader
                                        className="px-5 py-2 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                                    >
                                        {header.label}
                                    </TableCell>
                                ))
                            }
                            {
                                props.actions != undefined &&
                                <TableCell
                                    isHeader
                                    className="px-5 py-2 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                                >
                                    &nbsp;
                                </TableCell>
                            }
                        </TableRow>
                    </TableHeader>
                    <TableBody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
                        {
                            props.isLoading != true && props.data.length == 0 
                            ?
                            <TableRow>
                                <TableCell cellSpan={(props.name != undefined ? 1 : 0) + (props.columns?.length ?? 0) + (props.actions != undefined ? 1 : 0)}>
                                    <p className="text-sm text-center text-gray-500 dark:text-gray-400 py-2">
                                        {t("common.noDataAvailable")}
                                    </p>
                                </TableCell>
                            </TableRow>
                            :
                            (
                                props.isLoading == true
                                ?
                                range(props.loadingItemsCount ?? 5).map(i => (
                                    <TableRow key={i}>
                                        {
                                            props.name != undefined &&
                                            <TableCell
                                                className="px-2 py-2 text-start"
                                            >
                                                <Skeleton className="w-full" />
                                            </TableCell>
                                        }
                                        {
                                            props.columns?.map(column => 
                                                <TableCell
                                                    key={column.key}
                                                    className="px-2 py-2 text-start"
                                                >
                                                    <Skeleton className="w-full" />
                                                </TableCell>
                                            )
                                        }
                                        {
                                            props.actions != undefined &&
                                            <TableCell
                                                className="px-2 py-2 text-start"
                                            >
                                                <Skeleton className="w-full" />
                                            </TableCell>
                                        }
                                    </TableRow>
                                ))
                                :
                                props.data.map(d => (
                                    <CollapsibleRow key={props.getKey(d)}
                                                    getKey={props.getKey}
                                                    getChildren={props.getChildren}
                                                    columns={props.columns}
                                                    actions={props.actions}
                                                    name={props.name}
                                                    data={d}
                                                    onRowClick={props.onRowClick} 
                                    />
                                ))
                            )
                        }
                    </TableBody>
                </Table>
                :
                <Table>
                    <TableHeader className="border-b border-gray-100 dark:border-white/[0.05]">
                        <TableRow>
                            {
                                props.name != undefined &&
                                <TableCell
                                    isHeader
                                    className="px-2 py-2 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                                >
                                    {props.name.label}
                                </TableCell>
                            }
                            {
                                props.columns?.map(header => (
                                    <TableCell
                                        key={header.key}
                                        isHeader
                                        className="px-2 py-2 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                                    >
                                        {header.label}
                                    </TableCell>
                                ))
                            }
                            {
                                props.actions != undefined &&
                                <TableCell
                                    isHeader
                                    className="px-2 py-2 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                                >
                                    &nbsp;
                                </TableCell>
                            }
                        </TableRow>
                    </TableHeader>
                    <TableBody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
                        {
                            props.isLoading != true && props.data.length == 0 
                            ?
                            <TableRow>
                                <TableCell cellSpan={(props.name != undefined ? 1 : 0) + (props.columns?.length ?? 0) + (props.actions != undefined ? 1 : 0)}>
                                    <p className="text-sm text-center text-gray-500 dark:text-gray-400 py-2">
                                        {t("common.noDataAvailable")}
                                    </p>
                                </TableCell>
                            </TableRow>
                            :
                            (
                                props.isLoading == true
                                ?
                                range(props.loadingItemsCount ?? 5).map(i => (
                                    <TableRow key={i}>
                                        {
                                            props.name != undefined &&
                                            <TableCell
                                                className="px-2 py-2 text-start"
                                            >
                                                <Skeleton className="w-full" />
                                            </TableCell>
                                        }
                                        {
                                            props.columns?.map(column => 
                                                <TableCell
                                                    key={column.key}
                                                    className="px-2 py-2 text-start"
                                                >
                                                    <Skeleton className="w-full" />
                                                </TableCell>
                                            )
                                        }
                                        {
                                            props.actions != undefined &&
                                            <TableCell
                                                className="px-2 py-2 text-start"
                                            >
                                                <Skeleton className="w-full" />
                                            </TableCell>
                                        }
                                    </TableRow>
                                ))
                                :
                                props.data.map(d => (
                                    <TableRow key={props.getKey(d)} 
                                        className={`justify-content-start animated fadeInDown ${props.rowClasses?.(d) ?? ""}`}
                                        style={{cursor: isClickableRow(d) ? "pointer" : "unset"}}
                                        onClick={() => props.onRowClick?.(d)}
                                    >
                                        {
                                            props.name != undefined &&
                                            <TableCell
                                                className="px-2 py-2 text-gray-500 text-theme-sm dark:text-gray-400"
                                            >
                                                {props.name.render(d)}
                                            </TableCell>
                                        }
                                        {
                                            props.columns?.map(column => 
                                                <React.Fragment key={`row_${column.key}`}>
                                                    {
                                                        <TableCell
                                                            className="px-2 py-2 text-gray-500 text-theme-sm dark:text-gray-400"
                                                        >
                                                            {column.render(d)}
                                                        </TableCell>
                                                    }
                                                </React.Fragment>
                                            )
                                        }
                                        {
                                            props.actions != undefined &&
                                            <TableCell
                                                className="px-2 sm:pr-5 py-2 flex items-center gap-1 justify-end"
                                            >
                                            {
                                                props.actions.map(a => (
                                                    <Tooltip 
                                                        message={a.label}
                                                        key={a.key}
                                                    >
                                                        <IconButton
                                                            onClick={e => rowAction(e, () => a.onClick?.(d))}
                                                            className="!text-gray-700 hover:!text-error-500 dark:!text-gray-400 dark:!hover:text-error-500"
                                                        >
                                                            {a.render(d)}
                                                        </IconButton>
                                                    </Tooltip>
                                                ))
                                            }
                                            </TableCell>
                                        }
                                    </TableRow>
                                ))
                            )
                        }
                    </TableBody>
                </Table>
            }
        </div>
    </>
}

interface CollapsibleRowProps<T> {
    readonly name?: ITableColumn<T>;
    readonly columns?: ITableColumn<T>[];
    readonly actions?: ITableAction<T>[];
    readonly data: T;

    readonly getKey: (row: T) => React.Key;
    readonly onRowClick?: (item: T) => any;
    readonly getChildren?: (row: T) => T[];
}
const CollapsibleRow = <T,>(props: CollapsibleRowProps<T>) => {
    const [isCollapse, setIsCollapse] = useState(true);
    
    const children = useMemo(() => {
        if(props.getChildren == undefined) {
            return [];
        }
        return props.getChildren(props.data);
    }, [props.data, props.getChildren])

    const isClickableRow = () => children.length > 0 || props.onRowClick != undefined;

    return (
        <>
            <TableRow 
                style={{cursor: isClickableRow() ? "pointer" : "unset"}} 
                onClick={() => {
                    props.onRowClick?.(props.data);
                    setIsCollapse(c => !c);
                }}
            >
                {
                    props.name != undefined &&
                    <TableCell className="px-4 py-2.5">
                        {props.name.render(props.data)}
                    </TableCell>
                }
                {
                    props.columns?.map(column => 
                        <React.Fragment key={column.key}>
                            <TableCell className="px-4 py-2.5">
                                {column.render(props.data)}
                            </TableCell>
                        </React.Fragment>
                    )
                }
                {
                    props.actions != undefined &&
                    <TableCell className="px-4 py-2.5">
                        &nbsp;
                    </TableCell>
                }
            </TableRow>
            {
                children.map((innerRow, index) => (
                    <TableRow
                        key={props.getKey(innerRow)} 
                        className={`settlement--child ${index + 1 != children!.length ? "settlement--bchild" : ""} ${isCollapse ? "collapse" : "show"}`} 
                        style={{cursor: props.onRowClick != undefined ? "pointer" : "unset"}}
                        onClick={() => props.onRowClick?.(innerRow)}
                    >
                        {
                            props.name != undefined && 
                            <TableCell className={"details payment-method"}>
                                {props.name.render(innerRow)}
                            </TableCell>
                        }
                        {
                            props.columns?.map((column, ci) => (
                                <TableCell key={column.key} className={props.name == undefined && ci == 0 ? "details payment-method" : "details"}>
                                    {column.render(innerRow)}
                                </TableCell>
                            ))
                        }
                        {
                            props.actions != undefined && 
                            <TableCell className={"details payment-method"}>
                            {
                                props.actions.map(a => {
                                    const render = a.render(innerRow);
                                    if(render == undefined) {
                                        return undefined;
                                    }
                                    
                                    return <Tooltip
                                        message={a.label}
                                        key={a.key}
                                    >
                                        <IconButton
                                            onClick={e => rowAction(e, () => a.onClick?.(innerRow))}
                                            className="!text-gray-700 hover:!text-error-500 dark:!text-gray-400 dark:!hover:text-error-500"
                                        >
                                            {render}
                                        </IconButton>
                                    </Tooltip>
                                })
                            }
                            </TableCell>
                        }
                    </TableRow>
                ))
            }
        </>
    );
}

const rowAction = (evt: React.MouseEvent<HTMLElement, MouseEvent>, action: () => any) => {
    evt.stopPropagation();
    action();
}