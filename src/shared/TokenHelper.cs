using Microsoft.Identity.Client;

public class TokenHelper
{
    private readonly AppOptions _opts;
    private readonly IConfidentialClientApplication _cca;

    public TokenHelper(AppOptions opts)
    {
        _opts = opts;
        _cca = ConfidentialClientApplicationBuilder
            .Create(_opts.ClientId)
            .WithClientSecret(_opts.ClientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{_opts.TenantId}")
            .Build();
    }

    public async Task<string> AcquireGraphOboTokenAsync(string incomingBearerJwt, string[] scopes, CancellationToken ct)
    {
        var userAssertion = new UserAssertion(incomingBearerJwt);
        var result = await _cca.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync(ct);
        return result.AccessToken;
    }
}
