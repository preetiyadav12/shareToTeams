# Functional non-functional requirements 

#### User Experience

##### V1

- The chat experience needs to be available for any web based app
- A webapp developer must be able to provide the user with a chat box adjacent to the entity that the user wishes to chat about

##### V2

- Support thick client
- Support chatting from Teams on an entity, for which a chat for which already exists
- Support initiating a chat from within Teams on an entity

-----------------------------------------------------------------
#### Logging in

- User is assumed to be a Teams user (AAD)
- User should not be moved away from the webapp page, for the user signin (silently signin the user)
------------------------------------------------------------------
#### Properties of the object supported

##### Chat name

- A format for a chat name with the entity name 
- The chat name must be unique
- This name will appear as chat topic in Teams

##### Embedded chat configuration

Embedded chat initialization to support two start modes
- Auto, where the chat thread is automatically created
- Manual, a click to chat. This should be the default

##### Participants

- support an initialized list of participants
- Support participant add modes of:
    + Manual
    + Closed

##### Unique Entity ID 

##### DOM element

----------------------------------------------------
#### Non-functional requirements

##### Execptions to be supported

- Authentication 
- ACS 
- Graph
- If the ACS proxy user is accidentally removed by the user

##### Observability

- Event log into App Insights

##### Performance

- If not using federation, the cycle time should be sub second

##### Storage

- Table storage in a storage account
----------------------------------------------------------------------
