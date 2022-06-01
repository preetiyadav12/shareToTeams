import { PhotoUtil } from "../api/photoUtil";
import { AuthInfo, EntityState } from "src/models";
import { AddParticipantDialog } from "./addParticipantDialog";
import { Person } from "../models/person";
import { ParticipantList } from "./participantList";
import { Message } from "src/models/message";
import { ChatItem } from "./chatItem";
import { PeopleItem } from "./peopleItem";
import { GraphUtil } from "../api/graphUtil";

const template = document.createElement("template");
template.innerHTML = `
    <div class="teams-embed-container">
        <div class="teams-embed-header">
            <div class="teams-embed-header-text">
                <h2></h2>
            </div>
            <div class="teams-embed-header-participants">
                <button class="teams-embed-header-participants-button">
                    <div class="teams-embed-header-participants-icon">
                        <svg viewBox="-6 -6 32 32" role="presentation" class="app-svg icons-team-operation icons-team-create" focusable="false">
                            <g class="icons-default-fill icons-filled"><path d="M11 10C11.1035 10 11.2052 10.0079 11.3045 10.023C9.90933 11.0206 9 12.6541 9 14.5C9 15.3244 9.1814 16.1065 9.50646 16.8085C8.90367 16.9334 8.23233 17 7.5 17C4.08805 17 2 15.5544 2 13.5V12C2 10.8954 2.89543 10 4 10H11Z"></path><path d="M17 6.5C17 7.88071 15.8807 9 14.5 9C13.1193 9 12 7.88071 12 6.5C12 5.11929 13.1193 4 14.5 4C15.8807 4 17 5.11929 17 6.5Z"></path><path d="M7.5 2C9.433 2 11 3.567 11 5.5C11 7.433 9.433 9 7.5 9C5.567 9 4 7.433 4 5.5C4 3.567 5.567 2 7.5 2Z"></path><path fill-rule="evenodd" clip-rule="evenodd" d="M19 14.5C19 16.9853 16.9853 19 14.5 19C12.0147 19 10 16.9853 10 14.5C10 12.0147 12.0147 10 14.5 10C16.9853 10 19 12.0147 19 14.5ZM15 12.5C15 12.2239 14.7761 12 14.5 12C14.2239 12 14 12.2239 14 12.5V14H12.5C12.2239 14 12 14.2239 12 14.5C12 14.7761 12.2239 15 12.5 15H14V16.5C14 16.7761 14.2239 17 14.5 17C14.7761 17 15 16.7761 15 16.5V15H16.5C16.7761 15 17 14.7761 17 14.5C17 14.2239 16.7761 14 16.5 14H15V12.5Z"></path></g>
                            <g class="icons-default-fill icons-unfilled"><path d="M11 10C11.1035 10 11.2052 10.0079 11.3045 10.023C10.9143 10.302 10.5621 10.6308 10.2572 11H4C3.44772 11 3 11.4477 3 12V13.5C3 14.9071 4.57862 16 7.5 16C8.11725 16 8.67455 15.9512 9.16969 15.861C9.25335 16.1896 9.36661 16.5065 9.50646 16.8085C8.90367 16.9334 8.23233 17 7.5 17C4.08805 17 2 15.5544 2 13.5V12C2 10.8954 2.89543 10 4 10H11Z"></path><path fill-rule="evenodd" clip-rule="evenodd" d="M17 6.5C17 7.88071 15.8807 9 14.5 9C13.1193 9 12 7.88071 12 6.5C12 5.11929 13.1193 4 14.5 4C15.8807 4 17 5.11929 17 6.5ZM14.5 5C13.6716 5 13 5.67157 13 6.5C13 7.32843 13.6716 8 14.5 8C15.3284 8 16 7.32843 16 6.5C16 5.67157 15.3284 5 14.5 5Z"></path><path fill-rule="evenodd" clip-rule="evenodd" d="M7.5 2C9.433 2 11 3.567 11 5.5C11 7.433 9.433 9 7.5 9C5.567 9 4 7.433 4 5.5C4 3.567 5.567 2 7.5 2ZM7.5 3C6.11929 3 5 4.11929 5 5.5C5 6.88071 6.11929 8 7.5 8C8.88071 8 10 6.88071 10 5.5C10 4.11929 8.88071 3 7.5 3Z"></path><path fill-rule="evenodd" clip-rule="evenodd" d="M19 14.5C19 16.9853 16.9853 19 14.5 19C12.0147 19 10 16.9853 10 14.5C10 12.0147 12.0147 10 14.5 10C16.9853 10 19 12.0147 19 14.5ZM15 12.5C15 12.2239 14.7761 12 14.5 12C14.2239 12 14 12.2239 14 12.5V14H12.5C12.2239 14 12 14.2239 12 14.5C12 14.7761 12.2239 15 12.5 15H14V16.5C14 16.7761 14.2239 17 14.5 17C14.7761 17 15 16.7761 15 16.5V15H16.5C16.7761 15 17 14.7761 17 14.5C17 14.2239 16.7761 14 16.5 14H15V12.5Z"></path></g>
                        </svg>
                    </div>
                    <span class="teams-embed-header-participants-count"></span>
                </button>
            </div>
        </div>
        <div class="teams-embed-chat">
            <div class="teams-embed-chat-container">
                <ul class="teams-embed-chat-items">
                </ul>
            </div>
        </div>
        <div class="teams-embed-footer">
            <div class="teams-embed-input-mention-container" style="display: none">
            </div>
            <div class="teams-embed-footer-container">
                <div class="teams-embed-footer-input" contentEditable="true"></div>
                <div class="teams-embed-footer-actions">
                    <button class="teams-embed-footer-send-message-button">
                        <div>
                            <span>
                                <svg focusable="false" viewBox="2 2 16 16" class="teams-embed-send-icon">
                                    <g>
                                        <path class="icons-unfilled"
                                            d="M2.72113 2.05149L18.0756 9.61746C18.3233 9.73952 18.4252 10.0393 18.3031 10.287C18.2544 10.3858 18.1744 10.4658 18.0756 10.5145L2.72144 18.0803C2.47374 18.2023 2.17399 18.1005 2.05193 17.8528C1.99856 17.7445 1.98619 17.6205 2.0171 17.5038L3.9858 10.0701L2.01676 2.62789C1.94612 2.36093 2.10528 2.08726 2.37224 2.01663C2.48893 1.98576 2.61285 1.99814 2.72113 2.05149ZM3.26445 3.43403L4.87357 9.51612L4.93555 9.50412L5 9.5H12C12.2761 9.5 12.5 9.72386 12.5 10C12.5 10.2455 12.3231 10.4496 12.0899 10.4919L12 10.5H5C4.9686 10.5 4.93787 10.4971 4.90807 10.4916L3.26508 16.6976L16.7234 10.066L3.26445 3.43403Z">
                                        </path>
                                        <path class="icons-filled"
                                            d="M2.72113 2.05149L18.0756 9.61746C18.3233 9.73952 18.4252 10.0393 18.3031 10.287C18.2544 10.3858 18.1744 10.4658 18.0756 10.5145L2.72144 18.0803C2.47374 18.2023 2.17399 18.1005 2.05193 17.8528C1.99856 17.7445 1.98619 17.6205 2.0171 17.5038L3.53835 11.7591C3.58866 11.5691 3.7456 11.4262 3.93946 11.3939L10.8204 10.2466C10.9047 10.2325 10.9744 10.1769 11.0079 10.1012L11.0259 10.0411C11.0454 9.92436 10.9805 9.81305 10.8759 9.76934L10.8204 9.7534L3.90061 8.6001C3.70668 8.56778 3.54969 8.4248 3.49942 8.23473L2.01676 2.62789C1.94612 2.36093 2.10528 2.08726 2.37224 2.01663C2.48893 1.98576 2.61285 1.99814 2.72113 2.05149Z">
                                        </path>
                                    </g>
                                </svg>
                            </span>
                        </div>
                    </button>
                </div>
            </div>
        </div>
    </div>`;

