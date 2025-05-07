import { PagedRequest } from "../PagedRequest";

export interface GetNotificationMessagesRequest extends PagedRequest {
    readonly isRead?: boolean;
}