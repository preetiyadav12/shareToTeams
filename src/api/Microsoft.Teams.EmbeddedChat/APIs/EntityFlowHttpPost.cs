using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Teams.EmbeddedChat.Models;

namespace Microsoft.Teams.EmbeddedChat.APIs
{
    public class EntityFlowHttpPost
    {
        private readonly Processing _processing;

        public EntityFlowHttpPost(Processing processing)
        {
            _processing = processing;
        }

        /// <summary>
        /// Get Entity Mapping or create a new one
        /// </summary>
        /// <param name="request"></param>
        /// <param name="client"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.EntityMappingAPIHttpPost)]
        public async Task<IActionResult> EntityMapping(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = Constants.GetEntityStateRoute)] HttpRequestMessage request,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            log.LogInformation($"Started new {Constants.EntityMappingAPIHttpPost} flow.");

            var requestData = await request.Content.ReadAsAsync<ChatInfoRequest>();
            log.LogInformation($"Started new flow for the Entity ID = '{requestData.EntityId}'.");

            return await _processing.ProcessFlow(ParseOperation(request), requestData, request, client);
        }

        /// <summary>
        /// Update the entity state
        /// </summary>
        /// <param name="request"></param>
        /// <param name="client"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.EntityUpdateAPIHttpPost)]
        public async Task<IActionResult> EntityUpdate(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = Constants.UpdateEntityStateRoute)] HttpRequestMessage request,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            log.LogInformation($"Started new {Constants.EntityUpdateAPIHttpPost} flow.");

            var requestData = await request.Content.ReadAsAsync<ChatInfoRequest>();
            log.LogInformation($"Started new flow for the Entity ID = '{requestData.EntityId}'.");

            return await _processing.ProcessFlow(ParseOperation(request), requestData, request, client);
        }


        /// <summary>
        /// Parse the Http Request into ApiOperation
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private ApiOperation ParseOperation(HttpRequestMessage request)
        {
            if (request.RequestUri.PathAndQuery.Equals($"/api/{Constants.GetEntityStateRoute}"))
                return ApiOperation.GetEntityState;

            if (request.RequestUri.PathAndQuery.Equals($"/api/{Constants.UpdateEntityStateRoute}"))
                return ApiOperation.UpdateEntityState;

            return ApiOperation.UknownOperation;
        }
    }
}
