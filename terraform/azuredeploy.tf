data "azurerm_client_config" "current" {}

# Create a random app configuration name
resource "random_id" "config" {
  byte_length  = 4
}

locals {
  resource_group_name           = join("-", [var.org_name, var.project_name, terraform.workspace, var.resource_group])
  storage_account_name          = join("", [var.org_name, var.project_name, terraform.workspace, var.storage_account])
  keyvault_name                 = join("-", [var.org_name, var.project_name, terraform.workspace, var.keyvault])
  log_analytics_name            = join("-", [var.org_name, var.project_name, terraform.workspace, var.log_analytics_workspace])
  application_insights_name     = join("-", [var.org_name, var.project_name, terraform.workspace, var.application_insights])
  app_service_plan_name         = join("-", [var.org_name, var.project_name, terraform.workspace, var.app_service_plan])
  app_config_name               = join("-", [var.org_name, var.project_name, terraform.workspace, var.app_config, random_id.config.hex ])
  function_app_name             = join("-", [var.org_name, var.project_name, terraform.workspace, var.service_name])
  acs_name                      = join("-", [var.org_name, var.project_name, terraform.workspace, var.acs_name])
  front_door_profile_name       = join("-", [var.org_name, var.project_name, terraform.workspace, var.front_door_profile_name])
  cdn_name                      = join("", [var.org_name, var.project_name, terraform.workspace, "cdn"])
  function_compiled_source_path = var.function_publish_path
  function_health_check_name    = "health"
  subscription_scope            = join("", ["/subscriptions/", var.subscription_id])
}

# Create the main resource group
resource "azurerm_resource_group" "this" {
  name            = local.resource_group_name
  location        = var.location
  tags  = {
    Environment   = terraform.workspace
  } 
}

# Create a zip file of our compiled function code
data "archive_file" "function_releases" {
  type            = "zip"
  source_dir      = local.function_compiled_source_path
  output_path     = "function_release.zip"
}

# Create an Azure storage account to host the function releases, function executions and triggers
resource "azurerm_storage_account" "this" {
  name                     = local.storage_account_name
  resource_group_name      = azurerm_resource_group.this.name
  location                 = azurerm_resource_group.this.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  # allow_nested_items_to_be_public = false

  # network_rules {
  #   default_action  = "Deny"
  # }

  tags  = {
    environment   = terraform.workspace
  }  
}

# Create a storage container for our function releases
resource "azurerm_storage_container" "deployments" {
  name                    = "function-releases"
  storage_account_name    = azurerm_storage_account.this.name
  container_access_type   = "private"
}


# Create a storage container for bundle javascript data files
resource "azurerm_storage_container" "this" {
  name                  = var.storage_container_name
  storage_account_name  = azurerm_storage_account.this.name
  container_access_type = "private"
}


# Create a storage container for public Auth web files
resource "azurerm_storage_container" "auth" {
  name                  = "auth${var.storage_container_name}"
  storage_account_name  = azurerm_storage_account.this.name
  container_access_type = "container"
}


# Upload the zip file to our storage container.
resource "azurerm_storage_blob" "function_blob" {
  # The name of the file will be "filehash.zip" where file hash is the SHA256 hash of the file.
  name                   = "${filesha256(data.archive_file.function_releases.output_path)}.zip"
  source                 = data.archive_file.function_releases.output_path
  storage_account_name   = azurerm_storage_account.this.name
  storage_container_name = azurerm_storage_container.deployments.name
  type                   = "Block"
}


# Create a read-only SAS for the Blob
data "azurerm_storage_account_blob_container_sas" "ui_files" {
  connection_string     = azurerm_storage_account.this.primary_connection_string
  container_name        = azurerm_storage_container.this.name
  https_only            = true

  start                 = var.sas_start_date
  expiry                = var.sas_end_date

  permissions {
    read   = true
    add    = false
    create = false
    write  = false
    delete = false
    list   = false
  }
}


