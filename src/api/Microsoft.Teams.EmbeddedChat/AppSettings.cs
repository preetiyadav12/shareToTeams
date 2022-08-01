
namespace Microsoft.Teams.EmbeddedChat
{
    public class AppSettings
    {
        public string StorageConnectionString { get; set; }
        //public string AcsEndpoint { get; set; }
        public string AcsConnectionString { get; set; }
        public string AzureTableName { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
        public string AuthenticationAuthority { get; set; }

        public static (bool isInitialized, string errMessage) IsInitialized(AppSettings appSettings)
        {
            if (appSettings == null)
                return (false, "AppSettings DI was not properly initialized");
            if (appSettings.StorageConnectionString == null)
                return (false, "Can't get the 'StorageConnectionString' from AppConfig");
            //if (appSettings.AcsEndpoint == null)
            //    return (false, "Can't get the 'AcsEndpoint' from AppConfig");
            if (appSettings.AcsConnectionString == null)
                return (false, "Can't get the 'AcsConnectionString' from AppConfig");
            if (appSettings.AzureTableName == null)
                return (false, "Can't get the 'AzureTableName' from AppConfig");
            if (appSettings.ClientId == null)
                return (false, "Can't get the 'ClientId' from AppConfig");
            if (appSettings.ClientSecret == null)
                return (false, "Can't get the 'ClientSecret' from AppConfig");
            if (appSettings.TenantId == null)
                return (false, "Can't get the 'TenantId' from AppConfig");
            if (appSettings.AuthenticationAuthority == null)
                return (false, "Can't get the 'AuthenticationAuthority' from AppConfig");

            return (true, "");
        }
    }
}
