import { WeeklyAvailability } from "./WeeklyAvailability";

export interface Availability {
    readonly id: string;
    readonly name: string;
    readonly autoAddNewMenuItems: boolean;
    readonly autoAddNewChannelProfiles: boolean;
    readonly weeklyAvailabilities: WeeklyAvailability[];
}