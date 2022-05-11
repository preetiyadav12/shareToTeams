import React, { useEffect, useState } from "react";
import MSTeamsExt, { AppSettings, EntityState } from "@msteams/embedchat";
import "./App.css";

const config: AppSettings = {
  hostDomain: process.env.REACT_APP_HOST_DOMAIN as string,
  apiBaseUrl: process.env.REACT_APP_API_BASE_URL as string,
  clientId: process.env.REACT_APP_CLIENT_ID as string,
  tenant: process.env.REACT_APP_TENANT as string,
  acsEndpoint: process.env.REACT_APP_ACS_ENDPOINT as string,
};

const App = () => {
  const [entityState, setEntityState] = useState<EntityState | undefined>();
  const [entityId, setEntityId] = useState("");
  const [idToken, setToken] = useState("");

  // This function is triggered when the select changes
  const selectChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    setEntityId(event.target.value);
  };

  useEffect(() => {
    const embedChat = new MSTeamsExt.EmbeddedChat(config);
    const data = embedChat.PrintName("Hello from React App!") as string;
    setEntityState(data);
    // if (entityId !== "") {
    //   embedChat.getEntityMapping(entityId).then((data) => {
    //     setEntityState(data);
    //   });
    // }
  }, [entityId, entityState, idToken]);

  return (
    <div className="App">
      <div>
        <select title="Entity Id" onChange={selectChange}>
          <option selected disabled>
            Choose Entity Id
          </option>
          <option value="0001">0001</option>
          <option value="0002">0002</option>
          <option value="0003">0003</option>
          <option value="0004">0004</option>
          <option value="0005">0005</option>
        </select>
      </div>
      <div className="container">
        <table>
          <tr>
            <th>Entity Id</th>
            <td>{entityState?.entityId}</td>
          </tr>
          <tr>
            <th>ACS User Id</th>
            <td>{entityState?.acsUserId}</td>
          </tr>
        </table>
      </div>
    </div>
  );
};

export default App;
