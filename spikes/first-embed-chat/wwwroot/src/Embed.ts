import { ChatClient, ChatMessage as CM, CreateChatThreadOptions, CreateChatThreadRequest } from "@azure/communication-chat";
import { AzureCommunicationTokenCredential } from "@azure/communication-common";
import { Console } from "console";
import { InitResponse } from "./Models/InitResponse";
import { authUtil } from "./Utils/AuthUtil";
import { configUtil } from "./Utils/ConfigUtil";
import { mappingUtil } from "./Utils/MappingUtil";
import { PhotoUtil } from "./Utils/PhotoUtil";

export class teamsEmbeddedChat {
    private creds?:AzureCommunicationTokenCredential;
    private chatClient?:ChatClient;
    private profilePics: Record<string, string> = {};

    public renderEmbed(element:Element, entityId:string, chatTitle?:string) {
        authUtil.acquireToken(element).then(async (token) => {
            // try to get the mapping for this entity id
            let response:InitResponse = await mappingUtil.getMapping(entityId, token.idToken);
            console.log(response);

            // initialize the ACS Client
            this.creds = new AzureCommunicationTokenCredential(response.acsToken);
            this.chatClient = new ChatClient(configUtil.ACS_ENDPOINT, this.creds);
            await this.chatClient.startRealtimeNotifications();
            
            // determine if this is a new thread or not
            let messages:CM[] = [];
            if (!response.mapping.threadId || response.mapping.threadId === "") {
                // this is a new thread...start it
                console.log("TODO");
            }
            else {
                // load the existing thread messages
                let chatThreadClient = await this.chatClient.getChatThreadClient(response.mapping.threadId);
                console.log(chatThreadClient);
                for await (const chatMessage of chatThreadClient.listMessages()) {
                    messages.unshift(chatMessage);
                    if (chatMessage.sender && !this.profilePics[(chatMessage.sender as any).microsoftTeamsUserId]) {
                        let userId = (chatMessage.sender as any).microsoftTeamsUserId;
                        this.profilePics[userId] = await PhotoUtil.getGraphPhotoAsync(token.accessToken, userId);
                    }
                }
            }

            // listen for events
            this.chatClient.on("chatMessageReceived", async (e) => {
                console.log("TODO: chatMessageReceived");
                console.log(e);
            });
            this.chatClient.on("chatMessageEdited", async (e) => {
                console.log("TODO: chatMessageEdited");
                console.log(e);
            });
            this.chatClient.on("chatMessageDeleted", async (e) => {
                console.log("TODO: chatMessageDeleted");
                console.log(e);
            });
            this.chatClient.on("participantsAdded", async (e) => {
                console.log("TODO: participantsAdded");
                console.log(e);
            });
            this.chatClient.on("participantsRemoved", async (e) => {
                console.log("TODO: participantsRemoved");
                console.log(e);
            });
        });
    }
}