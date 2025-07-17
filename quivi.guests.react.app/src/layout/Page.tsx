import { motion } from "framer-motion"
import { pageTransition, pageVariants } from "../pages/transitions";
import { TableNav } from "./Navbar/TableNav";

interface HeaderProps {
    readonly hideCart?: boolean;
    readonly hideOrder?: boolean;
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
        <div className="body" style={{display: "flex", height: "100dvh", flexWrap: "nowrap", justifyContent: "space-between", flexDirection: "column",}}>
            <TableNav title={title} hideCart={headerProps?.hideCart} hideOrder={headerProps?.hideOrder}/>
            <div className="container" style={{display: "flex", flexDirection: "column", paddingBottom: 0, flexGrow: 1}}>
                {children}
            </div>
            {
                footer != undefined &&
                <div style={{
                    alignSelf: "flex-end",

                    zIndex: 3,
                    position: "sticky",
                    bottom: 0,
                    left: 0,
                    right: 0,
                    margin: 0,
                    width: "100%",
                }}>
                    <div className="container">
                        {footer}
                    </div>
                </div>
            }
        </div>
    </motion.div>
}