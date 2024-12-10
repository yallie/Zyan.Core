// http://zyan.com.de
// https://gist.github.com/yallie/d39a2e81106dc5572f1e2b5f519510eb
//
// Compile using: csc test.cs /r:Zyan.Communication.dll
//
// Start up test.exe several times.
// The first process is the server, the rest are clients.
//

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using Zyan.Communication;

try
{
    StartServer();
}
catch (SocketException)
{
    StartClient();
}

// ------------ Client code --------------

static void StartClient()
{
    // protocol defaults to tcp
    // host defaults to localhost
    // port defaults to CoreRemoting's default port
    Console.Title = "Delegate Client " + DateTime.Now.TimeOfDay.Seconds;
    Console.WriteLine("Server already running. Creating connection...");
    var sw = Stopwatch.StartNew();

    using (var conn = new ZyanConnection())
    {
        Console.WriteLine($"Connected to server: took {sw.Elapsed.TotalSeconds} seconds to connect.");
        Console.WriteLine("Creating proxies...");

        var callback = new Action(() =>
            Console.WriteLine("This code was called by server!"));

        var proxy = conn.CreateProxy<IService>();
        proxy.RegisterCallback(callback);

        Console.WriteLine("Client started. Hit ENTER to test, ^C to quit.");

        while (true)
        {
            Console.ReadLine();
            proxy.DoCallback();
        }
    }
}

// ------------ Server code --------------

static void StartServer()
{
    var sw = Stopwatch.StartNew();

    using (var host = new ZyanComponentHost())
    {
        host.RegisterComponent<IService, Service>();

        Console.Title = "Delegate Server";
        Console.WriteLine($"Server started. Took {sw.Elapsed.TotalSeconds} seconds to start.");
        Console.WriteLine("Press ENTER to quit.");
        Console.ReadLine();
    }
}

public class Service : IService
{
    private static ConcurrentDictionary<Guid, Action> Callbacks { get; } =
        new ConcurrentDictionary<Guid, Action>();

    private Guid ClientID => ServerSession.CurrentSession.SessionID;

    public void RegisterCallback(Action callback)
    {
        // register my delegate
        Callbacks[ClientID] = callback;
        Console.WriteLine("Client {0} registered the delegate.", ClientID);
    }

    public void DoCallback()
    {
        if (Callbacks.TryGetValue(ClientID, out var action))
        {
            Console.WriteLine("Calling client {0}'s delegate.", ClientID);

            // call my delegate, if registered
            action?.Invoke();
        }
    }
}

// ------------ Shared code --------------

public interface IService
{
    void RegisterCallback(Action callback);

    void DoCallback();
}

