import { WeeklyAvailability } from "./WeeklyAvailability";

export interface CreateAvailabilityRequest {
    readonly name: string;
    readonly autoAddNewChannelProfiles: boolean;
    readonly autoAddNewMenuItems: boolean;
    readonly weeklyAvailabilities: WeeklyAvailability[];
}