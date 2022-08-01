output "health_check" {
  value = "https://${azurerm_function_app.this.name}.azurewebsites.net/api/${local.function_health_check_name}"
}

output "function_app_name" {
  value = azurerm_function_app.this.name
  description = "Deployed function app name"
}

output "function_app_default_hostname" {
  value = azurerm_function_app.this.default_hostname
  description = "Deployed function app hostname"
}

output "storage_account_name" {
  value = azurerm_storage_account.this.name
  description = "Storage Account Name"
}

output "cdn_endpoint" {
  value = azurerm_cdn_endpoint.this.fqdn
  description = "The Fully Qualified Domain Name of the CDN Endpoint"
}
