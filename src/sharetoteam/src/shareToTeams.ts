import { AuthUtil } from "./api/authUtil";
import { AppSettings } from "./config/appSettings";
import { Waiting } from "./components/waiting";
import { AppContainer } from "./components/appContainer";
import { GraphUtil } from "./api/graphUtil";
import { AuthInfo, ShareToTeamsConfig } from "./models";
import { ErrorDescriptorDialogue } from "./components/errorDescriptorDialogue";


export class ShareToTeams {
  private readonly appSettings: AppSettings;
  private waiting: Waiting;
  private graphAuthResult?: AuthInfo;
  private errorDialogue: ErrorDescriptorDialogue;
  constructor(config: AppSettings) {
    this.appSettings = config;
    this.waiting = new Waiting();
    this.errorDialogue = new ErrorDescriptorDialogue();
  }

  public async renderShareToTeams(element: Element, ShareToTeamsConfig: ShareToTeamsConfig) {
     
    console.log(`HTML Element: ${element.id}`);

    element.appendChild(this.waiting);
    this.waiting.show();

    // get graph token and then application token
    this.graphAuthResult = await AuthUtil.acquireToken(
      element,
      AuthUtil.graphDefaultScope,
      this.appSettings,
      this.waiting,
    );
    console.log(this.graphAuthResult);

    if (!this.graphAuthResult) {
      const msg = "graphAuthResult cannot be null!";
      console.log(msg);
      element.prepend(this.errorDialogue);
      this.errorDialogue.printError(msg);
      this.waiting.hide();
      return;
    }

    try {
     
    GraphUtil.postInChannel(this.graphAuthResult.accessToken);

    // insert the appComponent
    const appComponent: AppContainer = new AppContainer();
    element.appendChild(appComponent);
    } catch(err) {
      element.prepend(this.errorDialogue);
      this.errorDialogue.printError("Error while retriving entityState");
      this.waiting.hide();
    }
  }
}
