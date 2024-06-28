﻿// http://zyan.com.de
// https://gist.github.com/yallie/10a166266d5bab61675e1b226c99720f
//
// Compile using: csc test.cs /r:Zyan.Communication.dll
//
// Start up test.exe several times.
// The first process is the server, the rest are clients.
//

using System;
using System.Collections.Concurrent;
using Zyan.Communication;
using Zyan.Communication.Protocols.Tcp;

class CanceledSubscriptionTest
{
	static void Main()
	{
		try
		{
			StartServer();
		}
		catch
		{
			StartClient();
		}
	}

	// ------------ Server code --------------

	static void StartServer()
	{
		var proto = new TcpDuplexServerProtocolSetup(4321);

		using (var host = new ZyanComponentHost(nameof(CanceledSubscriptionTest), proto))
		{
			host.RegisterComponent<IService, Service>();
			host.SubscriptionCanceled += (s, e) => Console.WriteLine("Subscription canceled: {0}.{1}", e.ComponentType, e.DelegateMemberName);

			Console.Title = "Server " + proto.TcpPort;
			Console.WriteLine("Server started. Press ENTER to quit.");
			Console.ReadLine();
		}
	}

	public class Service : IService
	{
		public event EventHandler MyEvent;

		private Guid ClientID
		{
			get { return ServerSession.CurrentSession.SessionID; }
		}

		public void OnMyEvent()
		{
			Console.WriteLine("Client {0} invoked the event.", ClientID);
			MyEvent?.Invoke(null, EventArgs.Empty);
		}

		public void StressTest(int count)
		{
			Console.WriteLine("Client {0} invoked the event {1} times.", ClientID, count);
			for (var i = 0; i < count; i++)
			{
				MyEvent?.Invoke(null, EventArgs.Empty);
			}
		}
	}

	// ------------ Shared code --------------

	public interface IService
	{
		event EventHandler MyEvent;

		void OnMyEvent();

		void StressTest(int count);
	}

	// ------------ Client code --------------

	static void StartClient()
	{
		var proto = new TcpDuplexClientProtocolSetup();
		var url = proto.FormatUrl("localhost", 4321, nameof(CanceledSubscriptionTest));

		using (var conn = new ZyanConnection(url))
		{
			var handler = new EventHandler((s, e) =>
				Console.WriteLine("This code was called by server!"));

			var proxy = conn.CreateProxy<IService>();
			proxy.MyEvent += handler;

			Console.Title = "Client " + DateTime.Now.TimeOfDay.Seconds;
			Console.WriteLine("Client started. Hit ENTER to test single event, '1000' to stress test, ^C to quit.");
			while (true)
			{
				var command = Console.ReadLine();
				var count = 0;
				if (int.TryParse(command, out count))
				{
					proxy.StressTest(count);
					continue;
				}

				proxy.OnMyEvent();
			}
		}
	}
}