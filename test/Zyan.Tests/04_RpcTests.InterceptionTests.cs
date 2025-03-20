using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CoreRemoting.Threading;
using CoreRemoting.Toolbox;
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
                    data.Intercepted = intercepted = true;
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

        // the incerceptor is synchronous, but returns Task<string>
        var intercepted = false;
        config.CallInterceptors.Add(
            CallInterceptor.For<IHelloServer>().Func<string, Task<string>>(
            (comp, arg) => comp.HelloAsync(arg),
            (data, arg) =>
            {
                if (arg != "Hello")
                {
                    data.Intercepted = intercepted = true;
                    return Task.FromResult("Goodbye!");
                }

                return data.MakeRemoteCall();
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

    [Fact]
    public async Task AsyncCallInterception_supports_await_and_supports_PauseInterception()
    {
        var config = ConnConfig;

        // the interceptor is asynchronous and returns Task<string>
        var intercepted = false;
        config.CallInterceptors.Add(
            CallInterceptor.For<IHelloServer>().Func<string, Task<string>>(
            (comp, arg) => comp.HelloAsync(arg),
            async (data, arg) =>
            {
                if (arg != "Hello")
                {
                    // set data.Intercepted before the first await!
                    data.Intercepted = intercepted = true;
                    await Task.Delay(100);
                    return "Goodbye!";
                }

                return await data.MakeRemoteCall();
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

    [Fact]
    public void SyncCallInterception_can_use_PauseInterception_to_call_unintercepted_method()
    {
        var config = ConnConfig;

        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>();
        using var conn = new ZyanConnection(config);

        var executedLocally = false;
        var innerProxyCalled = false;

        config.CallInterceptors.Add(
            CallInterceptor.For<IHelloServer>().Func<string, string>(
            (comp, arg) => comp.Hello(arg),
            (data, arg) =>
            {
                data.Intercepted = true;

                if (arg != "Hello")
                {
                    executedLocally = true;
                    return "Goodbye!";
                }

                using (CallInterceptor.PauseInterception())
                {
                    innerProxyCalled = true;
                    var innerProxy = conn.CreateProxy<IHelloServer>();
                    return innerProxy.Hello("[" + arg + "]");
                }
            }));

        var proxy = conn.CreateProxy<IHelloServer>();
        var result = proxy.Hello("Hello");

        // no local execution
        Assert.False(executedLocally);
        Assert.True(innerProxyCalled);
        Assert.Equal("[Hello] World!", result);

        // interception succeeded
        innerProxyCalled = false;
        executedLocally = false;
        result = proxy.Hello("Hi there!");
        Assert.True(executedLocally);
        Assert.False(innerProxyCalled);
        Assert.Equal("Goodbye!", result);

        // interception is paused
        innerProxyCalled = false;
        executedLocally = false;
        using (CallInterceptor.PauseInterception())
        {
            result = proxy.Hello("Hi there");
            Assert.False(executedLocally);
            Assert.False(innerProxyCalled);
            Assert.Equal("Hi there World!", result);
        }

        // interception unpaused
        innerProxyCalled = false;
        executedLocally = false;
        result = proxy.Hello("Anybody?");
        Assert.True(executedLocally);
        Assert.False(innerProxyCalled);
        Assert.Equal("Goodbye!", result);
    }

    [Fact]
    public async Task AsyncCallInterception_can_use_PauseInterception_to_call_unintercepted_method()
    {
        var config = ConnConfig;

        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>();
        using var conn = new ZyanConnection(config);

        var executedLocally = false;
        var innerProxyCalled = false;

        config.CallInterceptors.Add(
            CallInterceptor.For<IHelloServer>().Func<string, Task<string>>(
            (comp, arg) => comp.HelloAsync(arg),
            async (data, arg) =>
            {
                data.Intercepted = true;

                if (arg != "Hello")
                {
                    executedLocally = true;
                    await Task.Yield();
                    return "Goodbye!";
                }

                using (CallInterceptor.PauseInterception())
                {
                    innerProxyCalled = true;
                    var innerProxy = conn.CreateProxy<IHelloServer>();
                    return await innerProxy.HelloAsync("[" + arg + "]");
                }
            }));

        var proxy = conn.CreateProxy<IHelloServer>();
        var result = await proxy.HelloAsync("Hello");

        // no local execution
        Assert.False(executedLocally);
        Assert.True(innerProxyCalled);
        Assert.Equal("[Hello] World!", result);

        // interception succeeded
        innerProxyCalled = false;
        executedLocally = false;
        result = await proxy.HelloAsync("Hi there!");
        Assert.True(executedLocally);
        Assert.False(innerProxyCalled);
        Assert.Equal("Goodbye!", result);

        // interception is paused
        innerProxyCalled = false;
        executedLocally = false;
        using (CallInterceptor.PauseInterception())
        {
            result = await proxy.HelloAsync("Hi there");
            Assert.False(executedLocally);
            Assert.False(innerProxyCalled);
            Assert.Equal("Hi there World!", result);
        }

        // interception unpaused
        innerProxyCalled = false;
        executedLocally = false;
        result = await proxy.HelloAsync("Anybody?");
        Assert.True(executedLocally);
        Assert.False(innerProxyCalled);
        Assert.Equal("Goodbye!", result);
    }

    [Fact]
    public void SyncCallInterception_can_cache_return_values()
    {
        var config = ConnConfig;
        var resultCache = new ConcurrentDictionary<string, string>();

        var gotFromCache = false;
        config.CallInterceptors.Add(
            CallInterceptor.For<IHelloServer>().Func<string, string>(
            (comp, arg) => comp.Hello(arg),
            (data, arg) =>
            {
                data.Intercepted = true;

                if (gotFromCache = resultCache.TryGetValue(arg, out var result))
                    return result;

                return resultCache[arg] = data.MakeRemoteCall();
            }));

        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>();
        using var conn = new ZyanConnection(config);

        var proxy = conn.CreateProxy<IHelloServer>();
        var result = proxy.Hello("Hello");

        // no interception
        Assert.False(gotFromCache);
        Assert.Equal("Hello World!", result);

        // interception succeeded
        result = proxy.Hello("Hello");
        Assert.True(gotFromCache);
        Assert.Equal("Hello World!", result);

        // interception is paused
        using (CallInterceptor.PauseInterception())
        {
            gotFromCache = false;
            result = proxy.Hello("Hi there");
            Assert.False(gotFromCache);
            Assert.Equal("Hi there World!", result);
        }

        // interception unpaused: result not cached
        result = proxy.Hello("Goodbye");
        Assert.False(gotFromCache);
        Assert.Equal("Goodbye World!", result);

        // result is cached
        result = proxy.Hello("Goodbye");
        Assert.True(gotFromCache);
        Assert.Equal("Goodbye World!", result);
    }

    [Fact]
    public async Task AsyncCallInterception_can_cache_return_values()
    {
        var config = ConnConfig;
        var resultCache = new ConcurrentDictionary<string, string>();

        var gotFromCache = false;
        config.CallInterceptors.Add(
            CallInterceptor.For<IHelloServer>().Func<string, Task<string>>(
            (comp, arg) => comp.HelloAsync(arg),
            async (data, arg) =>
            {
                data.Intercepted = true;

                if (gotFromCache = resultCache.TryGetValue(arg, out var result))
                {
                    await Task.Yield();
                    return result;
                }

                return resultCache[arg] = await data.MakeRemoteCall();
            }));

        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>();
        using var conn = new ZyanConnection(config);

        var proxy = conn.CreateProxy<IHelloServer>();
        var result = await proxy.HelloAsync("Hello");

        // no interception
        Assert.False(gotFromCache);
        Assert.Equal("Hello World!", result);

        // interception succeeded
        result = await proxy.HelloAsync("Hello");
        Assert.True(gotFromCache);
        Assert.Equal("Hello World!", result);

        // interception is paused
        using (CallInterceptor.PauseInterception())
        {
            gotFromCache = false;
            result = await proxy.HelloAsync("Hi there");
            Assert.False(gotFromCache);
            Assert.Equal("Hi there World!", result);
        }

        // interception unpaused: no cached result
        result = await proxy.HelloAsync("Goodbye");
        Assert.False(gotFromCache);
        Assert.Equal("Goodbye World!", result);

        // result is cached
        result = await proxy.HelloAsync("Goodbye");
        Assert.True(gotFromCache);
        Assert.Equal("Goodbye World!", result);
    }

    [Fact]
    public void SyncCallInterception_works_with_alternative_syntax()
    {
        var config = ConnConfig;

        var intercepted = false;
        config.CallInterceptors.For<IHelloServer>().Add<string, string>(
            (c, s) => c.Hello(s),
            (data, arg) =>
            {
                if (arg != "Hello")
                {
                    data.Intercepted = intercepted = true;
                    return "Goodbye!";
                }

                return data.MakeRemoteCall();
            });

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
    public async Task AsyncCallInterception_works_with_alternative_syntax()
    {
        var config = ConnConfig;

        // the incerceptor is synchronous, but returns Task<string>
        var intercepted = false;
        config.CallInterceptors.For<IHelloServer>().Add<string, Task<string>>(
            (comp, arg) => comp.HelloAsync(arg),
            async (data, arg) =>
            {
                if (arg != "Hello")
                {
                    data.Intercepted = intercepted = true;
                    return "Goodbye!";
                }

                return await data.MakeRemoteCall();
            });

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

    [Fact]
    public async Task SyncCallInterception_works_for_Action()
    {
        var config = ConnConfig;
        var callbackEnabled = true;
        var callbackCounter = new AsyncCounter();

        config.CallInterceptors.Add(
            CallInterceptor.For<ICallbackService>().Action(
            svc => svc.DoCallback(),
            data => data.Intercepted = !callbackEnabled));

        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<ICallbackService, CallbackService>();
        using var conn = new ZyanConnection(config);

        var proxy = conn.CreateProxy<ICallbackService>();
        proxy.RegisterCallback(() => callbackCounter.Increment());

        // no interception
        Assert.True(callbackEnabled);
        proxy.DoCallback();
        await callbackCounter.WaitForValue(1).Timeout(0.5);

        // interception succeeded
        callbackEnabled = false;
        proxy.DoCallback();
        await Assert.ThrowsAsync<TimeoutException>(() =>
            callbackCounter.WaitForValue(2).Timeout(0.5));

        // interception is paused
        using (CallInterceptor.PauseInterception())
        {
            Assert.False(callbackEnabled);
            proxy.DoCallback();
            await callbackCounter.WaitForValue(2).Timeout(0.5);
        }
    }

    [Fact]
    public async  Task AsyncCallInterception_works_for_Task()
    {
        var config = ConnConfig;
        var callbackEnabled = true;
        var callbackCounter = new AsyncCounter();

        config.CallInterceptors.Add(
            CallInterceptor.For<ICallbackService>().Func(
            svc => svc.DoCallbackAsync(),
            data =>
            {
                data.Intercepted = !callbackEnabled;
                return Task.CompletedTask;
            }));

        using var host = new ZyanComponentHost(HostConfig).RegisterComponent<ICallbackService, CallbackService>();
        using var conn = new ZyanConnection(config);

        var proxy = conn.CreateProxy<ICallbackService>();
        proxy.RegisterCallback(() => callbackCounter.Increment());

        // no interception
        Assert.True(callbackEnabled);
        await proxy.DoCallbackAsync();
        await callbackCounter.WaitForValue(1).Timeout(0.5);

        // interception succeeded
        callbackEnabled = false;
        await proxy.DoCallbackAsync();
        await Assert.ThrowsAsync<TimeoutException>(() =>
            callbackCounter.WaitForValue(2).Timeout(0.5));

        // interception is paused
        using (CallInterceptor.PauseInterception())
        {
            Assert.False(callbackEnabled);
            await proxy.DoCallbackAsync();
            await callbackCounter.WaitForValue(2).Timeout(0.5);
        }
    }
}