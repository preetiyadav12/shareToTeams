import {
  ChatClient,
  ChatClientOptions,
  ChatMessage as CM,
} from "@azure/communication-chat";
import { AzureCommunicationTokenCredential } from "@azure/communication-common";
import { AccessToken, EntityState } from "./models";
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

  public async echoMessage(message: string): Promise<string | undefined> {
    return message;
  }

  public async getEntityMapping(
    entityId: string,
    token: string
  ): Promise<EntityState | undefined> {
    const entityApi = new EntityApi(this.appSettings, token);
    const response = await entityApi.getMapping(entityId);
    return response;
  }

  public async renderEmbed(element: Element, entityId: string) {
    const idToken = await AuthUtil.acquireToken(element, this.appSettings);
    if (!idToken) {
      console.log("Token cannot be Null!");
      return;
    }

    const entityApi = new EntityApi(this.appSettings, idToken.accessToken);

    const token = idToken as AccessToken;
    // try to get the mapping for this entity id
    const entityState: EntityState = (await entityApi.getMapping(
      entityId
    )) as EntityState;
    console.log(`ACS Token: ${entityState.acsToken}`);

    // // initialize the ACS Client
    this.creds = new AzureCommunicationTokenCredential(entityState.acsToken!);
    this.chatClient = new ChatClient(this.appSettings.acsEndpoint!, this.creds);
<<<<<<< HEAD
    await this.chatClient.startRealtimeNotifications();

    // determine if this is a new thread or not
    const messages: CM[] = [];
    if (!entityState.threadId || entityState.threadId === "") {
      // this is a new thread...start it
      console.log("TODO");
    } else {
      // load the existing thread messages
      const chatThreadClient = await this.chatClient.getChatThreadClient(
        entityState.threadId
      );
      console.log(chatThreadClient);
      for await (const chatMessage of chatThreadClient.listMessages()) {
        messages.unshift(chatMessage);
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        if (
          chatMessage.sender &&
          !this.profilePics[(chatMessage.sender as any).microsoftTeamsUserId]
        ) {
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          const userId = (chatMessage.sender as any).microsoftTeamsUserId;
          this.profilePics[userId] = await PhotoUtil.getGraphPhotoAsync(
            token.accessToken,
            userId
          );
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
=======
    // await this.chatClient.startRealtimeNotifications();

    // // determine if this is a new thread or not
    // const messages: CM[] = [];
    // if (!entityState.threadId || entityState.threadId === "") {
    //   // this is a new thread...start it
    //   console.log("TODO");
    // } else {
    //   // load the existing thread messages
    //   const chatThreadClient = await this.chatClient.getChatThreadClient(
    //     entityState.threadId
    //   );
    //   console.log(chatThreadClient);
    //   for await (const chatMessage of chatThreadClient.listMessages()) {
    //     messages.unshift(chatMessage);
    //     // eslint-disable-next-line @typescript-eslint/no-explicit-any
    //     if (
    //       chatMessage.sender &&
    //       !this.profilePics[(chatMessage.sender as any).microsoftTeamsUserId]
    //     ) {
    //       // eslint-disable-next-line @typescript-eslint/no-explicit-any
    //       const userId = (chatMessage.sender as any).microsoftTeamsUserId;
    //       this.profilePics[userId] = await PhotoUtil.getGraphPhotoAsync(
    //         token.accessToken,
    //         userId
    //       );
    //     }
    //   }
    // }

    // // listen for events
    // this.chatClient.on("chatMessageReceived", async (e) => {
    //   console.log("TODO: chatMessageReceived");
    //   console.log(e);
    // });
    // this.chatClient.on("chatMessageEdited", async (e) => {
    //   console.log("TODO: chatMessageEdited");
    //   console.log(e);
    // });
    // this.chatClient.on("chatMessageDeleted", async (e) => {
    //   console.log("TODO: chatMessageDeleted");
    //   console.log(e);
    // });
    // this.chatClient.on("participantsAdded", async (e) => {
    //   console.log("TODO: participantsAdded");
    //   console.log(e);
    // });
    // this.chatClient.on("participantsRemoved", async (e) => {
    //   console.log("TODO: participantsRemoved");
    //   console.log(e);
    // });
>>>>>>> b3e7e82ad3539cf5bd4b468fd6faeddc6de5f816
  }
}
