# FunctionApp-PowerAutomate-Integration

## ✅ Quick Start

1. **Deploy the Azure Function App** to Azure using VS Code or Azure CLI.
2. **Register the Function App** in Entra ID and expose the API scope `access_as_user`.
3. **Import the custom connector** (`connector.json`) into Power Automate.
4. **Import the sample flow** (`ApplyLabelAndTeamify.json`) and bind variables.

---

## 🛠️ Detailed Setup Instructions

### 1. Azure Function App

- Use .NET 8 isolated process.
- Add the following app settings:
  - `TenantId`
  - `ClientId`
  - `ClientSecret`
  - `GraphScopes`:  
    ```
    https://graph.microsoft.com/Group.ReadWrite.All https://graph.microsoft.com/Directory.Read.All https://graph.microsoft.com/Label.Read.All
    ```

### 2. App Registration

- Register the Function App in Entra ID.
- Expose an API scope: `api://<client-id>/access_as_user`
- Add delegated Graph permissions:
  - `Group.ReadWrite.All`
  - `Directory.Read.All`
  - `Label.Read.All`
- Admin consent required.

### 3. Power Automate Connector

- Import `connector.json` into Power Automate.
- Use OAuth 2.0 Authorization Code flow.
- Scope: `api://<client-id>/access_as_user`

### 4. Sample Flow

- Import `ApplyLabelAndTeamify.json`.
- Set variables:
  - `GroupId`
  - `LabelId`
  - `SiteType` (Teams or SharePoint)
- Flow logic:
  - Apply label always.
  - Create Team only if `SiteType = Teams`.

---
