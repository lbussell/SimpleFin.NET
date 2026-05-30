// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace SimpleFin;

/// <summary>
/// A SimpleFIN Access URL: an HTTPS URL with embedded HTTP Basic Auth credentials, obtained
/// by claiming a <see cref="SimpleFinToken"/>. Store this securely; it grants read-only
/// access to the user's financial data.
/// </summary>
public sealed record AccessUrl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccessUrl"/> record.
    /// </summary>
    /// <param name="baseUrl">The base URL with no embedded credentials (e.g. the SimpleFIN root URL).</param>
    /// <param name="username">The HTTP Basic Auth username.</param>
    /// <param name="password">The HTTP Basic Auth password.</param>
    public AccessUrl(Uri baseUrl, string username, string password)
    {
        ArgumentNullException.ThrowIfNull(baseUrl);
        BaseUrl = baseUrl;
        Username = username ?? string.Empty;
        Password = password ?? string.Empty;
    }

    /// <summary>
    /// Gets the base URL (the SimpleFIN root URL) without embedded credentials. Endpoint paths
    /// such as <c>/accounts</c> are relative to this.
    /// </summary>
    public Uri BaseUrl { get; }

    /// <summary>
    /// Gets the HTTP Basic Auth username.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Gets the HTTP Basic Auth password.
    /// </summary>
    public string Password { get; }

    /// <summary>
    /// Parses a full Access URL string of the form
    /// <c>https://username:password@host/simplefin</c> into an <see cref="AccessUrl"/>.
    /// </summary>
    /// <param name="accessUrl">The Access URL string to parse.</param>
    /// <returns>The parsed <see cref="AccessUrl"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="accessUrl"/> is null or empty.
    /// </exception>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="accessUrl"/> is not an absolute HTTPS URL.
    /// </exception>
    public static AccessUrl Parse(string accessUrl)
    {
        if (!TryParse(accessUrl, out AccessUrl? result, out string error))
        {
            if (string.IsNullOrWhiteSpace(accessUrl))
            {
                throw new ArgumentException(error, nameof(accessUrl));
            }

            throw new FormatException(error);
        }

        return result;
    }

    /// <summary>
    /// Attempts to parse a full Access URL string of the form
    /// <c>https://username:password@host/simplefin</c> into an <see cref="AccessUrl"/>.
    /// </summary>
    /// <param name="accessUrl">The Access URL string to parse.</param>
    /// <param name="result">
    /// The parsed <see cref="AccessUrl"/> when parsing succeeds; otherwise <see langword="null"/>.
    /// </param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(string? accessUrl, [NotNullWhen(true)] out AccessUrl? result) =>
        TryParse(accessUrl, out result, out _);

    /// <summary>
    /// Returns the full Access URL string, including embedded Basic Auth credentials, suitable
    /// for secure storage.
    /// </summary>
    /// <returns>The full Access URL string.</returns>
    public override string ToString()
    {
        UriBuilder builder = new(BaseUrl)
        {
            UserName = Uri.EscapeDataString(Username),
            Password = Uri.EscapeDataString(Password),
        };
        return builder.Uri.AbsoluteUri;
    }

    private static bool TryParse(
        string? accessUrl,
        [NotNullWhen(true)] out AccessUrl? result,
        out string error
    )
    {
        result = null;

        if (string.IsNullOrWhiteSpace(accessUrl))
        {
            error = "Access URL must not be empty.";
            return false;
        }

        if (!Uri.TryCreate(accessUrl.Trim(), UriKind.Absolute, out Uri? uri))
        {
            error = "The Access URL is not an absolute URL.";
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttps)
        {
            error = "The Access URL must use HTTPS.";
            return false;
        }

        string username = string.Empty;
        string password = string.Empty;
        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            string[] parts = uri.UserInfo.Split(':', 2);
            username = Uri.UnescapeDataString(parts[0]);
            password = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
        }

        UriBuilder builder = new(uri) { UserName = string.Empty, Password = string.Empty };
        result = new AccessUrl(builder.Uri, username, password);
        error = string.Empty;
        return true;
    }
}
