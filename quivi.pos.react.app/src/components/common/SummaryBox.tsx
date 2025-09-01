import { Grid, Skeleton, styled} from "@mui/material";
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
                    !!item &&
                    <SummaryGridCell size="grow" key={`${item.label}_${index}`}>
                        <label>{item.label}</label>
                        <span>
                        {
                            props.isLoading == true
                            ? <Skeleton animation="wave" style={{width: "4rem"}}/>
                            : item.content
                        }
                        </span>
                    </SummaryGridCell>
                ))
            }
        </Grid>
    )
}