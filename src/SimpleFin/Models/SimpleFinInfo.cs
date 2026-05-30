// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace SimpleFin.Models;

/// <summary>
/// The response returned from the SimpleFIN <c>GET /info</c> endpoint, describing which
/// versions of the protocol a server supports.
/// </summary>
public sealed record SimpleFinInfo
{
    /// <summary>
    /// An array of version string prefixes that the server supports, in <c>MAJOR.MINOR.FIX</c>
    /// or <c>MAJOR.MINOR</c> format. Never <c>null</c>.
    /// </summary>
    [JsonPropertyName("versions")]
    public IReadOnlyList<string> Versions
    {
        get => _versions;
        init => _versions = value ?? [];
    }

    private readonly IReadOnlyList<string> _versions = [];
}
