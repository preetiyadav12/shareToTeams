variable "location" {
  type = string
}

variable "org_name" {
  type = string
}

variable "project_name" {
  type = string
}


variable "project_type" {
  type = string
  default = "shared"
}

variable "function_publish_path" {
  type = string
}

variable "subscription_id" {
  type = string
}

variable "client_id" {
  type = string
}

variable "client_secret" {
  type = string
}

variable "tenant_id" {
  type = string
}

variable "service_principal_object_id" {
  type = string
}

variable "sas_start_date" {
  type = string
}

variable "sas_end_date" {
  type = string
}


variable "resource_group" {
  type        = string
  description = "Name of the resource group to deploy the resources to"
  default     = "rg"
}

variable "storage_account" {
  type        = string
  description = "Name of the storage account to use that is required by Azure Functions"
  default     = "sa"
}

variable "storage_container_name" {
  type    = string
  default = "web"

  validation {
    condition     = length(var.storage_container_name) > 2 && length(var.storage_container_name) < 63 && can(regex("^[a-z0-9][a-z0-9-]*$", var.storage_container_name))
    error_message = "Invalid storage account name, see documentation: https://docs.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata#container-names."
  }
}

variable "table_name" {
  type    = string
  default = "Entities"
}

variable "auth_authority" {
  type    = string
  default = "https://login.microsoftonline.com"
}


variable "keyvault" {
  type    = string
  default = "kv"
}

variable "app_config" {
  type    = string
  default = "config"
}

variable "service_name" {
  type    = string
  default = "api"
}

variable "acs_name" {
  type    = string
  default = "acs"
}

variable "app_settings" {
  type        = map(any)
  description = "Other app settings to set in app service"
  default     = {}
}

variable "log_analytics_workspace" {
  type        = string
  description = "Name of the shared Log analytics Workspace resource"
  default     = "logws"
}

variable "application_insights" {
  type        = string
  description = "Name of the shared application insights resource"
  default     = "appins"
}

variable "application_insights_type" {
  type        = string
  description = "Specifies the type of Application Insights to create. Please note these values are case sensitive"
  default     = "web"
}

variable "app_service_plan" {
  type        = string
  description = "Name of the app service plan that will host all the web apps and functions"
  default     = "appplan"
}

variable "swap_mode" {
  type    = string
  default = "auto_swap"

  validation {
    condition     = contains(["auto_swap", "swap_and_stop", "none"], var.swap_mode)
    error_message = "Valid options for swap_mode are 'auto_swap', 'swap_and_stop' or 'none'."
  }
}


variable "front_door_profile_name" {
  type        = string
  description = "Name of the Azure Front Door Profile"
  default     = "fdp"
}
