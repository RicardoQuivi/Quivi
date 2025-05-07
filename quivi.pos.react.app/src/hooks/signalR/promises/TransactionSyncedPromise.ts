import { Transaction } from "../../api/Dtos/transactions/Transaction";
import { OnPosChargeSyncAttemptEvent } from "../Dtos/OnPosChargeSyncAttemptEvent";
import { MerchantEventListener } from "../MerchantEventListener";
import { IWebClient } from "../SignalRClient";

export class TransactionSyncedPromise implements Promise<void>, MerchantEventListener {
    private wrappedPromise: Promise<void>;
    private resolver?: (value: void | PromiseLike<void>) => void;
    private intervalTimer: NodeJS.Timeout; 
    private timerCalls: number = 0;

    constructor (private transactionId: string, private client: IWebClient, private getTransaction: (chargeId: string) => Promise<Transaction | undefined>) {
        this.wrappedPromise = new Promise<void>((resolver) => {
            this.resolver = resolver;
            client.addMerchantListener(this);
        });
        this.intervalTimer = setInterval(() => this.polling(), 3000);
    }

    private polling() {
        this.timerCalls++;        
        this.getTransaction(this.transactionId)
            .then(transaction => {
                if (transaction?.isSynced == true) {
                    clearInterval(this.intervalTimer);
                    this.resolver?.();
                }
            });

        if (this.timerCalls > 20) {
            clearInterval(this.intervalTimer);
        }
    }

    onPosChargeSyncAttemptEvent(event: OnPosChargeSyncAttemptEvent): any {
        if(event.id != this.transactionId) {
            return;
        }
        this.client.removeMerchantListener(this);
        this.resolver && this.resolver();
    }

    get [Symbol.toStringTag]() {
        return this.wrappedPromise[Symbol.toStringTag];
    }

    then<TResult1 = void, TResult2 = never>(onfulfilled?: ((value: void) => TResult1 | PromiseLike<TResult1>) | null | undefined, onrejected?: ((reason: any) => TResult2 | PromiseLike<TResult2>) | null | undefined): Promise<TResult1 | TResult2> {
        return this.wrappedPromise.then(onfulfilled, onrejected);
    }

    catch<TResult = never>(onrejected?: ((reason: any) => TResult | PromiseLike<TResult>) | null | undefined): Promise<void | TResult> {
        return this.wrappedPromise.catch(onrejected);
    }

    finally(onfinally?: (() => void) | null | undefined): Promise<void> {
        return this.wrappedPromise.finally(onfinally);
    }
}