export class AppContainer extends HTMLElement {
    private chatTitle: string;
    private authInfo: AuthInfo;
    private photoUtil: PhotoUtil;
    private dialog:AddParticipantDialog;
    private participantList?:ParticipantList;
    private messages:Message[];
    private mentionResults: Person[];
    private mentionInput: string;
    private personList: Person[];
    private entityState: EntityState;
    constructor(messages:Message[], chatTitle: string, authInfo: AuthInfo, entityState: EntityState) {
        super();
        this.chatTitle = chatTitle;
        this.authInfo = authInfo;
        this.photoUtil = new PhotoUtil();
        this.dialog =  new AddParticipantDialog(this.authInfo, this.photoUtil);
        this.messages = messages;
        this.mentionResults = [];
        this.mentionInput = "";
        this.personList = [];
        this.entityState = entityState;
        this.render();
    }

    messageReceived = async (message:Message) => {
        await this.renderMessage(message);
        this.messages.push(message);
    };

    renderMessage = async (message:Message) => {
        message.sender.photo = this.photoUtil.emptyPic;
        await this.photoUtil.getGraphPhotoAsync(this.authInfo.accessToken, message.sender.id).then((pic:string) => {
            message.sender.photo = pic;
            //chatItem.refresh(message);
        });
        const chatItem:ChatItem = new ChatItem(message, message.sender.id == this.authInfo.uniqueId);
        
        const chatItems = <HTMLElement>this.querySelector(".teams-embed-chat-items");
        chatItems.appendChild(chatItem);

        const chatContainer = (<HTMLElement>document.querySelector(".teams-embed-chat"))
        chatContainer.scrollTop = chatContainer.scrollHeight;
    };

