# Sent Message Flow Sequence Diagram

``` mermaid

sequenceDiagram
    participant User
    participant 3rd Party App
    participant Embed Script
    participant Graph API
    Participant Microsoft Teams
    participant Teams User    
    

    activate User
    activate 3rd Party App
    User->>3rd Party App:Send Message details
    activate Embed Script
    3rd Party App->> Embed Script:Send Message details
    activate Graph API
    Embed Script ->>Graph API:POST the message on chatThreadID using graphToken
    activate Microsoft Teams
    Graph API ->> Microsoft Teams: Send/POST Message to participants on Microsoft Team
    activate Teams User
    Microsoft Teams->>Teams User: render message for User
    deactivate Teams User
    deactivate Microsoft Teams
    Graph API -->> Embed Script: Response:Message posted successfully
    Embed Script ->> Graph API:Get profile photo/details
    Graph API -->>Embed Script: Response: Profiles photo/details
    deactivate Graph API
    Embed Script-->>3rd Party App:Response message posted successfully along with profile pic
    deactivate Embed Script
    3rd Party App-->>User: Render message for user
    deactivate 3rd Party App
    deactivate User

```


    
    
    
    
    
    
    
   
    
    