# Download the code

Find the project for the embedded chat on [Azure Devops](https://dev.azure.com/csedevs/EmbeddedChats)

# Overview

The Sample app that features embedded chat control that syncs with Teams group chats.

# Prerequisites

* (A) An Azure account with an active subscription, You'll need to record your Application details registered in Azure Active Directory.
* (B) Register a embedded chat application in Azure Active Directory, Azure AD applications in the Microsoft identity platform can provide authentication and authorization services for applications and its users. For applications to perform identity and access management (IAM), they need to be registered with the Microsoft identity platform.
Register a single-tenant Azure AD application with a client secret in your tenant.
1. Log in to the Azure Portal for your subscription, open the Azure Active Directory panel, and go to the "[App registrations](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps)" blade.
2. Click on "New registration", and create an Azure AD application.

* Name: The name of your application. If you are following the sample for a default deployment, we recommend "Embedded Chat".
> Note: This name is not visible to users.
* Supported account types: Select "Accounts in this organizational directory only  (Single tenant)
)"
3.  Click on "Register". You'll be taken to the app's "Overview" page.
4. From the "Overview" page:
* Copy the Application (client) ID and note it down as the "AppId"
* Copy the Directory (tenant) ID and note it down as the "TenantId".
* Verify that Supported account types is set to "My organization only".
5. On the side rail under the "Manage" section, navigate to the "Certificates & secrets" section. Under "Client secrets", click on "New client secret".
* Add a Description for the secret.
* Choose when the secret will expire.
* Click "Add".
6. Once the client secret is created, copy its Value and note it down as the "AppPassword" we will need it later.
7. Navigate to and click on API permissions menu item on the left to open the page where access to the APIs needed by your application will be defined.
* (a) Click on Add a permission.
* (b) Select Microsoft Graph.
* (c) Select Delegated permissions for the type of permissions required by your app.
* (d) On the permission list, scroll to Chat and expand it, then check Chat.ReadWrite, same way look for User group and expand it, then check User.Read and User.ReadBasic.All. We also need (OpenId permissions) email, offline_access, openid and
profile pemrissions.

* (C) An Azure Communication Services resource, You'll need to record your resource connection string.

* (D) An Azure Cosmos DB resource, You'll need to record Connection String and Database name.

* (E) Visual Studio Code [Stable Build](https://code.visualstudio.com/Download)

## Before running the sample for the first time

- Open an instance of PowerShell, Windows Terminal, Command Prompt or equivalent and navigate to the directory that you'd like to clone the sample to.

``` git clone https://csedevs@dev.azure.com/csedevs/EmbeddedChats/_git/EmbeddedChats ```

- Get the ACS Connection String, AppId, AppPassword, TenantId, HostDomain, Azure Cosmos ConnectionString and Database name from the Azure portal. Once you have these value  add these to the \spikes\EntityMapping\appsettings.json file. 

## Local Run

Install dependencies, build and run the project
