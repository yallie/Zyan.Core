// http://zyan.com.de
// https://gist.github.com/yallie/781ce4e2f5d2764940c0
//
// Compile this code using: csc sample.cs /r:Zyan.Communication.dll
// First run — starts server.
// Second run — starts client.

using System;
using Zyan.Communication;

struct Program
{
    static void Main()
    {
        try
        {
            RunServer();
        }
        catch // can't start two servers on the same port
        {
            RunClient();
        }
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

    // ------------------------------- Client code --------

    static void RunClient()
    {
        // protocol defaults to tcp
        // host defaults to localhost
        // port defaults to CoreRemoting's default port

        using (var conn = new ZyanConnection())
        {
            Console.Title = "Client " + DateTime.Now.TimeOfDay.Seconds;
            Console.WriteLine("Connected to server.");

            var config = conn.CreateProxy<IConfigurationServer>();
            Console.WriteLine("Calling configuration server. Config name: {0}", config.GetConfigName());

            var proxy = conn.CreateProxy<IActionServer>();
            Console.WriteLine("Calling action server. Executed: {0} -> {1}", "Hello", proxy.ExecuteAction("Hello"));
        }
    }

    // ------------------------------- Server code --------

    static void RunServer()
    {
        using (var host = new ZyanComponentHost())
        {
            host.RegisterComponent<IConfigurationServer, ConfigurationServer>(ActivationType.SingleCall);
            host.RegisterComponent<IActionServer, ActionServer>(ActivationType.SingleCall);
            host.Start();

            Console.Title = "Server";
            Console.WriteLine("Server started. Press ENTER to quit.");
            Console.ReadLine();
        }
    }

    internal class ConfigurationServer : IConfigurationServer
    {
        public string GetConfigName()
        {
            return "SampleConfig";
        }
    }

    internal class ActionServer : IActionServer
    {
        public string ExecuteAction(string hello)
        {
            return hello + " World";
        }
    }
}