## Delete event for a message

```mermaid


sequenceDiagram
    participant User
    participant 3rd Party App
    participant Embed Script
    participant Azure Function
    participant Azure Storage
    participant ACS

    
    
    activate Embed Script
    Embed Script-->>ACS: Subscribe and listen to event Microsoft.Communication.ChatMessageDeleted
    ACS ->>Embed Script:Microsoft.Communication.ChatMessageDeleted event (id, topic, subject, senderId for ex. x)
    activate Azure Function
    Embed Script ->>Azure Function:DELETE Message sent by sender x in storage via Azure Function
    activate Azure Storage
    Azure Function ->> Azure Storage: DELETE Message sent by sender x
    Azure Storage -->>Azure Function:Response - Message DELETED
    deactivate Azure Storage
    Azure Function -->> Embed Script: Response - Message DELETED
    deactivate Azure Function
    activate 3rd Party App
    Embed Script->>3rd Party App: deleted message details 
    deactivate Embed Script
    activate User
    3rd Party App->> User: Display Message has been deleted
    deactivate 3rd Party App
    deactivate User

    
    ```
    
