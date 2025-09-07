using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace Graph
{
    public sealed class GraphClient
    {
        private readonly HttpClient _http = new();
        private readonly string _base;

        public GraphClient(IConfiguration cfg)
        {
            _base = (cfg["Graph:BaseUrl"] ?? cfg["Graph__BaseUrl"] ?? "https://graph.microsoft.com").TrimEnd('/');
        }

        public async Task<HttpResponseMessage> PatchAsync(string token, string path, HttpContent content)
        {
            using var req = new HttpRequestMessage(HttpMethod.Patch, $"{_base}/v1.0/{path.TrimStart('/')}")
            { Content = content };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _http.SendAsync(req);
        }

        public async Task<HttpResponseMessage> PutAsync(string token, string path, HttpContent content)
        {
            using var req = new HttpRequestMessage(HttpMethod.Put, $"{_base}/v1.0/{path.TrimStart('/')}")
            { Content = content };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _http.SendAsync(req);
        }
    }
}
