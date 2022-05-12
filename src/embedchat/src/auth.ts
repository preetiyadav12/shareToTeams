import * as msal from "@azure/msal-browser";

export class Auth {
  private msalConfig: msal.Configuration;
  private hostUri:string;
  private clientId:string;
  private tenant:string;

  constructor() {
    // get configuration
    if (window.location.search.length > 0) {
      // get configuration from url parameters and save in session for authorization reply
      const params = window.location.search.split('&');

      // hostUri
      const hostUriParam = params.find(i => i.indexOf("host_uri") != -1);
      this.hostUri = (hostUriParam && hostUriParam.split('=').length == 2) ? hostUriParam.split('=')[1] : "";
      sessionStorage.setItem("hostUri", this.hostUri);

      // clientId
      const clientIdParam = params.find(i => i.indexOf("client_id") != -1);
      this.clientId = (clientIdParam && clientIdParam.split('=').length == 2) ? clientIdParam.split('=')[1] : "";
      sessionStorage.setItem("clientId", this.clientId);

      // tenant
      const tenantParam = params.find(i => i.indexOf("tenant") != -1);
      this.tenant = (tenantParam && tenantParam.split('=').length == 2) ? tenantParam.split('=')[1] : "";
      sessionStorage.setItem("tenant", this.tenant);
    }
    else {
      // get configuration from session state
      this.hostUri = sessionStorage.getItem("hostUri") as string;
      this.clientId = sessionStorage.getItem("clientId") as string;
      this.tenant = sessionStorage.getItem("tenant") as string;
    }

    this.msalConfig = {
      auth: {
          clientId: this.clientId,
          redirectUri: `https://${this.hostUri}/auth.html`,
          authority: `https://login.microsoftonline.com/${this.tenant}/`
      },
      cache: {
          cacheLocation: "localStorage",
          storeAuthStateInCookie: true
      }
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
        console.log("This was an attempt at silent auth that failed...return null");
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
new Auth();