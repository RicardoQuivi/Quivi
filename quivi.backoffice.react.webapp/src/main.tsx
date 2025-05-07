import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import "swiper/swiper-bundle.css";
import "flatpickr/dist/flatpickr.css";
import { AppWrapper } from "./components/common/PageMeta.tsx";
import { ThemeProvider } from "./context/ThemeContext.tsx";
import { App } from "./App.tsx";
import './i18n';
import { QueryContextProvider } from "./context/QueryContextProvider.tsx";
import { AuthProvider } from "./context/AuthContext.tsx";
import { WebEventsProvider } from "./hooks/signalR/useWebEvents.tsx";
import { ToastProvider } from "./layout/ToastProvider.tsx";

const rootElement = document.getElementById("root")!;
createRoot(rootElement).render(
    <StrictMode>
        <ThemeProvider>
            <ToastProvider>
                <AuthProvider>
                    <WebEventsProvider>
                        <QueryContextProvider>
                            <AppWrapper>
                                <App />
                            </AppWrapper>
                        </QueryContextProvider>
                    </WebEventsProvider>
                </AuthProvider>
            </ToastProvider>
        </ThemeProvider>
    </StrictMode>,
);