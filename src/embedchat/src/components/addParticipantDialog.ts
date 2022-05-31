import { PhotoUtil } from "src/api/photoUtil";
import { AuthInfo } from "src/models";
import { PeoplePicker } from "./peoplePicker";

const template = document.createElement("template");
template.innerHTML = `
    <div class="teams-embed-add-participant-dialog" style="display: none;">
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
  private onSave?: any;
  private onCancel?: any;
  constructor(authInfo: AuthInfo, photoUtil: PhotoUtil, onSave?: any, onCancel?: any) {
    super();
    this.authInfo = authInfo;
    this.photoUtil = photoUtil;
    this.onSave = onSave;
    this.onCancel = onCancel;
    this.render();
  }

  show = (meetingExists: boolean) => {
    (<HTMLElement>this.querySelector(".teams-embed-add-participant-dialog")).style.display = "block";
    const radios = <NodeListOf<HTMLElement>>this.querySelectorAll(".teams-embed-add-participant-dialog-radio");
    radios.forEach((element: HTMLElement) => {
      element.style.display = meetingExists ? "block" : "none";
    });
  };

  hide = () => {
    (<HTMLElement>this.querySelector(".teams-embed-add-participant-dialog")).style.display = "none";
  };

  render = () => {
    const dom = <HTMLElement>template.content.cloneNode(true);

    // initialize and add the people picker
    const peoplePicker: PeoplePicker = new PeoplePicker(this.authInfo, this.photoUtil);
    (<HTMLElement>dom.querySelector(".teams-embed-add-participant-dialog-form")).children[0].after(peoplePicker);

    (<HTMLElement>dom.querySelector(".teams-embed-add-participant-dialog-add")).addEventListener("click", () => {
      //TODO: Do add logic to add the participant to meeting
      if (this.onSave) this.onSave(peoplePicker.getSelections());
      this.hide();
    });

    (<HTMLElement>dom.querySelector(".teams-embed-add-participant-dialog-cancel")).addEventListener("click", () => {
      if (this.onCancel) this.onCancel();
      this.hide();
    });

    this.appendChild(dom);
  };
}

customElements.define("add-participant-dialog", AddParticipantDialog);
