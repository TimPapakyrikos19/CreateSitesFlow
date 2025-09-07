using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

public class GetLabelsFunc(TokenHelper tokens, GraphHelper graph, AppOptions opts)
{
    [Function("GetLabels")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "labels")] HttpRequestData req,
        FunctionContext ctx,
        CancellationToken ct)
    {
        string incoming = req.Headers.GetValues("Authorization").FirstOrDefault()?.Split(' ').Last()
                          ?? throw new InvalidOperationException("Missing Authorization header");

        var accessToken = await tokens.AcquireGraphOboTokenAsync(incoming, opts.GraphScopes, ct);

        var resp = await graph.SendWithRetryAsync(
            () => GraphHelper.Get("v1.0/security/informationProtection/labels", accessToken),
            ct: ct);

        var outResp = req.CreateResponse(resp.StatusCode);
        outResp.Headers.Add("Content-Type", "application/json");
        outResp.WriteString(await resp.Content.ReadAsStringAsync(ct));
        return outResp;
    }
}
