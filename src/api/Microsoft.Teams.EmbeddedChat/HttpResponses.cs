using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Teams.EmbeddedChat.Models;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Teams.EmbeddedChat
{
    public static class HttpResponses
    {
        public static HttpResponseData CreateOkTextResponse(
            HttpRequestData request,
            string text)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString(text);
            return response;
        }

        public static async Task<HttpResponseData> CreateOkResponseAsync(
            HttpRequestData request,
            BaseModel content)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteAsJsonAsync(content);
            return response;
        }


        public static HttpResponseData CreateNotFoundResponse(
            HttpRequestData request,
            string text)
        {
            var response = request.CreateResponse(HttpStatusCode.NotFound);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString(text);
            return response;
        }

        public static HttpResponseData CreateAccessDeniedResponse(
            HttpRequestData request,
            string text)
        {
            var response = request.CreateResponse(HttpStatusCode.Forbidden);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString(text);
            return response;
        }

        public static HttpResponseData CreateFailedResponse(
            HttpRequestData request,
            string text)
        {
            var response = request.CreateResponse(HttpStatusCode.InternalServerError);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString(text);
            return response;
        }

    }
}
