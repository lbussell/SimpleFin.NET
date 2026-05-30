// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace SimpleFin.Models;

/// <summary>
/// A structured error returned by a SimpleFIN Server in an <see cref="AccountSet.Errors"/>
/// list. See the SimpleFIN protocol's Error object.
/// </summary>
public sealed record SimpleFinError
{
    /// <summary>
    /// One of the SimpleFIN error codes, in the format <c>prefix.[subcode]</c> where
    /// <c>prefix</c> is <c>gen</c>, <c>con</c>, or <c>act</c>. Consumers should handle unknown
    /// subcodes by falling back to the naked prefix.
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    /// <summary>
    /// A human-readable error message suitable for displaying to users. Always sanitize
    /// before display.
    /// </summary>
    [JsonPropertyName("msg")]
    public required string Message { get; init; }

    /// <summary>
    /// The connection id, supplied only when the error is specific to a particular
    /// <see cref="Connection"/>.
    /// </summary>
    [JsonPropertyName("conn_id")]
    public string? ConnectionId { get; init; }

    /// <summary>
    /// The account id, supplied only when the error is specific to a particular
    /// <see cref="Account"/>.
    /// </summary>
    [JsonPropertyName("account_id")]
    public string? AccountId { get; init; }
}
