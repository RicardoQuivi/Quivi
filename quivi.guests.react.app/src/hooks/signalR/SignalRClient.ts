import { HubConnection, HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import type { UserEventListener } from "./UserEventListener";
import type { JobListener } from "./JobListener";
import type { ChannelListener } from "./ChannelListener";
import type { OnSessionUpdatedEvent } from "./dtos/OnSessionUpdatedEvent";
import type { OnOrderOperationEvent } from "./dtos/OnOrderOperationEvent";
import type { OnPosChargeOperationEvent } from "./dtos/OnPosChargeOperationEvent";
import type { TransactionListener } from "./TransactionListener";
import type { JobChangedEvent } from "./dtos/JobChangedEvent";
import type { OnTransactionInvoiceOperationEvent } from "./dtos/OnTransactionInvoiceOperationEvent";

export interface IWebClient {
    addUserListener(listener: UserEventListener): void;
    removeUserListener(listener: UserEventListener): void;

    addChannelListener(listener: ChannelListener): void;
    removeChannelListener(listener: ChannelListener): void;

    addJobListener(listener: JobListener): void;
    removeJobListener(listener: JobListener): void;

    addTransactionListener(listener: TransactionListener): void;
    removeTransactionListener(listener: TransactionListener): void;
}

export interface ISignalRListener {
    onConnectionChanged(isConnected: boolean): void;
}

const delay = (ms: number): Promise<void> => new Promise(resolve => setTimeout(resolve, ms));

export class SignalRClient implements IWebClient {
    private connection: HubConnection;
    private signalRHost: string;
    private signalRListeners: Set<ISignalRListener> = new Set<ISignalRListener>();

    public readonly channelListeners: Set<ChannelListener> = new Set<ChannelListener>();
    public readonly jobListeners: Set<JobListener> = new Set<JobListener>();
    public readonly transactionListeners: Set<TransactionListener> = new Set<TransactionListener>();

    private connected = false;

    constructor(signalRHost: string) {
        this.signalRHost = signalRHost;
        const url = new URL(`Guests`, this.signalRHost).toString();
        this.connection = new HubConnectionBuilder().withUrl(url).build();
        this.start();
    }

    public get isConnected() {
        return this.connected;
    }
    
    private async start(): Promise<void> {
        try {
            await navigator.locks.request("SignalRClient.Start.Lock", async () => {    
                if (this.connection.state == HubConnectionState.Connecting) {
                    return;
                }
    
                if (this.connection.state == HubConnectionState.Reconnecting) {
                    return;
                }
    
                if (this.connection.state == HubConnectionState.Connected) {
                    return;
                }
    
                if(this.connected == true) {
                    this.connected = false;
                    this.signalRListeners.forEach(l => l.onConnectionChanged(false));
                }
    
                try {
                    const url = new URL(`Guests`, this.signalRHost).toString();
                    this.connection.baseUrl = url;
                    await this.connection.start().then(async () => {
                        if(this.connection.state != HubConnectionState.Connected) {
                            return;
                        }
                        this.connected = true;
                        await this.connect();
                        this.signalRListeners.forEach(l => l.onConnectionChanged(true));
                    });
                } catch (e) {
                }
            })
        } finally {
            await delay(2000);
            this.start();
        }
    }

    private async connect() {
        this.connectToUserEvents();
        this.connectToChannelEvents();
        this.connectToJobEvents();
        this.connectToTransactionEvents();
    }

    private connectToUserEvents() {
    }

    private connectToChannelEvents() {
        this.connection.off('OnSessionUpdated');
        this.connection.on('OnSessionUpdated', (evt: OnSessionUpdatedEvent) => this.channelListeners.forEach(l => l.onSessionUpdatedEvent?.(evt)));

        this.connection.off('OnOrderOperation');
        this.connection.on('OnOrderOperation', (evt: OnOrderOperationEvent) => this.channelListeners.forEach(l => l.onOrderOperationEvent?.(evt)));
        
        this.connection.off('OnPosChargeOperation');
        this.connection.on('OnPosChargeOperation', (evt: OnPosChargeOperationEvent) => this.channelListeners.forEach(l => l.onPosChargeOperation?.(evt)));

        this.channelListeners.forEach(l => this.connection.invoke('JoinChannelEvents', l.channelId));
    }

    private connectToJobEvents() {

        this.connection.off('OnBackgroundJobUpdated');
        this.connection.on('OnBackgroundJobUpdated', (evt: JobChangedEvent) => this.jobListeners.forEach(l => {
            if(l.jobId != evt.id) {
                return;
            }

            l.OnJobChanged?.(evt);
        }));

        this.jobListeners.forEach(l => this.connection.invoke('JoinJobEvents', l.jobId));
    }

    private connectToTransactionEvents() {
        if(this.jobListeners.size == 0) {
            return;
        }

        this.connection.off('OnTransactionInvoiceOperation');
        this.connection.on('OnTransactionInvoiceOperation', (evt: OnTransactionInvoiceOperationEvent) => this.transactionListeners.forEach(l => {
            if(l.transactionId != evt.id) {
                return;
            }

            l.onTransactionInvoiceOperation?.(evt);
        }));
        
        this.jobListeners.forEach(l => this.connection.invoke('JoinTransactionEvents', l.jobId));
    }


    addUserListener(_: UserEventListener): void {
    }

    removeUserListener(_: UserEventListener): void {
    }

    addChannelListener(listener: ChannelListener): void {
        let listeners = this.channelListeners;
        if(listeners.has(listener)) {
            return;
        }

        listeners.add(listener);
        if (this.connection.state == HubConnectionState.Connected) {
            this.connection.invoke("JoinChannelEvents", listener.channelId);
        }
    }
    removeChannelListener(listener: ChannelListener): void {
        let listeners = this.channelListeners;
        if (listeners.delete(listener) && this.connection.state == HubConnectionState.Connected) {
            for (let item of listeners.values()) {
                if (item.channelId == listener.channelId)
                    return;
            }
            this.connection.invoke("LeaveChannelEvents", listener.channelId);
        }
    }

    addJobListener(listener: JobListener): void {
        let listeners = this.jobListeners;
        if(listeners.has(listener)) {
            return;
        }

        listeners.add(listener);
        if (this.connection.state == HubConnectionState.Connected) {
            this.connection.invoke("JoinJobEvents", listener.jobId);
        }
    }
    removeJobListener(listener: JobListener): void {
        let listeners = this.jobListeners;
        if (listeners.delete(listener) && this.connection.state == HubConnectionState.Connected) {
            for (let item of listeners.values()) {
                if (item.jobId == listener.jobId)
                    return;
            }
            this.connection.invoke("LeaveJobEvents", listener.jobId);
        }
    }

    addTransactionListener(listener: TransactionListener): void {
        let listeners = this.transactionListeners;
        if(listeners.has(listener)) {
            return;
        }

        listeners.add(listener);
        if (this.connection.state == HubConnectionState.Connected) {
            this.connection.invoke("JoinTransactionEvents", listener.transactionId);
        }
    }
    removeTransactionListener(listener: TransactionListener): void {
        let listeners = this.transactionListeners;
        if (listeners.delete(listener) && this.connection.state == HubConnectionState.Connected) {
            for (let item of listeners.values()) {
                if (item.transactionId == listener.transactionId)
                    return;
            }
            this.connection.invoke("LeaveTransactionEvents", listener.transactionId);
        }
    }

    addSignalRListener(listener: ISignalRListener): void {
        !this.signalRListeners.has(listener) && this.signalRListeners.add(listener);
    }
    removeSignalRListener(listener: ISignalRListener): void {
        this.signalRListeners.delete(listener);
    }
}