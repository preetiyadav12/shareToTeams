# Problem Statement

Organizations want to embed Microsoft Teams chat into 3rd party applications at the entity level (ex: purchase order, resource request, help desk ticket)

## Key Requirements

The goal is to investigate and build a sample app that features embedded chat control that syncs with Teams group chats.
Key requirements are:

• Ability to initiate a group chat with team members
• Ability for team members who are on MS Teams to be able to chat with the application user in real-time
• Ability to form a chat centered around an entity in the app such as order number
• Chat names automatically created are unique and understandable

## Deployment to Azure

For deployment of this solution to Azure, we use GitHub Actions CI/CD workflow in combination with Terraform infrastructure deployment scripts.
To follow the steps to deploy your solution to Azure, please follow this [guidance](deploy/README.md)

## Development Locally

To be able to install this solution on a local machine and use development tools to test and make any changes in it, please follow this [guidance](src/embedchat/README.md)
