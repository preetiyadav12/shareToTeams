import { ACSInfo, BaseModel, ChatInfo, Person } from "./";

export interface EntityState extends BaseModel {
  entityId: string;
  owner: Person;
  chatInfo: ChatInfo;
  acsInfo: ACSInfo;
  participants: Person[];
}
