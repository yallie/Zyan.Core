﻿using System;
using System.Threading.Tasks;
using Xunit;
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

        var result = await IsInTime(tcs1.Task);
        Assert.True(result);
    }

    [Fact]
    public async Task SyncTwoClientEvents()
    {
        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IEventServer, EventServer>();
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
        Assert.True(await IsInTime(tcs1.Task));
        Assert.True(await IsInTime(tcs2.Task));
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

        var result = await IsInTime(tcs1.Task);
        Assert.True(result);
    }

    [Fact]
    public async Task AsyncTwoClientEvents()
    {
        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IEventServer, EventServer>();
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
        Assert.True(await IsInTime(tcs1.Task));
        Assert.True(await IsInTime(tcs2.Task));
    }

    [Fact]
    public async Task SyncTwoClientEventsStressTest()
    {
        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IEventServer, EventServer>();
        using var conn1 = new ZyanConnection(ConnConfig);
        using var conn2 = new ZyanConnection(ConnConfig);

        var max = 200;
        var timeout = TimeSpan.FromSeconds(5);

        var cnt1 = new AsyncCounter(max);
        var proxy1 = conn1.CreateProxy<IEventServer>();
        proxy1.MyEvent += (s, e) => cnt1.Increment();

        var cnt2 = new AsyncCounter(max);
        var proxy2 = conn2.CreateProxy<IEventServer>();
        proxy2.MyEvent += (s, e) => cnt2.Increment();
        proxy2.StressTest(max);

        // both clients should get all the events
        Assert.True(await IsInTime(cnt1.Task, timeout));
        Assert.True(await IsInTime(cnt2.Task, timeout));
    }

    [Fact]
    public async Task AsyncTwoClientEventsStressTest()
    {
        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IEventServer, EventServer>();
        using var conn1 = new ZyanConnection(ConnConfig);
        using var conn2 = new ZyanConnection(ConnConfig);

        var max = 200;
        var timeout = TimeSpan.FromSeconds(5);

        var cnt1 = new AsyncCounter(max);
        var proxy1 = conn1.CreateProxy<IEventServer>();
        proxy1.MyEvent += (s, e) => cnt1.Increment();

        var cnt2 = new AsyncCounter(max);
        var proxy2 = conn2.CreateProxy<IEventServer>();
        proxy2.MyEvent += (s, e) => cnt2.Increment();
        await proxy2.StressTestAsync(max);

        // both clients should get all the events
        Assert.True(await IsInTime(cnt1.Task, timeout));
        Assert.True(await IsInTime(cnt2.Task, timeout));
    }

    [Fact]
    public async Task SyncTwoClientEventsShouldWorkWhenClientDisconnects()
    {
        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IEventServer, EventServer>();
        using var conn2 = new ZyanConnection(ConnConfig);

        var max = 200;
        var timeout = TimeSpan.FromSeconds(5);

        var cnt1 = new AsyncCounter(max);
        var conn1 = new ZyanConnection(ConnConfig);
        var proxy1 = conn1.CreateProxy<IEventServer>();
        proxy1.MyEvent += (s, e) => cnt1.Increment();

        var cnt2 = new AsyncCounter(max);
        var proxy2 = conn2.CreateProxy<IEventServer>();
        proxy2.MyEvent += (s, e) => cnt2.Increment();

        // close the first client
        conn1.Dispose();

        // second client should get the event
        await Task.Delay(1000);
        proxy2.OnMyEvent();
        await Task.Delay(100);

        Assert.False(cnt1.CurrentValue > 0);
        Assert.True(cnt2.CurrentValue > 0);
        await Task.Delay(300);
    }

    [Fact]
    public void Server_can_register_and_invoke_client_callback_delegates()
    {
        using var host = new ZyanComponentHost(HostConfig)
            .RegisterComponent<ICallbackService, CallbackService>();

        using var conn1 = new ZyanConnection(ConnConfig);
        using var conn2 = new ZyanConnection(ConnConfig);
        var proxy1 = conn1.CreateProxy<ICallbackService>();
        var proxy2 = conn2.CreateProxy<ICallbackService>();

        var counter1 = 0;
        proxy1.RegisterCallback(() => counter1++);
        Assert.Equal(0, counter1);

        proxy1.DoCallback();
        Assert.Equal(1, counter1);

        var counter2 = 0;
        proxy2.RegisterCallback(() => counter2++);
        Assert.Equal(0, counter2);

        proxy2.DoCallback();
        proxy2.DoCallback();
        Assert.Equal(2, counter2);
        Assert.Equal(1, counter1);
    }
}
