import { PhotoUtil } from "src/api/photoUtil";
import { AuthInfo } from "src/models";
import { PeoplePicker } from "./peoplePicker";

const template = document.createElement("template");
template.innerHTML = `
    <div class="teams-embed-add-participant-dialog">
        <div class="teams-embed-add-participant-dialog-form">
            <h3>Add</h3>
            
            <div class="teams-embed-add-participant-dialog-radio">
                <input name="history" value="NoHistory" type="radio"/>
                <label for="NoHistory">Don't include chat history</label>
            </div>
            <div class="teams-embed-add-participant-dialog-radio">
                <input name="history" value="history" type="radio" checked/>
                <label for="history">Include all chat history</label>
            </div>
        </div>
        <div class="teams-embed-add-participant-dialog-buttons">
            <div style="flex-grow: 1"></div>
            <button class="teams-embed-add-participant-dialog-cancel">Cancel</button>
            <button class="teams-embed-add-participant-dialog-add">Add</button>
        </div>
    </div>`;

export class AddParticipantDialog extends HTMLElement {
  private authInfo: AuthInfo;
  private photoUtil: PhotoUtil;
  constructor(authInfo: AuthInfo, photoUtil: PhotoUtil) {
    super();
    this.authInfo = authInfo;
    this.photoUtil = photoUtil;
    this.render();
  }

  render = () => {
    const dom = <HTMLElement>template.content.cloneNode(true);

    // initialize and add the people picker
    const peoplePicker: PeoplePicker = new PeoplePicker(this.authInfo, this.photoUtil);
    (<HTMLElement>dom.querySelector(".teams-embed-add-participant-dialog-form")).children[0].after(peoplePicker);

    (<HTMLElement>dom.querySelector(".teams-embed-add-participant-dialog-add")).addEventListener("click", () => {
      //TODO: Do add logic to add the participant to meeting
      (<HTMLElement>this.querySelector(".teams-embed-add-participant-dialog")).style.display = "none";
    });

    (<HTMLElement>dom.querySelector(".teams-embed-add-participant-dialog-cancel")).addEventListener("click", () => {
      (<HTMLElement>this.querySelector(".teams-embed-add-participant-dialog")).style.display = "none";
    });

    this.appendChild(dom);
  };
}

customElements.define("add-participant-dialog", AddParticipantDialog);
