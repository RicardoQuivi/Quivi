import { PagedQueryResult } from "../../hooks/queries/QueryResult"
import { Pagination } from "./Pagination"

interface Props<TEntity,> {
    readonly query: PagedQueryResult<TEntity>;
    readonly pageSize: number;
    readonly onPageIndexChange?: (page: number) => void;
}
export const QueryPagination = <TEntity,>(props: Props<TEntity>) => {
    return <Pagination 
        isLoading={props.query.isFirstLoading}
        pageIndex={props.query.page}
        totalItems={props.query.totalItems}
        pageSize={props.pageSize}
        onPageIndexChange={props.onPageIndexChange}
    />
}