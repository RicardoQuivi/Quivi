import { Box, SxProps, Theme } from "@mui/material";
import { QuiviIcon } from "../../icons";

interface Props {
    readonly sx?: SxProps<Theme>;
}
export const LoadingAnimation = (props: Props) => {
    return (
        <Box
            sx={{
                ...(props.sx ?? {}),

                "& svg": {
                    width: "100%",
                    height: "100%",

                    animation: "spin 2s linear infinite",
                    "@keyframes spin": {
                        from: { 
                            transform: "rotate(0deg)",
                        },
                        to: { 
                            transform: "rotate(360deg)",
                        },
                    },
                }
            }}
        >
            <QuiviIcon />
        </Box>
    );
};