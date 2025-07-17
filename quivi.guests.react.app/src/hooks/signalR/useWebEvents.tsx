import { createContext, useContext, useState, useEffect } from 'react';
import { SignalRClient, type ISignalRListener, type IWebClient } from "./SignalRClient";

interface WebEventsContextType {
    readonly client: IWebClient;
    readonly connected: boolean;
}

const WebEventsContext = createContext<WebEventsContextType | undefined>(undefined);

const signalRClient = new SignalRClient(import.meta.env.VITE_SIGNALR_URL);

export const WebEventsProvider = ({ children }: { children: React.ReactNode }): React.ReactNode => {
    const [connectionState, setConnectionState] = useState(signalRClient.isConnected);

    useEffect(() => {
        const listener: ISignalRListener = {
            onConnectionChanged: (e) => setConnectionState(e),
        }

        signalRClient.addSignalRListener(listener);
        return () => signalRClient.removeSignalRListener(listener);
    }, [])

    return (
        <WebEventsContext.Provider value={{
            client: signalRClient,
            connected: connectionState,
        }}>
            {children}
        </WebEventsContext.Provider>
    );
};

export const useWebEvents = (): WebEventsContextType => {
    const context = useContext(WebEventsContext);
    if (!context) {
        throw new Error('useWebEvents must be used within a WebEventsProvider');
    }
    return context;
};