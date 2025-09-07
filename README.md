
# FunctionApp-Delegated-OBO


Azure Functions (.NET 8 isolated) middle-tier to perform **delegated** Microsoft Graph calls using the **On-Behalf-Of (OBO)** flow from Power Automate.

Endpoints:
- `POST /api/apply-label` — Apply a sensitivity label to an M365 Group.
- `POST /api/enable-team` — Convert an M365 Group into a Team (with simple retries).

## 1) Prereqs

- .NET 8 SDK
- Azure subscription + Azure Functions
- Two Entra ID app registrations (or one if you prefer):
  - **API app** (this Function)
    - Expose API: scope like `Flow.Access`
    - Delegated Graph permissions: `Group.ReadWrite.All`, `Directory.ReadWrite.All` (admin-consented)
    - Client secret
  - **Client app** (used by your Custom Connector)
    - Redirect URI: `https://global.consent.azure-apim.net/redirect`
    - Add permission to the **API app** scope above
    - Client secret

> For GCC High / national clouds, set `Graph__BaseUrl` accordingly (e.g., `https://graph.microsoft.us`).

## 2) Local configuration

Create `local.settings.json` from the example in this repo and fill in values:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AzureAd__TenantId": "00000000-0000-0000-0000-000000000000",
    "AzureAd__ClientId": "API-APP-CLIENT-ID",
    "AzureAd__ClientSecret": "API-APP-CLIENT-SECRET",
    "Graph__Scopes": "https://graph.microsoft.com/.default",
    "Graph__BaseUrl": "https://graph.microsoft.com",
    "AllowedScopes": "api://API-APP-ID/Flow.Access"
  }
}
```

## 3) Build & run

```
dotnet restore
dotnet build
func start
```

## 4) Deploy

Use your preferred method (VS Code, Azure CLI, or the included GitHub Actions sample). Ensure **Authentication** is enabled on the Function App and uses the **API app** registration.

## 5) Custom Connector

Use the `connector-openapi.json` in this repo to create a Custom Connector in Power Automate:

- **Security**:
  - OAuth 2.0 (Azure AD)
  - Client ID/Secret: from **Client app**
  - Auth URL: `https://login.microsoftonline.com/<tenant>/oauth2/v2.0/authorize`
  - Token URL: `https://login.microsoftonline.com/<tenant>/oauth2/v2.0/token`
  - Scope: `api://<API-APP-ID>/Flow.Access`

- **Base URL**: `https://<your-function>.azurewebsites.net`

## 6) Usage from Flow

1. After you create the M365 group and have `groupId` and `labelId` variables:
   - Call **ApplyLabel** with `{ "groupId": "...", "labelId": "..." }`.
2. If user selected Teams site, call **EnableTeam** with `{ "groupId": "..." }`.
3. Ensure the group has **at least one owner** before team enablement.

## 7) Notes

- Label apply requires **delegated** context; this is why OBO is needed.
- Team creation occasionally returns 404 for a brand-new group; the function retries up to 3× with 10s delay.
- For GCC High, set `Graph__BaseUrl` to the appropriate national cloud and confirm Teams/Graph endpoints availability.
