﻿// http://zyan.com.de
// https://gist.github.com/yallie/781ce4e2f5d2764940c0
//
// Compile this code using: csc sample.cs /r:Zyan.Communication.dll
// First run — starts server.
// Second run — starts client.

using System;
using System.Diagnostics;
using System.Net.Sockets;
using CoreRemoting.DependencyInjection;
using Zyan.Communication;

try
{
    RunServer();
}
catch (SocketException) // can't start two servers on the same port
{
    RunClient();
}

// ------------------------------- Client code --------

static void RunClient()
{
    // protocol defaults to tcp
    // host defaults to localhost
    // port defaults to CoreRemoting's default port
    Console.Title = "Client " + DateTime.Now.TimeOfDay.Seconds;
    Console.WriteLine("Server already running. Creating connection...");
    var sw = Stopwatch.StartNew();

    using var conn = new ZyanConnection();
    Console.WriteLine($"Connected to server: took {sw.Elapsed.TotalSeconds} seconds to connect.");
    Console.WriteLine("Creating proxies...");

    var config = conn.CreateProxy<IConfigurationServer>();
    Console.WriteLine("Created the first proxy. Calling its method...");
    Console.WriteLine("Calling configuration server. Config name: {0}", config.GetConfigName());

    var proxy = conn.CreateProxy<IActionServer>();
    Console.WriteLine("Created the second proxy. Calling its method...");
    Console.WriteLine("Calling action server. Executed: {0} -> {1}", "Hello", proxy.ExecuteAction("Hello"));
}

// ------------------------------- Server code --------

static void RunServer()
{
    var sw = Stopwatch.StartNew();

    using var host = new ZyanComponentHost();
    host.RegisterComponent<IConfigurationServer, ConfigurationServer>(ServiceLifetime.Singleton);
    host.RegisterComponent<IActionServer, ActionServer>();

    Console.Title = "Server";
    Console.WriteLine($"Server started. Took {sw.Elapsed.TotalSeconds} seconds to start.");
    Console.WriteLine("Press ENTER to quit.");
    Console.ReadLine();
}

internal class ConfigurationServer : IConfigurationServer
{
    public string GetConfigName() => "SampleConfig";
}

internal class ActionServer : IActionServer
{
    public string ExecuteAction(string hello) =>
        hello + " World";
}

// ------------------------------- Shared code --------

public interface IConfigurationServer
{
    string GetConfigName();
}

public interface IActionServer
{
    string ExecuteAction(string action);
}

