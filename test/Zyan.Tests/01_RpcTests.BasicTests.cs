using System.Threading.Tasks;
using Xunit;
using Zyan.Communication;
using Zyan.Tests.Tools;
using Test = Xunit.FactAttribute;
using System;
using CoreRemoting;
using CoreRemoting.Serialization;

namespace Zyan.Tests;

public partial class RpcTests : TestBase
{
    [Test]
    public void SyncRpcTest()
    {
        using (var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>())
        using (var conn = new ZyanConnection(ConnConfig))
        {
            var proxy = conn.CreateProxy<IHelloServer>();
            var result = proxy.Hello("Hello");
            Assert.Equal("Hello World!", result);
        }
    }

    [Test]
    public async Task AsyncRpcTest()
    {
        using (var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>())
        using (var conn = new ZyanConnection(ConnConfig))
        {
            var proxy = conn.CreateProxy<IHelloServer>();
            var result = await proxy.HelloAsync("Hello");
            Assert.Equal("Hello World!", result);
        }
    }

    [Test]
    public void SyncExceptionTest()
    {
        using (var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>())
        using (var conn = new ZyanConnection(ConnConfig))
        {
            var proxy = conn.CreateProxy<IHelloServer>();
            var ex = Assert.Throws<RemoteInvocationException>(() =>
                    proxy.Error("Sync"))
                .GetInnermostException();

            Assert.Equal("Sync", ex.Message);
        }
    }

    [Test]
    public async Task AsyncExceptionTest()
    {
        using (var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>())
        using (var conn = new ZyanConnection(ConnConfig))
        {
            var proxy = conn.CreateProxy<IHelloServer>();
            var ex = (await Assert.ThrowsAsync<RemoteInvocationException>(
                    async () => await proxy.ErrorAsync("Async")))
                .GetInnermostException();

            Assert.Equal("Async", ex.Message);
        }
    }

    [Test]
    public void NonSerializableExceptionTest()
    {
        using (var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>())
        using (var conn = new ZyanConnection(ConnConfig))
        {
            var proxy = conn.CreateProxy<IHelloServer>();
            var ex = Assert.Throws<RemoteInvocationException>(() =>
                    proxy.NonSerializableError("Hello", "Serializable", "World"))
                .GetInnermostException();

            Assert.NotNull(ex);
            Assert.IsType<SerializableException>(ex);

            if (ex is SerializableException sx)
            {
                Assert.Equal("NonSerializable", sx.SourceTypeName);
                Assert.Equal("Hello", ex.Message);
                Assert.Equal("Serializable", ex.Data["Serializable"]);
                Assert.Equal("World", ex.Data["World"]);
                Assert.NotNull(ex.StackTrace);
            }
        }
    }

    [Test]
    public void Certain_services_can_be_excluded_from_authentication()
    {
        var hostConfig = HostConfig;
        hostConfig.AuthenticationRequired = true;
        hostConfig.AuthenticationProvider = new FakeAuthProvider(c => true);

        // authenticated client
        var connConfig1 = ConnConfig;
        connConfig1.Credentials = [new()];

        // not authenticated client
        var connConfig2 = ConnConfig;

        // host requires authentication
        using var host = new ZyanComponentHost(hostConfig)
            .RegisterComponent<IHelloServer, HelloServer>()
            .RegisterComponent<ISessionServer, SessionServer>();

        // session server doesn't require authentication
        host.BeforeInvoke += (s, e) =>
            e.Context.AuthenticationRequired =
                !e.InterfaceName.Contains("SessionServer");

        // authentication succeeded
        using var conn1 = new ZyanConnection(connConfig1);
        var hello1 = conn1.CreateProxy<IHelloServer>();
        Assert.Equal("Hello World!", hello1.Hello("Hello"));
        var sess1 = conn1.CreateProxy<ISessionServer>();
        Assert.NotEqual(Guid.Empty, sess1.GetSessionID());

        // authentication not required
        using var conn2 = new ZyanConnection(connConfig2);
        var sess2 = conn2.CreateProxy<ISessionServer>();
        Assert.NotEqual(Guid.Empty, sess2.GetSessionID());
        Assert.NotEqual(sess1.GetSessionID(), sess2.GetSessionID());

        // authentication required
        var hello2 = conn2.CreateProxy<IHelloServer>();
        var ex = Assert.Throws<RemoteInvocationException>(() => hello2.Hello("Hello"));

        Assert.NotNull(ex);
        Assert.Contains("auth", ex.Message);
    }
}