import { Grid, Skeleton, styled, Typography} from "@mui/material";
import React from "react";

const SummaryGridCell = styled(Grid)({
    display:"flex", 
    flexDirection: "column", 
    textAlign: "center", 
    alignItems: "center",
})

export interface SummaryItem {
    readonly label: string;
    readonly content: React.ReactNode;
}

interface Props {
    readonly isLoading?: boolean;
    readonly items: (SummaryItem | undefined)[];
    readonly style?: React.CSSProperties;
}
export const SummaryBox: React.FC<Props> = (props) => {
    return (
        <Grid container className="table-summary-header" justifyContent="space-around" width="100%" style={props.style}>
            {
                props.items.map((item, index) => (
                    item != undefined &&
                    <SummaryGridCell size="grow" key={`${item.label}_${index}`}>
                        <Typography variant="body2" component="label" gutterBottom fontWeight="bold">
                            {item.label}
                        </Typography>
                        <Typography variant="subtitle1" component="label" gutterBottom>
                        {
                            props.isLoading == true ? <Skeleton animation="wave" style={{width: "4rem"}}/> : item.content
                        }
                        </Typography>
                    </SummaryGridCell>
                ))
            }
        </Grid>
    )
}