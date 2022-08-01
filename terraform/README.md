# Deploying an Azure Function with isolated dotNet mode and App Configuration

## Overview

This code creates an Azure Function, and then creates an App Configuration where the secret values stores within.

## Actions

The following actions are carried out:

1. Creates a Storage account to hold Embedded Chat resources as well as the App Function release artifacts.
2. Creates an Application Insights to monitor the performance of the App Function and help with troubleshooting.
3. Creates an App Configuration to hold Function app settings.
4. Stores all the secret values and app settings used by App Function.
5. Creates an Azure Communication Service component.
6. Creates an Azure Service Plan for App Function.
