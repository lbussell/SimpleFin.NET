// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace SimpleFin.Models;

/// <summary>
/// The response returned from the SimpleFIN <c>GET /accounts</c> endpoint.
/// </summary>
public sealed record AccountSet
{
    /// <summary>
    /// List of structured errors. Prefer this over the deprecated <see cref="LegacyErrors"/>.
    /// Never <c>null</c>.
    /// </summary>
    [JsonPropertyName("errlist")]
    public IReadOnlyList<SimpleFinError> Errors
    {
        get => _errors;
        init => _errors = value ?? [];
    }

    private readonly IReadOnlyList<SimpleFinError> _errors = [];

    /// <summary>
    /// Deprecated list of plain error strings suitable for display. Superseded by
    /// <see cref="Errors"/>; retained for backward compatibility with v1 servers. Never
    /// <c>null</c>.
    /// </summary>
    [JsonPropertyName("errors")]
    public IReadOnlyList<string> LegacyErrors
    {
        get => _legacyErrors;
        init => _legacyErrors = value ?? [];
    }

    private readonly IReadOnlyList<string> _legacyErrors = [];

    /// <summary>
    /// List of connections represented in this account set. Never <c>null</c>.
    /// </summary>
    [JsonPropertyName("connections")]
    public IReadOnlyList<Connection> Connections
    {
        get => _connections;
        init => _connections = value ?? [];
    }

    private readonly IReadOnlyList<Connection> _connections = [];

    /// <summary>
    /// List of accounts. Never <c>null</c>.
    /// </summary>
    [JsonPropertyName("accounts")]
    public IReadOnlyList<Account> Accounts
    {
        get => _accounts;
        init => _accounts = value ?? [];
    }

    private readonly IReadOnlyList<Account> _accounts = [];
}
