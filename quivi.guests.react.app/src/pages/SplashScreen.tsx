import { motion } from 'framer-motion';
import { pageTransition, pageVariants } from './transitions';
import { LoadingAnimation } from '../components/LoadingAnimation/LoadingAnimation';

const SplashScreen = () => {
    return (
        <motion.div initial='initial' animate='in' exit='out' transition={pageTransition} variants={pageVariants}>
            <div className="animation__background">
                <LoadingAnimation />
            </div>
        </motion.div>
    );
};

export default SplashScreen;
