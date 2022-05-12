export type EntityState = {
  entityId: string;
  userId: string;
  threadId?: string;
  acsUserId?: string;
  acsToken?: string;
  tokenExpiresOn?: string;
};
