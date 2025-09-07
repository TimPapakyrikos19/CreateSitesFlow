using System.Net;
using System.Net.Http.Headers;
using System.Text;

public class GraphHelper
{
    private readonly HttpClient _http;

    public GraphHelper(HttpClient http) => _http = http;

    public async Task<HttpResponseMessage> SendWithRetryAsync(
        Func<HttpRequestMessage> requestFactory,
        bool treat404AsRetryable = false,
        int maxRetries = 5,
        CancellationToken ct = default)
    {
        var rng = new Random();
        TimeSpan NextDelay(int attempt)
        {
            var baseDelayMs = Math.Min(32000, (int)Math.Pow(2, attempt) * 250);
            var jitterMs = rng.Next(0, 250);
            return TimeSpan.FromMilliseconds(baseDelayMs + jitterMs);
        }

        for (int attempt = 0; ; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            using var req = requestFactory();
            var resp = await _http.SendAsync(req, ct);

            if ((int)resp.StatusCode < 400) return resp;

            if (resp.StatusCode == (HttpStatusCode)429 || ((int)resp.StatusCode >= 500 && (int)resp.StatusCode <= 599)
                || (treat404AsRetryable && resp.StatusCode == HttpStatusCode.NotFound))
            {
                if (attempt >= maxRetries)
                    return resp;

                TimeSpan delay = NextDelay(attempt);

                if (resp.Headers.TryGetValues("Retry-After", out var vals) && int.TryParse(vals.FirstOrDefault(), out var sec))
                    delay = TimeSpan.FromSeconds(Math.Min(sec, 60));

                await Task.Delay(delay, ct);
                continue;
            }

            return resp;
        }
    }

    public static HttpRequestMessage PatchJson(string url, string json, string accessToken)
    {
        var req = new HttpRequestMessage(HttpMethod.Patch, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return req;
    }

    public static HttpRequestMessage PutJson(string url, string json, string accessToken)
    {
        var req = new HttpRequestMessage(HttpMethod.Put, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return req;
    }

    public static HttpRequestMessage Get(string url, string accessToken)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return req;
    }
}
