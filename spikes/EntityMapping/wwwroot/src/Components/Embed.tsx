import * as msal from "@azure/msal-browser";
import { ChatClient, ChatMessage as CM, CreateChatThreadOptions, CreateChatThreadRequest } from "@azure/communication-chat";
import { AzureCommunicationTokenCredential } from "@azure/communication-common";
import * as React from "react";
import { Chat, Input, Provider, teamsTheme, Avatar, Divider } from "@fluentui/react-northstar";
import { SendIcon } from '@fluentui/react-icons-northstar';
import ReactHtmlParser from "react-html-parser";

import { Mapping } from "../Models/Mapping";
import { PhotoUtil } from "../Utils/PhotoUtil";
import { ApiUtil } from "../Utils/ApiUtil";
import { InitResponse } from "../Models/InitResponse";

// component properties
export interface EmbedProps {
    entityId:string;
    chatTitle?:string;
    acsEndpoint:string;
    clientId:string;
    acsAccountName:string;
}

// component state
export interface EmbedState {
    mapping?:Mapping;
    graphToken?: string;
    idToken?: string;
    aadUserId?: string;
    chatThreadId?:string;
    newMsg:string;
    messages:CM[];
    profilePics: Record<string, string>;
}

// App component
export default class Embed extends React.Component<EmbedProps, EmbedState> {
    private messagesEndRef = React.createRef()
    private photoUtil:PhotoUtil = new PhotoUtil();
    private msalConfig:msal.Configuration = {
        auth: {
            clientId: this.props.clientId,
            authority: "https://login.microsoftonline.com/msteamsdemos.onmicrosoft.com/"
        },
        cache: {
            cacheLocation: "sessionStorage",
            storeAuthStateInCookie: true
        }
    };
    private msalInstance:msal.PublicClientApplication = new msal.PublicClientApplication(this.msalConfig);
    private creds?:AzureCommunicationTokenCredential;
    private chatClient?:ChatClient;

    constructor(props: EmbedProps) {
        super(props); 
        this.state = {
            newMsg: "",
            messages: [],
            profilePics: {}
        };
    }

    componentDidMount = async () => {
        // HACK...get client_id from url
        await this.msalInstance.handleRedirectPromise();
        const accounts = this.msalInstance.getAllAccounts();

        // if the user is signed in, acquire the token
        if (accounts.length !== 0) {
            const resp = await this.msalInstance.acquireTokenSilent({
                scopes: ["https://graph.microsoft.com/.default"],
                account: accounts[0]
            });
            if (resp.accessToken) {
                await this.initializeAcsAsync(resp);
            }
            else {
                //TODO???
            }
        }
        else {
            // No user signed in
            await this.msalInstance.acquireTokenRedirect({
                scopes: ["https://graph.microsoft.com/.default"],
                redirectStartPage: window.location.href
            });
        }
    };

    initializeAcsAsync = async (authResult:msal.AuthenticationResult) => {
        // try to get the mapping for this entity id
        const response:InitResponse = await ApiUtil.getMapping(this.props.entityId, authResult.idToken);

        // initialize the ACS Client
        this.creds = new AzureCommunicationTokenCredential(response.acsToken);
        this.chatClient = new ChatClient(this.props.acsEndpoint, this.creds);
        await this.chatClient.startRealtimeNotifications();

        // determine if this is a new thread or not
        const messages:CM[] = [];
        const profilePics = this.state.profilePics;
        if (!response.mapping.threadId || response.mapping.threadId === "") {
            // this is a new thread...start it
            // TODO...who to start this with???
            const createChatThreadRequest:CreateChatThreadRequest = { topic: this.props.chatTitle };
            const createChatThreadOptions:CreateChatThreadOptions = {
                participants: [
                    //{ id: { communicationUserId: response.mapping.acsUserId}, displayName: this.props.acsAccountName },
                    { id: { microsoftTeamsUserId: "7377d1e6-2e66-456d-a91a-0c1749331cef" }, displayName: "Richard diZerega" },
                    { id: { microsoftTeamsUserId: "bd65ed4e-0c9c-4951-b04e-1cb9a4591730" }, displayName: "Brennen Cage" },
                    { id: { microsoftTeamsUserId: "bf3a74cc-852e-4fcf-9256-298f6319d9e1" }, displayName: "Lily Shen" },
                    { id: { microsoftTeamsUserId: "53160cb2-988e-4b0f-bf47-0767494ee1da" }, displayName: "Elizabeth diZerega" }
                ]
            };

            const createChatThreadResult = await this.chatClient.createChatThread(
                createChatThreadRequest,
                createChatThreadOptions
            );

            // update the mapping with the threadId
            response.mapping.threadId = createChatThreadResult.chatThread.id;
            await ApiUtil.updateMapping(response.mapping, authResult.idToken);
        }
        else {
            // load the existing thread messages            
            const chatThreadClient = await this.chatClient.getChatThreadClient(response.mapping.threadId);
            for await (const chatMessage of chatThreadClient.listMessages()) {
                messages.unshift(chatMessage);
                if (chatMessage.sender && !profilePics[(chatMessage.sender as any).microsoftTeamsUserId]) {
                    const userId = (chatMessage.sender as any).microsoftTeamsUserId;
                    profilePics[userId] = await this.photoUtil.getGraphPhotoAsync(authResult.accessToken, userId);
                }
            }
        }

        // save details into state
        this.setState({
            graphToken: authResult.accessToken, 
            idToken: authResult.idToken, 
            aadUserId: authResult.uniqueId, 
            mapping: response.mapping, 
            messages, 
            profilePics
        });

        // listen for events
        this.chatClient.on("chatMessageReceived", async (e) => {
            const messages = this.state.messages;
            messages.push({
                id: e.id,
                createdOn: e.createdOn,
                sender: e.sender,
                senderDisplayName: e.senderDisplayName,
                content: {message: e.message},
                sequenceId: messages.length.toString(),
                type: "html",
                version: e.version
            });

            const profilePics = this.state.profilePics;
            if (e.sender && !profilePics[(e.sender as any).microsoftTeamsUserId]) {
                const userId = (e.sender as any).microsoftTeamsUserId;
                profilePics[userId] = await this.photoUtil.getGraphPhotoAsync(authResult.accessToken, userId);
            }
            this.setState({messages, profilePics});
        });
        //TODO: others
    };

