﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat.Diagnostics
{
    public class DiagnosticsApi
    {
        [FunctionName(Constants.GetOrchestrationStatus)]
        public async Task<IActionResult> Diagnostics(
         [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "diagnostics/status/{id}")] HttpRequest req,
         string id,
         [DurableClient] IDurableOrchestrationClient starter,
         ILogger log)
        {
            if (string.IsNullOrEmpty(id))
                return new BadRequestObjectResult(new Exception("Must provide `instanceId` in the request!"));

            log.LogInformation($"Started DiagnosticsApi with ID = '{id}'.");

            var data = await starter.GetStatusAsync(id, true);
            return new OkObjectResult(data);
        }

        [FunctionName(Constants.TerminateOrchestration)]
        public async Task<IActionResult> TerminateOrchestration(
         [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "diagnostics/delete/{id}")] HttpRequest req,
         string id,
         [DurableClient] IDurableOrchestrationClient starter,
         ILogger log)
        {
            if (string.IsNullOrEmpty(id))
                return new BadRequestObjectResult(new Exception("Must provide the orchestration instance Id in the request!"));

            log.LogInformation($"Started DiagnosticsApi Terminating Orchestration with ID = '{id}'.");

            await starter.TerminateAsync(id, "User Request");
            return new OkResult();
        }

        [FunctionName(Constants.GetCompletedFlows)]
        public async Task<IActionResult> GetCompletedFlows(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "diagnostics/flows/completed")] HttpRequest req,
        [DurableClient] IDurableOrchestrationClient client,
        ILogger log)
        {
            var runtimeStatus = new List<OrchestrationRuntimeStatus> {
                OrchestrationRuntimeStatus.Completed
            };

            return await FindOrchestrations(req, client, runtimeStatus,
                DateTime.UtcNow.AddDays(GetDays(req)),
                DateTime.UtcNow, true);
        }

        [FunctionName(Constants.GetNotCompletedFlows)]
        public async Task<IActionResult> GetNotCompletedFlows(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "diagnostics/flows/notcompleted")] HttpRequest req,
        [DurableClient] IDurableOrchestrationClient client,
        ILogger log)
        {
            var runtimeStatus = new List<OrchestrationRuntimeStatus> {
                OrchestrationRuntimeStatus.Canceled,
                OrchestrationRuntimeStatus.ContinuedAsNew,
                OrchestrationRuntimeStatus.Failed,
                OrchestrationRuntimeStatus.Pending,
                OrchestrationRuntimeStatus.Terminated
            };

            return await FindOrchestrations(req, client, runtimeStatus,
                DateTime.UtcNow.AddDays(GetDays(req)),
                DateTime.UtcNow, true);
        }

        [FunctionName(Constants.GetAllFlows)]
        public async Task<IActionResult> GetAllFlows(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "diagnostics/flows")] HttpRequest req,
        [DurableClient] IDurableOrchestrationClient client,
        ILogger log)
        {
            var runtimeStatus = new List<OrchestrationRuntimeStatus> {
                OrchestrationRuntimeStatus.Running,
                OrchestrationRuntimeStatus.Canceled,
                OrchestrationRuntimeStatus.ContinuedAsNew,
                OrchestrationRuntimeStatus.Failed,
                OrchestrationRuntimeStatus.Pending,
                OrchestrationRuntimeStatus.Terminated,
                OrchestrationRuntimeStatus.Completed
            };

            return await FindOrchestrations(req, client, runtimeStatus,
                DateTime.UtcNow.AddDays(GetDays(req)),
                DateTime.UtcNow, true);
        }

        private async Task<IActionResult> FindOrchestrations(
            HttpRequest req,
            IDurableOrchestrationClient client,
            IEnumerable<OrchestrationRuntimeStatus> runtimeStatus,
            DateTime from,
            DateTime to,
            bool showInput = false)
        {
            // Define the cancellation token.
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            var instances = await client.ListInstancesAsync(
                new OrchestrationStatusQueryCondition
                {
                    CreatedTimeFrom = from,
                    CreatedTimeTo = to,
                    RuntimeStatus = runtimeStatus,
                    ShowInput = showInput
                },
                token
            );

            return new OkObjectResult(instances);
        }

        private static int GetDays(HttpRequest req)
        {
            string daysString = req.Query["days"];
            if (!string.IsNullOrEmpty(daysString))
            {
                var ok = int.TryParse(daysString, out int days);
                if (!ok)
                {
                    return -1;
                }
                return -days;
            }

            return -1;
        }
    }
}