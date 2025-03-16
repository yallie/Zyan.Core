using System.Threading.Tasks;
using Xunit;
using Zyan.Communication;
using Zyan.Communication.CallInterception;
using Zyan.Tests.Tools;

namespace Zyan.Tests;

public partial class RpcTests : TestBase
{
    [Fact]
    public void SyncCallInterception_doesnt_work_yet()
    {
        var config = ConnConfig;

        var intercepted = false;
        config.CallInterceptors.Add(
            CallInterceptor.For<IHelloServer>().Func<string, string>(
            (comp, arg) => comp.Hello(arg),
            (data, arg) =>
            {
                if (arg == "Hello")
                {
                    intercepted = true;
                    data.Intercepted = true;
                    return "Goodbye";
                }

                return data.MakeRemoteCall();
            }));

        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>();
        using var conn = new ZyanConnection(config);

        var proxy = conn.CreateProxy<IHelloServer>();
        var result = proxy.Hello("Hello");

        Assert.False(intercepted);
        Assert.Equal("Hello World!", result);
    }

    [Fact]
    public async Task AsyncCallInterception_doesnt_work_yet()
    {
        var config = ConnConfig;

        var intercepted = false;
        config.CallInterceptors.Add(
            CallInterceptor.For<IHelloServer>().Func<string, Task<string>>(
            (comp, arg) => comp.HelloAsync(arg),
            (data, arg) =>
            {
                if (arg == "Hello")
                {
                    intercepted = true;
                    data.Intercepted = true;
                    return Task.FromResult("Goodbye");
                }

                return data.MakeRemoteCall();
            }));

        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>();
        using var conn = new ZyanConnection(config);

        var proxy = conn.CreateProxy<IHelloServer>();
        var result = await proxy.HelloAsync("Hello");

        Assert.False(intercepted);
        Assert.Equal("Hello World!", result);
    }
}