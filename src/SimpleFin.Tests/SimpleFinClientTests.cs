// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleFin.Models;

namespace SimpleFin.Tests;

[TestClass]
public class SimpleFinClientTests
{
    private const string DemoToken =
        "aHR0cHM6Ly9icmlkZ2Uuc2ltcGxlZmluLm9yZy9zaW1wbGVmaW4vY2xhaW0vZGVtbw==";

    private const string Host = "bridge.simplefin.org";

    private static readonly string AccessUrlString = $"https://demo:secret@{Host}/simplefin";

    private static readonly AccessUrl Access = AccessUrl.Parse(AccessUrlString);

    [TestMethod]
    public async Task ClaimAccessUrlAsync_PostsToClaimUrlAndReturnsAccessUrl()
    {
        StubHttpMessageHandler handler = new(HttpStatusCode.OK, AccessUrlString);
        SimpleFinClient client = new(new HttpClient(handler));

        AccessUrl result = await client.ClaimAccessUrlAsync(new SimpleFinToken(DemoToken));

        Assert.AreEqual("demo", result.Username);
        Assert.AreEqual("secret", result.Password);
        HttpRequestMessage request = handler.Requests.Single();
        Assert.AreEqual(HttpMethod.Post, request.Method);
        Assert.AreEqual(
            "https://bridge.simplefin.org/simplefin/claim/demo",
            request.RequestUri!.AbsoluteUri
        );
    }

    [TestMethod]
    public async Task ClaimAccessUrlAsync_ThrowsOnForbidden()
    {
        StubHttpMessageHandler handler = new(HttpStatusCode.Forbidden, string.Empty);
        SimpleFinClient client = new(new HttpClient(handler));

        SimpleFinException ex = await Assert.ThrowsExactlyAsync<SimpleFinException>(() =>
            client.ClaimAccessUrlAsync(new SimpleFinToken(DemoToken))
        );

        Assert.AreEqual(HttpStatusCode.Forbidden, ex.StatusCode);
    }

    [TestMethod]
    public async Task GetAccountsAsync_SendsBasicAuthAndParsesResponse()
    {
        const string json = """
            {
              "errlist": [],
              "connections": [],
              "accounts": [
                {
                  "id": "2930002",
                  "name": "Savings",
                  "currency": "USD",
                  "balance": "100.23",
                  "balance-date": 978366153,
                  "transactions": []
                }
              ]
            }
            """;
        StubHttpMessageHandler handler = new(HttpStatusCode.OK, json);
        SimpleFinClient client = new(new HttpClient(handler), Access);

        AccountSet set = await client.GetAccountsAsync();

        Assert.AreEqual("Savings", set.Accounts.Single().Name);
        HttpRequestMessage request = handler.Requests.Single();
        Assert.AreEqual(
            "https://bridge.simplefin.org/simplefin/accounts",
            request.RequestUri!.AbsoluteUri
        );
        Assert.AreEqual("Basic", request.Headers.Authorization!.Scheme);
        string expected = Convert.ToBase64String(Encoding.UTF8.GetBytes("demo:secret"));
        Assert.AreEqual(expected, request.Headers.Authorization.Parameter);
    }

    [TestMethod]
    public async Task GetAccountsAsync_BuildsQueryString()
    {
        StubHttpMessageHandler handler = new(
            HttpStatusCode.OK,
            """{ "errlist": [], "connections": [], "accounts": [] }"""
        );
        SimpleFinClient client = new(new HttpClient(handler), Access);

        AccountsQuery query = new()
        {
            StartDate = DateTimeOffset.FromUnixTimeSeconds(1000),
            EndDate = DateTimeOffset.FromUnixTimeSeconds(2000),
            IncludePending = true,
            BalancesOnly = true,
            AccountIds = ["a1", "a2"],
        };

        await client.GetAccountsAsync(query);

        string actualQuery = handler.Requests.Single().RequestUri!.Query;
        StringAssert.Contains(actualQuery, "start-date=1000");
        StringAssert.Contains(actualQuery, "end-date=2000");
        StringAssert.Contains(actualQuery, "pending=1");
        StringAssert.Contains(actualQuery, "balances-only=1");
        StringAssert.Contains(actualQuery, "account=a1");
        StringAssert.Contains(actualQuery, "account=a2");
    }

    [TestMethod]
    public async Task GetAccountsAsync_ThrowsOnForbidden()
    {
        StubHttpMessageHandler handler = new(HttpStatusCode.Forbidden, string.Empty);
        SimpleFinClient client = new(new HttpClient(handler), Access);

        SimpleFinException ex = await Assert.ThrowsExactlyAsync<SimpleFinException>(() =>
            client.GetAccountsAsync()
        );

        Assert.AreEqual(HttpStatusCode.Forbidden, ex.StatusCode);
    }

    [TestMethod]
    public async Task GetAccountsAsync_ThrowsWhenNoAccessConfigured()
    {
        StubHttpMessageHandler handler = new(HttpStatusCode.OK, string.Empty);
        SimpleFinClient client = new(new HttpClient(handler));

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.GetAccountsAsync());
    }

    [TestMethod]
    public async Task GetInfoAsync_ParsesVersions()
    {
        StubHttpMessageHandler handler = new(HttpStatusCode.OK, """{ "versions": ["1","2"] }""");
        SimpleFinClient client = new(new HttpClient(handler), Access);

        SimpleFinInfo info = await client.GetInfoAsync();

        CollectionAssert.AreEqual(new[] { "1", "2" }, info.Versions.ToArray());
        Assert.AreEqual(
            "https://bridge.simplefin.org/simplefin/info",
            handler.Requests.Single().RequestUri!.AbsoluteUri
        );
    }
}
