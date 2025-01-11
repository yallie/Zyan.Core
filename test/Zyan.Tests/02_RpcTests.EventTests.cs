using System;
using System.Threading.Tasks;
using Xunit;
using CoreRemoting.Toolbox;
using Zyan.Communication;
using Zyan.Tests.Tools;

namespace Zyan.Tests;

public partial class RpcTests : TestBase
{
    [Fact]
    public async Task SyncSelfEvent()
    {
        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IEventServer, EventServer>();
        using var conn1 = new ZyanConnection(ConnConfig);

        var tcs1 = new TaskCompletionSource<bool>();
        var proxy1 = conn1.CreateProxy<IEventServer>();
        proxy1.MyEvent += (s, e) => tcs1.TrySetResult(true);
        proxy1.OnMyEvent();

        var result = await tcs1.Task.Timeout(1);
        Assert.True(result);
    }

    [Fact]
    public async Task SyncTwoClientEvents()
    {
        using var host = new ZyanComponentHost(HostConfig)
            .RegisterComponent<IEventServer, EventServer>();

        using var conn1 = new ZyanConnection(ConnConfig);
        using var conn2 = new ZyanConnection(ConnConfig);

        var tcs1 = new TaskCompletionSource<bool>();
        var proxy1 = conn1.CreateProxy<IEventServer>();
        proxy1.MyEvent += (s, e) => tcs1.TrySetResult(true);

        var tcs2 = new TaskCompletionSource<bool>();
        var proxy2 = conn2.CreateProxy<IEventServer>();
        proxy2.MyEvent += (s, e) => tcs2.TrySetResult(true);
        proxy2.OnMyEvent();

        // both clients should get the event
        Assert.True(await tcs1.Task.Timeout(1));
        Assert.True(await tcs2.Task.Timeout(1));
    }

    [Fact]
    public async Task AsyncSelfEvent()
    {
        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IEventServer, EventServer>();
        using var conn1 = new ZyanConnection(ConnConfig);

        var tcs1 = new TaskCompletionSource<bool>();
        var proxy1 = conn1.CreateProxy<IEventServer>();
        proxy1.MyEvent += (s, e) => tcs1.TrySetResult(true);
        await proxy1.OnMyEventAsync();

        var result = await tcs1.Task.Timeout(1);
        Assert.True(result);
    }

    [Fact]
    public async Task AsyncTwoClientEvents()
    {
        using var host = new ZyanComponentHost(HostConfig)
            .RegisterComponent<IEventServer, EventServer>();

        using var conn1 = new ZyanConnection(ConnConfig);
        using var conn2 = new ZyanConnection(ConnConfig);

        var tcs1 = new TaskCompletionSource<bool>();
        var proxy1 = conn1.CreateProxy<IEventServer>();
        proxy1.MyEvent += (s, e) => tcs1.TrySetResult(true);

        var tcs2 = new TaskCompletionSource<bool>();
        var proxy2 = conn2.CreateProxy<IEventServer>();
        proxy2.MyEvent += (s, e) => tcs2.TrySetResult(true);
        await proxy2.OnMyEventAsync();

        // both clients should get the event
        Assert.True(await tcs1.Task.Timeout(1));
        Assert.True(await tcs2.Task.Timeout(1));
    }

    [Fact]
    public async Task SyncTwoClientEventsStressTest()
    {
        using var host = new ZyanComponentHost(HostConfig)
            .RegisterComponent<IEventServer, EventServer>();

        using var conn1 = new ZyanConnection(ConnConfig);
        using var conn2 = new ZyanConnection(ConnConfig);

        var max = 200;
        var timeout = 5.0;

        var cnt1 = new AsyncCounter();
        var proxy1 = conn1.CreateProxy<IEventServer>();
        proxy1.MyEvent += (s, e) => cnt1.Increment();

        var cnt2 = new AsyncCounter();
        var proxy2 = conn2.CreateProxy<IEventServer>();
        proxy2.MyEvent += (s, e) => cnt2.Increment();
        proxy2.StressTest(max);

        // both clients should get all the events
        Assert.Equal(max, await cnt1.WaitForValue(max).Timeout(timeout));
        Assert.Equal(max, await cnt2.WaitForValue(max).Timeout(timeout));
    }

    [Fact]
    public async Task AsyncTwoClientEventsStressTest()
    {
        using var host = new ZyanComponentHost(HostConfig)
            .RegisterComponent<IEventServer, EventServer>();

        using var conn1 = new ZyanConnection(ConnConfig);
        using var conn2 = new ZyanConnection(ConnConfig);

        var max = 200;
        var timeout = 5.0;

        var cnt1 = new AsyncCounter();
        var proxy1 = conn1.CreateProxy<IEventServer>();
        proxy1.MyEvent += (s, e) => cnt1.Increment();

        var cnt2 = new AsyncCounter();
        var proxy2 = conn2.CreateProxy<IEventServer>();
        proxy2.MyEvent += (s, e) => cnt2.Increment();
        await proxy2.StressTestAsync(max);

        // both clients should get all the events
        Assert.Equal(max, await cnt1.WaitForValue(max).Timeout(timeout));
        Assert.Equal(max, await cnt2.WaitForValue(max).Timeout(timeout));
    }

    [Fact]
    public async Task SyncTwoClientEventsShouldWorkWhenClientDisconnects()
    {
        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IEventServer, EventServer>();
        using var conn2 = new ZyanConnection(ConnConfig);

        var cnt1 = new AsyncCounter();
        var conn1 = new ZyanConnection(ConnConfig);
        var proxy1 = conn1.CreateProxy<IEventServer>();
        proxy1.MyEvent += (s, e) => cnt1.Increment();

        var cnt2 = new AsyncCounter();
        var proxy2 = conn2.CreateProxy<IEventServer>();
        proxy2.MyEvent += (s, e) => cnt2.Increment();

        // close the first client
        conn1.Dispose();

        // second client should get the event
        await Task.Delay(1000);
        proxy2.OnMyEvent();
        await cnt2.WaitForValue(1);

        Assert.False(cnt1.Value > 0);
        Assert.True(cnt2.Value > 0);
    }

    [Fact]
    public async Task Server_can_register_and_invoke_client_callback_delegates()
    {
        using var host = new ZyanComponentHost(HostConfig)
            .RegisterComponent<ICallbackService, CallbackService>();

        using var conn1 = new ZyanConnection(ConnConfig);
        using var conn2 = new ZyanConnection(ConnConfig);
        var proxy1 = conn1.CreateProxy<ICallbackService>();
        var proxy2 = conn2.CreateProxy<ICallbackService>();

        var counter1 = new AsyncCounter();
        proxy1.RegisterCallback(() => counter1++);
        Assert.Equal(0, counter1.Value);

        proxy1.DoCallback();
        await counter1.WaitForValue(1);
        Assert.Equal(1, counter1.Value);

        var counter2 = new AsyncCounter();
        proxy2.RegisterCallback(() => counter2++);
        Assert.Equal(0, counter2.Value);

        proxy2.DoCallback();
        proxy2.DoCallback();
        await counter2.WaitForValue(2);
        Assert.Equal(2, counter2.Value);
        Assert.Equal(1, counter1.Value);
    }
}
