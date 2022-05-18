using Azure.Communication.Identity;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Teams.EmbeddedChat.ACS;
using Microsoft.Teams.EmbeddedChat.Models;

namespace Microsoft.Teams.EmbeddedChat.Activities
{
    public class CreateOnlineMeetingActivity
    {
        private readonly AppSettings _appConfiguration;

        public CreateOnlineMeetingActivity(IOptions<AppSettings> configuration)
        {
            _appConfiguration = configuration.Value;
        }

        /// <summary>
        /// The Activity to create online meeting
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.CreateOnlineMeetingActivity)]
        public string CreateOnlineMeetingAsync(
            [ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            // retrieves the entity state from the orchestration
            var entityState = context.GetInput<EntityState>();

            log.LogInformation($"Activity {Constants.UpdateParticipantsActivity} has started.");

            return "";
        }
    }
}
