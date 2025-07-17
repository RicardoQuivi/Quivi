import { useEffect, useRef, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import React from "react";
import type { DialogProps } from "./Dialog";
import { makeStyles } from '@mui/styles';

const useStyles = makeStyles({
    overflowContainer: {
        position: "fixed",
        top: 0,
        bottom: 0,
        left: 0,
        right: 0,
        overflow: "hidden",
        zIndex: 1000,
    },
    backdrop: {
        position: "absolute",
        top: 0,
        left: 0,
        width: "100%",
        height: "100%",
        backdropFilter: "blur(4px)",
        backgroundColor: "rgba(0, 0, 0, 0.3)",
    },
    dialog: {
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
    },
});

const Drawer: React.FC<DialogProps> = (props) => {
    const animationDuration = (props.animationDurationMilliseconds || 350) / 1000.0;

    const ref = useRef<HTMLDivElement>(null);
    const classes = useStyles();

    const [isOpen, setIsOpen] = useState(false);
    const [isAnimating, setIsAnimating] = useState(true);
    const [isAnimationStarted, setIsAnimationStarted] = useState(false);

    const [heightsConfig, setHeightsConfig] = useState({
        contentHeight: 0,
        windowHeight: window.innerHeight,
    })

    const setWindowHeight = (height: number) => setHeightsConfig(p => ({...p, windowHeight: height}));
    const setHeight = (height: number) => setHeightsConfig(p => ({
        ...p, 
        contentHeight: height,
    }))

    useEffect(() => {
        const handleResize = () => setWindowHeight(window.innerHeight);
        window.addEventListener('resize', handleResize);
        return () => window.removeEventListener('resize', handleResize);
    }, []);

    useEffect(() => {
        if (!isAnimationStarted) {
            return;
        }

        if(isOpen && !isAnimating) {
            return;
        }
        
        const element = ref.current;
        const currentHeight = (ref.current?.clientHeight ?? 0);
        setHeight(currentHeight);

        if(element == undefined) {
            return;
        }

        const resizeObserver = new ResizeObserver(entries => {
            for (const entry of entries) {
                setHeight(entry.contentRect.height);
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
        <AnimatePresence onExitComplete={() => props.onClose()}>
        {
            isOpen &&
            <>
                <div className={classes.overflowContainer}>
                    <motion.div transition={{ duration: animationDuration, ease: "easeInOut" }} 
                            onAnimationStart={() => setIsAnimationStarted(true)}
                            onAnimationComplete={() => setIsAnimationStarted(false)}
                            initial={{ height: heightsConfig.windowHeight }}
                            exit={{ height: heightsConfig.windowHeight }}
                            animate={{ height: heightsConfig.windowHeight - heightsConfig.contentHeight }}
                    />

                    <div ref={ref} className={`${classes.dialog} ${props.className ?? ""}`} style={props.style}>
                        {props.children}
                    </div>

                    <motion.div variants={{ initial: { opacity: 0 }, in: { opacity: [0, 1] }, out: { opacity: [1, 0] }}}
                        className={classes.backdrop}
                        initial="initial"
                        animate="in" 
                        exit="out" 
                        transition={{ duration: animationDuration }}
                        onAnimationStart={() => setIsAnimating(true)}
                        onAnimationComplete={() => setIsAnimating(false)}
                        onClick={() => !isAnimating && (props.disableClosing !== true) && setIsOpen(false)}>
                    </motion.div>
                </div>
            </>
        }
        </AnimatePresence>
    );
};
export default Drawer;