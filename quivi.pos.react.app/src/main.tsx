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
            main: '#1C3A11',
            light: '#F0F8EE',

            "50": "#F0F8EE",
            "100": "#E0F1DB",
            "200": "#C8E6C9",
            "300": "#A4D98F",
            "400": "#91CF6D",
            "500": "#7AC943",
            "600": "#65B33B",
            "700": "#2F631C",
            "800": "#3B7D22",
            "900": "#E0F1DB",
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