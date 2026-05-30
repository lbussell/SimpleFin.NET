// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;

namespace SimpleFin;

/// <summary>
/// A SimpleFIN Token: a Base64-encoded one-time-use claim URL handed to an application by a
/// user. Decode it with <see cref="GetClaimUrl"/> and exchange it for an
/// <see cref="AccessUrl"/> using <see cref="SimpleFinClient.ClaimAccessUrlAsync"/>.
/// </summary>
public readonly record struct SimpleFinToken
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleFinToken"/> struct.
    /// </summary>
    /// <param name="value">The raw Base64-encoded token value.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is null, empty, or whitespace.
    /// </exception>
    public SimpleFinToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Token must not be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    /// <summary>
    /// Gets the raw Base64-encoded token value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Decodes the token and returns the one-time-use claim URL it contains.
    /// </summary>
    /// <returns>The HTTPS claim URL to POST to in order to obtain an Access URL.</returns>
    /// <exception cref="FormatException">
    /// Thrown when the token is not valid Base64 or does not decode to an absolute HTTPS URL.
    /// </exception>
    public Uri GetClaimUrl()
    {
        byte[] decoded;
        try
        {
            decoded = Convert.FromBase64String(Value);
        }
        catch (FormatException ex)
        {
            throw new FormatException("The SimpleFIN Token is not valid Base64.", ex);
        }

        string url = Encoding.UTF8.GetString(decoded);
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            throw new FormatException("The decoded SimpleFIN Token is not an absolute URL.");
        }

        if (uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new FormatException("The decoded SimpleFIN Token must use HTTPS.");
        }

        return uri;
    }

    /// <summary>
    /// Returns the raw token value.
    /// </summary>
    /// <returns>The raw Base64-encoded token value.</returns>
    public override string ToString() => Value;
}
