// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleFin.Json;

namespace SimpleFin.Models;

/// <summary>
/// A single transaction within an <see cref="Account"/>.
/// </summary>
public sealed record Transaction
{
    /// <summary>
    /// An ID that uniquely identifies this transaction within its account. Transaction ids
    /// may be reused across accounts but never within an account.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// When the transaction posted to the account. If the transaction is pending, this may be
    /// the Unix epoch (<c>0</c>).
    /// </summary>
    [JsonPropertyName("posted")]
    [JsonConverter(typeof(UnixEpochJsonConverter))]
    public required DateTimeOffset Posted { get; init; }

    /// <summary>
    /// Amount of the transaction. Positive values indicate money deposited into the account.
    /// </summary>
    [JsonPropertyName("amount")]
    [JsonConverter(typeof(DecimalStringJsonConverter))]
    public required decimal Amount { get; init; }

    /// <summary>
    /// A human-readable description of what the transaction was for.
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>
    /// When the transaction actually happened, if provided.
    /// </summary>
    [JsonPropertyName("transacted_at")]
    [JsonConverter(typeof(NullableUnixEpochJsonConverter))]
    public DateTimeOffset? TransactedAt { get; init; }

    /// <summary>
    /// <see langword="true"/> indicates that this transaction has not yet posted. Defaults to
    /// <see langword="false"/>.
    /// </summary>
    [JsonPropertyName("pending")]
    public bool Pending { get; init; }

    /// <summary>
    /// Optional extra transaction-specific data not defined by the SimpleFIN standard.
    /// </summary>
    [JsonPropertyName("extra")]
    public IReadOnlyDictionary<string, JsonElement>? Extra { get; init; }
}