# Create a read-only SAS for the Blob
data "azurerm_storage_account_blob_container_sas" "function_blob" {
  connection_string = azurerm_storage_account.this.primary_connection_string
  container_name    = azurerm_storage_container.deployments.name
  https_only        = true

  start             = var.sas_start_date
  expiry            = var.sas_end_date

  permissions {
    read   = true
    add    = false
    create = false
    write  = false
    delete = false
    list   = false
  }
}

resource "azurerm_key_vault" "this" {
  name                        = local.keyvault_name
  location                    = azurerm_resource_group.this.location
  resource_group_name         = azurerm_resource_group.this.name
  enabled_for_disk_encryption = true
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = false

  sku_name                    = "standard"

  access_policy {
    tenant_id       = data.azurerm_client_config.current.tenant_id
    object_id       = data.azurerm_client_config.current.object_id

    key_permissions = [
      "Create",
      "Get",
      "List",
      "Purge",
      "Recover"
    ]

    secret_permissions = [
      "Get",
      "Set",
      "List",
      "Delete",
      "Purge",
      "Recover"
    ]

    storage_permissions = [
      "Get",
    ]
  }
}


resource "azurerm_key_vault_secret" "this" {
  name            = "BlobSAS"
  value           = data.azurerm_storage_account_blob_container_sas.ui_files.sas
  key_vault_id    = azurerm_key_vault.this.id
}


resource "azurerm_cdn_profile" "this" {
  name                  = local.front_door_profile_name
  location              = "global"
  resource_group_name   = azurerm_resource_group.this.name
  sku                   = "Standard_Microsoft"

  tags  = {
    environment     = terraform.workspace
  }  

}

resource "azurerm_cdn_endpoint" "this" {
  name                          = local.cdn_name
  profile_name                  = azurerm_cdn_profile.this.name
  location                      = azurerm_cdn_profile.this.location
  resource_group_name           = azurerm_resource_group.this.name
  is_http_allowed               = true
  is_https_allowed              = true
  querystring_caching_behaviour = "UseQueryString"

  origin {
    name        = "${local.cdn_name}-origin"
    host_name   = azurerm_storage_account.this.primary_blob_host
  }

  origin_host_header = azurerm_storage_account.this.primary_blob_host
  origin_path = "/${var.storage_container_name}"
  content_types_to_compress = [ "text/html", "text/javascript", "text/css" ]
  is_compression_enabled    = true

  delivery_rule {
    name    = "EnforceHttps"
    order   = 1

    request_scheme_condition {
      operator      = "Equal"
      match_values  = ["HTTP"]
    }

    url_redirect_action {
      redirect_type = "Found"
      protocol      = "Https"
    }
  }

  delivery_rule {
    name    = "authhtml"
    order   = 2

    url_file_name_condition {
      operator      = "EndsWith"
      match_values  = [ "auth.html" ]
      transforms    = [ "Lowercase" ]
    }

    url_rewrite_action {
      source_pattern  = "/auth.html"
      destination     = "/auth.html${data.azurerm_storage_account_blob_container_sas.ui_files.sas}" 
    }
  }

  delivery_rule {
    name    = "authjs"
    order   = 3

    url_file_name_condition {
      operator      = "EndsWith"
      match_values  = [ "auth.js" ]
      transforms    = [ "Lowercase" ]
    }

    url_rewrite_action {
      source_pattern  = "/auth.js"
      destination     = "/auth.js${data.azurerm_storage_account_blob_container_sas.ui_files.sas}" 
    }
  }

  delivery_rule {
    name    = "embedChatjs"
    order   = 4

    url_file_name_condition {
      operator      = "EndsWith"
      match_values  = [ "embedchat.js" ]
      transforms    = [ "Lowercase" ]
    }

    url_rewrite_action {
      source_pattern  = "/embedChat.js"
      destination     = "/embedChat.js${data.azurerm_storage_account_blob_container_sas.ui_files.sas}" 
    }
  }

  delivery_rule {
    name    = "embedChatcss"
    order   = 5

    url_file_name_condition {
      operator      = "EndsWith"
      match_values  = [ "embedchat.css" ]
      transforms    = [ "Lowercase" ]
    }

    url_rewrite_action {
      source_pattern  = "/embedChat.css"
      destination     = "/embedChat.css${data.azurerm_storage_account_blob_container_sas.ui_files.sas}" 
    }
  }

}





