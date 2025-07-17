import { motion } from 'framer-motion';
import { pageTransition, pageVariants } from './transitions';

const SplashScreen = () => {
    return (
        <motion.div initial='initial' animate='in' exit='out' transition={pageTransition} variants={pageVariants}>
            <div className="animation__background">
                <span className="loader" />
            </div>
        </motion.div>
    );
};

export default SplashScreen;
