import { AccessToken } from "../models";
import { AppSettings } from "../config/appSettings";

export class AuthUtil {
  public static async acquireToken(element: Element, config: AppSettings): Promise<AccessToken | undefined> {
    console.log("Trying silenty acquire token.");
    // first try silent auth
    return new Promise((resolve, reject) => {
      this.acquireTokenSilent(element, config).then((token) => {
        console.log("Token is obtained!");
        if (token) resolve(token);
        else {
          console.log("Cannot obtain silently access token. Requires an interactive login");
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
              resolve(e.data);
            });
            const t = setInterval(() => {
              if (popupRef && popupRef.closed) {
                clearInterval(t);
                resolve(undefined);
              }
            }, 500);
          });
          element.append(btn);
          console.log(`Created a Sign In button in https://${config.hostDomain}/auth.html`);
        }
      });
      console.log(`Updated HTML Element: ${element.innerHTML}`);
    });
  }

  private static async acquireTokenSilent(element: Element, config: AppSettings): Promise<any> {
    console.log(`Creating iFrame for https://${config.hostDomain}/auth.html`);
    return new Promise((resolve, reject) => {
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
