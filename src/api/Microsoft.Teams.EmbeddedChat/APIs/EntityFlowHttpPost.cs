using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.Teams.EmbeddedChat.Models;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat.APIs
{
    // The web API will only accept tokens 1) for users, and 2) having the "api-scope" scope for this API
    [Authorize]
    public class EntityFlowHttpPost
    {
        private readonly Processing _processing;
        private readonly IHttpContextAccessor _contextAccessor;

        public EntityFlowHttpPost(IHttpContextAccessor contextAccessor, Processing processing)
        {
            _contextAccessor = contextAccessor;
            _processing = processing;
        }

        /// <summary>
        /// Get Entity Mapping or create a new one
        /// </summary>
        /// <param name="request"></param>
        /// <param name="client"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.GetEntityAPIHttpPost)]
        [RequiredScope("access_as_user")]
        public async Task<IActionResult> EntityMapping(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Constants.GetEntityStateRoute)] HttpRequest request,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            log.LogInformation($"{Constants.GetEntityAPIHttpPost} triggerred by a HTTP request from {System.Security.Claims.ClaimsPrincipal.Current}.");

            if (request.Headers.Authorization.Count == 0)
                return new UnauthorizedObjectResult("Authorization Header is missing");

            //var (authenticationStatus, authenticationResponse) =
            //    await request.HttpContext.AuthenticateAzureFunctionAsync();
            //if (!authenticationStatus)
            //    return authenticationResponse;

            string name = request.HttpContext.User.Identity.IsAuthenticated ? request.HttpContext.User.GetDisplayName() : null;

            log.LogInformation($"User with name of {name} has requested {Constants.GetEntityAPIHttpPost} flow.");

            var requestData = await request.HttpContext.Request.ReadFromJsonAsync<ChatInfoRequest>();
            log.LogInformation($"Started new flow for the Entity ID = '{requestData.EntityId}'.");

            return await _processing.ProcessFlow(ParseOperation(request), requestData, request, client);
        }

        /// <summary>
        /// Create a new chat and the entity state
        /// </summary>
        /// <param name="request"></param>
        /// <param name="client"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.CreateChatAPIHttpPost)]
        public async Task<IActionResult> CreateChat(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = Constants.CreateEntityStateRoute)] HttpRequest request,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            log.LogInformation($"Started new {Constants.CreateChatAPIHttpPost} flow.");

            var requestData = await request.HttpContext.Request.ReadFromJsonAsync<ChatInfoRequest>();
            log.LogInformation($"Started new flow for the Entity ID = '{requestData.EntityId}'.");

            return await _processing.ProcessFlow(ParseOperation(request), requestData, request, client);
        }


        /// <summary>
        /// Parse the Http Request into ApiOperation
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private ApiOperation ParseOperation(HttpRequest request)
        {
            if (request.Path.Equals($"/api/{Constants.GetEntityStateRoute}"))
                return ApiOperation.GetEntityState;

            if (request.Path.Equals($"/api/{Constants.CreateEntityStateRoute}"))
                return ApiOperation.CreateEntityState;

            return ApiOperation.UknownOperation;
        }
    }
}
