import { HubConnection, HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import { MerchantEventListener } from "./MerchantEventListener";
import { OnChannelEvent } from "./Dtos/OnChannelEvent";
import { OnChannelProfileEvent } from "./Dtos/OnChannelProfileEvent";
import { OnLocalEvent } from "./Dtos/OnLocalEvent";
import { OnItemCategoryEvent } from "./Dtos/OnItemCategoryEvent";
import { OnMenuItemEvent } from "./Dtos/OnMenuItemEvent";
import { OnEmployeeEvent } from "./Dtos/OnEmployeeEvent";
import { OnItemsModifierGroupEvent } from "./Dtos/OnItemsModifierGroupEvent";
import { OnSessionUpdatedEvent } from "./Dtos/OnSessionUpdatedEvent";
import { BackgroundJobEventListener } from "./BackgroundJobEventListener";
import { OnBackgroundJobChangedEvent } from "./Dtos/OnBackgroundJobChangedEvent";
import { OnCustomChargeMethodEvent } from "./Dtos/OnCustomChargeMethodEvent";
import { OnPosChargeEvent } from "./Dtos/OnPosChargeEvent";
import { OnPosChargeSyncAttemptEvent } from "./Dtos/OnPosChargeSyncAttemptEvent";
import { OnPreparationGroupOperationEvent } from "./Dtos/OnPreparationGroupOperationEvent";
import { OnOrderOperationEvent } from "./Dtos/OnOrderOperationEvent";
import { OnConfigurableFieldAssociationOperation } from "./Dtos/OnConfigurableFieldAssociationOperation";
import { OnConfigurableFieldOperation } from "./Dtos/OnConfigurableFieldOperation";
import { OnOrderAdditionalInfoOperation } from "./Dtos/OnOrderAdditionalInfoOperation";

export interface IWebClient {
    addMerchantListener(listener: MerchantEventListener): void;
    removeMerchantListener(listener: MerchantEventListener): void;

    addBackgroundJobListener(listener: BackgroundJobEventListener): void;
    removeBackgroundJobListener(listener: BackgroundJobEventListener): void;
}

export interface ISignalRListener {
    onConnectionChanged(isConnected: boolean): void;
}

function delay(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
}

export class SignalRClient implements IWebClient {
    private connection: HubConnection;
    private signalRListeners: Set<ISignalRListener> = new Set<ISignalRListener>();

    private getJwt: undefined | (() => string);

    public set jwtFetcher(value: undefined | (() => string)) {
        this.getJwt = value;
        this.connection.stop().then(() => this.start());
    }

    private connected = false;

    private merchantListeners: Set<MerchantEventListener> = new Set<MerchantEventListener>();
    private backgroundJobListeners: Set<BackgroundJobEventListener> = new Set<BackgroundJobEventListener>();
    // private employeeListeners: Set<IEmployeeEventListener> = new Set<IEmployeeEventListener>();
    // private globalListeners: Set<IGlobalEventListener> = new Set<IGlobalEventListener>();

    constructor(private signalRHost: string) {
        const url = new URL(`Pos`, signalRHost).toString();
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
                    const url = new URL(`Pos`, this.signalRHost).toString();
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
        this.connectToMerchantChannels();
        this.connectBackgroundJobToChannels();
    }

    private connectToMerchantChannels() {
        this.connection.off('OnChannelProfileOperation');
        this.connection.on('OnChannelProfileOperation', (evt: OnChannelProfileEvent) => this.merchantListeners.forEach(l => l.onChannelProfileEvent?.(evt)));

        this.connection.off('OnLocationOperation');
        this.connection.on('OnLocationOperation', (evt: OnLocalEvent) => this.merchantListeners.forEach(l => l.onLocalEvent?.(evt)));

        this.connection.off('OnChannelOperation');
        this.connection.on('OnChannelOperation', (evt: OnChannelEvent) => this.merchantListeners.forEach(l => l.onChannelEvent?.(evt)));

        this.connection.off('OnItemCategoryOperation');
        this.connection.on('OnItemCategoryOperation', (evt: OnItemCategoryEvent) => this.merchantListeners.forEach(l => l.onItemCategoryEvent?.(evt)));

        this.connection.off('OnMenuItemOperation');
        this.connection.on('OnMenuItemOperation', (evt: OnMenuItemEvent) => this.merchantListeners.forEach(l => l.onMenuItemEvent?.(evt)));

        this.connection.off('OnEmployeeOperation');
        this.connection.on('OnEmployeeOperation', (evt: OnEmployeeEvent) => this.merchantListeners.forEach(l => l.onEmployeeEvent?.(evt)));
        
        this.connection.off('OnItemsModifierGroupOperation');
        this.connection.on('OnItemsModifierGroupOperation', (evt: OnItemsModifierGroupEvent) => this.merchantListeners.forEach(l => l.onItemsModifierGroupEvent?.(evt)));

        this.connection.off('OnCustomChargeMethodOperation');
        this.connection.on('OnCustomChargeMethodOperation', (evt: OnCustomChargeMethodEvent) => this.merchantListeners.forEach(l => l.onCustomChargeMethodEvent?.(evt)));

        this.connection.off('OnSessionUpdated');
        this.connection.on('OnSessionUpdated', (evt: OnSessionUpdatedEvent) => this.merchantListeners.forEach(l => l.onSessionUpdatedEvent?.(evt)));

        this.connection.off('OnPosChargeOperation');
        this.connection.on('OnPosChargeOperation', (evt: OnPosChargeEvent) => this.merchantListeners.forEach(l => l.onPosChargeEvent?.(evt)));

        this.connection.off('OnPosChargeSyncAttemptOperation');
        this.connection.on('OnPosChargeSyncAttemptOperation', (evt: OnPosChargeSyncAttemptEvent) => this.merchantListeners.forEach(l => l.onPosChargeSyncAttemptEvent?.(evt)));

        this.connection.off('OnOrderOperation');
        this.connection.on('OnOrderOperation', (evt: OnOrderOperationEvent) => this.merchantListeners.forEach(l => l.onOrderOperationEvent?.(evt)));

        this.connection.off('OnPreparationGroupOperation');
        this.connection.on('OnPreparationGroupOperation', (evt: OnPreparationGroupOperationEvent) => this.merchantListeners.forEach(l => l.onPreparationGroupOperationEvent?.(evt)));

        this.connection.off('OnConfigurableFieldOperation');
        this.connection.on('OnConfigurableFieldOperation', (evt: OnConfigurableFieldOperation) => this.merchantListeners.forEach(l => l.onConfigurableFieldOperation?.(evt)));

        this.connection.off('OnConfigurableFieldAssociationOperation');
        this.connection.on('OnConfigurableFieldAssociationOperation', (evt: OnConfigurableFieldAssociationOperation) => this.merchantListeners.forEach(l => l.onConfigurableFieldAssociationOperation?.(evt)));

        this.connection.off('OnOrderAdditionalInfoOperation');
        this.connection.on('OnOrderAdditionalInfoOperation', (evt: OnOrderAdditionalInfoOperation) => this.merchantListeners.forEach(l => l.onOrderAdditionalInfoOperation?.(evt)));
    }

    private connectBackgroundJobToChannels() {
        this.connection.off('OnBackgroundJobUpdated');
        this.connection.on('OnBackgroundJobUpdated', (evt: OnBackgroundJobChangedEvent) => {
            this.merchantListeners.forEach(l => l.onBackgroundJobChangedEvent?.(evt));
            this.backgroundJobListeners.forEach(l => {
                if(l.jobId != evt.id)
                    return;
                l.onBackgroundJobChangedEvent?.(evt);
            });
        });
    }

    addMerchantListener(listener: MerchantEventListener): void {
        !this.merchantListeners.has(listener) && this.merchantListeners.add(listener);
    }
    removeMerchantListener(listener: MerchantEventListener): void {
        this.merchantListeners.delete(listener);
    }

    addBackgroundJobListener(listener: BackgroundJobEventListener): void {
        !this.backgroundJobListeners.has(listener) && this.backgroundJobListeners.add(listener);
    }
    removeBackgroundJobListener(listener: BackgroundJobEventListener): void {
        this.backgroundJobListeners.delete(listener);
    }

    addSignalRListener(listener: ISignalRListener): void {
        !this.signalRListeners.has(listener) && this.signalRListeners.add(listener);
    }
    removeSignalRListener(listener: ISignalRListener): void {
        this.signalRListeners.delete(listener);
    }
}