import { useEffect, useRef, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import type { DialogProps } from "./Dialog";
import { Box } from "@mui/material";

const Drawer= (props: DialogProps) => {
    const animationDuration = (props.animationDurationMilliseconds || 350) / 1000.0;

    const ref = useRef<HTMLDivElement>(null);

    const [isOpen, setIsOpen] = useState(false);
    const [isAnimating, setIsAnimating] = useState(false);

    const [heightsConfig, setHeightsConfig] = useState({
        contentHeight: 0,
        windowHeight: window.innerHeight,
    })

    useEffect(() => {
        const handleResize = () => setHeightsConfig(p => ({...p, windowHeight: window.innerHeight}))
        window.addEventListener('resize', handleResize);
        return () => window.removeEventListener('resize', handleResize);
    }, []);

    useEffect(() => {
        const element = ref.current;
        const currentHeight = element?.clientHeight;
        setHeightsConfig(p => ({
            ...p, 
            contentHeight: currentHeight ?? 0,
        }))

        if(element == undefined) {
            return;
        }

        const resizeObserver = new ResizeObserver(entries => {
            for (const entry of entries) {
                setHeightsConfig(p => ({
                    ...p, 
                    contentHeight: entry.contentRect.height,
                }))
            }
        });
        resizeObserver.observe(element);
        return () => {
            resizeObserver.unobserve(element);
            resizeObserver.disconnect();
        };
    }, [ref.current]);
    
    useEffect(() => setIsOpen(props.isOpen), [props.isOpen]);

    return (
        <AnimatePresence
            onExitComplete={props.onClose}
        >
        {
            isOpen &&
            <Box
                sx={{
                    position: "fixed",
                    top: 0,
                    bottom: 0,
                    left: 0,
                    right: 0,
                    overflow: "hidden",
                    zIndex: 1000,
                }}
            >
                <motion.div
                    transition={{
                        duration: animationDuration,
                        ease: "easeInOut",
                    }}
                    initial={{
                        height: heightsConfig.windowHeight,
                    }}
                    exit={{
                        height: heightsConfig.windowHeight,
                    }}
                    animate={{
                        height: heightsConfig.windowHeight - heightsConfig.contentHeight,
                    }}
                />

                <Box
                    ref={ref}
                    className={props.className}
                    sx={{
                        position: "relative",
                        width: "100%",
                        zIndex: 1299,
                        backgroundColor: "white",
                        borderRadius: "15px 15px 0 0",
                        maxHeight: "100%",
                        overflowY: "auto",

                        display: "flex",
                        flexDirection: "column",
                        justifyContent: "flex-end",
                        height: "auto",
                    }}
                    style={props.style}
                >
                    {props.children}
                </Box>

                <motion.div
                    variants={{
                        initial: {
                            opacity: 0,
                        },
                        in: {
                            opacity: [0, 1],
                        },
                        out: {
                            opacity: [1, 0],
                        }
                    }}
                    style={{
                        position: "absolute",
                        top: 0,
                        left: 0,
                        width: "100%",
                        height: "100%",
                        backdropFilter: "blur(4px)",
                        backgroundColor: "rgba(0, 0, 0, 0.3)",
                    }}
                    initial="initial"
                    animate="in" 
                    exit="out" 
                    transition={{
                        duration: animationDuration,
                    }}
                    onAnimationStart={() => setIsAnimating(true)}
                    onAnimationComplete={() => setIsAnimating(false)}
                    onClick={() => !isAnimating && (props.disableClosing !== true) && setIsOpen(false)}
                />
            </Box>
        }
        </AnimatePresence>
    );
};
export default Drawer;