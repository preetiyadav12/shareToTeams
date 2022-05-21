using Azure.Communication.Identity;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Teams.EmbeddedChat.ACS;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Microsoft.Teams.EmbeddedChat.Activities
{
	public class CreateAcsClientActivity
	{
		private readonly AppSettings _appConfiguration;

        public CreateAcsClientActivity(IOptions<AppSettings> configuration)
        {
            _appConfiguration = configuration.Value;
        }

        /// <summary>
        /// The Activity to create a new entity state
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.CreateACSClientActivity)]
        public async Task<ACSInfo> CreateACSClientActivity([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            log.LogInformation($"Activity {Constants.CreateACSClientActivity} has started.");

            try
            {
                // create ACS Communication Identity Client Service
                //var comClient = new CommServices(_appConfiguration.AcsConnectionString,
                //    new[] { CommunicationTokenScope.Chat, CommunicationTokenScope.VoIP });

                // create ACS Communication Identity Client Service
                var comClient = new CommServices(new Uri(_appConfiguration.AcsEndpoint),
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
                log.LogError(e.Message);
            }

            return null;
        }

    }
}

