import { createRoot } from 'react-dom/client'
import './index.scss'
import './i18n';
import { QueryContextProvider } from './context/QueryContextProvider.tsx'
import { WebEventsProvider } from './hooks/signalR/useWebEvents.tsx'
import { App } from './App.tsx'
import { AuthProvider } from './context/AuthContext.tsx'
import { createTheme } from '@mui/material/styles';
import { ThemeProvider } from '@mui/styles';

const theme = createTheme({
  
});
createRoot(document.getElementById('root')!).render(
  <ThemeProvider theme={theme}>
    <AuthProvider>
      <WebEventsProvider>
        <QueryContextProvider>
          <App />
        </QueryContextProvider>
      </WebEventsProvider>
    </AuthProvider>
  </ThemeProvider>,
)