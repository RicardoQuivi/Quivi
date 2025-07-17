import type { JobState } from "./JobState";

export interface Job {
    readonly id: string;
    readonly state: JobState;
}