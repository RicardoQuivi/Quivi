import React, { useCallback, useMemo, useState } from "react";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "../ui/table";
import { useTranslation } from "react-i18next";
import { Skeleton } from "../ui/skeleton/Skeleton";
   
const range = (count: number, startNumber: number = 1) => Array.from({length: count}, (_, i) => i + startNumber);

export interface ITableColumn<T> {
    readonly render: (row: T) => React.ReactNode;
    readonly label: React.ReactNode;
    readonly key: React.Key;
    readonly isActions?: boolean;
}

interface Props<T> {
    readonly columns: ITableColumn<T>[];
    readonly data: T[];
    
    readonly getKey: (row: T) => React.Key;
    readonly getChildren?: (row: T) => T[];
    readonly hasInnerRows?: (item: T) => boolean;
    readonly onRowClick?: (item: T) => any;

    readonly isLoading?: boolean;
    readonly loadingItemsCount?: number;
}
const ResponsiveTable = <T,>(props: Props<T>) => {
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

    return (
        <>
            {
                hasInnerRows() == true
                ?
                <Table>
                    <TableHeader className="border-b border-gray-100 dark:border-white/[0.05]">
                        <TableRow>
                            {
                                props.columns.map(header => (
                                    <TableCell
                                        key={header.key}
                                        isHeader
                                        className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                                    >
                                        {header.label}
                                    </TableCell>
                                ))
                            }
                        </TableRow>
                    </TableHeader>
                    <TableBody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
                        {
                            props.isLoading != true && props.data.length == 0 
                            ?
                            <TableRow>
                                <TableCell cellSpan={props.columns.length}>
                                    <p className="text-sm text-center text-gray-500 dark:text-gray-400 py-3">
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
                                            props.columns.map(column => 
                                                <TableCell
                                                    key={column.key}
                                                    className="px-5 py-4 sm:px-6 text-start"
                                                >
                                                    <Skeleton className="w-full" />
                                                </TableCell>
                                            )
                                        }
                                    </TableRow>
                                ))
                                :
                                props.data.map(d => (
                                    <CollapsibleRow key={props.getKey(d)}
                                                    getKey={props.getKey}
                                                    getChildren={props.getChildren}
                                                    columns={props.columns}
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
                                props.columns.map(header => (
                                    <TableCell
                                        key={header.key}
                                        isHeader
                                        className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                                    >
                                        {header.label}
                                    </TableCell>
                                ))
                            }
                        </TableRow>
                    </TableHeader>
                    <TableBody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
                        {
                            props.isLoading != true && props.data.length == 0 
                            ?
                            <TableRow>
                                <TableCell cellSpan={props.columns.length}>
                                    <p className="text-sm text-center text-gray-500 dark:text-gray-400 py-3">
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
                                            props.columns.map(column => 
                                                <TableCell
                                                    key={column.key}
                                                    className="px-5 py-4 sm:px-6 text-start"
                                                >
                                                    <Skeleton className="w-full" />
                                                </TableCell>
                                            )
                                        }
                                    </TableRow>
                                ))
                                :
                                props.data.map(d => (
                                    <TableRow key={props.getKey(d)} 
                                        className="justify-content-start animated fadeInDown" 
                                        style={{cursor: isClickableRow(d) ? "pointer" : "unset"}}
                                        onClick={() => props.onRowClick?.(d)}
                                    >
                                        {
                                            props.columns.map(column => 
                                                <React.Fragment key={`row_${column.key}`}>
                                                    {
                                                        column.isActions
                                                        ?
                                                        <TableCell
                                                            className="px-4 sm:px-6 py-3.5"
                                                        >
                                                            <div className="flex items-center w-full gap-2 justify-end">
                                                                {column.render(d)}
                                                            </div>
                                                        </TableCell>
                                                        :
                                                        <TableCell
                                                            className="px-4 py-3 text-gray-500 text-theme-sm dark:text-gray-400"
                                                        >
                                                            {column.render(d)}
                                                        </TableCell>
                                                    }
                                                </React.Fragment>
                                            )
                                        }
                                    </TableRow>
                                ))
                            )
                        }
                    </TableBody>
                </Table>
            }
        </>
    )
}
export default ResponsiveTable;

interface CollapsibleRowProps<T> {
    readonly columns: ITableColumn<T>[];
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
                    props.columns.map(column => 
                        <React.Fragment key={column.key}>
                            <TableCell className="px-4 sm:px-6 py-3.5">
                                {column.render(props.data)}
                            </TableCell>
                        </React.Fragment>
                    )
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
                            props.columns.map((column, ci) => (
                                <TableCell key={column.key} className={ci == 0 ? "details payment-method" : "details"}>
                                    {column.render(innerRow)}
                                </TableCell>
                            ))
                        }
                    </TableRow>
                ))
            }
        </>
    );
}