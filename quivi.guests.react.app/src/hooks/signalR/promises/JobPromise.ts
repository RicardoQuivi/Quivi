import { JobState } from "../../api/Dtos/jobs/JobState";
import type { JobListener } from "../JobListener";
import type { IWebClient } from "../SignalRClient";

export class JobPromise implements Promise<void>, JobListener {
    public readonly jobId: string;
    private wrappedPromise: Promise<void>;
    private resolver?: (value: void | PromiseLike<void>) => void;
    private rejecter?: (reason?: any) => void;
    private intervalTimer: number; 
    private timerCalls: number = 0;
    private state: JobState;

    constructor (jobId: string, private client: IWebClient, private getJobStatus: (jobId: string) => Promise<JobState>) {
        this.jobId = jobId;
        this.state = JobState.AwaitingCompletion;
        client.addJobListener(this);
        this.wrappedPromise = new Promise<void>((resolver, rejecter) => {
            this.resolver = resolver;
            this.rejecter = rejecter;
            this.processResult();
        });

        this.intervalTimer = setInterval(() => this.polling(), 3000);
    }
    
    dispose(success: boolean) {
        clearInterval(this.intervalTimer);
        this.client.removeJobListener(this);

        if(success) {
            this.resolver?.();
        } else {
            this.rejecter?.();
        }
    }
    
    processResult() {
        switch(this.state)
        {
            case JobState.Completed: this.dispose(true); break;
            case JobState.Failed: this.dispose(false); break;
        }
    }
    
    OnJobChanged() {
        this.getJobStatus(this.jobId).then(status => {
            this.state = status;
            this.processResult();
        });
    }

    private polling() {
        this.timerCalls++;        
        this.OnJobChanged()
        if (this.timerCalls > 20) {
            this.dispose(false);
            return;
        }
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