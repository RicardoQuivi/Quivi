import { WeeklyAvailability } from "./WeeklyAvailability";

export interface PatchAvailabilityRequest {
    readonly id: string;
    readonly name?: string;
    readonly autoAddNewChannelProfiles?: boolean;
    readonly autoAddNewMenuItems?: boolean;
    readonly weeklyAvailabilities?: WeeklyAvailability[];
}