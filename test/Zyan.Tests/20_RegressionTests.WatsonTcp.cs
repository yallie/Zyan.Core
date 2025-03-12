using System;
using System.Text;
using System.Threading.Tasks;
using WatsonTcp;
using Xunit;
using Test = Xunit.FactAttribute;

namespace Zyan.Tests;

public partial class RegressionTests : TestBase
{
    // [Test] // FAILS: throws TaskCanceledException
    internal async Task WatsonTcpThrowsCanceledExceptionOnDispose()
    {
        // see https://github.com/dotnet/WatsonTcp/issues/303
        // and https://github.com/dotnet/WatsonTcp/issues/304
        var server = new WatsonTcpServer("127.0.0.1", 9000);
        server.Events.MessageReceived += (s, e) => Console.WriteLine("srecv");
        server.Start();

        using var client = new WatsonTcpClient("127.0.0.1", 9000);
        client.Events.MessageReceived += (s, e) => Console.WriteLine("crecv");
        client.Connect();

        await client.SendAsync("aaa");
        await Task.Delay(1000);
        server.Dispose();
    }

    // [Test] // https://github.com/dotnet/WatsonTcp/issues/316
    public void WatsonTcpServerSecondDisposalThrowsNullReferenceException()
    {
        var server = new WatsonTcpServer("127.0.0.1", TestPort);

        server.Dispose();
        server.Dispose();
    }

    // [Test] // https://github.com/dotnet/WatsonTcp/issues/303
    public async Task WatsonTcpServerAndClientInstantDisposal()
    {
        // doesn't reproduce locally, but quite often fails on CI
        var server = new WatsonTcpServer("127.0.0.1", TestPort);
        server.Events.MessageReceived += (s, e) => { };
        server.Callbacks.SyncRequestReceivedAsync += r => Task.FromResult(new SyncResponse(r, r.Data));
        server.Start();

        var client = new WatsonTcpClient("127.0.0.1", TestPort);
        var clientConnected = new TaskCompletionSource<bool>();
        client.Events.MessageReceived += (s, e) => { };
        client.Events.ServerConnected += (s, e) => clientConnected.TrySetResult(true);
        client.Connect();

        await clientConnected.Task;
        Assert.True(client.Connected);

        var response = await client.SendAndWaitAsync(1000, "Hello");
        Assert.Equal("Hello", Encoding.UTF8.GetString(response.Data));
        client.Disconnect();
    }
}