export enum Scope {
  INDIVIDUAL = 'INDIVIDUAL',
  GROUP = 'GROUP',
  CHANNEL = 'CHANNEL'
}

export type ShareToTeamsConfig = {
  threadId?: string; 
  comments ?: boolean;
  preview?: boolean;
  scope?: Scope; 
};
