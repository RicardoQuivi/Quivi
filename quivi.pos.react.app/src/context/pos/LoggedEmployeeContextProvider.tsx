import { createContext, ReactNode, useContext, useMemo } from "react";
import { Employee } from "../../hooks/api/Dtos/employees/Employee";
import { useEmployeeManager } from "../employee/EmployeeContextProvider";

interface LoggedEmployeeContextType {
    readonly signOut: () => void;
    readonly employee: Employee;
    readonly token: string;
}

const LoggedEmployeeContext = createContext<LoggedEmployeeContextType | undefined>(undefined);
export const LoggedEmployeeContextProvider = ({ children }: { children: ReactNode }) => {
    const context = useEmployeeManager();

    const state = useMemo(() => {
        if(context.employee == undefined || context.token == undefined) {
            return undefined;
        }

        return {
            signOut: context.signOut,
            token: context.token,
            employee: context.employee,
        } as LoggedEmployeeContextType;
    }, [context])

    if(state == undefined) {
        return <></>
    }

    return (
        <LoggedEmployeeContext.Provider value={state}>
            {children}
        </LoggedEmployeeContext.Provider>
    );
}
export const useLoggedEmployee = (): LoggedEmployeeContextType => {
    const context = useContext(LoggedEmployeeContext);
    if (!context) {
        throw new Error('usePos must be used within a PosContextProvider');
    }
    return context;
};