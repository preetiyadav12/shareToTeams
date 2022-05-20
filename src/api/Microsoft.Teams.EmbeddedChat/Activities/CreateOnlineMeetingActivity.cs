using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System.Threading.Tasks;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Services;
using System;
using System.Collections.Generic;


namespace Microsoft.Teams.EmbeddedChat.Activities
{
    public class CreateOnlineMeetingActivity
    {
        
        private readonly AppSettings _appConfiguration;

        public CreateOnlineMeetingActivity(IOptions<AppSettings> configuration, IDurableActivityContext context, ILogger log)
        {
            _appConfiguration = configuration.Value;


        }

        /// <summary>
        /// The Activity to create online meeting
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
    }

}