# create Azure Log Analytics Workspace to be used by Azure AppInsights
resource "azurerm_log_analytics_workspace" "this" {
  name                  = local.log_analytics_name
  location              = azurerm_resource_group.this.location
  resource_group_name   = azurerm_resource_group.this.name
  sku                   = "PerGB2018"
  retention_in_days     = 30
}

# # create Azure AppInsights
resource "azurerm_application_insights" "this" {
  name                  = local.application_insights_name
  location              = azurerm_resource_group.this.location
  resource_group_name   = azurerm_resource_group.this.name
  workspace_id          = azurerm_log_analytics_workspace.this.id
  application_type      = var.application_insights_type

  depends_on = [
    azurerm_log_analytics_workspace.this
  ]
}

# create Azure App Configuration store
resource "azurerm_app_configuration" "this" {
  name                  = local.app_config_name
  resource_group_name   = azurerm_resource_group.this.name
  location              = azurerm_resource_group.this.location
  sku                   = "standard"
  identity {
    type    = "SystemAssigned"
  }   
}

# # create Azure Communication Service
resource "azurerm_communication_service" "this" {
  name                  = local.acs_name
  resource_group_name   = azurerm_resource_group.this.name
  data_location         = "United States"
}

# Create an App Service plan for Azure Function
resource "azurerm_service_plan" "this" {
  name                  = local.app_service_plan_name
  resource_group_name   = azurerm_resource_group.this.name
  location              = azurerm_resource_group.this.location
  os_type               = "Linux"
  sku_name              = "Y1"      
}


# Create Function App
resource "azurerm_function_app" "this" {
  name                          = local.function_app_name
  resource_group_name           = local.resource_group_name
  location                      = azurerm_resource_group.this.location
  app_service_plan_id           = azurerm_service_plan.this.id
  storage_account_name          = azurerm_storage_account.this.name
  storage_account_access_key    = azurerm_storage_account.this.primary_access_key
  version                       = "~4"
  os_type                       = "linux"

  app_settings = {
    "APPHOSTING_ENVIRONMENT"                    = terraform.workspace
    "APPINSIGHTS_INSTRUMENTATIONKEY"            = azurerm_application_insights.this.instrumentation_key
    "APPLICATIONINSIGHTS_CONNECTION_STRING"     = azurerm_application_insights.this.connection_string
    "APP_CONFIG_CONNECTION_STRING"              = azurerm_app_configuration.this.primary_read_key[0].connection_string
    "APP_CONFIG_ENDPOINT"                       = azurerm_app_configuration.this.endpoint
    FUNCTIONS_WORKER_RUNTIME                    = "dotnet-isolated"
    FUNCTIONS_EXTENSION_VERSION                 = "~4"
    HASH                                        = "${base64encode(filesha256(data.archive_file.function_releases.output_path))}" 
    WEBSITE_RUN_FROM_PACKAGE                    = "https://${azurerm_storage_account.this.name}.blob.core.windows.net/${azurerm_storage_container.deployments.name}/${azurerm_storage_blob.function_blob.name}${data.azurerm_storage_account_blob_container_sas.function_blob.sas}"
  }

  site_config {
    dotnet_framework_version = "v6.0"

    health_check_path           = "/api/${local.function_health_check_name}"
    use_32_bit_worker_process   = false

    cors {
        allowed_origins = ["*"]
    }
  }

  identity {
    type  = "SystemAssigned"
  }

  depends_on = [
     azurerm_app_configuration.this,
     data.azurerm_storage_account_blob_container_sas.function_blob
  ]

}

