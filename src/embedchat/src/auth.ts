import * as msal from "@azure/msal-browser";
import { AppSettings } from "./config/appSettings";

export class Auth {
  private appSettings: AppSettings;
  private msalConfig: msal.Configuration;

  constructor(config: AppSettings) {
    this.appSettings = config;
    this.msalConfig = {
      auth: {
        clientId: this.appSettings.clientId,
        redirectUri: `https://${this.appSettings.hostDomain}/auth.html`,
        authority: `https://login.microsoftonline.com/${this.appSettings.tenant}/`,
      },
      cache: {
        cacheLocation: "localStorage",
        storeAuthStateInCookie: true,
      },
    };
    this.init();
  }

  init = async () => {
    const msalInstance = new msal.PublicClientApplication(this.msalConfig);
    await msalInstance.handleRedirectPromise();

    const accounts = msalInstance.getAllAccounts();

    // if the user is signed in, acquire the token
    if (accounts.length != 0) {
      const resp = await msalInstance.acquireTokenSilent({
        scopes: ["https://graph.microsoft.com/.default"],
        account: accounts[0],
      });
      if (resp.accessToken) {
        // return the token based on mode
        if (window.location.href.indexOf("?mode=interactive") === -1) {
          console.log("Returning token from silent auth");
          parent.postMessage(resp, "*");
        } else {
          console.log("Returning token from interactive auth");
          window.opener.postMessage(resp, "*");
        }
      } else {
        // return error based on how this was launched
        if (window.location.href.indexOf("?mode=interactive") === -1) {
          // this should never happen??? Maybe when tokens stale
          console.log("Silent auth failed");
          parent.postMessage(null, "*");
        } else {
          console.log("Interactive auth failed");
          window.opener.postMessage(null, "*");
        }
      }
    } else {
      // No user signed in
      if (window.location.href.indexOf("?mode=interactive") === -1) {
        console.log(
          "This was an attempt at silent auth that failed...return null"
        );
        parent.postMessage(null, "*");
      } else {
        console.log("This was an attempt at interative auth...start redirect");
        await msalInstance.acquireTokenRedirect({
          scopes: ["https://graph.microsoft.com/.default"],
          redirectStartPage: window.location.href,
        });
      }
    }
  };
}
