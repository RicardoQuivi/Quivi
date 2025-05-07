import React, { createContext, ReactNode, useState } from 'react'

export interface StepperContextData {
    isInitialized: boolean,
    isNextStepAllowed: boolean,
    setIsNextStepAllowed: (value: boolean) => void;
}

const initialContextValue: StepperContextData = {
    isInitialized: false,
    isNextStepAllowed: false,
    setIsNextStepAllowed: function (): void {
        throw new Error('Not initialized yet.');
    },
};
export const StepperContext = createContext<StepperContextData>(initialContextValue);

export interface Props {
    readonly children?: React.ReactElement;
}

const StepperContextProvider = ({ children }: { children: ReactNode })  => {
    const onSetIsNextStepAllowed = (value: boolean) => setValue(prev => ({...prev, isNextStepAllowed: value}))

    const [value, setValue] = useState<StepperContextData>({
        ...initialContextValue, 
        isInitialized: true,
        setIsNextStepAllowed: onSetIsNextStepAllowed,
    });
    
    return (
        <StepperContext.Provider value={value}>
            {children}
        </StepperContext.Provider>
    );
}
export default StepperContextProvider;