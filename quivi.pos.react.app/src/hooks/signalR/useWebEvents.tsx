import { createContext, useContext, ReactNode, useState, useEffect } from 'react';
import { ISignalRListener, IWebClient, SignalRClient } from "./SignalRClient";
import { useAuth } from "../../context/AuthContextProvider";

interface WebEventsContextType {
    readonly client: IWebClient;
    readonly connected: boolean;
}

const WebEventsContext = createContext<WebEventsContextType | undefined>(undefined);

const signalRClient = new SignalRClient(import.meta.env.VITE_SIGNALR_URL);

export const WebEventsProvider = ({ children }: { children: ReactNode }): ReactNode => {
    const auth = useAuth();
    const [client] = useState(signalRClient)
    const [connectionState, setConnectionState] = useState(client.isConnected);

    useEffect(() => {
        const token = auth.principal?.token;
        if(token == undefined) {
            client.jwtFetcher = undefined;
        } else {
            client.jwtFetcher = () => token;
        }
    }, [auth.principal?.token])

    useEffect(() => {
        const listener: ISignalRListener = {
            onConnectionChanged: (e) => setConnectionState(e),
        }

        client.addSignalRListener(listener);
        return () => client.removeSignalRListener(listener);
    }, [client])

    return (
        <WebEventsContext.Provider value={{
            client: client,
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