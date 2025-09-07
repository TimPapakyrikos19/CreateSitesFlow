
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text;
using System.Text.Json;
using System.Net;
using Auth;
using Graph;

namespace Functions
{
    public sealed class ApplyLabelFunction
    {
        private readonly OboTokenService _obo;
        private readonly GraphClient _graph;

        public ApplyLabelFunction(OboTokenService obo, GraphClient graph)
        {
            _obo = obo;
            _graph = graph;
        }

        private static bool TryGetIncomingBearer(HttpRequestData req, out string bearer)
        {
            bearer = string.Empty;
            if (!req.Headers.TryGetValues("Authorization", out var values)) return false;
            var authHeader = values.FirstOrDefault();
            if (authHeader is null || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return false;
            bearer = authHeader["Bearer ".Length..];
            return !string.IsNullOrWhiteSpace(bearer);
        }

        private sealed record Input(string groupId, string labelId);

        [Function("apply-label")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "apply-label")] HttpRequestData req)
        {
            if (!TryGetIncomingBearer(req, out var incomingUserJwt))
            {
                var r = req.CreateResponse(HttpStatusCode.Unauthorized);
                await r.WriteStringAsync("Missing or invalid Authorization header.");
                return r;
            }

            var body = await JsonSerializer.DeserializeAsync<Input>(req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (body is null || string.IsNullOrWhiteSpace(body.groupId) || string.IsNullOrWhiteSpace(body.labelId))
            {
                var r = req.CreateResponse(HttpStatusCode.BadRequest);
                await r.WriteStringAsync("Body must include 'groupId' and 'labelId'.");
                return r;
            }

            var graphToken = await _obo.AcquireGraphTokenOnBehalfOfAsync(incomingUserJwt);

            var payload = JsonSerializer.Serialize(new
            {
                assignedLabels = new[] { new { labelId = body.labelId } }
            });
            var res = await _graph.PatchAsync(graphToken, $"groups/{body.groupId}", new StringContent(payload, Encoding.UTF8, "application/json"));

            var resp = req.CreateResponse((HttpStatusCode)res.StatusCode);
            var text = await res.Content.ReadAsStringAsync();
            await resp.WriteStringAsync(text);
            return resp;
        }
    }
}
