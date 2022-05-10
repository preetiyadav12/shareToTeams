/// <reference types="@msteams/embedchat" />
// import MSTeamsExt from "@msteams/embedchat";
const MSTeamsExt = require("@msteams/embedchat/dist/embedChat");

const config = {
  hostDomain: "localhost:3000",
  apiBaseUrl: "http://localhost:7071",
  clientId: "",
  tenant: "",
  acsEndpoint: "",
};

const chat = new MSTeamsExt.EmbeddedChat(config);

// const chat = new MSTeamsExt.EmbeddedChat(config);
chat.PrintName("Danny");

var div = {};

chat
  .renderEmbed(div, "0001")
  .then(() => {
    console.log("Success!");
  })
  .catch((error) => {
    console.error(error.message);
  });
