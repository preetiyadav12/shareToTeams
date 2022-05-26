export type EmbedChatConfig = {
  entityId: string;
  topicName?: string;
  participants?: string[];
  allowAddParticipant?: string;
  autoStart?: string;
  topHistoryMessages?: number;
};
