# 🔐 JWT Authorization Claims Middleware

[![CI](https://github.com/Isma-L154/MiddleWare/actions/workflows/ci.yml/badge.svg)](https://github.com/Isma-L154/MiddleWare/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

An ASP.NET Core middleware that **enriches an already-authenticated request principal** with identity claims (the user's id, name and email) and one **role claim per profile**, resolved from a SQL Server security store. It's shipped as a small set of **NuGet packages** so it can be dropped into any .NET 8 web app.

> This is a personal side project. It sits between the HTTP request and your application logic, turning a bare authenticated token into a fully-populated `ClaimsPrincipal` your authorization policies can rely on.

---

## ✨ What it does

Once a request has been authenticated (by JWT bearer auth, for example), the middleware:

1. Reads the configured user-name claim from the incoming principal.
2. Looks up the matching user in the security database (via a stored procedure).
3. Adds `Email`, `Name` and `IdUsuario` (`ClaimsEnrichmentMiddleware.UserIdClaimType`) claims.
4. Looks up the user's profiles and adds a `Role` claim for each one.

If anything goes wrong resolving that data (missing claim, unknown user, database outage), the request **degrades gracefully**: it continues unenriched instead of crashing the pipeline.

---

## 🧱 Architecture

The solution is layered so each concern is isolated and independently testable:

| Project | Responsibility |
| --- | --- |
| `Authorization.Abstractions` | Contracts: models, options and interfaces. No external dependencies. |
| `Authorization.DataAccess` | Dapper + `Microsoft.Data.SqlClient` access to stored procedures; maps database rows to models. |
| `Authorization.Business` | Thin business layer orchestrating identity resolution. |
| `Authorization.Middleware` | The ASP.NET Core middleware plus DI and pipeline extensions. |

```
Request ─▶ Authentication ─▶ ClaimsEnrichmentMiddleware ─▶ your app
                                     │
                     IAuthorizationManager (Business)
                                     │
                       ISecurityRepository (DataAccess)
                                     │
                     IDbConnectionFactory ─▶ SQL Server
```

---

## 📦 Installation

The packages are published to **GitHub Packages**. Add the feed and install the entry-point package (it pulls the rest in transitively):

```bash
dotnet nuget add source "https://nuget.pkg.github.com/Isma-L154/index.json" \
  --name github --username <your-user> --password <your-PAT>

dotnet add package Authorization.Middleware
```

---

## 🚀 Usage

**1. Register the services** (wires the connection factory, repository and business manager):

```csharp
using Authorization.Middleware;

builder.Services.AddAuthorizationClaims();
```

**2. Add the middleware to the pipeline**, after authentication:

```csharp
app.UseAuthentication();
app.UseAuthorizationClaims(); // enrich the principal
app.UseAuthorization();
```

**3. Configure the connection string** in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SecurityDb": "Server=...;Database=...;Trusted_Connection=True;Encrypt=True;"
  }
}
```

---

## ⚙️ Configuration

Claim and database settings are configurable through `ClaimsEnrichmentOptions`. They are validated at startup, so a missing connection string or blank setting fails the host instead of individual requests.

```csharp
builder.Services.AddAuthorizationClaims(options =>
{
    options.ConnectionStringName = "SecurityDb";      // ConnectionStrings key
    options.UserNameClaimType    = "usuario";         // inbound JWT claim to read
    options.GetUserProcedure     = "ObtenerUsuario";  // stored procedure names
    options.GetProfilesProcedure = "ObtenerPerfilesxUsuario";
});
```

| Option | Default | Description |
| --- | --- | --- |
| `ConnectionStringName` | `SecurityDb` | Key under `ConnectionStrings` for the security DB. |
| `UserNameClaimType` | `usuario` | Inbound claim type carrying the user name. |
| `GetUserProcedure` | `ObtenerUsuario` | Stored procedure returning a user by name/email. |
| `GetProfilesProcedure` | `ObtenerPerfilesxUsuario` | Stored procedure returning a user's profiles. |

---

## 🧪 Building & testing

```bash
dotnet build Authorization.sln -c Release
dotnet test  Authorization.sln -c Release
```

CI runs on every push and pull request.

To release, bump `<Version>` in `src/Directory.Build.props` through a PR, then tag the merged commit on `main`:

```bash
git tag v3.0.0 && git push origin v3.0.0
```

The release workflow fails if the tag does not match the project version or the version already exists on the feed.

---

## 🛠️ Technologies

- **.NET 8.0**, C# latest
- **Dapper** for micro-ORM data access
- **Microsoft.Data.SqlClient** (the maintained SQL Server driver)
- **xUnit** + **Moq** for unit tests
- Distributed as **NuGet packages** via GitHub Packages

---

## 📄 License

Released under the [MIT License](LICENSE).
