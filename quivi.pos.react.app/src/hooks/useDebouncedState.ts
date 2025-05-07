import { useEffect } from "react";
import { Dispatch, SetStateAction, useState } from "react";

const useDebouncedState = <S>(initialState: S | (() => S), debounceMillis: number): [S, Dispatch<SetStateAction<S>>, S, Dispatch<SetStateAction<S>>] => {
    const [variable, setVariable] = useState(initialState);
    const [debouncedVariable, setDebouncedVariable] = useState(initialState);

    useEffect(() => {
        if(variable == debouncedVariable) {
            return;
        }

        const timer = setTimeout(() => setDebouncedVariable(variable), debounceMillis);
        return () => clearTimeout(timer);
    }, [variable]);

    useEffect(() => {
        if(variable == debouncedVariable) {
            return;
        }

        setVariable(debouncedVariable);
    }, [debouncedVariable])
    
    return [variable, setVariable, debouncedVariable, setDebouncedVariable];
}
export default useDebouncedState;