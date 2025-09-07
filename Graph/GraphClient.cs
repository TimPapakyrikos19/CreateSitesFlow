
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
            _base = (cfg["Graph__BaseUrl"] ?? "https://graph.microsoft.com").TrimEnd('/');
        }

        private HttpRequestMessage WithAuth(HttpMethod method, string path, string token, HttpContent? content = null)
        {
            var req = new HttpRequestMessage(method, $"{_base}/v1.0/{path.TrimStart('/')}");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (content is not null) req.Content = content;
            return req;
        }

        public Task<HttpResponseMessage> PatchAsync(string token, string path, HttpContent content) =>
            _http.SendAsync(WithAuth(HttpMethod.Patch, path, token, content));

        public Task<HttpResponseMessage> PutAsync(string token, string path, HttpContent content) =>
            _http.SendAsync(WithAuth(HttpMethod.Put, path, token, content));
    }
}
