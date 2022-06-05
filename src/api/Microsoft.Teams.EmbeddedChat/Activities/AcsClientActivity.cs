using Azure.Communication.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.EmbeddedChat.ACS;
using Microsoft.Teams.EmbeddedChat.Models;
using System;
using System.Threading.Tasks;


namespace Microsoft.Teams.EmbeddedChat.Activities;

public class AcsClientActivity
{
    /// <summary>
    /// The Activity to create a new entity state
    /// </summary>
    /// <param name="context"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static async Task<ACSInfo> CreateACSClientAsync(AppSettings appConfiguration, ILogger logger)
    {
        logger.LogInformation($"Activity {nameof(CreateACSClientAsync)} has started.");

        try
        {
            // create ACS Communication Identity Client Service
            var comClient = new CommServices(new Uri(appConfiguration.AcsEndpoint),
                new[] { CommunicationTokenScope.Chat, CommunicationTokenScope.VoIP });

            // Create user Id and ACS token
            var acsInfo = await comClient.CreateIdentityAndGetTokenAsync();

            // populate Ethe ACS User info
            return new ACSInfo()
            {
                AcsToken = acsInfo.accessToken,
                AcsUserId = acsInfo.userId,
                TokenExpiresOn = acsInfo.expiresOn.ToString("F")
            };
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            throw;
        }
    }
}

