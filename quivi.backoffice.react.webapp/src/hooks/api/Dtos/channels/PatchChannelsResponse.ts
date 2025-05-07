import { DataResponse } from "../DataResponse";
import { Channel } from "./Channel";

export interface PatchChannelsResponse  extends DataResponse<Channel[]> {
}