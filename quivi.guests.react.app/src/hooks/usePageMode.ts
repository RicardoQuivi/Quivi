import { useMediaQuery, useTheme } from "@mui/material";

export enum PageMode {
    Mobile,
    Kiosk,
}

export const usePageMode = (): PageMode => {
    const theme = useTheme();
    const isNotMobile = useMediaQuery(theme.breakpoints.up("md"));

    return isNotMobile ? PageMode.Kiosk : PageMode.Mobile;
}