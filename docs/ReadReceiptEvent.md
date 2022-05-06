## Read Receipt event for a message

```mermaid


sequenceDiagram
    participant User
    participant 3rd Party App
    participant Embed Script
    participant Azure Function
    participant Azure Storage
    participant ACS

    
    
    activate Embed Script
    Embed Script-->>ACS: Subscribe and listen to event readReceiptReceived
    ACS ->>Embed Script:readReceiptReceived event is published(id, topic, subject)
    activate Azure Function
    Embed Script ->>Azure Function:UPDATE Message in storage via Azure Function
    activate Azure Storage
    Azure Function ->> Azure Storage: UPDATE Message read
    Azure Storage -->>Azure Function:Response - Message UPDATED
    deactivate Azure Storage
    Azure Function -->> Embed Script: Response - Message UPDATED
    deactivate Azure Function
    activate 3rd Party App
    Embed Script->>3rd Party App: updated message state 
    deactivate Embed Script
    activate User
    3rd Party App->> User: Display Message has been read
    deactivate 3rd Party App
    deactivate User

    
    ```
    
