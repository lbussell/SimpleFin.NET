// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleFin.Tests;

[TestClass]
public class AccessUrlTests
{
    private const string Host = "bridge.simplefin.org";

    // Build the '@' separator via interpolation so URL literals are not mistaken for emails.
    private const char At = '@';

    [TestMethod]
    public void Parse_ExtractsCredentialsAndBaseUrl()
    {
        AccessUrl access = AccessUrl.Parse($"https://user123:secret{At}{Host}/simplefin");

        Assert.AreEqual("user123", access.Username);
        Assert.AreEqual("secret", access.Password);
        Assert.AreEqual("https://bridge.simplefin.org/simplefin", access.BaseUrl.AbsoluteUri);
    }

    [TestMethod]
    public void Parse_HandlesMissingCredentials()
    {
        AccessUrl access = AccessUrl.Parse("https://bridge.simplefin.org/simplefin");

        Assert.AreEqual(string.Empty, access.Username);
        Assert.AreEqual(string.Empty, access.Password);
    }

    [TestMethod]
    public void Parse_DecodesPercentEncodedCredentials()
    {
        AccessUrl access = AccessUrl.Parse($"https://us%40r:p%3Ass{At}example.com/sfin");

        Assert.AreEqual("us@r", access.Username);
        Assert.AreEqual("p:ss", access.Password);
    }

    [TestMethod]
    public void Parse_ThrowsOnNonHttps()
    {
        Assert.ThrowsExactly<FormatException>(() =>
            AccessUrl.Parse($"http://user:secret{At}example.com/sfin")
        );
    }

    [TestMethod]
    public void ToString_RoundTripsCredentials()
    {
        AccessUrl original = AccessUrl.Parse($"https://user123:secret{At}{Host}/simplefin");

        AccessUrl roundTripped = AccessUrl.Parse(original.ToString());

        Assert.AreEqual(original.Username, roundTripped.Username);
        Assert.AreEqual(original.Password, roundTripped.Password);
        Assert.AreEqual(original.BaseUrl, roundTripped.BaseUrl);
    }
}
