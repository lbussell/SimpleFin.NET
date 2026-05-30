// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Net;

namespace SimpleFin;

/// <summary>
/// The exception thrown when a SimpleFIN request fails, for example when claiming an Access
/// URL or retrieving accounts returns a non-success HTTP status.
/// </summary>
public sealed class SimpleFinException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleFinException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code that triggered the exception, if any.</param>
    /// <param name="innerException">The inner exception, if any.</param>
    public SimpleFinException(
        string message,
        HttpStatusCode? statusCode = null,
        Exception? innerException = null
    )
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the HTTP status code that triggered the exception, if applicable.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }
}
