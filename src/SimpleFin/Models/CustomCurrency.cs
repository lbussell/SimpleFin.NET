// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace SimpleFin.Models;

/// <summary>
/// Metadata about a custom currency, resolved from an account's currency URL. See the
/// SimpleFIN protocol's Custom Currencies section.
/// </summary>
public sealed record CustomCurrency
{
    /// <summary>
    /// Human-readable name of the currency. Sanitize before display.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Human-readable short name of the currency. Sanitize before display.
    /// </summary>
    [JsonPropertyName("abbr")]
    public required string Abbreviation { get; init; }
}
