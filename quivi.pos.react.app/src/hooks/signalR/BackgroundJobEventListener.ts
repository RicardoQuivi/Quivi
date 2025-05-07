import { OnBackgroundJobChangedEvent } from "./Dtos/OnBackgroundJobChangedEvent";

export interface BackgroundJobEventListener {
    readonly jobId: string;
    onBackgroundJobChangedEvent?(event: OnBackgroundJobChangedEvent): any;
}