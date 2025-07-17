import type { Transition } from "framer-motion";

export const pageVariants = {
    initial: {
        opacity: 0.4
    },
    in: {
        opacity: 1

    },
    out: {
        opacity: 0.4
    }
};

export const pageTransition: Transition<any> = {
    duration: 0.2,
    ease: "easeInOut",
}

export const fromRightVariants = {
    initial: {
        x: "100%"
    },
    in: {
        x: 0
    },
    out: {
        x:"100%"
    }
};

export const fromLeftVariants = {
    initial: {
        x: "-100%"
    },
    in: {
        x: 0
    },
    out: {
        x: "-100%"
    }
};