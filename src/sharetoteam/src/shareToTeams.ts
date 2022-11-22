import { AppSettings } from "./config/appSettings";
import { Waiting } from "./components/waiting";
import { ShareToTeamsConfig } from "./models";
import { ErrorDescriptorDialogue } from "./components/errorDescriptorDialogue";


export class ShareToTeams {
  private waiting: Waiting;
  private errorDialogue: ErrorDescriptorDialogue;
  constructor(config: AppSettings) {
    this.waiting = new Waiting();
    this.errorDialogue = new ErrorDescriptorDialogue();
    const linkElement = document.createElement("link");
    linkElement.setAttribute("rel", "stylesheet");
    linkElement.setAttribute("type", "text/css");
    linkElement.setAttribute("href", `https://${config.hostUrl}/shareToTeams.css`);
    document.head.append(linkElement);
  }

  public async renderShareToTeams(element: Element, shareToTeamsConfig: ShareToTeamsConfig, jsonPayload: JSON) {

    element.appendChild(this.waiting);
    this.waiting.show();

    try {
      
      console.log("Hello World!"); 
    } catch(err) {
      element.prepend(this.errorDialogue);
      this.errorDialogue.printError("Error while retriving entityState");
      this.waiting.hide();
    }
  }
}
