import { Box, Pagination } from '@mui/material';

interface Props {
    readonly currentPage: number;
    readonly numberOfPages: number;
    readonly onPageChanged: (toPage: number) => void;
}

export const PaginationFooter: React.FC<Props> = ({
    currentPage,
    numberOfPages,
    onPageChanged,
}) => {
    return <Box
        sx={{
            py: "0.5rem",

            '& .MuiPagination-root': {
                display: "flex",
                justifyContent: "center",
            }
        }}
    >
        <Pagination
            count={numberOfPages}
            page={currentPage + 1}
            variant="outlined"
            shape="rounded"
            color="primary"
            size="large"
            onChange={(_, p) => onPageChanged(p - 1)}
            showFirstButton
            showLastButton
        />
    </Box>
}