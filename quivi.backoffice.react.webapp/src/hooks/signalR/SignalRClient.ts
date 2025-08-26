import { HubConnection, HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import { UserEventListener } from "./UserEventListener";
import { OnMerchantAssociatedEvent } from "./Dtos/OnMerchantAssociatedEvent";
import { MerchantEventListener } from "./MerchantEventListener";
import { OnChannelProfileEvent } from "./Dtos/OnChannelProfileEvent";
import { OnChannelEvent } from "./Dtos/OnChannelEvent";
import { OnItemCategoryEvent } from "./Dtos/OnItemCategoryEvent";
import { OnLocalEvent } from "./Dtos/OnLocalEvent";
import { OnMenuItemEvent } from "./Dtos/OnMenuItemEvent";
import { OnEmployeeEvent } from "./Dtos/OnEmployeeEvent";
import { OnItemsModifierGroupEvent } from "./Dtos/OnItemsModifierGroupEvent";
import { OnCustomChargeMethodEvent } from "./Dtos/OnCustomChargeMethodEvent";
import { OnPrinterWorkerEvent } from "./Dtos/OnPrinterWorkerEvent";
import { OnPrinterEvent } from "./Dtos/OnPrinterEvent";
import { OnPrinterMessageEvent } from "./Dtos/OnPrinterMessageEvent";
import { OnAcquirerConfigurationEvent } from "./Dtos/OnAcquirerConfigurationEvent";
import { OnReviewOperationEvent } from "./Dtos/OnReviewOperationEvent";
import { OnTransactionOperationEvent } from "./Dtos/OnTransactionOperationEvent";
import { OnTransactionSyncAttemptOperationEvent } from "./Dtos/OnTransactionSyncAttemptOperationEvent";
import { OnMerchantDocumentOperation } from "./Dtos/OnMerchantDocumentOperation";
import { OnConfigurableFieldAssociationOperation } from "./Dtos/OnConfigurableFieldAssociationOperation";
import { OnConfigurableFieldOperation } from "./Dtos/OnConfigurableFieldOperation";

export interface IWebClient {
    addUserListener(listener: UserEventListener): void;
    removeUserListener(listener: UserEventListener): void;

    addMerchantListener(listener: MerchantEventListener): void;
    removeMerchantListener(listener: MerchantEventListener): void;
}

export interface ISignalRListener {
    onConnectionChanged(isConnected: boolean): void;
}

const delay = (ms: number): Promise<void> => new Promise(resolve => setTimeout(resolve, ms));

export class SignalRClient implements IWebClient {
    private connection: HubConnection;
    private signalRListeners: Set<ISignalRListener> = new Set<ISignalRListener>();

    private getJwt: undefined | (() => string);

    public set jwtFetcher(value: undefined | (() => string)) {
        this.getJwt = value;
        this.connection.stop().then(() => this.start());
    }

    private connected = false;

    private userListeners: Set<UserEventListener> = new Set<UserEventListener>();
    private merchantListeners: Set<MerchantEventListener> = new Set<MerchantEventListener>();

    constructor(private signalRHost: string) {
        const url = new URL(`Backoffice`, signalRHost).toString();
        this.connection = new HubConnectionBuilder().withUrl(`${url}?`).build();
        this.start();
    }

    public get isConnected() {
        return this.connected;
    }
    
    private async start(): Promise<void> {
        try {
            await navigator.locks.request("SignalRClient.Start.Lock", async () => {
                if(this.getJwt == undefined) {
                    return;
                } 
    
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
                    let jwt = this.getJwt();
                    const url = new URL(`Backoffice`, this.signalRHost).toString();
                    this.connection.baseUrl = `${url}?access_token=${jwt}`;
                    await this.connection.start().then(async () => {
                        if(this.connection.state != HubConnectionState.Connected) {
                            return;
                        }
                        this.connected = true;
                        await this.connectToChannels();
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

    private async connectToChannels() {
        this.connectToUserChannels();
        this.connectToMerchantChannels();
    }

    private connectToUserChannels() {
        this.connection.off('OnMerchantAssociated');
        this.connection.on('OnMerchantAssociated', (evt: OnMerchantAssociatedEvent) => this.userListeners.forEach(l => l.onMerchantAssociatedEvent?.(evt)));
    }

    private connectToMerchantChannels() {
        this.connection.off('OnChannelProfileOperation');
        this.connection.on('OnChannelProfileOperation', (evt: OnChannelProfileEvent) => this.merchantListeners.forEach(l => l.onChannelProfileEvent?.(evt)));

        this.connection.off('OnChannelOperation');
        this.connection.on('OnChannelOperation', (evt: OnChannelEvent) => this.merchantListeners.forEach(l => l.onChannelEvent?.(evt)));

        this.connection.off('OnItemCategoryOperation');
        this.connection.on('OnItemCategoryOperation', (evt: OnItemCategoryEvent) => this.merchantListeners.forEach(l => l.onItemCategoryEvent?.(evt)));

        this.connection.off('OnLocationOperation');
        this.connection.on('OnLocationOperation', (evt: OnLocalEvent) => this.merchantListeners.forEach(l => l.onLocalEvent?.(evt)));

        this.connection.off('OnMenuItemOperation');
        this.connection.on('OnMenuItemOperation', (evt: OnMenuItemEvent) => this.merchantListeners.forEach(l => l.onMenuItemEvent?.(evt)));

        this.connection.off('OnEmployeeOperation');
        this.connection.on('OnEmployeeOperation', (evt: OnEmployeeEvent) => this.merchantListeners.forEach(l => l.onEmployeeEvent?.(evt)));
        
        this.connection.off('OnItemsModifierGroupOperation');
        this.connection.on('OnItemsModifierGroupOperation', (evt: OnItemsModifierGroupEvent) => this.merchantListeners.forEach(l => l.onItemsModifierGroupEvent?.(evt)));

        this.connection.off('OnCustomChargeMethodOperation');
        this.connection.on('OnCustomChargeMethodOperation', (evt: OnCustomChargeMethodEvent) => this.merchantListeners.forEach(l => l.onCustomChargeMethodEvent?.(evt)));

        this.connection.off('OnPrinterWorkerOperation');
        this.connection.on('OnPrinterWorkerOperation', (evt: OnPrinterWorkerEvent) => this.merchantListeners.forEach(l => l.onPrinterWorkerEvent?.(evt)));

        this.connection.off('OnPrinterOperation');
        this.connection.on('OnPrinterOperation', (evt: OnPrinterEvent) => this.merchantListeners.forEach(l => l.onPrinterEvent?.(evt)));

        this.connection.off('OnPrinterMessageOperation');
        this.connection.on('OnPrinterMessageOperation', (evt: OnPrinterMessageEvent) => this.merchantListeners.forEach(l => l.onPrinterMessageOperation?.(evt)));

        this.connection.off('OnAcquirerConfigurationOperation');
        this.connection.on('OnAcquirerConfigurationOperation', (evt: OnAcquirerConfigurationEvent) => this.merchantListeners.forEach(l => l.onAcquirerConfigurationOperation?.(evt)));

        this.connection.off('OnPosChargeOperation');
        this.connection.on('OnPosChargeOperation', (evt: OnTransactionOperationEvent) => this.merchantListeners.forEach(l => l.onTransactionOperation?.(evt)));

        this.connection.off('OnPosChargeSyncAttemptOperation');
        this.connection.on('OnPosChargeSyncAttemptOperation', (evt: OnTransactionSyncAttemptOperationEvent) => this.merchantListeners.forEach(l => l.onTransactionSyncAttemptOperation?.(evt)));

        this.connection.off('OnReviewOperation');
        this.connection.on('OnReviewOperation', (evt: OnReviewOperationEvent) => this.merchantListeners.forEach(l => l.onReviewOperation?.(evt)));

        this.connection.off('OnMerchantDocumentOperation');
        this.connection.on('OnMerchantDocumentOperation', (evt: OnMerchantDocumentOperation) => this.merchantListeners.forEach(l => l.onMerchantDocumentOperation?.(evt)));

        this.connection.off('OnConfigurableFieldOperation');
        this.connection.on('OnConfigurableFieldOperation', (evt: OnConfigurableFieldOperation) => this.merchantListeners.forEach(l => l.onConfigurableFieldOperation?.(evt)));

        this.connection.off('OnConfigurableFieldAssociationOperation');
        this.connection.on('OnConfigurableFieldAssociationOperation', (evt: OnConfigurableFieldAssociationOperation) => this.merchantListeners.forEach(l => l.onConfigurableFieldAssociationOperation?.(evt)));
    }

    addUserListener(listener: UserEventListener): void {
        !this.userListeners.has(listener) && this.userListeners.add(listener);
    }
    removeUserListener(listener: UserEventListener): void {
        this.userListeners.delete(listener);
    }

    addMerchantListener(listener: MerchantEventListener): void {
        !this.merchantListeners.has(listener) && this.merchantListeners.add(listener);
    }
    removeMerchantListener(listener: MerchantEventListener): void {
        this.merchantListeners.delete(listener);
    }

    addSignalRListener(listener: ISignalRListener): void {
        !this.signalRListeners.has(listener) && this.signalRListeners.add(listener);
    }
    removeSignalRListener(listener: ISignalRListener): void {
        this.signalRListeners.delete(listener);
    }
}