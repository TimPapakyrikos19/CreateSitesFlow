# CreateSitesFlow.Functions (.NET 8 isolated)

This is a minimal Azure Functions project using the **.NET isolated worker** with a sample HTTP trigger at `/api/ping`.

## Structure

```
CreateSitesFlow.Functions/
  CreateSitesFlow.Functions.csproj
  Program.cs
  PingFunction.cs
host.json
local.settings.json       # local only (excluded via .gitignore)
.github/workflows/deploy-functions.yml
```

## Run locally

1. Install Azure Functions Core Tools and .NET 8 SDK.
2. Start the emulator for Azurite (or use a Storage account) and set `AzureWebJobsStorage` in `local.settings.json`.
3. Run:
   ```bash
   func start
   ```
4. Test:
   ```bash
   curl http://localhost:7071/api/ping
   ```

## Deploy via GitHub Actions

- Set repository secret `AZURE_FUNCTIONAPP_PUBLISH_PROFILE` with your Function App's Publish Profile XML.
- Set `AZURE_FUNCTIONAPP_NAME` with your Function App name.

On push to `main`, the workflow builds, publishes, and deploys to your app.
