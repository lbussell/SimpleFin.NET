// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleFin.Tests;

[TestClass]
public class SimpleFinTokenTests
{
    private const string ClaimUrl = "https://bridge.simplefin.org/simplefin/claim/demo";

    private static string EncodeToken(string url) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(url));

    [TestMethod]
    public void GetClaimUrl_DecodesBase64ToUrl()
    {
        SimpleFinToken token = new(EncodeToken(ClaimUrl));

        Uri result = token.GetClaimUrl();

        Assert.AreEqual(ClaimUrl, result.AbsoluteUri);
    }

    [TestMethod]
    public void GetClaimUrl_MatchesProtocolDemoToken()
    {
        // The exact demo token from the SimpleFIN protocol documentation.
        SimpleFinToken token = new(
            "aHR0cHM6Ly9icmlkZ2Uuc2ltcGxlZmluLm9yZy9zaW1wbGVmaW4vY2xhaW0vZGVtbw=="
        );

        Uri result = token.GetClaimUrl();

        Assert.AreEqual(ClaimUrl, result.AbsoluteUri);
    }

    [TestMethod]
    public void Constructor_TrimsWhitespace()
    {
        SimpleFinToken token = new("  " + EncodeToken(ClaimUrl) + "\n");

        Assert.AreEqual(ClaimUrl, token.GetClaimUrl().AbsoluteUri);
    }

    [TestMethod]
    public void Constructor_ThrowsOnEmpty()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new SimpleFinToken(" "));
    }

    [TestMethod]
    public void TryParse_ReturnsTokenWhenValid()
    {
        bool parsed = SimpleFinToken.TryParse(EncodeToken(ClaimUrl), out SimpleFinToken token);

        Assert.IsTrue(parsed);
        Assert.AreEqual(ClaimUrl, token.GetClaimUrl().AbsoluteUri);
    }

    [TestMethod]
    public void TryParse_ReturnsFalseWhenInvalid()
    {
        bool parsed = SimpleFinToken.TryParse("not-valid-base64!!!", out SimpleFinToken token);

        Assert.IsFalse(parsed);
        Assert.AreEqual(default, token);
    }

    [TestMethod]
    public void TryParse_ReturnsFalseWhenDecodedUrlIsNotHttps()
    {
        bool parsed = SimpleFinToken.TryParse(
            EncodeToken("http://insecure.example.com/claim"),
            out SimpleFinToken token
        );

        Assert.IsFalse(parsed);
        Assert.AreEqual(default, token);
    }

    [TestMethod]
    public void GetClaimUrl_ThrowsOnInvalidBase64()
    {
        SimpleFinToken token = new("not-valid-base64!!!");

        Assert.ThrowsExactly<FormatException>(() => token.GetClaimUrl());
    }

    [TestMethod]
    public void GetClaimUrl_ThrowsWhenDecodedUrlIsNotHttps()
    {
        SimpleFinToken token = new(EncodeToken("http://insecure.example.com/claim"));

        Assert.ThrowsExactly<FormatException>(() => token.GetClaimUrl());
    }
}
