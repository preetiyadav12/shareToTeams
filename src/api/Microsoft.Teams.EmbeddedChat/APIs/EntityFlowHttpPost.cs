using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Teams.EmbeddedChat.Models;
using Microsoft.Teams.EmbeddedChat.Utils;

namespace Microsoft.Teams.EmbeddedChat.APIs
{
    public class EntityFlowHttpPost
    {
        private readonly Processing _processing;

        public EntityFlowHttpPost(Processing processing)
        {
            _processing = processing;
        }

        [FunctionName(Constants.EntityMappingAPIHttpPost)]
        public async Task<IActionResult> EntityMapping(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "entity/mapping")] HttpRequestMessage request,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            log.LogInformation($"Started new {Constants.EntityMappingAPIHttpPost} flow.");

            var requestData = await request.Content.ReadAsAsync<EntityState>();
            log.LogInformation($"Started new flow for the Entity ID = '{requestData.EntityId}'.");

            return await _processing.ProcessFlow(requestData, request, client);
        }

        [FunctionName(Constants.EntityUpdateAPIHttpPost)]
        public async Task<IActionResult> EntityUpdate(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "entity/mapping/update")] HttpRequestMessage request,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            log.LogInformation($"Started new {Constants.EntityUpdateAPIHttpPost} flow.");

            var requestData = await request.Content.ReadAsAsync<EntityState>();
            log.LogInformation($"Started new flow for the Entity ID = '{requestData.EntityId}'.");

            try
            {
                // Construct a new "TableServiceClient using a connection string.
                var tableService = new AzureDataTablesService<EntityState>(this._processing.AppConfiguration.StorageConnectionString, this._processing.AppConfiguration.AzureTableName);

                // check if the entity mapping already exists
                await tableService.UpdateEntityAsync(requestData);
            }
            catch (System.Exception e)
            {
                log.LogError(e.Message);
                throw;
            }

            return new OkResult();
        }
    }
}
