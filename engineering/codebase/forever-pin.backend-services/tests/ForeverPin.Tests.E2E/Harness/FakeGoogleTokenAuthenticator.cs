using WoW.Two.Sdk.Beta.Be.Identity.OAuth.Google;
using WoW.Two.Sdk.Beta.Be.Identity.OAuth.Google.Authenticators;

namespace ForeverPin.Tests.E2E.Harness;

/// <summary>Provides an authenticator that accepts only <c>fake:{subject}:{email}:{name}</c> tokens.</summary>
public sealed class FakeGoogleTokenAuthenticator : IGoogleIdTokenAuthenticator
{
    /// <inheritdoc />
    public Task<GoogleVerifiedIdentity?> AuthenticateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var parts = idToken.Split(':', 4);

        var identity = parts is ["fake", var subject, var email, var name]
            ? new GoogleVerifiedIdentity { Subject = subject, Email = email, Name = name, Picture = null }
            : null;

        return Task.FromResult(identity);
    }
}
