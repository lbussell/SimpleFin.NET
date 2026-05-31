#!/usr/bin/env dotnet

// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/SimpleFin

using SimpleFin;
using SimpleFin.Models;
using static System.Console;

string? tokenString = args.FirstOrDefault()
    ?? Environment.GetEnvironmentVariable("SIMPLEFIN_TOKEN");

if (string.IsNullOrWhiteSpace(tokenString))
{
    WriteLine("Go to https://beta-bridge.simplefin.org/info/developers and copy a demo setup token.");
    WriteLine("It can only be used once; refresh the page to generate another.");
    Write("Paste your SimpleFIN setup token here: ");
    tokenString = ReadLine();
}

if (!SimpleFinToken.TryParse(tokenString, out SimpleFinToken token))
{
    Error.WriteLine("The supplied SimpleFIN token is not valid.");
    return 1;
}

var httpClient = new HttpClient();
var simpleFinClient = new SimpleFinClient(httpClient);

WriteLine("Claiming Access URL...");
AccessUrl accessUrl = await simpleFinClient.ClaimAccessUrlAsync(token);
WriteLine("Claimed Access URL. Store this securely if you need to reuse it.");
WriteLine(accessUrl);
WriteLine();

AccountsQuery query = new()
{
    StartDate = DateTimeOffset.UtcNow.AddDays(-30),
    IncludePending = true,
};

WriteLine("Fetching accounts and recent transactions...");
AccountSet accountSet = await simpleFinClient.GetAccountsAsync(query, accessUrl);

foreach (SimpleFinError error in accountSet.Errors)
    Error.WriteLine($"Server error {error.Code}: {error.Message}");

foreach (string error in accountSet.LegacyErrors)
    Error.WriteLine($"Server error: {error}");

if (accountSet.Accounts.Count == 0)
    WriteLine("No accounts returned.");

foreach (Account account in accountSet.Accounts)
{
    WriteLine(account);
    foreach (Transaction transaction in account.Transactions)
        WriteLine(transaction);
    WriteLine();
}

return 0;
