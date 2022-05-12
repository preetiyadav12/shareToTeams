export interface AuthInfo {
  uniqueId: string;
  idToken: string;
  accessToken: string;
  expiresOn: Date;
  tenantId: string;
  scopes: string[];
}
