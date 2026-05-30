// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace SimpleFin.Models;

/// <summary>
/// A single connection to a financial institution. Users with two sets of login
/// credentials for a particular bank will have two different connections, each with the
/// same <c>org_*</c> fields.
/// </summary>
public sealed record Connection
{
    /// <summary>
    /// ID of a particular connection for a financial institution.
    /// </summary>
    [JsonPropertyName("conn_id")]
    public required string ConnectionId { get; init; }

    /// <summary>
    /// Human-friendly name for this connection, including the financial institution name.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// ID of the financial institution. Unique per SimpleFIN server, but not guaranteed to be
    /// globally unique.
    /// </summary>
    [JsonPropertyName("org_id")]
    public required string OrganizationId { get; init; }

    /// <summary>
    /// Domain name of the financial institution, if provided.
    /// </summary>
    [JsonPropertyName("org_url")]
    public string? OrganizationUrl { get; init; }

    /// <summary>
    /// Human-readable name of the financial institution, if provided.
    /// </summary>
    [JsonPropertyName("org_name")]
    public string? OrganizationName { get; init; }

    /// <summary>
    /// Root URL of the organization's SimpleFIN Server.
    /// </summary>
    [JsonPropertyName("sfin_url")]
    public required string SfinUrl { get; init; }
}