    mentionSelected = (selectedIndex: number) => {
        const selectedUser = this.mentionResults[selectedIndex];
        const input = (<HTMLElement>document.querySelector(".teams-embed-footer-input"));
        const atMentionHtml = `<readonly class="teams-embed-mention-user" contenteditable="false" userId="${selectedUser.id}">${selectedUser.displayName}</readonly>&ZeroWidthSpace;`;
        const inputHtml = input.innerHTML.replace("@"+this.mentionInput, atMentionHtml);
        input.innerHTML = inputHtml;
        
        // close mention dialog and clear results
        const mentionContainer = (<HTMLElement>document.querySelector(".teams-embed-input-mention-container"));
        mentionContainer.style.display = "none";
        this.mentionResults = [];
    };

    populateMentionContainer = (results: Person[]) => {
        const mentionContainer = (<HTMLElement>document.querySelector(".teams-embed-input-mention-container"));
        mentionContainer.innerHTML = "";
        this.mentionResults = [];
        results.forEach((person: Person, i: number) => {                
            this.mentionResults.push(person);
            const peopleItem = new PeopleItem(person, i, this.mentionSelected.bind(this, i));
            mentionContainer.appendChild(peopleItem);
        });

        mentionContainer.style.display = "block";
    }

    clearMentionContainer = () => {
        this.mentionResults = [];
        (<HTMLElement>document.querySelector(".teams-embed-input-mention-container")).style.display = "none";
    }

    createAtMention = (evt: KeyboardEvent) => {
        // close mention results window if hit escape
        if (evt.key == "Escape") {
            this.clearMentionContainer();
            return;
        } 
    
        const sel: any = window.getSelection();
        // if not input return
        if (sel.anchorNode.nodeValue == null) {
            this.clearMentionContainer();
            return;
        }

        // if the last character is '@', load the full participant list
        if (sel.anchorNode.nodeValue[sel.focusOffset - 1] === '@') {
            this.populateMentionContainer(this.personList);
        } else {
            // get the text from the start of the node up to the cursor focus
            const inputToFocus = sel.anchorNode.nodeValue.substring(0, sel.focusOffset);
            // get the last index of '@', there could be multiple @
            const atIndex = inputToFocus.lastIndexOf("@") + 1;
            if (atIndex == 0) {
                this.clearMentionContainer();
                return;
            }

            this.mentionInput = inputToFocus.substring(atIndex, sel.focusOffset).toLowerCase().trimEnd();
            
            const results: Person[] = [];
            // filter
            for (let i = 0; i < this.personList.length; i++) {
                if (this.personList[i].displayName.toLowerCase().indexOf(this.mentionInput) > -1) {
                    results.push(this.personList[i]);
                }
            }

            if (results.length == 0) {
                this.clearMentionContainer();
                return;
            }
            this.populateMentionContainer(results);
        }
    }

