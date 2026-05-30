// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Net;

namespace SimpleFin.Tests;

/// <summary>
/// A test <see cref="HttpMessageHandler"/> that returns canned responses and records the
/// requests it receives.
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    public StubHttpMessageHandler(HttpStatusCode statusCode, string content)
        : this(_ => new HttpResponseMessage(statusCode) { Content = new StringContent(content) })
    { }

    public List<HttpRequestMessage> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        Requests.Add(request);
        return Task.FromResult(_responder(request));
    }
}
