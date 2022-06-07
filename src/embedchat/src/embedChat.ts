import { ChatClient } from "@azure/communication-chat";
import { AzureCommunicationTokenCredential } from "@azure/communication-common";
import { CallClient } from "@azure/communication-calling";
import { v4 as uuidv4 } from "uuid";
import { AuthInfo, EntityState, EmbedChatConfig } from "./models";
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
import { GraphUtil } from "./api/graphUtil";
import { ErrorDescriptorDialogue } from "./components/errorDescriptorDialogue";

export class EmbeddedChat {
  private readonly appSettings: AppSettings;
  private creds?: AzureCommunicationTokenCredential;
  private chatClient?: ChatClient;
  private profilePics: Record<string, string> = {};
  private waiting: Waiting;
  private authResult?: AuthInfo;
  private topHistoryMessages = 50;
  private graphAuthResult?: AuthInfo;
  private appAuthResult?: AuthInfo;
  private errorDialogue: ErrorDescriptorDialogue;

  constructor(config: AppSettings) {
    this.appSettings = config;
    this.waiting = new Waiting();
    this.errorDialogue = new ErrorDescriptorDialogue();

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

  public async renderEmbed(element: Element, embedChatConfig: EmbedChatConfig) {
    const entityId = embedChatConfig.entityId;
    this.topHistoryMessages = embedChatConfig.topHistoryMessages ?? this.topHistoryMessages;

    console.log(`HTML Element: ${element.id}`);
    console.log(`Entity Id: ${entityId}`);

    // add waiting indicator to UI and display it while we authenticate and check for mapping
    element.appendChild(this.waiting);
    this.waiting.show();

    // get graph token and then application token
    this.graphAuthResult = await AuthUtil.acquireToken(element, AuthUtil.graphDefaultScope, this.appSettings, this.waiting);
    console.log(this.graphAuthResult);

    if (!this.graphAuthResult) {
      const msg = "graphAuthResult cannot be null!";
      console.log(msg);
      element.prepend(this.errorDialogue);
      this.errorDialogue.printError(msg);
      this.waiting.hide();
      return;
    }

    this.appAuthResult = await AuthUtil.acquireToken(element, `api://${this.appSettings.clientId}/access_as_user`, this.appSettings, this.waiting);
    console.log(this.appAuthResult);
    if (!this.appAuthResult) {
      const msg = "appAuthResult cannot be null!";
      console.log(msg);
      element.prepend(this.errorDialogue);
      this.errorDialogue.printError(msg);
      this.waiting.hide();
      return;
    }

    console.log(`Trying to get Entity Mapping. Calling ${this.appSettings.apiBaseUrl}/getMapping`);
    const entityApi = new EntityApi(this.appSettings, this.appAuthResult.accessToken);
    const chatOwner: Person = {
      id: this.appAuthResult.uniqueId,
      userPrincipalName: this.appAuthResult.account.username,
      displayName: this.appAuthResult.account.name,
      photo: "",
    };

    // create the Entity Request
    const chatRequest: ChatInfoRequest = {
      entityId,
      owner: chatOwner,
      topic: (embedChatConfig.topicName) ? embedChatConfig.topicName : `Chat for ${entityId}`,
      participants: [],
      correlationId: uuidv4(),
      isSuccess: false,
    };

    // try to get the mapping for this entity id
    //catch exception thrown by getMapping api and display on UI
    try {
      const entityState: EntityState = (await entityApi.getMapping(chatRequest))!;
      if (entityState && !entityState.isSuccess) {
        const msg = `There is at least one other chat for this entity is in progress. Please contact one of the owners of the existing chats: ${entityState.owner}`;
        alert(msg);
        element.prepend(this.errorDialogue);
        this.errorDialogue.printError(msg);
        return;
      }
      if (!entityState) {
        // No mapping exists for entityId. Check for autoStart or prompt
        // TODO: check autoStart value
        this.waiting.hide();

        const photoUtil: PhotoUtil = new PhotoUtil();
        const dialog: AddParticipantDialog = new AddParticipantDialog(
          this.graphAuthResult,
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
              const msg = "Failed to initialize a new chat!";
              element.prepend(this.errorDialogue);
              this.errorDialogue.printError(msg);
              this.waiting.hide();
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
      } else {
        await this.initializeChat(element, entityState, false);
      }
    } catch(err){
      element.prepend(this.errorDialogue);
      this.errorDialogue.printError("Error while retriving entityState");
      this.waiting.hide();
    }
  }

  private initializeChat = async (element: Element, entityState: EntityState, isNew: boolean) => {
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
    const deviceManager = await callClient.getDeviceManager();
    //Prompt a user to grant camera and/or microphone permissions, more info can be found 
    //https://docs.microsoft.com/en-us/azure/communication-services/how-tos/calling-sdk/manage-video?pivots=platform-web
    const result = await deviceManager.askDevicePermission({audio: false, video: false});
    //This resolves with an object that indicates whether audio and video permissions were granted
    console.log(result.audio);
    console.log(result.video);
    const callAgent = await callClient.createCallAgent(this.creds, { displayName: "My 3rd Party App" });
    const locator = { meetingLink: entityState.chatInfo.joinUrl };
    const meetingCall = callAgent.join(locator);

    console.log(`Meeting call Id: ${meetingCall.id}`);

    // load the existing thread messages if this is an existing chat
    const messages: Message[] = [];
    if (!isNew) {
      const chatHistory = await GraphUtil.getChatMessages(
        this.graphAuthResult?.accessToken as string,
        entityState.chatInfo.threadId,
        this.appAuthResult?.uniqueId as string,
        this.topHistoryMessages,
      );

      let messageCount = 0;
      chatHistory.map((m) => {
        if (m.messageType === "message") {
          messages.push({
            id: m.id,
            message: m.body.content,
            sender: {
              id: m.from.user.id,
              displayName: m.from.user.displayName,
              photo: "",
            },
            threadId: m.chatId,
            type: m.messageType,
            version: m.etag,
            createdOn: m.createdDateTime,
          });

          messageCount += 1;
        }
      });
      console.log(`Fetched ${messageCount} messages`);

      // After all the messages were retrieved,
      // we'll reverse the order to make them chronologically
      messages.reverse();
      messages.forEach((m) => {
        console.log(m.message);
      });
    }

    // inert the appComponent
    const appComponent: AppContainer = new AppContainer(messages, "TODO: Hello World", this.graphAuthResult!, entityState);
    element.appendChild(appComponent);

    //hide waiting indicator to show UI
    this.waiting.hide();

    // listen for events
    this.chatClient.on("chatMessageReceived", async (e: any) => {
      console.log("TODO: chatMessageReceived");
      console.log(e);

      // send the message to the appContainer
      appComponent.messageReceived({
        id: e.id,
        message: e.message,
        sender: {
          id: e.sender.microsoftTeamsUserId,
          displayName: e.senderDisplayName,
          photo: "",
        },
        threadId: e.threadId,
        type: e.type,
        version: e.version,
        createdOn: e.createdOn,
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
