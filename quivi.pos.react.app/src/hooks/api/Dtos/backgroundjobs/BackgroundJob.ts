import { JobState } from "./JobState";

export interface BackgroundJob {
    readonly id: string;
    readonly state: JobState;
}