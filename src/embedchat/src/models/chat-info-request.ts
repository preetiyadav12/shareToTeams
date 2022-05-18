import { BaseModel } from "./base-model";
import { Participant } from "./participant";

export interface ChatInfoRequest extends BaseModel {
  entityId: string;
  userId: string;
  topic: string;
  participants?: Participant[];
}
