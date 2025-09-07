using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CreateSitesFlow.Functions;

public class PingFunction
{
    private readonly ILogger _logger;

    public PingFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<PingFunction>();
    }

    [Function("Ping")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "ping")] HttpRequestData req)
    {
        _logger.LogInformation("Ping invoked.");
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        response.WriteString("{\"status\":\"ok\"}");
        return response;
    }
}
