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
            ApiOperation operation,
            ChatInfoRequest requestData,
            HttpRequestMessage request,
            IDurableOrchestrationClient client)
        {
            _log.LogInformation($"Started orchestration Instance Id: '{requestData.Id}' for the Api Operation Id: {operation} and Entity ID: '{requestData.EntityId}'.");
            var orchestrationRequest = new OrchestrationRequest { Operation = operation, Request = requestData };

            requestData.Id = await client.StartNewAsync(Constants.Orchestration, orchestrationRequest);

            TimeSpan timeout = TimeSpan.FromSeconds(Constants.Timeout);
            TimeSpan retryInterval = TimeSpan.FromSeconds(Constants.RetryInterval);

            // Execute the orchestration and wait for the completion
            await client.WaitForCompletionOrCreateCheckStatusResponseAsync(
                request,
                requestData.Id,
                timeout,
                retryInterval,
                true);

            // Retrieve the status of the completed orchestration
            var data = await client.GetStatusAsync(requestData.Id);

            // timeout
            if (data.RuntimeStatus != OrchestrationRuntimeStatus.Completed)
            {
                await client.TerminateAsync(requestData.Id, "Timeout. Something took too long");
                return new ContentResult()
                {
                    Content = "{ error: \"Timeout. Something took too long\" }",
                    ContentType = "application/json",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
            var completeResponseData = data.Output.ToObject<EntityState>();

            if (completeResponseData == null)
                return new NotFoundResult();

            if (!completeResponseData.IsSuccess)
            {
                var content = new ContentResult
                {
                    Content = completeResponseData.Owner.UserPrincipalName,
                    ContentType = "application/json",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
                return new OkObjectResult(content);
            }

            return new OkObjectResult(completeResponseData);
        }
    }
}
