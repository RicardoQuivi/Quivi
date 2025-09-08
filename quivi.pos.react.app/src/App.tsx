import { BrowserRouter, Outlet, Route, Routes } from 'react-router';
import { useAuth } from './context/AuthContextProvider';
import { Pos } from './Pos';
import { SignIn } from './SignIn';
import { useEmployeeManager } from './context/employee/EmployeeContextProvider';
import { LockPage } from './pages/LockPage';
import { LoggedEmployeeContextProvider } from './context/pos/LoggedEmployeeContextProvider';
import { PosSessionContextProvider } from './context/pos/PosSessionContextProvider';
import { NoEmployeeLayout } from './layouts/NoEmployeeLayout';
import { LoadingAnimation } from './components/Loadings/LoadingAnimation';

export const App = () => {
    return (
        <BrowserRouter>
            <Routes>
                <Route element={<SignInLayoutRoute />}>
                    <Route path="/signIn" element={<SignIn />}/>
                </Route>

                <Route element={<AuthLayoutRoute />}>
                    <Route path="/" element={<Pos />} />
                </Route>
            </Routes>
        </BrowserRouter>
    )
}

const SignInLayoutRoute = () => {
    return <NoEmployeeLayout>
        <Outlet />
    </NoEmployeeLayout>
}

const AuthLayoutRoute = () => {
    const auth = useAuth();
    const employeeManager = useEmployeeManager();
    
    if(auth.principal == undefined) {
        window.location.href = import.meta.env.VITE_BACKOFFICE_APP_URL
        return <NoEmployeeLayout>
            <LoadingAnimation
                sx={{
                    width: {
                        xs: "50%",
                        sm: "25%",
                    }
                }}
            />
        </NoEmployeeLayout>
    }

    if(employeeManager.employee == undefined) {
        return <NoEmployeeLayout>
            <LockPage />
        </NoEmployeeLayout>
    }
    
    return <LoggedEmployeeContextProvider>
        <PosSessionContextProvider>
            <Outlet />
        </PosSessionContextProvider>
    </LoggedEmployeeContextProvider>
}