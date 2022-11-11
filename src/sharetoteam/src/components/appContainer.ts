

const template = document.createElement("template");
template.innerHTML = `
        <div class=""><div class="" data-tid="share-form-announcement"><div class=""><label class="" data-tid="share-form-search-label"><p class="" dir="auto">Share to</p>
        <div><label id="unified-search-v2-input-label" hidden="">Type the name of a person, group, or channel</label><div class="ui-dropdown" data-tabster="{&quot;uncontrolled&quot;: {}}" data-tid="unified-search-v2"><div class=""><div class=""><div class="" role="combobox" aria-expanded="false" aria-haspopup="listbox" aria-labelledby="unified-search-v2-input-label"><div class="ui-box ap"><input class="" type="text" id="downshift-0-input" placeholder="Type the name of a person, group, or channel" aria-autocomplete="list" aria-labelledby="unified-search-v2-input-label" autocomplete="off" aria-describedby="dropdown-selected-items-count-2" value="" data-tabster="{&quot;observed&quot;:{&quot;names&quot;:[&quot;search-input&quot;]}}"></div></div></div><ul role="listbox" class="" aria-labelledby="unified-search-v2-input-label" id="downshift-0-menu" aria-hidden="true" style="margin: 0px;"></ul></div></div></div></label>
        <div class=""><p class="" dir="auto" id="share_compose_label" data-tid="share-form-compose-label">Say something about this</p><div class=""><div class=""><textarea id="share-form-compose" name="share-form-compose" tabindex="-1" data-tid="placeholder-share-form-compose" data-is-focusable="false" style="display: none;"></textarea><div contenteditable="true" class="cke_textarea_inline cke_editable cke_editable_inline cke_contents_ltr" tabindex="0" spellcheck="true" role="textbox" aria-multiline="true" data-tid="ckeditor" data-is-focusable="true" data-tabster="{&quot;focusable&quot;:{&quot;isDefault&quot;:true}, &quot;observed&quot;:{&quot;names&quot;:[&quot;chat-input&quot;]}}" aria-labelledby="share_compose_label"><p><br></p><p><br>
        </p><p></p></div></div><div class="ui-box gm ahq ed ahr"></div></div></div></div></div></div>`;

export class AppContainer extends HTMLElement {
  constructor() {
    super();
    this.render();
  }

  render = () => {
    // get the template
    const dom = <HTMLElement>template.content.cloneNode(true);
    this.appendChild(dom);
    return;
  }
}

customElements.define("app-container", AppContainer);
