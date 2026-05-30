// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleFin.Json;

namespace SimpleFin.Models;

/// <summary>
/// A financial account belonging to a <see cref="Connection"/>.
/// </summary>
public sealed record Account
{
    /// <summary>
    /// A string that uniquely identifies the account within its connection.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// A name that uniquely describes this account among the user's other accounts.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// ID of the account's <see cref="Connection"/>.
    /// </summary>
    [JsonPropertyName("conn_id")]
    public string? ConnectionId { get; init; }

    /// <summary>
    /// The currency of the account. For standard currencies this is the ISO 4217 currency
    /// code (for example <c>"USD"</c>). For custom currencies this is a URL that resolves to a
    /// <see cref="CustomCurrency"/>.
    /// </summary>
    [JsonPropertyName("currency")]
    public required string Currency { get; init; }

    /// <summary>
    /// The balance of the account as of <see cref="BalanceDate"/>.
    /// </summary>
    [JsonPropertyName("balance")]
    [JsonConverter(typeof(DecimalStringJsonConverter))]
    public required decimal Balance { get; init; }

    /// <summary>
    /// The available balance of the account as of <see cref="BalanceDate"/>. Omitted when it
    /// is the same as <see cref="Balance"/>.
    /// </summary>
    [JsonPropertyName("available-balance")]
    [JsonConverter(typeof(NullableDecimalStringJsonConverter))]
    public decimal? AvailableBalance { get; init; }

    /// <summary>
    /// The timestamp when the balance and available balance became what they are.
    /// </summary>
    [JsonPropertyName("balance-date")]
    [JsonConverter(typeof(UnixEpochJsonConverter))]
    public required DateTimeOffset BalanceDate { get; init; }

    /// <summary>
    /// A subset of transactions for this account, ordered by <see cref="Transaction.Posted"/>.
    /// May be empty when no transaction data was requested or available. Never <c>null</c>.
    /// </summary>
    [JsonPropertyName("transactions")]
    public IReadOnlyList<Transaction> Transactions
    {
        get => _transactions;
        init => _transactions = value ?? [];
    }

    private readonly IReadOnlyList<Transaction> _transactions = [];

    /// <summary>
    /// Optional extra account-specific data not defined by the SimpleFIN standard.
    /// </summary>
    [JsonPropertyName("extra")]
    public IReadOnlyDictionary<string, JsonElement>? Extra { get; init; }

    /// <summary>
    /// Gets a value indicating whether <see cref="Currency"/> refers to a custom currency
    /// (an absolute URL) rather than an ISO 4217 currency code.
    /// </summary>
    [JsonIgnore]
    public bool HasCustomCurrency =>
        Uri.TryCreate(Currency, UriKind.Absolute, out Uri? uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