    keypress = (evt:any) => {
        if (evt.key == "Enter") {
            // post the message with graph
            fetch(`https://graph.microsoft.com/v1.0/chats/${this.state.mapping.threadId}/messages`, {
                method: "POST",
                headers: new Headers({
                    "Authorization": "Bearer " + this.state.graphToken,
                    "Content-Type": "application/json"
                }),
                body: JSON.stringify({body: {contentType: "text", content: this.state.newMsg}})
            }).then((res:any) => {
                return res.json();
            }).then((jsonResponse: any) => {
                console.log(jsonResponse);
                this.setState({newMsg: ""});
            });
        }
    };

    getPhoto = (aadUserId: string) => {
        return this.photoUtil.emptyPic;
    };

    componentDidUpdate(prevProps: Readonly<EmbedProps>, prevState: Readonly<EmbedState>, snapshot?: any): void {
        (this.messagesEndRef.current as any).scrollIntoView({ behavior: "smooth" });
    }

    render = () => {
        const items:any[] = this.state.messages.map((message:CM, index: number) => {
            let item:any;
      
            if (message.sender && (message.type == "html" || message.type == "text"))
            {
                if ((message.sender as any).microsoftTeamsUserId == this.state.aadUserId) {
                    // this is from me
                    item = {
                        message: (
                            <Chat.Message content={ReactHtmlParser(message.content.message)} author={message.senderDisplayName} timestamp={message.createdOn.toLocaleString()} mine />
                        ),
                        contentPosition: "end",
                        attached: "top",
                        key: message.id,
                    };
                }
                else {
                    // this is from someone else in chat
                    item = {
                        gutter: <Avatar image={this.state.profilePics[(message.sender as any).microsoftTeamsUserId]} name={message.senderDisplayName} />,
                        message: <Chat.Message content={ReactHtmlParser(message.content.message)} author={message.senderDisplayName} timestamp={message.createdOn.toLocaleString()} />,
                        attached: 'top',
                        key: message.id,
                    };
                }
            }
            else if (message.type == "topicUpdated") {
                item = {
                    children: <Divider content={`Group name changed to ${message.content.topic}`} color="brand" important />,
                    key: 'message-id-9',
                };
            }
            else if (message.type == "participantAdded") {
                const msg = (message.content.participants.length > 1) 
                    ? `${message.content.participants[0].displayName} and ${message.content.participants.length - 1} others added to chat` 
                    : `${message.content.participants[0].displayName} added to chat`;
                item = {
                    children: <Divider content={msg} color="brand" important />,
                    key: 'message-id-9',
                };
            }
            return item;
        });

        return (
            <div className="appWrapper" style={{display: "flex", height: "100%", width: "100%", border: "1px solid rgb(243, 242, 241)"}}>
                <Provider theme={teamsTheme} style={{width: "100%", minHeight: "100%", display: "flex", flexDirection: "column", alignItems: "stretch"}}>
                    <div style={{overflowY: "scroll", width: "100%", display: "flex", flexFlow: "column", height: "100%"}}>
                        <Chat items={items} style={{width: "100%", flex: "1 1 auto", paddingBottom: "15px"}} />
                        <div ref={this.messagesEndRef as any} />
                    </div>
                    <div style={{flexShrink: 0}}>
                        <Input fluid inverted icon={<SendIcon />} value={this.state.newMsg} onChange={(e, data:any) => this.setState({newMsg: data.value.toString()})} onKeyPress={this.keypress.bind(this)}></Input>
                    </div>
                </Provider>
            </div>
        );
    };
}