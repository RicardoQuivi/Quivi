import { Box, Pagination, useMediaQuery, useTheme } from '@mui/material';

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
    
    const theme = useTheme();
    const xs = useMediaQuery(theme.breakpoints.only('xs'));

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
            size={xs ? "small" : "large"}
            onChange={(_, p) => onPageChanged(p - 1)}
            showFirstButton
            showLastButton
        />
    </Box>
}