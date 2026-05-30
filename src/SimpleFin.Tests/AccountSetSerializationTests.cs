// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleFin.Json;
using SimpleFin.Models;

namespace SimpleFin.Tests;

[TestClass]
public class AccountSetSerializationTests
{
    // Sample response adapted from the SimpleFIN protocol documentation.
    private const string SampleJson = """
        {
          "errlist": [
            {
              "code": "con.auth",
              "msg": "Authentication required",
              "conn_id": "CON-10829309823094234"
            }
          ],
          "connections": [
            {
              "conn_id": "CON-10829309823094234",
              "name": "My Bank - James",
              "org_id": "INST-129839182938123123",
              "org_url": "https://mybank.com",
              "sfin_url": "https://mybank.com"
            }
          ],
          "accounts": [
            {
              "id": "2930002",
              "name": "Savings",
              "conn_id": "CON-10829309823094234",
              "currency": "USD",
              "balance": "100.23",
              "available-balance": "75.23",
              "balance-date": 978366153,
              "transactions": [
                {
                  "id": "12394832938403",
                  "posted": 793090572,
                  "amount": "-33293.43",
                  "description": "Uncle Frank's Bait Shop",
                  "pending": true,
                  "extra": { "category": "food" }
                }
              ],
              "extra": { "account-open-date": 978360153 }
            }
          ]
        }
        """;

    private static AccountSet Deserialize(string json) =>
        JsonSerializer.Deserialize(json, SimpleFinJsonContext.Default.AccountSet)!;

    [TestMethod]
    public void Deserialize_ParsesErrors()
    {
        AccountSet set = Deserialize(SampleJson);

        Assert.AreEqual(1, set.Errors.Count);
        Assert.AreEqual("con.auth", set.Errors[0].Code);
        Assert.AreEqual("Authentication required", set.Errors[0].Message);
        Assert.AreEqual("CON-10829309823094234", set.Errors[0].ConnectionId);
    }

    [TestMethod]
    public void Deserialize_ParsesConnections()
    {
        AccountSet set = Deserialize(SampleJson);

        Connection connection = set.Connections.Single();
        Assert.AreEqual("My Bank - James", connection.Name);
        Assert.AreEqual("INST-129839182938123123", connection.OrganizationId);
        Assert.AreEqual("https://mybank.com", connection.SfinUrl);
    }

    [TestMethod]
    public void Deserialize_ParsesAccountMoneyAsDecimal()
    {
        Account account = Deserialize(SampleJson).Accounts.Single();

        Assert.AreEqual(100.23m, account.Balance);
        Assert.AreEqual(75.23m, account.AvailableBalance);
        Assert.AreEqual("USD", account.Currency);
        Assert.IsFalse(account.HasCustomCurrency);
    }

    [TestMethod]
    public void Deserialize_ParsesBalanceDateAsTimestamp()
    {
        Account account = Deserialize(SampleJson).Accounts.Single();

        Assert.AreEqual(DateTimeOffset.FromUnixTimeSeconds(978366153), account.BalanceDate);
    }

    [TestMethod]
    public void Deserialize_ParsesTransaction()
    {
        Transaction transaction = Deserialize(SampleJson).Accounts.Single().Transactions.Single();

        Assert.AreEqual("12394832938403", transaction.Id);
        Assert.AreEqual(-33293.43m, transaction.Amount);
        Assert.AreEqual("Uncle Frank's Bait Shop", transaction.Description);
        Assert.IsTrue(transaction.Pending);
        Assert.AreEqual(DateTimeOffset.FromUnixTimeSeconds(793090572), transaction.Posted);
    }

    [TestMethod]
    public void Deserialize_ParsesExtraData()
    {
        Transaction transaction = Deserialize(SampleJson).Accounts.Single().Transactions.Single();

        Assert.IsNotNull(transaction.Extra);
        Assert.AreEqual("food", transaction.Extra!["category"].GetString());
    }

    [TestMethod]
    public void Deserialize_MissingOptionalFieldsUseDefaults()
    {
        AccountSet set = Deserialize(
            """
            {
              "errlist": [],
              "connections": [],
              "accounts": [
                {
                  "id": "1",
                  "name": "Checking",
                  "currency": "USD",
                  "balance": "10.00",
                  "balance-date": 1000
                }
              ]
            }
            """
        );

        Account account = set.Accounts.Single();
        Assert.IsNull(account.AvailableBalance);
        Assert.AreEqual(0, account.Transactions.Count);
        Assert.IsNull(account.Extra);
    }

    [TestMethod]
    public void Deserialize_InvalidDecimalThrowsJsonException()
    {
        Assert.ThrowsExactly<JsonException>(() =>
            Deserialize(
                """
                {
                  "errlist": [],
                  "connections": [],
                  "accounts": [
                    {
                      "id": "1",
                      "name": "Checking",
                      "currency": "USD",
                      "balance": "not-a-decimal",
                      "balance-date": 1000
                    }
                  ]
                }
                """
            )
        );
    }

    [TestMethod]
    public void Deserialize_InvalidTimestampThrowsJsonException()
    {
        Assert.ThrowsExactly<JsonException>(() =>
            Deserialize(
                """
                {
                  "errlist": [],
                  "connections": [],
                  "accounts": [
                    {
                      "id": "1",
                      "name": "Checking",
                      "currency": "USD",
                      "balance": "10.00",
                      "balance-date": "not-a-timestamp"
                    }
                  ]
                }
                """
            )
        );
    }

    [TestMethod]
    public void Deserialize_DeprecatedErrorsList()
    {
        AccountSet set = Deserialize(
            """
            {
              "errors": ["Something went wrong"],
              "connections": [],
              "accounts": []
            }
            """
        );

        Assert.AreEqual(1, set.LegacyErrors.Count);
        Assert.AreEqual("Something went wrong", set.LegacyErrors[0]);
    }

    [TestMethod]
    public void HasCustomCurrency_TrueForUrlCurrency()
    {
        AccountSet set = Deserialize(
            """
            {
              "errlist": [],
              "connections": [],
              "accounts": [
                {
                  "id": "1",
                  "name": "Miles",
                  "currency": "https://www.example.com/flight-miles",
                  "balance": "100.23",
                  "balance-date": 1000
                }
              ]
            }
            """
        );

        Assert.IsTrue(set.Accounts.Single().HasCustomCurrency);
    }
}
