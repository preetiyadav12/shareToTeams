// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Teams.EmbeddedChat.Models;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat
{
    public interface IProcessing
    {
        Task<HttpResponseData> GetEntity(
            ChatInfoRequest requestData,
            HttpRequestData request);

        Task<HttpResponseData> CreateEntity(
            ChatInfoRequest requestData,
            HttpRequestData request);
    }
}
