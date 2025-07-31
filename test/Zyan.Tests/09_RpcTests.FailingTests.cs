using System;
using System.Threading.Tasks;
using Xunit;
using Zyan.Communication;
using Zyan.Tests.Tools;

namespace Zyan.Tests;

public partial class RpcTests : TestBase
{
    // [Fact] ValueTask<T> return type is not supported
    internal async Task AsyncValueTaskRpcTest()
    {
        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>();
        using var conn = new ZyanConnection(ConnConfig);

        var proxy = conn.CreateProxy<IHelloServer>();
        var result = await proxy.HelloValueTaskAsync("Hello");
        Assert.Equal("Hello World!", result);
    }

    // [Fact] // Fails: two channels try to listen on the same port
    internal async Task SameHostConfig()
    {
        // HostConfig creates a new instance every time
        var mainConfig = HostConfig;
        var clusterConfig = HostConfig;

        using (var host = new ZyanComponentHost(mainConfig).RegisterComponent<ISessionServer, SessionServer>())
        using (var cluster = new ZyanComponentHost(clusterConfig).RegisterComponent<IHelloServer, HelloServer>())
        using (var conn = new ZyanConnection(ConnConfig))
        {
            var proxy = conn.CreateProxy<ISessionServer>();
            Assert.True(conn.RemotingClient.HasSession);

            var sid1 = proxy.GetSessionID();
            var sid2 = await proxy.GetSessionIDAsync();
            Assert.NotEqual(sid1, Guid.Empty);
            Assert.Equal(sid1, sid2);
        }
    }
}