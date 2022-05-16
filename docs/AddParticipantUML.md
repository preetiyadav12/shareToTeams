# Add User From Embedded Chat Control Flow

```mermaid
sequenceDiagram
    autonumber
    participant User
    participant Embedded Chat Control
    participant Azure Function
    User->>Embedded Chat Control: Click Participants icon
    Embedded Chat Control-->>User: Render list of participants
    User->>Embedded Chat Control: Click Add people
    Embedded Chat Control-->>User: Render control to search and add people
    loop For Every User
        Embedded Chat Control->>Azure Function: Search For a User
        Azure Function-->>Embedded Chat Control: Return List of Users
        User->>Embedded Chat Control: Select User
        Embedded Chat Control-->>Embedded Chat Control: Add selected User to input box
    end
    User->>Azure Function: Click Add button
    Azure Function->>Azure Function: Add all users to the chat
    Azure Function-->>Embedded Chat Control: Return response and Update with new participants
    
```