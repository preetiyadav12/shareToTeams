## Typing Indicator event for a message

```mermaid


sequenceDiagram
    participant User
    participant 3rd Party App
    participant Embed Script
    participant ACS

    
    
    activate Embed Script
    Embed Script-->>ACS: Subscribe and listen to event Microsoft.Communication.TypingIndicatorReceived
    ACS ->>Embed Script:Microsoft.Communication.typingIndicatorReceived event (id, topic, subject, senderId)
    activate 3rd Party App
    Embed Script->>3rd Party App: Typing Indicator details 
    deactivate Embed Script
    activate User
    3rd Party App->> User: Display User x is typing
    deactivate 3rd Party App
    deactivate User

    
    ```
    
