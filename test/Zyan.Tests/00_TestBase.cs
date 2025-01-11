using System;
using System.Collections.Concurrent;
using System.Threading;
using Zyan.Communication;

namespace Zyan.Tests;

public class TestBase
{
    // Make sure that all tests use their own TCP ports when run in parallel
    private static int LastUsedTcpPort = 9091;

    private static ConcurrentDictionary<Type, int> Ports { get; } =
        new ConcurrentDictionary<Type, int>();

    protected virtual int TestPort => Ports.GetOrAdd(GetType(), t =>
        Interlocked.Increment(ref LastUsedTcpPort));

    protected virtual ZyanComponentHostConfig HostConfig => new ZyanComponentHostConfig
    {
        NetworkPort = TestPort,
        MessageEncryption = false,
    };

    protected virtual ZyanConnectionConfig ConnConfig => new ZyanConnectionConfig
    {
        // note: looks like RemotingClient keeps track
        // of all client instances created out of the same config
        // and disposes of the old clients automatically :-0
        ServerPort = TestPort,
        AuthenticationTimeout = 15,
        ConnectionTimeout = 120,
        InvocationTimeout = 15,
        SendTimeout = 15,
        MessageEncryption = false,
    };

    protected T Set<T>(T cfg, Action<T> setter)
    {
        setter(cfg);
        return cfg;
    }
}