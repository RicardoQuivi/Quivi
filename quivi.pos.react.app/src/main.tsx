import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import './i18n';
import { AuthProvider } from './context/AuthContextProvider.tsx'
import { WebEventsProvider } from './hooks/signalR/useWebEvents.tsx'
import { QueryContextProvider } from './context/QueryContextProvider.tsx'
import { createTheme, ThemeProvider } from '@mui/material/styles'
import { ToastProvider } from './context/ToastProvider.tsx'
import { App } from './App.tsx'
import { EmployeeProvider } from './context/employee/EmployeeContextProvider.tsx';

const theme = createTheme({
    palette: {
        primary: {
            main: '#1C3A11'
        }
    }
});

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <ThemeProvider theme={theme}>
            <ToastProvider>
                <AuthProvider>
                    <WebEventsProvider>
                        <QueryContextProvider>
                            <EmployeeProvider>
                                <App />
                            </EmployeeProvider>
                        </QueryContextProvider>
                    </WebEventsProvider>
                </AuthProvider>
            </ToastProvider>
        </ThemeProvider>
    </StrictMode>,
)
