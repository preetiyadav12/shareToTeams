using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Teams.EmbeddedChat.Models;
using System.Collections.Generic;

namespace Microsoft.Teams.EmbeddedChat.Activities
{
    public class UpdateParticipantsActivity
    {
        private readonly AppSettings _appConfiguration;

        public UpdateParticipantsActivity(IOptions<AppSettings> configuration)
        {
            _appConfiguration = configuration.Value;
        }

        /// <summary>
        /// The Activity to update participants list
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.UpdateParticipantsActivity)]
        public IEnumerable<Participant> UpdateParticipantsAsync(
            [ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            // retrieves the entity state from the orchestration
            var participants = context.GetInput<IEnumerable<Participant>>();

            log.LogInformation($"Activity {Constants.UpdateParticipantsActivity} has started.");

            return participants;
        }
    }
}
