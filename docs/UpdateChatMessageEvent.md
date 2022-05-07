## Update event for a message

```mermaid


sequenceDiagram
    participant User
    participant 3rd Party App
    participant Embed Script
    participant Azure Function
    participant Azure Storage
    participant ACS

    
    
    activate Embed Script
    Embed Script-->>ACS: Subscribe and listen to event Microsoft.Communication.ChatMessageEdited
    ACS ->>Embed Script:Microsoft.Communication.ChatMessageEdited event (id, topic, subject, senderId)
    activate Azure Function
    Embed Script ->>Azure Function:UPDATE Message detail in storage via Azure Function
    activate Azure Storage
    Azure Function ->> Azure Storage: UPDATE Message details
    Azure Storage -->>Azure Function:Response - Message details Updated
    deactivate Azure Storage
    Azure Function -->> Embed Script: Response - Message details Updated
    deactivate Azure Function
    activate 3rd Party App
    Embed Script->>3rd Party App: updated message details 
    deactivate Embed Script
    activate User
    3rd Party App->> User: Display Message Edited
    deactivate 3rd Party App
    deactivate User
    
    ```
    
