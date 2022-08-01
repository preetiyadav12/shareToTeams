using Azure.Communication.Identity;
using Dynamitey;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.EmbeddedChat.ACS;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Services;
using System;
using System.Threading.Tasks;


namespace Microsoft.Teams.EmbeddedChat.Activities;

public class AcsClientActivity
{
    private static string ManagedTeamsCallsScope = "https://auth.msft.communication.azure.com/Teams.ManageCalls";

    /// <summary>
    /// The Activity to create a new entity state
    /// </summary>
    /// <param name="context"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static async Task<ACSInfo> CreateACSClientAsync(
        ChatInfoRequest requestData,
        AppSettings appConfiguration,
        IGraphService graphService,
        ILogger logger)
    {
        logger.LogInformation($"Activity {nameof(CreateACSClientAsync)} has started.");

        try
        {
            // create ACS Communication Identity Client Service
            //var comClient = new CommServices(new Uri(appConfiguration.AcsEndpoint),
            //    new[] { CommunicationTokenScope.Chat, CommunicationTokenScope.VoIP });
            var comClient = new CommServices(appConfiguration.AcsConnectionString,
                new[] { CommunicationTokenScope.Chat, CommunicationTokenScope.VoIP });

            // Create user Id and ACS token
            var acsInfo = await comClient.CreateIdentityAndGetTokenAsync();

            // Exchange the user Azure AD Access token of the Teams User for a Communication Identity access token
            var commIdentityAccessToken = await GetCommunicationIdentityAccessToken(
                requestData.AccessToken,
                comClient,
                graphService,
                logger);

            // populate Ethe ACS User info
            return new ACSInfo()
            {
                AcsToken = acsInfo.accessToken,
                AcsUserId = acsInfo.userId,
                TokenExpiresOn = acsInfo.expiresOn.ToString("F"),
                CommIdentityToken = commIdentityAccessToken
            };
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            throw;
        }
    }


    /// <summary>
    /// Exchange the Azure AD access token of the Teams User for a Communication Identity access token
    /// </summary>
    /// <param name="userToken"></param>
    /// <param name="commServices"></param>
    /// <param name="graphService"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static async Task<string> GetCommunicationIdentityAccessToken(string userToken, CommServices commServices, IGraphService graphService, ILogger logger)
    {
        // Step 1: In the token exchange flow get a token for the Teams user by using Graph Service.
        var teamsUserAadToken = await graphService.GetTeamsUserAadToken(ManagedTeamsCallsScope, userToken, logger);

        // Step 2: Exchange the Azure AD access token of the Teams User for a Communication Identity access token
        return await commServices.GetTokenForTeamsUserAsync(teamsUserAadToken);
    }

}

