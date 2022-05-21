using Azure;
using Azure.Communication;
using Azure.Communication.Identity;
using Azure.Core;
using Azure.Identity;
using System;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat.ACS
{
    public class CommServices
    {
        private DefaultAzureCredential tokenCredential = new DefaultAzureCredential();
        private readonly CommunicationIdentityClient identityClient;
        private CommunicationTokenScope[] communicationTokenScopes;

        public CommServices(Uri resourceEndpoint, CommunicationTokenScope[] communicationTokenScopes)
        {
            this.identityClient = new CommunicationIdentityClient(resourceEndpoint, this.tokenCredential);
            this.communicationTokenScopes = communicationTokenScopes;
        }

        public CommServices(string connectionString, CommunicationTokenScope[] communicationTokenScopes)
        {
            this.identityClient = new CommunicationIdentityClient(connectionString);
            this.communicationTokenScopes = communicationTokenScopes;
        }

        public async Task<(string userId, string accessToken, DateTimeOffset expiresOn)> CreateIdentityAndGetTokenAsync()
        {
            // Create an identity
            var identityResponse = await this.identityClient.CreateUserAsync();
            var identity = identityResponse.Value;
            Console.WriteLine($"\nCreated an identity with ID: {identity.Id}");

            // Issue access tokens
            // Issue an access token with the "voip" scope for an identity
            var tokenResponse = await this.identityClient.GetTokenAsync(identity, scopes: this.communicationTokenScopes);

            // Get the token from the response
            var token = tokenResponse.Value.Token;
            var expiresOn = tokenResponse.Value.ExpiresOn;

            // Write the token details to the screen
            Console.WriteLine($"\nIssued an access token with {this.communicationTokenScopes.ToString()} scope that expires at {expiresOn}:");
            Console.WriteLine($"Token: {token}");

            return (identity.Id, token, expiresOn);
        }

        /// <summary>
        /// Refresh token
        /// </summary>
        /// <param name="acsUserId"></param>
        /// <returns></returns>
        public async Task<AccessToken> RefreshAccessToken(string acsUserId)
        {
            var identityToRefresh = new CommunicationUserIdentifier(acsUserId);
            var tokenResponse = await this.identityClient.GetTokenAsync(identityToRefresh, scopes: this.communicationTokenScopes);
            return tokenResponse.Value;
        }
    }
}
