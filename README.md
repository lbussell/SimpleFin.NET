# SimpleFin.NET

A small, dependency-free **.NET client library for the [SimpleFIN protocol](https://www.simplefin.org/protocol.html)** (v2.0.0-draft).

It implements the application/consumer side of SimpleFIN: claim an **Access URL** from a
user-provided **SimpleFIN Token**, then retrieve read-only account and transaction data.
The library is trim- and AOT-friendly (it uses `System.Text.Json` source generators) and
wraps an injected `HttpClient`, so it works well with `IHttpClientFactory`.

## Install

```sh
dotnet add package SimpleFin
```

## Usage

### 1. Claim an Access URL from a SimpleFIN Token

The user pastes a SimpleFIN Token into your app. Exchange it for an Access URL **once**
and store the result securely.

```csharp
using SimpleFin;

var http = new HttpClient();
var client = new SimpleFinClient(http);

// The Base64 token the user gave you.
var token = new SimpleFinToken(userProvidedToken);

// POST to the claim URL to obtain an Access URL. Store accessUrl.ToString() securely.
AccessUrl accessUrl = await client.ClaimAccessUrlAsync(token);
```

A `403` while claiming throws a `SimpleFinException` (the token may be compromised — advise
the user to disable it).

### 2. Fetch accounts and transactions

Reuse a stored Access URL by parsing it back:

```csharp
using SimpleFin;
using SimpleFin.Models;

AccessUrl accessUrl = AccessUrl.Parse(storedAccessUrl);

var client = new SimpleFinClient(new HttpClient(), accessUrl);

AccountSet data = await client.GetAccountsAsync();

foreach (Account account in data.Accounts)
{
    Console.WriteLine($"{account.Name}: {account.Balance} {account.Currency}");

    foreach (Transaction tx in account.Transactions)
    {
        Console.WriteLine($"  {tx.Posted:d}  {tx.Amount,12}  {tx.Description}");
    }
}

// Always surface (sanitized) server errors to the user.
foreach (SimpleFinError error in data.Errors)
{
    Console.Error.WriteLine($"{error.Code}: {error.Message}");
}
```

### Query options

```csharp
var query = new AccountsQuery
{
    StartDate = DateTimeOffset.UtcNow.AddMonths(-1),
    EndDate = DateTimeOffset.UtcNow,
    IncludePending = true,    // pending=1
    BalancesOnly = false,     // balances-only=1 to skip transactions
    AccountIds = ["2930002"], // only these accounts
};

AccountSet data = await client.GetAccountsAsync(query);
```

### Server info

```csharp
SimpleFinInfo info = await client.GetInfoAsync(accessUrl);
Console.WriteLine(string.Join(", ", info.Versions)); // e.g. "1, 2"
```

## Types

| Type | Description |
| --- | --- |
| `SimpleFinToken` | A Base64 SimpleFIN Token; `GetClaimUrl()` decodes it to the claim URL. |
| `AccessUrl` | An HTTPS URL with embedded Basic Auth credentials. `Parse` / `ToString` round-trip for secure storage. |
| `SimpleFinClient` | Wraps `HttpClient`; `ClaimAccessUrlAsync`, `GetInfoAsync`, `GetAccountsAsync`. |
| `AccountSet` | The `GET /accounts` response: `Errors`, `Connections`, `Accounts`. |
| `Account` / `Transaction` | Money as `decimal`, timestamps as `DateTimeOffset`, `Extra` as JSON. |
| `SimpleFinException` | Thrown on non-success responses; exposes `StatusCode`. |

## Notes

- All requests must use HTTPS; the library rejects non-HTTPS URLs.
- Monetary values are exposed as `decimal`; timestamps as `DateTimeOffset` (from Unix epoch
  seconds).
- Prefer `AccountSet.Errors` (`errlist`); `LegacyErrors` (`errors`) is retained for v1
  servers.
- Always sanitize error and currency strings before displaying them to users.

## Scripts

The `scripts/` folder contains interactive scripts to help set up the repo.

- `SetupRepository.cs` - Automatically sets up the repo with my preferred settings (with confirmation).
- `SetupPublishing.cs` - Helps you set up NuGet trusted publishing from GitHub Workflows.
- `Release.cs` - Interactive release management tool. Run this to when you want to release software
  (stable or pre-release) or bump the version number.

These scripts assume the GitHub CLI is installed and authenticated with `gh auth login`.
