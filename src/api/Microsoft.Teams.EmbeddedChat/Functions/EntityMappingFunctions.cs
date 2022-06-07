// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.DurableTask;
using Microsoft.Teams.EmbeddedChat.Authorization;
using System.Threading.Tasks;
using Microsoft.Teams.EmbeddedChat.Models;

namespace Microsoft.Teams.EmbeddedChat.Functions;


// If an Authorize attribute is placed at class-level,
// requests to any function within the class
// must pass the authorization checks
//[Authorize(
//    Scopes = new[] { Scopes.FunctionsAccess },
//    UserRoles = new[] { UserRoles.User, UserRoles.Admin },
//    AppRoles = new[] { AppRoles.AccessAllFunctions })]
public class EntityMappingFunctions
{
    private readonly ILogger<EntityMappingFunctions> _logger;
    private readonly IProcessing _process;

    public EntityMappingFunctions(IProcessing processing, ILogger<EntityMappingFunctions> logger)
    {
        _logger = logger;
        _process = processing;
    }

    /// <summary>
    /// HTTP-triggered function that starts the <see cref="EntityMappingOrchestration"/> orchestration.
    /// </summary>
    /// <param name="req">The HTTP request that was used to trigger this function.</param>
    /// <param name="durableContext">The Durable Functions client binding context object that is used to start and manage orchestration instances.</param>
    /// <param name="executionContext">The Azure Functions execution context, which is available to all function types.</param>
    /// <returns>Returns an HTTP response with more information about the started orchestration instance.</returns>
    [Authorize(
        Scopes = new[] { Scopes.FunctionsAccess })]
    [Function(nameof(GetEntityMapping))]
    public async Task<HttpResponseData> GetEntityMapping(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = HttpRoutes.GetEntityStateRoute)] HttpRequestData req,
        [DurableClient] DurableClientContext durableContext,
        FunctionContext executionContext)
    {
        try
        {
            var requestData = await req.ReadFromJsonAsync<ChatInfoRequest>();

            if (requestData == null)
            {
                var msg = $"Must provide the input body of {nameof(ChatInfoRequest)} type";
                _logger.LogError(msg);
                return HttpResponses.CreateFailedResponse(req, msg);
            }

            _logger.LogInformation(
                $"The Function {nameof(GetEntityMapping)} was triggered by {requestData?.Owner?.DisplayName}");

            var response = await _process.GetEntity(requestData, req, durableContext);

            return response;
        }
        catch (System.Exception ex)
        {
            var errMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return HttpResponses.CreateFailedResponse(req, errMsg);
        }
    }


    /// <summary>
    /// HTTP-triggered function that starts the <see cref="EntityMappingOrchestration"/> orchestration.
    /// </summary>
    /// <param name="req">The HTTP request that was used to trigger this function.</param>
    /// <param name="durableContext">The Durable Functions client binding context object that is used to start and manage orchestration instances.</param>
    /// <param name="executionContext">The Azure Functions execution context, which is available to all function types.</param>
    /// <returns>Returns an HTTP response with more information about the started orchestration instance.</returns>
    [Authorize(
        Scopes = new[] { Scopes.FunctionsAccess })]
    [Function(nameof(CreateEntityMapping))]
    public async Task<HttpResponseData> CreateEntityMapping(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = HttpRoutes.CreateEntityStateRoute)] HttpRequestData req,
        [DurableClient] DurableClientContext durableContext,
        FunctionContext executionContext)
    {
        try
        {
            var requestData = await req.ReadFromJsonAsync<ChatInfoRequest>();

            if (requestData == null)
            {
                var msg = $"Must provide the input body of {nameof(ChatInfoRequest)} type";
                _logger.LogError(msg);
                return HttpResponses.CreateFailedResponse(req, msg);
            }

            _logger.LogInformation(
                $"The Function {nameof(CreateEntityMapping)} was triggered by {requestData?.Owner?.DisplayName}");

            var response = await _process.CreateEntity(requestData, req, durableContext);
            return response;
        }
        catch (System.Exception ex)
        {
            var errMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return HttpResponses.CreateFailedResponse(req, errMsg);
        }
    }
}
