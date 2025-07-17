import type { JobChangedEvent } from "./dtos/JobChangedEvent";

export interface JobListener {
    readonly jobId: string;
    readonly OnJobChanged: (event: JobChangedEvent) => void | Promise<void>;
}