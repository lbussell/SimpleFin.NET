---
_layout: landing
---

# Documentation for SimpleFIN.NET

SimpleFIN.NET is a client library for the [SimpleFIN protocol].

## Installation

Add the NuGet package to your project.

```shell
dotnet add package SimpleFIN.NET
```

## Quick start

First, get a Setup Token from your user. For testing, you can generate a demo
token on the [SimpleFIN Developer Guide] page. Exchange the setup token for an
access URL:

```cs
using SimpleFin;
using SimpleFin.Models;

if (!SimpleFinToken.TryParse(tokenString, out SimpleFinToken token)) return;
var simpleFinClient = new SimpleFinClient(new HttpClient());
AccessUrl accessUrl = await simpleFinClient.ClaimAccessUrlAsync(token);
```

**Store the access URL securely.** Then you're ready to access account and
transaction data:

```cs
AccountsQuery query = new()
{
    StartDate = DateTimeOffset.UtcNow.AddDays(-30),
    IncludePending = true,
};
AccountSet accounts = await simpleFinClient.GetAccountsAsync(query, accessUrl);
foreach (Account account in accounts.Accounts)
{
    Console.WriteLine(account);
    foreach (Transaction transaction in account.Transactions)
        Console.WriteLine(transaction);
}
```

For more details, please read the [SimpleFIN Developer Guide].

## Disclaimer

This project is not affiliated with SimpleFIN or SimpleFIN Bridge. This project
is intended for hobby use only.

## License

SimpleFIN.NET is licensed under the MIT license.

[SimpleFIN Developer Guide]: https://beta-bridge.simplefin.org/info/developers
[SimpleFIN protocol]: https://www.simplefin.org/protocol.html
