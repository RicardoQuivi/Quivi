import { motion } from "framer-motion"
import { pageTransition, pageVariants } from "../pages/transitions";
import { TableNav } from "./Navbar/TableNav";
import { Box } from "@mui/material";

interface HeaderProps {
    readonly hideCart?: boolean;
    readonly hideOrder?: boolean;
    readonly hideFlag?: boolean;
}

interface PageProps {
    readonly title?: string;
    readonly children: React.ReactNode;
    readonly headerProps?: HeaderProps;
    readonly footer?: React.ReactNode;
}
export const Page = ({
    title,

    children,
    footer,

    headerProps,
}: PageProps) => {
    return <motion.div initial='initial' animate='in' exit='out' transition={pageTransition} variants={pageVariants}>
        <Box 
            className="body"
            sx={{
                display: "flex",
                height: "100dvh",
                flexWrap: "nowrap",
                justifyContent: "space-between",
                flexDirection: "column",
            }}
        >
            <TableNav
                title={title}
                hideCart={headerProps?.hideCart}
                hideOrder={headerProps?.hideOrder}
                hideFlag={headerProps?.hideFlag}
            />
            <Box 
                className="container"
                sx={{
                    display: "flex",
                    flexDirection: "column",
                    paddingBottom: 0,
                    flexGrow: 1
                }}
            >
                {children}
            </Box>
            {
                footer != undefined &&
                <Box
                    sx={{
                        alignSelf: "flex-end",

                        zIndex: 3,
                        position: "sticky",
                        bottom: 0,
                        left: 0,
                        right: 0,
                        margin: 0,
                        width: "100%",
                    }}
                >
                    <Box className="container">
                        {footer}
                    </Box>
                </Box>
            }
        </Box>
    </motion.div>
}