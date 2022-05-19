import { BaseModel } from "./base-model";
import { Participant } from "./participant";

export interface ChatInfoRequest extends BaseModel {
  entityId: string;
  userName: string;
  accessToken: string;
  topic: string;
  threadId?: string;
  participants?: Participant[];
}