    sendMessage = async () => {
        const replaceEmptyDiv = "<div><br></div>";
        const input = (<HTMLElement>document.querySelector(".teams-embed-footer-input"))
        if (input.textContent?.trim() === '') return;
        const person: Person = {
            id: "asdf",
            userPrincipalName: "asdf",
            displayName: "asdf",
            photo: "asdf"
        }
        const message: Message = {
            message: input.innerHTML.replace(replaceEmptyDiv, ""),
            sender: person,
            id: "asdf",
            threadId: "asdf",
            version: "asdf",
            type: "asdf",
            createdOn: new Date()
        };

        // call graph to get matches
        const results = await GraphUtil.sendChatMessage(this.authInfo.accessToken, this.entityState.chatInfo.threadId, input.innerHTML);
        input.innerHTML = "";
    }

    render = () => {
        // get the template
        const dom = <HTMLElement>template.content.cloneNode(true);
    
        // set chat title
        (<HTMLElement>dom.querySelector(".teams-embed-header-text")).innerHTML = `<h2>${this.chatTitle}</h2>`;
    
        // Concatenating owner and participants to create a list of all participants
        this.personList = this.entityState.participants.concat(this.entityState.owner);// getPartipicipants("");

        // set participant count in header
        (<HTMLElement>dom.querySelector(".teams-embed-header-participants-count")).innerHTML = this.personList.length.toString();
    
        // add the participants list
        this.participantList = createParticipantList(this.personList, () => {
            if (this.participantList)
                this.participantList.hide();
            this.dialog.show(true);
        });
        (<HTMLElement>dom.querySelector(".teams-embed-container")).appendChild(this.participantList);

        // add the add participant dialog
        (<HTMLElement>dom.querySelector(".teams-embed-container")).appendChild(this.dialog);

        // wire even to toggle participant list
        (<HTMLElement>dom.querySelector(".teams-embed-header-participants-button")).addEventListener("click", () => {
            if (this.participantList)
                this.participantList.toggle();
        });

        // wire event to sent message
        (<HTMLElement>dom.querySelector(".teams-embed-footer-send-message-button")).addEventListener("click", () => {
            this.sendMessage();
        });
        
        // wire event to send message on ENTER
        (<HTMLElement>dom.querySelector(".teams-embed-footer-input")).addEventListener("keyup", (e) => {
            if (e.key == "Enter" && !e.shiftKey) {
                e.stopPropagation();
                e.preventDefault();
                const input = <HTMLElement>document.querySelector(".teams-embed-footer-input");
                // the Enter button was pressed
                // remove the last node which is an empty line break
                input.lastChild?.remove();
                this.sendMessage();
                return;
            }

            // handle at mention
            this.createAtMention(e);
        });

        this.appendChild(dom);
    };
}

function createParticipantList(participantList: Person[], callback: any) {
  //todo:retrieve participantlist from entitystate

  const pList = new ParticipantList(participantList, callback);
  return pList;
}

function getPartipicipants(entityId: string) {
  const personList = [];

  const person1: Person = {
    id: "1",
    displayName: "Emily Braun",
    userPrincipalName: "a@b.c",
    photo: "https://www.ugx-mods.com/forum/Themes/UGX-Mods/images/default-avatar.png",
  };
  const person2: Person = {
    id: "2",
    displayName: "Isaiah Langer",
    userPrincipalName: "b@b.c",
    photo: "https://www.ugx-mods.com/forum/Themes/UGX-Mods/images/default-avatar.png",
  };
  const person3: Person = {
    id: "3",
    displayName: "Enrico Cattaneo",
    userPrincipalName: "c@b.c",
    photo: "https://www.ugx-mods.com/forum/Themes/UGX-Mods/images/default-avatar.png",
  };
  const person4: Person = {
    id: "4",
    displayName: "Patti Fernandez",
    userPrincipalName: "d@b.c",
    photo: "https://www.ugx-mods.com/forum/Themes/UGX-Mods/images/default-avatar.png",
  };
  personList.push(person1);
  personList.push(person2);
  personList.push(person3);
  personList.push(person4);

  return personList;
}

customElements.define("app-container", AppContainer);
