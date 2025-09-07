
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text;
using System.Text.Json;
using System.Net;
using Auth;
using Graph;

namespace Functions
{
    public sealed class EnableTeamFunction
    {
        private readonly OboTokenService _obo;
        private readonly GraphClient _graph;

        public EnableTeamFunction(OboTokenService obo, GraphClient graph)
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

        private sealed record Input(string groupId);

        [Function("enable-team")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "enable-team")] HttpRequestData req)
        {
            if (!TryGetIncomingBearer(req, out var incomingUserJwt))
            {
                var r = req.CreateResponse(HttpStatusCode.Unauthorized);
                await r.WriteStringAsync("Missing or invalid Authorization header.");
                return r;
            }

            var body = await JsonSerializer.DeserializeAsync<Input>(req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (body is null || string.IsNullOrWhiteSpace(body.groupId))
            {
                var r = req.CreateResponse(HttpStatusCode.BadRequest);
                await r.WriteStringAsync("Body must include 'groupId'.");
                return r;
            }

            var graphToken = await _obo.AcquireGraphTokenOnBehalfOfAsync(incomingUserJwt);

            var team = JsonSerializer.Serialize(new {
                memberSettings = new { allowCreatePrivateChannels = true, allowCreateUpdateChannels = true },
                messagingSettings = new { allowUserEditMessages = true, allowUserDeleteMessages = true },
                funSettings = new { allowGiphy = true, giphyContentRating = "strict" }
            });

            HttpResponseMessage? res = null;
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                res = await _graph.PutAsync(graphToken, $"groups/{body.groupId}/team", new StringContent(team, Encoding.UTF8, "application/json"));
                if (res.IsSuccessStatusCode) break;
                if ((int)res.StatusCode != 404) break;
                await Task.Delay(TimeSpan.FromSeconds(10));
            }

            var resp = req.CreateResponse((HttpStatusCode)(res?.StatusCode ?? HttpStatusCode.InternalServerError));
            var text = await (res?.Content.ReadAsStringAsync() ?? Task.FromResult("No response content."));
            await resp.WriteStringAsync(text);
            return resp;
        }
    }
}
