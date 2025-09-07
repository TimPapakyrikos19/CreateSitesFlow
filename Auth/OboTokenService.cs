using Microsoft.Identity.Client;
using Microsoft.Extensions.Configuration;

namespace Auth
{
    public sealed class OboTokenService
    {
        private readonly IConfidentialClientApplication _cca;
        private readonly string[] _graphScopes;

        public OboTokenService(IConfiguration cfg)
        {
            // Prefer ':' keys; fall back to '__' just in case
            var tenantId     = cfg["AzureAd:TenantId"]     ?? cfg["AzureAd__TenantId"];
            var clientId     = cfg["AzureAd:ClientId"]     ?? cfg["AzureAd__ClientId"];
            var clientSecret = cfg["AzureAd:ClientSecret"] ?? cfg["AzureAd__ClientSecret"];
            var authorityHost= cfg["AzureAd:AuthorityHost"]?? cfg["AzureAd__AuthorityHost"] ?? "https://login.microsoftonline.com";

            if (string.IsNullOrWhiteSpace(tenantId))     throw new InvalidOperationException("AzureAd:TenantId missing.");
            if (string.IsNullOrWhiteSpace(clientId))     throw new InvalidOperationException("AzureAd:ClientId missing.");
            if (string.IsNullOrWhiteSpace(clientSecret)) throw new InvalidOperationException("AzureAd:ClientSecret missing.");

            _cca = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority($"{authorityHost}/{tenantId}")
                .Build();

            _graphScopes = new[] { cfg["Graph:Scopes"] ?? cfg["Graph__Scopes"] ?? "https://graph.microsoft.com/.default" };
        }

        public async Task<string> AcquireGraphTokenOnBehalfOfAsync(string incomingBearer)
        {
            var assertion = new UserAssertion(incomingBearer);
            var result = await _cca.AcquireTokenOnBehalfOf(_graphScopes, assertion).ExecuteAsync().ConfigureAwait(false);
            return result.AccessToken;
        }
    }
}
