using Microsoft.Graph;
using Azure.Identity;

namespace Microsoft.Teams.EmbeddedChat.Services
{
    public class GraphService
    {
        private readonly GraphServiceClient _graphServiceClient;
        /// <summary>
        /// Constructor: create graph service client 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// /// <param name="tenantId"></param>
        public GraphService(string clientId, string clientSecret, string tenantId)
        {
            // The client credentials flow requires that you request the
            // /.default scope, and preconfigure your permissions on the
            // app registration in Azure. An administrator must grant consent
            // to those permissions beforehand.
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            // Multi-tenant apps can use "common",
            // single-tenant apps must use the tenant ID from the Azure portal
            // using Azure.Identity;
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // https://docs.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret, options);

            _graphServiceClient = new GraphServiceClient(clientSecretCredential, scopes);
        }
        /// <summary>
        /// create GraphServiceClient
        /// </summary>
        /// <returns>GraphServiceClient</returns>
        public GraphServiceClient GetGraphServiceClient()
        {
            return _graphServiceClient;
        }



    }

}
