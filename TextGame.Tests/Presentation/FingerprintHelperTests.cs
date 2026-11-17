using System.Net;
using Microsoft.AspNetCore.Http;
using TextGame.Presentation.Helpers;

namespace TextGame.Tests.Presentation;

/// <summary>
/// Fingerprint (User-Agent + IP) is hashed into every refresh token, so login and the rotation
/// middleware must build the exact same string — otherwise a rotated token never matches the
/// stored hash and the player is logged out. notes/AGENTS.md, «Термины»: one helper now owns it.
/// </summary>
public class FingerprintHelperTests
{
    [Fact]
    public void GetFingerprint_WithUserAgentAndIp_JoinsThemWithSeparator()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.UserAgent = "Mozilla/5.0";
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");

        // Act
        string fingerprint = context.GetFingerprint();

        // Assert
        fingerprint.Should().Be("Mozilla/5.0 | 10.0.0.1");
    }

    [Fact]
    public void GetFingerprint_WithoutRemoteIp_LeavesSecondPartEmpty()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.UserAgent = "Mozilla/5.0";

        // Act
        string fingerprint = context.GetFingerprint();

        // Assert
        // The old duplicated implementations returned "UA | " for a missing IP; the contract
        // stays the same so fingerprints stored by earlier logins keep matching.
        fingerprint.Should().Be("Mozilla/5.0 | ");
    }
}
