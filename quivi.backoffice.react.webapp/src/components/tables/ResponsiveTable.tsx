import React from "react";
import { MobileCardsTable } from "./MobileCardsTable";
import { DesktopTable } from "./DesktopTable";

export interface ITableAction<T> {
    readonly render: (row: T) => React.ReactNode;
    readonly label: React.ReactNode;
    readonly key: React.Key;
    readonly onClick?: (row: T) => any;
}

export type TableColumn<T> = ITableColumn<T> | IParentTableColumn<T>;

export const isParentColumn = <T,>(col: TableColumn<T>): col is IParentTableColumn<T> => {
    return 'children' in col;
}

export interface IParentTableColumn<T> {
    readonly label: React.ReactNode;
    readonly key: React.Key;
    readonly children: ITableColumn<T>[];
}

export interface ITableColumn<T> {
    readonly render: (row: T) => React.ReactNode;
    readonly label: React.ReactNode;
    readonly key: React.Key;
}

export interface ResponsiveTableProps<T> {
    readonly name?: ITableColumn<T>;
    readonly columns?: TableColumn<T>[];
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

export const ResponsiveTable = <T,>(props: ResponsiveTableProps<T>) => {
    return <>
        <div className="block sm:hidden">
            <MobileCardsTable {...props} />
        </div>

        <div className="hidden sm:block">
            <DesktopTable {...props} />
        </div>
    </>
}