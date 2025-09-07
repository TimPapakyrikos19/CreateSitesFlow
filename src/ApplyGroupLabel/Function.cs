using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;

public class ApplyGroupLabelFunc(TokenHelper tokens, GraphHelper graph, AppOptions opts)
{
    [Function("ApplyGroupLabel")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "groups/{id}/label")] HttpRequestData req,
        string id,
        FunctionContext ctx,
        CancellationToken ct)
    {
        string incoming = req.Headers.GetValues("Authorization").FirstOrDefault()?.Split(' ').Last()
                          ?? throw new InvalidOperationException("Missing Authorization header");

        var body = await JsonSerializer.DeserializeAsync<ApplyLabelRequest>(req.Body, cancellationToken: ct)
                   ?? throw new InvalidOperationException("Invalid body");

        var accessToken = await tokens.AcquireGraphOboTokenAsync(incoming, opts.GraphScopes, ct);

        var payload = JsonSerializer.Serialize(new
        {
            assignedLabels = new[] { new { labelId = body.LabelId } }
        });

        var resp = await graph.SendWithRetryAsync(
            () => GraphHelper.PatchJson($"v1.0/groups/{id}", payload, accessToken),
            treat404AsRetryable: false,
            ct: ct);

        var outResp = req.CreateResponse(resp.StatusCode);
        outResp.Headers.Add("Content-Type", "application/json");
        outResp.WriteString(await resp.Content.ReadAsStringAsync(ct));
        return outResp;
    }
}
