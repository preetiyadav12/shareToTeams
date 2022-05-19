import { BaseModel } from "./base-model";
import { Participant } from "./participant";

export interface EntityState extends BaseModel {
  entityId: string;
  owner: string;
  threadId: string;
  acsUserId: string;
  acsToken: string;
  tokenExpiresOn: string;
  participants: Participant[];
}
