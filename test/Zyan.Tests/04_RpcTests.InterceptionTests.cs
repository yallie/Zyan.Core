using System.Threading.Tasks;
using Xunit;
using Zyan.Communication;
using Zyan.Communication.CallInterception;
using Zyan.Tests.Tools;

namespace Zyan.Tests;

public partial class RpcTests : TestBase
{
    [Fact]
    public void SyncCallInterception_works_and_supports_PauseInterception()
    {
        var config = ConnConfig;

        var intercepted = false;
        config.CallInterceptors.Add(
            CallInterceptor.For<IHelloServer>().Func<string, string>(
            (comp, arg) => comp.Hello(arg),
            (data, arg) =>
            {
                if (arg != "Hello")
                {
                    intercepted = true;
                    data.Intercepted = true;
                    return "Goodbye!";
                }

                return data.MakeRemoteCall();
            }));

        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>();
        using var conn = new ZyanConnection(config);

        var proxy = conn.CreateProxy<IHelloServer>();
        var result = proxy.Hello("Hello");

        // no interception
        Assert.False(intercepted);
        Assert.Equal("Hello World!", result);

        // interception succeeded
        result = proxy.Hello("Hi there!");
        Assert.True(intercepted);
        Assert.Equal("Goodbye!", result);

        // interception is paused
        intercepted = false;
        using (CallInterceptor.PauseInterception())
        {
            result = proxy.Hello("Hi there");
            Assert.False(intercepted);
            Assert.Equal("Hi there World!", result);
        }

        // interception unpaused
        result = proxy.Hello("Anybody?");
        Assert.True(intercepted);
        Assert.Equal("Goodbye!", result);
    }

    [Fact]
    public async Task AsyncCallInterception_works_and_supports_PauseInterception()
    {
        var config = ConnConfig;

        var intercepted = false;
        config.CallInterceptors.Add(
            CallInterceptor.For<IHelloServer>().Func<string, Task<string>>(
            (comp, arg) => comp.HelloAsync(arg),
            (data, arg) =>
            {
                if (arg != "Hello")
                {
                    intercepted = true;
                    data.Intercepted = true;
                    return Task.FromResult("Goodbye!");
                }

                var task = data.MakeRemoteCall();
                return task;
            }));

        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>();
        using var conn = new ZyanConnection(config);

        var proxy = conn.CreateProxy<IHelloServer>();
        var result = await proxy.HelloAsync("Hello");

        // no interception
        Assert.False(intercepted);
        Assert.Equal("Hello World!", result);

        // interception succeeded
        result = await proxy.HelloAsync("Hi there!");
        Assert.True(intercepted);
        Assert.Equal("Goodbye!", result);

        // interception is paused
        intercepted = false;
        using (CallInterceptor.PauseInterception())
        {
            result = await proxy.HelloAsync("Hi there");
            Assert.False(intercepted);
            Assert.Equal("Hi there World!", result);
        }

        // interception unpaused
        result = await proxy.HelloAsync("Anybody?");
        Assert.True(intercepted);
        Assert.Equal("Goodbye!", result);
    }
}