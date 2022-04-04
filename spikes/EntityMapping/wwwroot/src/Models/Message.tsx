export interface Message {
    id: string;
    senderDisplayName?: string;
    sequenceId: string;
    type: string;
    version: string;
    createdOn: Date;
}

export interface MessageContent {
    message?: string;
    topic?: string;
    participants?: ChatParticipant[];
}

export interface ChatParticipant {
    displayName: string;
    id:ChatParticipantId;
}

export interface ChatParticipantId {
    kind: string;
    communicationUserId?: string;
    microsoftTeamsUserId?: string;
    rawId?: string;
    cloud?: string;
    isAnonymous?: boolean;
}