# Allow our function's managed identity to have read access to the app configuration
resource "azurerm_role_assignment" "appconf_datareader" {
  scope                 = azurerm_app_configuration.this.id
  role_definition_name  = "App Configuration Data Reader"
  #principal_id          = data.azurerm_client_config.current.object_id
  principal_id          = azurerm_function_app.this.identity.0.principal_id 

  depends_on = [
    azurerm_app_configuration.this,
    azurerm_function_app.this
  ]
}


# Add Configuration Keys
# Settings:Sentinel
resource "azurerm_app_configuration_key" "sentinel" {
  configuration_store_id    = azurerm_app_configuration.this.id
  key                       = "Settings:Sentinel"

  depends_on = [
    azurerm_app_configuration.this
  ]
}


# AppSettings:StorageConnectionString
resource "azurerm_app_configuration_key" "connection_string" {
  configuration_store_id    = azurerm_app_configuration.this.id
  key                       = "AppSettings:StorageConnectionString"
  label                     = terraform.workspace
  value                     = azurerm_storage_account.this.primary_connection_string

  depends_on = [
    azurerm_app_configuration.this,
    azurerm_app_configuration_key.sentinel
  ]
}


# AppSettings:AcsConnectionString
resource "azurerm_app_configuration_key" "acs_connection_string" {
  configuration_store_id    = azurerm_app_configuration.this.id
  key                       = "AppSettings:AcsConnectionString"
  label                     = terraform.workspace
  value                     = azurerm_communication_service.this.primary_connection_string

  depends_on = [
    azurerm_app_configuration.this,
    azurerm_app_configuration_key.connection_string
  ]
}


# AppSettings:AzureTableName
resource "azurerm_app_configuration_key" "azure_table" {
  configuration_store_id    = azurerm_app_configuration.this.id
  key                       = "AppSettings:AzureTableName"
  label                     = terraform.workspace
  value                     = var.table_name

  depends_on = [
    azurerm_app_configuration.this,
    azurerm_app_configuration_key.acs_connection_string
  ]
}

# AppSettings:AuthenticationAuthority
resource "azurerm_app_configuration_key" "auth_authority" {
  configuration_store_id    = azurerm_app_configuration.this.id
  key                       = "AppSettings:AuthenticationAuthority"
  value                     = var.auth_authority

  depends_on = [
    azurerm_app_configuration.this,
    azurerm_app_configuration_key.azure_table
  ]
}

# AppSettings:ClientId
resource "azurerm_app_configuration_key" "client_id" {
  configuration_store_id    = azurerm_app_configuration.this.id
  key                       = "AppSettings:ClientId"
  label                     = terraform.workspace
  value                     = var.client_id

  depends_on  = [
    azurerm_app_configuration.this,
    azurerm_app_configuration_key.auth_authority
  ]
}


# AppSettings:ClientSecret
resource "azurerm_app_configuration_key" "client_secret" {
  configuration_store_id    = azurerm_app_configuration.this.id
  key                       = "AppSettings:ClientSecret"
  # type                      = "vault"
  label                     = terraform.workspace
  value                     = var.client_secret 
  # vault_key_reference       = data.azurerm_key_vault_secret.client_secret.versionless_id

  depends_on  = [
    azurerm_app_configuration.this,
    azurerm_app_configuration_key.client_id
  ]
}


# AppSettings:TenantId
resource "azurerm_app_configuration_key" "tenant_id" {
  configuration_store_id    = azurerm_app_configuration.this.id
  key                       = "AppSettings:TenantId"
  label                     = terraform.workspace
  value                     = var.tenant_id

  depends_on  = [
    azurerm_app_configuration.this,
    azurerm_app_configuration_key.client_secret
  ]
}



