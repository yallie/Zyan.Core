using System;
using System.Threading.Tasks;
using CoreRemoting.Authentication;

namespace Zyan.Tests.Tools;

internal class FakeAuthProvider(Func<Credential[], bool> authenticateFake = null) : IAuthenticationProvider
{
    public bool Authenticate(Credential[] credentials, out RemotingIdentity authenticatedIdentity)
    {
        authenticatedIdentity = new RemotingIdentity
        {
            AuthenticationType = "Fake",
            Domain = "None",
            IsAuthenticated = true,
            Name = "root",
            Roles = ["Admin"]
        };

        return authenticateFake?.Invoke(credentials) ?? false;
    }

    public Task<AuthenticationResponseMessage> Authenticate(AuthenticationRequestMessage request)
    {
        var success = Authenticate(request.Credentials, out var identity);
        return Task.FromResult(new AuthenticationResponseMessage
        {
            AuthenticatedIdentity = identity,
            IsAuthenticated = success,
        });
    }
}
