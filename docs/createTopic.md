## Create Topic, while creating chatThread

```mermaid


sequenceDiagram
    participant User
    participant 3rd Party App
    participant Embed Script
    participant MSAL
    participant Azure Functions
    participant Azure Storage
    participant ACS

    
    activate User
    activate 3rd Party App
    User->>3rd Party App:Selects an Entity
    activate Embed Script
    3rd Party App->>Embed Script:Invokes Embed Script
    activate MSAL
    Embed Script->>MSAL:Authenticate User
    MSAL-->>Embed Script:Return Token
    deactivate MSAL
    activate ACS
    Embed Script->>ACS:Create Chat Client
    ACS-->>Embed Script:Response-Chat Client Created
    activate Azure Functions
    Embed Script->>Azure Functions:Get EntityID details & participant details from Azure stoage via Azure Functions
    activate Azure Storage
    Azure Functions->>Azure Storage:Get EntityID details & participant details
    Azure Storage-->>Azure Functions:Return EntityID details & participant details
    Azure Functions-->>Embed Script:Return EntityId details & participant details
    Embed Script->>Embed Script: using EntityID details, create Topic
    Embed Script->>ACS:Create Chat ThreadID by passing participant details + topic
    ACS -->>Embed Script:return created chatThread
    deactivate ACS
    Embed Script->>Azure Functions:Update chatThread details to Azure storage via Azure functions
    Azure Functions->>Azure Storage:Update chatThread details
    Azure Storage-->>Azure Functions:Success update of chatThread details
    deactivate Azure Storage
    Azure Functions-->>Embed Script:Success update of chatThread details
    deactivate Azure Functions
    Embed Script-->>3rd Party App:Embed Chat is created
    deactivate Embed Script
    deactivate 3rd Party App
    deactivate User
    
    ```

    ## Flow describe first time creation of ChatThread by pasisng topic & participants details, when starting a new chat

    ## For authetication details refer to authetication flow UML

    ## For chatThread details refer InitialiseThread UML 

    ## For add/remove Participant details refer to add/remove participants UML
    
