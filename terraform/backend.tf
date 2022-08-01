terraform {
  backend "azurerm" {
    resource_group_name   = "ms-embchat-dev-tfstate-rg"
    storage_account_name  = "msembchatdevtfstatesa"
    container_name        = "tfstate-dev2"
    key                   = "terraform.tfstate"
  }
}
