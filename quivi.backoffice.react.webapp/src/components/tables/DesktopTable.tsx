import { useTranslation } from "react-i18next";
import { ITableAction, ITableColumn, ResponsiveTableProps } from "./ResponsiveTable";
import { useCallback, useMemo, useState } from "react";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "../ui/table";
import { Tooltip } from "../ui/tooltip/Tooltip";
import { IconButton } from "../ui/button/IconButton";
import React from "react";
import { Skeleton } from "../ui/skeleton/Skeleton";

const range = (count: number, startNumber: number = 1) => Array.from({length: count}, (_, i) => i + startNumber);

export const DesktopTable = <T,>(props: ResponsiveTableProps<T>) => {
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