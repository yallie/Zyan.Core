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

    // [Test] // Hangs up!
    private async Task ReconnectTest()
    {
        using (var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>())
        using (var conn = new ZyanConnection(ConnConfig))
        {
            var proxy = conn.CreateProxy<IHelloServer>();
            var result = await proxy.HelloAsync("Hello");
            Assert.Equal("Hello World!", result);

            conn.Disconnect();
            await Task.Delay(TimeSpan.FromSeconds(2));

            // FAILS to reconnect because the client tcp channel is null
            conn.Connect();

            result = await proxy.HelloAsync("Goodbye");
            Assert.Equal("Goodbye World!", result);
        }
    }
}