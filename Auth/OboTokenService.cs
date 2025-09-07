
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
            _cca = ConfidentialClientApplicationBuilder
                .Create(cfg["AzureAd__ClientId"]!)
                .WithClientSecret(cfg["AzureAd__ClientSecret"]!)
                .WithAuthority($"https://login.microsoftonline.com/{cfg["AzureAd__TenantId"]}")
                .Build();

            _graphScopes = new[] { cfg["Graph__Scopes"] ?? "https://graph.microsoft.com/.default" };
        }

        public async Task<string> AcquireGraphTokenOnBehalfOfAsync(string incomingBearer)
        {
            var assertion = new UserAssertion(incomingBearer);
            var result = await _cca.AcquireTokenOnBehalfOf(_graphScopes, assertion).ExecuteAsync().ConfigureAwait(false);
            return result.AccessToken;
        }
    }
}
