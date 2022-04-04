// <copyright file="MappingController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TeamsEmbeddedChat.Controllers
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;
    using Microsoft.Azure.Cosmos;
    using TeamsEmbeddedChat.Models;
    using Azure.Communication.Identity;

    /// <summary>
    /// /// Web API for initializing an embedded chat user.
    /// </summary>
    [Authorize]
    [ApiController]
    public class MappingController : ControllerBase
    {
        private CosmosClient cosmosClient { get; }
        private Container cosmosContainer;
        private readonly IOptions<Models.AzureADSettings> azureADOptions;
        private readonly IOptions<Models.CosmosDBSettings> cosmosDBSettings;
        private readonly IOptions<Models.AcsSettings> acsSettings;

        public MappingController(
            CosmosClient cosmosClient,
            IOptions<Models.AzureADSettings> azureADOptions,
            IOptions<Models.CosmosDBSettings> cosmosDBSettings,
            IOptions<Models.AcsSettings> acsSettings)
        {
            this.cosmosClient = cosmosClient;
            this.azureADOptions = azureADOptions;
            this.cosmosDBSettings = cosmosDBSettings;
            this.acsSettings = acsSettings;
        }

        [HttpGet]
        [Route("/api/mapping/{entityId}")]
        public async Task<IActionResult> Initalize(string entityId)
        {
            // ensure entity id passed in
            if (string.IsNullOrEmpty(entityId))
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest, "entityId is required for initialization");

            // check for an existing mapping for the entity id
            var mapping = await getMappingAsync(entityId);
            CommunicationIdentityClient client = new CommunicationIdentityClient(this.acsSettings.Value.ConnectionString);
            if (mapping == null)
            {
                // create new ACS user for this entity
                var user = await client.CreateUserAsync();
                mapping = new Models.Mapping() {
                    Id = Guid.NewGuid().ToString(),
                    AcsUserId = user.Value.Id,
                    EntityId = entityId
                };

                // store the entity in ACS
                await this.cosmosContainer.CreateItemAsync<Models.Mapping>(mapping, new PartitionKey(entityId));
            }

            InitResponse resp = new InitResponse() {
                Mapping = mapping
            };
            var token = await client.GetTokenAsync(new Azure.Communication.CommunicationUserIdentifier(mapping.AcsUserId), scopes: new [] { CommunicationTokenScope.Chat });
            resp.AcsToken = token.Value.Token;

            // return the response
            return Ok(resp);
        }

        [HttpPatch]
        [Route("/api/mapping")]
        public async Task<IActionResult> UpdateMappingAsync([FromBody] Models.Mapping mapping)
        {
            // ensure mapping isn't null and has an entityId
            if (mapping == null)
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest, "Mapping is null");
            if (string.IsNullOrEmpty(mapping.EntityId))
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest, "Mapping is invalid (entityId)");
            if (string.IsNullOrEmpty(mapping.ThreadId))
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest, "Mapping is invalid (threadId)");

            // try to get the mapping
            var dbMapping = await getMappingAsync(mapping.EntityId);
            if (dbMapping == null)
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest, "Mapping is invalid (entityId)");

            // update the mapping
            await this.cosmosContainer.UpsertItemAsync<Models.Mapping>(mapping, new PartitionKey(mapping.EntityId));

            return Ok();
        }

        private async Task<Models.Mapping> getMappingAsync(string entityId)
        {
            await InitializeDatabaseAsync();
            try
            {
                List<Mapping> results = new List<Mapping>();
                using (var feedIterator = this.cosmosContainer.GetItemQueryIterator<Mapping>($"SELECT * FROM m WHERE m.entityId = '{entityId}'"))
                {
                    while (feedIterator.HasMoreResults)
                    {
                        FeedResponse<Mapping> feedResponse = await feedIterator.ReadNextAsync();
                        results.AddRange(feedResponse);
                    }
                }
                var mapping = results.FirstOrDefault();
                return mapping;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
        }

        /// <summary>
        /// Initializes Cosmos DB by creating the necessary database and container if needed.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        private async Task InitializeDatabaseAsync()
        {
            var databaseName = this.cosmosDBSettings.Value.DatabaseName;

            // Create the database and container if they don't exist
            var databaseResponse = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
            var containerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync("Mappings", "/entityId", 400);

            this.cosmosContainer = containerResponse.Container;
        }
    }
}