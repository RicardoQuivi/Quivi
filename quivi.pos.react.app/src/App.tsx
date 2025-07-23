import { BrowserRouter, Outlet, Route, Routes } from 'react-router';
import { useAuth } from './context/AuthContextProvider';
import { Pos } from './Pos';
import { SignIn } from './SignIn';
import { useEmployeeManager } from './context/employee/EmployeeContextProvider';
import { LockPage } from './pages/LockPage';
import { LoggedEmployeeContextProvider } from './context/pos/LoggedEmployeeContextProvider';
import { PosSessionContextProvider } from './context/pos/PosSessionContextProvider';

export const App = () => {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/signIn" element={<SignIn />}/>
                
                <Route element={<AuthLayoutRoute />}>
                    <Route path="/" element={<Pos />} />
                </Route>
            </Routes>
        </BrowserRouter>
    )
}

const AuthLayoutRoute = () => {
    const auth = useAuth();
    const employeeManager = useEmployeeManager();
    
    if(auth.principal == undefined) {
        window.location.href = import.meta.env.VITE_BACKOFFICE_APP_URL
        return <></>
    }

    if(employeeManager.employee == undefined) {
        return <LockPage />
    }
    
    return <LoggedEmployeeContextProvider>
        <PosSessionContextProvider>
            <Outlet />
        </PosSessionContextProvider>
    </LoggedEmployeeContextProvider>
}