import { IconButton, Skeleton, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Tooltip } from "@mui/material";
import React, { useCallback, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";

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

        {/* Other Devices View */}
        <TableContainer
            component={"div"}
            sx={{
                "& .MuiTableCell-head.MuiTableCell-root": {
                    fontWeight: "bold",
                }
            }}
        >
            {
                hasInnerRows() == true
                ?
                <Table>
                    <TableHead>
                        <TableRow>
                            {
                                props.name != undefined &&
                                <TableCell>
                                    {props.name.label}
                                </TableCell>
                            }
                            {
                                props.columns?.map(header => (
                                    <TableCell key={header.key}>
                                        {header.label}
                                    </TableCell>
                                ))
                            }
                            {
                                props.actions != undefined &&
                                <TableCell>
                                    &nbsp;
                                </TableCell>
                            }
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {
                            props.isLoading != true && props.data.length == 0 
                            ?
                            <TableRow>
                                <TableCell colSpan={(props.name != undefined ? 1 : 0) + (props.columns?.length ?? 0) + (props.actions != undefined ? 1 : 0)}>
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
                                            props.name != undefined &&
                                            <TableCell>
                                                <Skeleton animation="wave" />
                                            </TableCell>
                                        }
                                        {
                                            props.columns?.map(column => 
                                                <TableCell
                                                    key={column.key}
                                                >
                                                    <Skeleton animation="wave" />
                                                </TableCell>
                                            )
                                        }
                                        {
                                            props.actions != undefined &&
                                            <TableCell>
                                                <Skeleton animation="wave" />
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
                    <TableHead>
                        <TableRow>
                            {
                                props.name != undefined &&
                                <TableCell>
                                    {props.name.label}
                                </TableCell>
                            }
                            {
                                props.columns?.map(header => (
                                    <TableCell key={header.key}>
                                        {header.label}
                                    </TableCell>
                                ))
                            }
                            {
                                props.actions != undefined &&
                                <TableCell>
                                    &nbsp;
                                </TableCell>
                            }
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {
                            props.isLoading != true && props.data.length == 0 
                            ?
                            <TableRow>
                                <TableCell colSpan={(props.name != undefined ? 1 : 0) + (props.columns?.length ?? 0) + (props.actions != undefined ? 1 : 0)}>
                                    <p>
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
                                                className="px-5 py-4 sm:px-6 text-start"
                                            >
                                                <Skeleton className="w-full" />
                                            </TableCell>
                                        }
                                        {
                                            props.columns?.map(column => 
                                                <TableCell
                                                    key={column.key}
                                                    className="px-5 py-4 sm:px-6 text-start"
                                                >
                                                    <Skeleton className="w-full" />
                                                </TableCell>
                                            )
                                        }
                                        {
                                            props.actions != undefined &&
                                            <TableCell
                                                className="px-5 py-4 sm:px-6 text-start"
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
                                                className="px-4 py-3 text-gray-500 text-theme-sm dark:text-gray-400"
                                            >
                                                {props.name.render(d)}
                                            </TableCell>
                                        }
                                        {
                                            props.columns?.map(column => 
                                                <React.Fragment key={`row_${column.key}`}>
                                                    {
                                                        <TableCell
                                                            className="px-4 py-3 text-gray-500 text-theme-sm dark:text-gray-400"
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
                                                className="px-4 sm:px-6 py-3.5"
                                            >
                                                <div className="flex items-center w-full gap-2 justify-end">
                                                    {
                                                        props.actions.map(a => (
                                                            <Tooltip 
                                                                title={a.label}
                                                                key={a.key}
                                                            >
                                                                <IconButton onClick={e => rowAction(e, () => a.onClick?.(d))}>
                                                                    {a.render(d)}
                                                                </IconButton>
                                                            </Tooltip>
                                                        ))
                                                    }
                                                </div>
                                            </TableCell>
                                        }
                                    </TableRow>
                                ))
                            )
                        }
                    </TableBody>
                </Table>
            }
        </TableContainer>
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
                    <TableCell className="px-4 sm:px-6 py-3.5">
                        {props.name.render(props.data)}
                    </TableCell>
                }
                {
                    props.columns?.map(column => 
                        <React.Fragment key={column.key}>
                            <TableCell className="px-4 sm:px-6 py-3.5">
                                {column.render(props.data)}
                            </TableCell>
                        </React.Fragment>
                    )
                }
                {
                    props.actions != undefined &&
                    <TableCell className="px-4 sm:px-6 py-3.5">
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
                                        title={a.label}
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