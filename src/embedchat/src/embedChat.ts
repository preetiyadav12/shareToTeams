import { ChatClient, ChatClientOptions, ChatMessage as CM, CreateChatThreadRequest } from "@azure/communication-chat";
import { AzureCommunicationTokenCredential } from "@azure/communication-common";
import { AuthInfo, EntityState } from "./models";
import { AuthUtil } from "./api/authUtil";
import { AppSettings } from "./config/appSettings";
import { EntityApi } from "./api/entityMapping";
import { PhotoUtil } from "./api/photoUtil";

export class EmbeddedChat {
  private readonly appSettings: AppSettings;
  private creds?: AzureCommunicationTokenCredential;
  private chatClient?: ChatClient;
  private profilePics: Record<string, string> = {};

  constructor(config: AppSettings) {
    this.appSettings = config;
  }

  public PrintName = (name: string): string => {
    const msg = "Hello " + name + " 🎉";
    console.log(msg);
    return msg;
  };

  public async renderEmbed(element: Element, entityId: string) {
    console.log(`HTML Element: ${element.id}`);
    console.log(`Entity Id: ${entityId}`);

    const authResult = await AuthUtil.acquireToken(element, this.appSettings);
    console.log(authResult);
    if (!authResult) {
      console.log("authResult cannot be null!");
      return;
    }

    console.log(`User Id: ${authResult.uniqueId}`);
    console.log(`Graph Token: ${authResult.accessToken}`);
    console.log(`Id Token: ${authResult.idToken}`);
    console.log(`Token Expires On: ${authResult.expiresOn}`);

    console.log(`Trying to get Entity Mapping. Calling ${this.appSettings.apiBaseUrl}/getMapping`);
    const entityApi = new EntityApi(this.appSettings, authResult.accessToken);

    // try to get the mapping for this entity id
    const entityState: EntityState = (await entityApi.getMapping({
      entityId,
      userId: authResult.uniqueId,
    })) as EntityState;

    console.log(`Entity Id: ${entityState.entityId}`);
    console.log(`Thread Id: ${entityState.threadId}`);
    console.log(`ACS User Id: ${entityState.acsUserId}`);
    console.log(`ACS Token: ${entityState.acsToken}`);

    // // initialize the ACS Client
    console.log("Initializing ACS Client...");
    this.creds = new AzureCommunicationTokenCredential(entityState.acsToken!);
    this.chatClient = new ChatClient(this.appSettings.acsEndpoint!, this.creds);

    console.log("Successfully initialized ACS Chat Client!");
    const threads = this.chatClient.listChatThreads();
    console.log(`Total of chat threads for this user is: ${threads?.byPage.length}`);

    await this.chatClient.startRealtimeNotifications();

    // determine if this is a new thread or not
    const messages: CM[] = [];
    if (!entityState.threadId || entityState.threadId === "") {
      // this is a new thread...start it
      console.log("Starting a new Chat thread...");
      const chatRequest: CreateChatThreadRequest = {
        topic: entityId,
      };
      const chatThreadResult = await this.chatClient.createChatThread(chatRequest);

      console.log(
        `New Chat Thread was created for the topic: ${chatThreadResult.chatThread?.topic} and chat Id: ${chatThreadResult.chatThread?.id}`,
      );
    } else {
      // load the existing thread messages
      const chatThreadClient = await this.chatClient.getChatThreadClient(entityState.threadId);
      console.log(chatThreadClient);
      for await (const chatMessage of chatThreadClient.listMessages()) {
        messages.unshift(chatMessage);
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        if (chatMessage.sender && !this.profilePics[(chatMessage.sender as any).microsoftTeamsUserId]) {
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          const userId = (chatMessage.sender as any).microsoftTeamsUserId;
          this.profilePics[userId] = await PhotoUtil.getGraphPhotoAsync(authResult.accessToken, userId);
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
  }
}
