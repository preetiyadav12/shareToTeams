using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Teams.EmbeddedChat.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat
{
    public class Processing
    {
        private readonly ILogger<Processing> _log;
        
        public readonly AppSettings AppConfiguration;

        public Processing(ILoggerFactory loggerFactory, IOptions<AppSettings> configuration)
        {
            _log = loggerFactory.CreateLogger<Processing>();
            AppConfiguration = configuration.Value;
        }

        public async Task<IActionResult> ProcessFlow(
            EntityState requestData,
            HttpRequestMessage request,
            IDurableOrchestrationClient client)
        {
            requestData.RowKey = await client.StartNewAsync(Constants.Orchestration, requestData);
            _log.LogInformation($"Started orchestration Instance Id: '{requestData.RowKey}' for the Entity ID: '{requestData.EntityId}'.");

            TimeSpan timeout = TimeSpan.FromSeconds(Constants.Timeout);
            TimeSpan retryInterval = TimeSpan.FromSeconds(Constants.RetryInterval);

            // Execute the orchestration and wait for the completion
            await client.WaitForCompletionOrCreateCheckStatusResponseAsync(
                request,
                requestData.RowKey,
                timeout,
                retryInterval,
                true);

            // Retrieve the status of the completed orchestration
            var data = await client.GetStatusAsync(requestData.RowKey);

            // timeout
            if (data.RuntimeStatus != OrchestrationRuntimeStatus.Completed)
            {
                await client.TerminateAsync(requestData.RowKey, "Timeout. Something took too long");
                return new ContentResult()
                {
                    Content = "{ error: \"Timeout. Something took too long\" }",
                    ContentType = "application/json",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
            var completeResponseData = data.Output.ToObject<EntityState>();

            return new OkObjectResult(completeResponseData);
        }
    }
}
