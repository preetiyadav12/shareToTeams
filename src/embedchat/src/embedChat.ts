import { ChatClient, ChatMessage as CM } from "@azure/communication-chat";
import { AzureCommunicationTokenCredential } from "@azure/communication-common";
import { CallClient } from "@azure/communication-calling";
import { v4 as uuidv4 } from "uuid";
import { AuthInfo, EntityState } from "./models";
import { AuthUtil } from "./api/authUtil";
import { AppSettings } from "./config/appSettings";
import { EntityApi } from "./api/entityMapping";
import { PhotoUtil } from "./api/photoUtil";
import { ButtonPage } from "./components/buttonPage";
import { ChatInfoRequest } from "./models/chat-info-request";
import { Waiting } from "./components/waiting";
import { AddParticipantDialog } from "./components/addParticipantDialog";
import { Person } from "./models/person";
import { AppContainer } from "./components/appContainer";
import { Message } from "./models/message";

export class EmbeddedChat {
  private readonly appSettings: AppSettings;
  private creds?: AzureCommunicationTokenCredential;
  private chatClient?: ChatClient;
  private profilePics: Record<string, string> = {};
  private chatTopic = "Chat Topic Name";
  private waiting: Waiting;
  private authResult?:AuthInfo;

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
    this.authResult = await AuthUtil.acquireToken(element, this.appSettings, this.waiting);
    console.log(this.authResult);
    if (!this.authResult) {
      console.log("authResult cannot be null!");
      return;
    }

    console.log(`User Id: ${this.authResult.uniqueId}`);
    console.log(`Graph Token: ${this.authResult.accessToken}`);
    console.log(`Id Token: ${this.authResult.idToken}`);
    console.log(`Token Expires On: ${this.authResult.expiresOn}`);

    console.log(`Trying to get Entity Mapping. Calling ${this.appSettings.apiBaseUrl}/getMapping`);
    const entityApi = new EntityApi(this.appSettings, this.authResult.idToken);
    const chatOwner: Person = {
      id: this.authResult.uniqueId,
      userPrincipalName: this.authResult.account.username,
      displayName: this.authResult.account.name,
      photo: "",
    };

    // create the Entity Request
    const chatRequest: ChatInfoRequest = {
      entityId,
      owner: chatOwner,
      accessToken: this.authResult.accessToken,
      topic: this.chatTopic,
      participants: [],
      correlationId: uuidv4(),
      isSuccess: false,
    };

    // try to get the mapping for this entity id
    const entityState: EntityState = (await entityApi.getMapping(chatRequest))!;
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
        this.authResult,
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
          
          element.removeChild(btn);
          await this.initializeChat(element, chatResponse, true);
        },
      );

      const btn = new ButtonPage("Start Teams Chat", async () => {
        dialog.show(false);
      });
      element.append(dialog);
      element.append(btn);
    }
    else {
      await this.initializeChat(element, entityState, false);
    }
  }

  private initializeChat =  async (element: Element, entityState: EntityState, isNew: boolean) => {
    // Hide the waiting indicator
    this.waiting.hide();

    console.log(`Entity State: ${entityState}`);
    console.log(entityState);

    // // initialize the ACS Client
    console.log("Initializing ACS Client...");
    this.creds = new AzureCommunicationTokenCredential(entityState.acsInfo.acsToken!);
    this.chatClient = new ChatClient(this.appSettings.acsEndpoint!, this.creds);

    // start the realtime notificationa
    await this.chatClient.startRealtimeNotifications();

    // establish the call
    const callClient = new CallClient({});
    const callAgent = await callClient.createCallAgent(this.creds, {displayName: "My 3rd Party App"});
    const locator = { meetingLink: entityState.chatInfo.joinUrl };
    const meetingCall = callAgent.join(locator);

    // load the existing thread messages if this is an existing chat
    const messages: Message[] = [];
    if (!isNew)
    {
      const chatThreadClient = await this.chatClient.getChatThreadClient(entityState.chatInfo.threadId);
      console.log(chatThreadClient);
      for await (const chatMessage of chatThreadClient.listMessages()) {
        console.log(chatMessage);
        if (chatMessage.type == "html") {
          messages.push({
            id: chatMessage.id,
            message: (<any>chatMessage).content.message,
            sender: {
              id: (<any>chatMessage).sender.microsoftTeamsUserId,
              displayName: chatMessage.senderDisplayName!,
              photo: ""
            },
            threadId: entityState.chatInfo.threadId,
            type: chatMessage.type,
            version: chatMessage.version,
            createdOn: chatMessage.createdOn
          });
        }
      }
      console.log(messages);
    }

    // inert the appComponent
    const appComponent:AppContainer = new AppContainer(messages, "Hello World", this.authResult!);
    element.appendChild(appComponent);

    //hide waiting indicator to show UI
    this.waiting.hide();

    // listen for events
    this.chatClient.on("chatMessageReceived", async (e:any) => {
      console.log("TODO: chatMessageReceived");
      console.log(e);

      // send the message to the appContainer
      appComponent.messageReceived({
        id: e.id,
        message: e.message,
        sender: {
          id: e.sender.microsoftTeamsUserId,
          displayName: e.senderDisplayName,
          photo: ""
        },
        threadId: e.threadId,
        type: e.type,
        version: e.version,
        createdOn: e.createdOn
      });
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
  };
}
