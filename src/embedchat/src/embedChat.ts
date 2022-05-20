import { ChatClient, ChatMessage as CM } from "@azure/communication-chat";
import { AzureCommunicationTokenCredential } from "@azure/communication-common";
import { v4 as uuidv4 } from "uuid";
import { EntityState } from "./models";
import { AuthUtil } from "./api/authUtil";
import { AppSettings } from "./config/appSettings";
import { EntityApi } from "./api/entityMapping";
import { PhotoUtil } from "./api/photoUtil";
import { ButtonPage } from "./components/buttonPage";
import { ChatInfoRequest } from "./models/chat-info-request";
import { Waiting } from "./components/waiting";
import { AddParticipantDialog } from "./components/addParticipantDialog";
import { Person } from "./models/person";

export class EmbeddedChat {
  private readonly appSettings: AppSettings;
  private creds?: AzureCommunicationTokenCredential;
  private chatClient?: ChatClient;
  private profilePics: Record<string, string> = {};
  private chatTopic = "Chat Topic Name";
  private waiting: Waiting;

  constructor(config: AppSettings) {
    this.appSettings = config;
    this.waiting = new Waiting();

    // add link to css
    // TODO: we might need to check if it already exists for SPAs
    const linkElement = document.createElement("link");
    linkElement.setAttribute("rel", "stylesheet");
    linkElement.setAttribute("type", "text/css");
    linkElement.setAttribute("href", `https://${config.hostDomain}/embedChat.css`);
    document.head.append(linkElement);
  }

  public PrintName = (name: string): string => {
    const msg = "Hello " + name + " 🎉";
    console.log(msg);
    return msg;
  };

  public async renderEmbed(element: Element, entityId: string) {
    console.log(`HTML Element: ${element.id}`);
    console.log(`Entity Id: ${entityId}`);

    // add waiting indicator to UI and display it while we authenticate and check for mapping
    element.appendChild(this.waiting);
    this.waiting.show();
    const authResult = await AuthUtil.acquireToken(element, this.appSettings, this.waiting);
    console.log(authResult);
    if (!authResult) {
      console.log("authResult cannot be null!");
      return;
    }

    console.log(`User Id: ${authResult.uniqueId}`);
    console.log(`User email: ${authResult.account.username}`);
    console.log(`Graph Token: ${authResult.accessToken}`);
    console.log(`Id Token: ${authResult.idToken}`);
    console.log(`Token Expires On: ${authResult.expiresOn}`);

    console.log(`Trying to get Entity Mapping. Calling ${this.appSettings.apiBaseUrl}/getMapping`);
    const entityApi = new EntityApi(this.appSettings, authResult.idToken);
    const chatOwner: Person = {
      id: authResult.uniqueId,
      userPrincipalName: authResult.account.username,
      displayName: authResult.account.name,
      photo: "",
    };

    // create the Entity Request
    const chatRequest: ChatInfoRequest = {
      entityId,
      owner: chatOwner,
      accessToken: authResult.accessToken,
      topic: this.chatTopic,
      participants: [],
      correlationId: uuidv4(),
      isSuccess: false,
    };

    // try to get the mapping for this entity id
    let entityState: EntityState = (await entityApi.getMapping(chatRequest))!;
    if (entityState && !entityState.isSuccess) {
      alert(
        `There is at least one other chat for this entity is in progress. Please contact one of the owners of the existing chats: ${entityState.owner}`,
      );
      return;
    }
    if (!entityState) {
      // alert(`No entity mapping found for this entity: ${entityId}`);
      // TODO: check autoStart value
      this.waiting.hide();

      const photoUtil: PhotoUtil = new PhotoUtil();
      const dialog: AddParticipantDialog = new AddParticipantDialog(
        authResult,
        photoUtil,
        async (participants: Person[]) => {
          console.log(participants);
          // Update the list of participants
          chatRequest.participants = participants;
          // Wait until the chat is created
          this.waiting.show();

          // Start the chat
          const chatResponse = await entityApi.createChat(chatRequest);
          if (!chatResponse) {
            alert("Failed to initialize a new chat!");
            return;
          }
          // Update the entity state object
          entityState = chatResponse;

          // Hide the waiting indicator
          this.waiting.hide();

          console.log(`Entity Id: ${entityState.entityId}`);
          console.log(`Thread Id: ${entityState.chatInfo.threadId}`);
          console.log(`ACS User Id: ${entityState.acsInfo.acsUserId}`);
          console.log(`ACS Token: ${entityState.acsInfo.acsToken}`);

          // // initialize the ACS Client
          console.log("Initializing ACS Client...");
          this.creds = new AzureCommunicationTokenCredential(entityState.acsInfo.acsToken!);
          this.chatClient = new ChatClient(this.appSettings.acsEndpoint!, this.creds);

          console.log("Successfully initialized ACS Chat Client!");
          const threads = this.chatClient.listChatThreads();
          console.log(`Total of chat threads for this user is: ${threads?.byPage.length}`);

          await this.chatClient.startRealtimeNotifications();

          // determine if this is a new thread or not
          const messages: CM[] = [];
          // load the existing thread messages
          const chatThreadClient = await this.chatClient.getChatThreadClient(entityState.chatInfo.threadId);
          console.log(chatThreadClient);
          for await (const chatMessage of chatThreadClient.listMessages()) {
            messages.unshift(chatMessage);
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            if (chatMessage.sender && !this.profilePics[(chatMessage.sender as any).microsoftTeamsUserId]) {
              // eslint-disable-next-line @typescript-eslint/no-explicit-any
              const userId = (chatMessage.sender as any).microsoftTeamsUserId;
              //this.profilePics[userId] = await PhotoUtil.getGraphPhotoAsync(authResult.accessToken, userId);
            }
          }

          //hide waiting indicator to show UI
          this.waiting.hide();

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
        },
      );

      const btn = new ButtonPage("Start Teams Chat", async () => {
        dialog.show(false);
      });
      element.append(dialog);
      element.append(btn);
    }
  }
}
