using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;

public class CreateTeamFunc(TokenHelper tokens, GraphHelper graph, AppOptions opts)
{
    [Function("CreateTeam")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "groups/{id}/team")] HttpRequestData req,
        string id,
        FunctionContext ctx,
        CancellationToken ct)
    {
        string incoming = req.Headers.GetValues("Authorization").FirstOrDefault()?.Split(' ').Last()
                          ?? throw new InvalidOperationException("Missing Authorization header");

        var bodyJson = await new StreamReader(req.Body).ReadToEndAsync(ct);
        if (string.IsNullOrWhiteSpace(bodyJson))
        {
            bodyJson = JsonSerializer.Serialize(new
            {
                memberSettings = new { allowCreateUpdateChannels = true }
            });
        }

        var accessToken = await tokens.AcquireGraphOboTokenAsync(incoming, opts.GraphScopes, ct);

        var resp = await graph.SendWithRetryAsync(
            () => GraphHelper.PutJson($"v1.0/groups/{id}/team", bodyJson, accessToken),
            treat404AsRetryable: true,
            ct: ct);

        var outResp = req.CreateResponse(resp.StatusCode);
        outResp.Headers.Add("Content-Type", "application/json");
        outResp.WriteString(await resp.Content.ReadAsStringAsync(ct));
        return outResp;
    }
}
