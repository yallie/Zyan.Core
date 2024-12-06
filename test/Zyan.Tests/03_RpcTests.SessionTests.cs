using System;
using System.Threading.Tasks;
using Xunit;
using Zyan.Communication;
using Zyan.Tests.Tools;
using Test = Xunit.FactAttribute;

namespace Zyan.Tests;

public partial class RpcTests : TestBase
{
    [Test]
    public async Task SessionTest()
    {
        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<ISessionServer, SessionServer>();
        using var conn = new ZyanConnection(ConnConfig);
        var proxy = conn.CreateProxy<ISessionServer>();
        Assert.True(conn.RemotingClient.HasSession);

        var sid1 = proxy.GetSessionID();
        var sid2 = await proxy.GetSessionIDAsync();
        Assert.NotEqual(sid1, Guid.Empty);
        Assert.Equal(sid1, sid2);
    }

    [Test]
    public void CurrentSessionCapturedByComponentConstructorIsTheSame()
    {
        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<ISessionServer, SessionServer>();
        using var conn = new ZyanConnection(ConnConfig);
        var proxy = conn.CreateProxy<ISessionServer>();

        Assert.True(conn.RemotingClient.HasSession);
        Assert.True(proxy.SessionsAreSame());
    }
}