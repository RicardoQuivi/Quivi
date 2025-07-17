import { Grid } from "@mui/material";

interface Props {
    readonly children: React.ReactNode[];
    readonly transparent?: boolean;
}
export const ButtonsSection = ({
    children,
    transparent,
}: Props) => {
    return (
    <Grid 
        container
        spacing={2}
        sx={{
            justifyContent: "center", 
            backgroundColor: transparent == true ? "transparent" : "white",
            width: "100%",
        }}
    >
    {
        children.map((b, i) => b != undefined && b != false && <Grid size={{ xs: 12, sm: 12, md: 6, lg: 6, xl: 6}} key={i}>{b}</Grid>)
    }
    </Grid>
    )
}