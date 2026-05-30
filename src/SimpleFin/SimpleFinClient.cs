// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SimpleFin.Json;
using SimpleFin.Models;

namespace SimpleFin;

/// <summary>
/// A client for the application/consumer side of the SimpleFIN protocol. Use it to claim an
/// <see cref="AccessUrl"/> from a <see cref="SimpleFinToken"/> and to retrieve account and
/// transaction data.
/// </summary>
/// <remarks>
/// The client wraps an injected <see cref="HttpClient"/> and is friendly to
/// <c>IHttpClientFactory</c>. It never mutates the underlying <see cref="HttpClient"/>; all
/// authentication is applied per request.
/// </remarks>
public sealed class SimpleFinClient
{
    private readonly HttpClient _httpClient;
    private readonly AccessUrl? _accessUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleFinClient"/> class.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> used for all requests.</param>
    public SimpleFinClient(HttpClient httpClient)
        : this(httpClient, accessUrl: null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleFinClient"/> class configured with a
    /// default <see cref="AccessUrl"/> used by <see cref="GetInfoAsync"/> and
    /// <see cref="GetAccountsAsync"/> when none is supplied per call.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> used for all requests.</param>
    /// <param name="accessUrl">The default Access URL, or <see langword="null"/>.</param>
    public SimpleFinClient(HttpClient httpClient, AccessUrl? accessUrl)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        _accessUrl = accessUrl;
    }

    /// <summary>
    /// Claims an <see cref="AccessUrl"/> by POSTing to the claim URL contained in the given
    /// <see cref="SimpleFinToken"/>.
    /// </summary>
    /// <param name="token">The SimpleFIN Token received from the user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The claimed <see cref="AccessUrl"/>.</returns>
    /// <exception cref="SimpleFinException">
    /// Thrown when the claim fails. A <c>403</c> indicates the token does not exist or has
    /// already been claimed, which may mean the user's data has been compromised; the user
    /// should be advised to disable the token.
    /// </exception>
    public async Task<AccessUrl> ClaimAccessUrlAsync(
        SimpleFinToken token,
        CancellationToken cancellationToken = default
    )
    {
        Uri claimUrl = token.GetClaimUrl();

        using HttpRequestMessage request = new(HttpMethod.Post, claimUrl);
        using HttpResponseMessage response = await _httpClient
            .SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new SimpleFinException(
                "Claiming the Access URL was forbidden (403). The token may not exist or may "
                    + "already have been claimed. Advise the user to disable the token.",
                response.StatusCode
            );
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new SimpleFinException(
                $"Claiming the Access URL failed with status {(int)response.StatusCode}.",
                response.StatusCode
            );
        }

        string body = (
            await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)
        ).Trim();

        try
        {
            return AccessUrl.Parse(body);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            throw new SimpleFinException(
                "The claim response did not contain a valid Access URL.",
                response.StatusCode,
                ex
            );
        }
    }

    /// <summary>
    /// Retrieves the protocol versions supported by the SimpleFIN Server
    /// (<c>GET /info</c>).
    /// </summary>
    /// <param name="accessUrl">
    /// The Access URL to query. Falls back to the client's configured Access URL when
    /// <see langword="null"/>.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The server's <see cref="SimpleFinInfo"/>.</returns>
    public async Task<SimpleFinInfo> GetInfoAsync(
        AccessUrl? accessUrl = null,
        CancellationToken cancellationToken = default
    )
    {
        AccessUrl access = ResolveAccess(accessUrl);
        Uri requestUri = BuildUri(access, "info", query: null);

        using HttpRequestMessage request = new(HttpMethod.Get, requestUri);
        ApplyAuthentication(request, access);

        return await SendAndDeserializeAsync(
                request,
                SimpleFinJsonContext.Default.SimpleFinInfo,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves account and transaction data (<c>GET /accounts</c>).
    /// </summary>
    /// <param name="query">Optional query parameters.</param>
    /// <param name="accessUrl">
    /// The Access URL to query. Falls back to the client's configured Access URL when
    /// <see langword="null"/>.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="AccountSet"/> returned by the server.</returns>
    /// <exception cref="SimpleFinException">
    /// Thrown when the request fails, including a <c>403</c> when access has been revoked or
    /// the credentials are incorrect, or a <c>402</c> when payment is required.
    /// </exception>
    public async Task<AccountSet> GetAccountsAsync(
        AccountsQuery? query = null,
        AccessUrl? accessUrl = null,
        CancellationToken cancellationToken = default
    )
    {
        AccessUrl access = ResolveAccess(accessUrl);
        string? queryString = query?.ToQueryString();
        Uri requestUri = BuildUri(access, "accounts", queryString);

        using HttpRequestMessage request = new(HttpMethod.Get, requestUri);
        ApplyAuthentication(request, access);

        return await SendAndDeserializeAsync(
                request,
                SimpleFinJsonContext.Default.AccountSet,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    private AccessUrl ResolveAccess(AccessUrl? accessUrl)
    {
        return accessUrl
            ?? _accessUrl
            ?? throw new InvalidOperationException(
                "No Access URL was provided. Supply one to this method or configure the client "
                    + "with one."
            );
    }

    private static Uri BuildUri(AccessUrl access, string resource, string? query)
    {
        string basePath = access.BaseUrl.AbsoluteUri.TrimEnd('/');
        string url = $"{basePath}/{resource}";
        if (!string.IsNullOrEmpty(query))
        {
            url += "?" + query;
        }

        Uri uri = new(url, UriKind.Absolute);
        if (uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new SimpleFinException("SimpleFIN requests must use HTTPS.");
        }

        return uri;
    }

    private static void ApplyAuthentication(HttpRequestMessage request, AccessUrl access)
    {
        string credentials = $"{access.Username}:{access.Password}";
        string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encoded);
    }

    private async Task<T> SendAndDeserializeAsync<T>(
        HttpRequestMessage request,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken
    )
    {
        using HttpResponseMessage response = await _httpClient
            .SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new SimpleFinException(
                $"SimpleFIN request to '{request.RequestUri}' failed with status "
                    + $"{(int)response.StatusCode}.",
                response.StatusCode
            );
        }

        await using Stream stream = await response
            .Content.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        T? result;
        try
        {
            result = await JsonSerializer
                .DeserializeAsync(stream, typeInfo, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (JsonException ex)
        {
            throw new SimpleFinException(
                "The SimpleFIN response could not be parsed as JSON.",
                response.StatusCode,
                ex
            );
        }

        return result
            ?? throw new SimpleFinException(
                "The SimpleFIN response body was empty.",
                response.StatusCode
            );
    }
}
