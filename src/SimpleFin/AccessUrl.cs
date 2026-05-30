// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

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
        if (string.IsNullOrWhiteSpace(accessUrl))
        {
            throw new ArgumentException("Access URL must not be empty.", nameof(accessUrl));
        }

        string trimmed = accessUrl.Trim();

        int schemeIndex = trimmed.IndexOf("://", StringComparison.Ordinal);
        if (schemeIndex < 0)
        {
            throw new FormatException("The Access URL is not an absolute URL.");
        }

        string scheme = trimmed[..schemeIndex];
        string rest = trimmed[(schemeIndex + 3)..];

        // Credentials, when present, live in the authority (before the first path '/').
        int pathIndex = rest.IndexOf('/');
        string authority = pathIndex < 0 ? rest : rest[..pathIndex];
        string remainder = pathIndex < 0 ? string.Empty : rest[pathIndex..];

        string username = string.Empty;
        string password = string.Empty;
        int atIndex = authority.LastIndexOf('@');
        if (atIndex >= 0)
        {
            string userInfo = authority[..atIndex];
            authority = authority[(atIndex + 1)..];

            string[] parts = userInfo.Split(':', 2);
            username = Uri.UnescapeDataString(parts[0]);
            password = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
        }

        if (
            !Uri.TryCreate($"{scheme}://{authority}{remainder}", UriKind.Absolute, out Uri? baseUrl)
        )
        {
            throw new FormatException("The Access URL is not an absolute URL.");
        }

        if (baseUrl.Scheme != Uri.UriSchemeHttps)
        {
            throw new FormatException("The Access URL must use HTTPS.");
        }

        return new AccessUrl(baseUrl, username, password);
    }

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
        return builder.Uri.ToString();
    }
}
