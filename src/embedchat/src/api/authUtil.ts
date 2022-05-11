import { AccessToken } from "../models";
import { AppSettings } from "../config/appSettings";

export class AuthUtil {
  public static async acquireToken(element: Element, config: AppSettings): Promise<AccessToken | undefined> {
    // first try silent auth
    const token = await this.acquireTokenSilent(element, config);
    if (token) return token;
    else {
      // requires an interactive login
      const btn = document.createElement("button");
      btn.innerText = "Sign-in";
      btn.addEventListener("click", () => {
        let popupRef = window.open(
          `https://${config.hostDomain}/auth.html?mode=interactive`,
          "Teams Embed",
          "width=700,height=700,toolbar=yes",
        );
        window.addEventListener("message", (e) => {
          element.removeChild(btn); //TODO: check if successful
          popupRef?.close();
          popupRef = null;
          return e.data;
        });
        const t = setInterval(() => {
          if (popupRef && popupRef.closed) {
            clearInterval(t);
            return undefined;
          }
        }, 500);
      });
      element.append(btn);
    }
  }

  private static async acquireTokenSilent(element: Element, config: AppSettings): Promise<AccessToken> {
    return new Promise((resolve) => {
      const loginframe = document.createElement("iframe");
      loginframe.setAttribute("src", `https://${config.hostDomain}/auth.html`);
      loginframe.style.display = "none";
      element.append(loginframe);
      window.addEventListener("message", (event) => {
        //element.removeChild(loginframe);
        resolve(event.data);
      });
    });
  }
}
