// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace SimpleFin;

/// <summary>
/// Optional query parameters for the SimpleFIN <c>GET /accounts</c> endpoint.
/// </summary>
public sealed record AccountsQuery
{
    private const string VersionQueryParameter = "version=2";

    /// <summary>
    /// If set, transactions are restricted to those on or after this timestamp.
    /// </summary>
    public DateTimeOffset? StartDate { get; init; }

    /// <summary>
    /// If set, transactions are restricted to those strictly before this timestamp.
    /// </summary>
    public DateTimeOffset? EndDate { get; init; }

    /// <summary>
    /// If <see langword="true"/>, pending transactions are included when supported by the
    /// server. Defaults to <see langword="false"/>.
    /// </summary>
    public bool IncludePending { get; init; }

    /// <summary>
    /// If <see langword="true"/>, no transaction data is returned (<c>balances-only=1</c>).
    /// </summary>
    public bool BalancesOnly { get; init; }

    /// <summary>
    /// If set, only the accounts with these ids are returned.
    /// </summary>
    public IReadOnlyList<string>? AccountIds { get; init; }

    /// <summary>
    /// Builds the query string (without a leading <c>?</c>) for this query.
    /// </summary>
    /// <returns>The encoded query string.</returns>
    internal string ToQueryString()
    {
        List<string> parts = [VersionQueryParameter];

        if (StartDate is { } start)
        {
            parts.Add(
                $"start-date={start.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)}"
            );
        }

        if (EndDate is { } end)
        {
            parts.Add($"end-date={end.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)}");
        }

        if (IncludePending)
        {
            parts.Add("pending=1");
        }

        if (BalancesOnly)
        {
            parts.Add("balances-only=1");
        }

        if (AccountIds is not null)
        {
            foreach (string accountId in AccountIds)
            {
                parts.Add($"account={Uri.EscapeDataString(accountId)}");
            }
        }

        return string.Join('&', parts);
    }

    internal static string ToQueryString(AccountsQuery? query) =>
        query?.ToQueryString() ?? VersionQueryParameter;
}
