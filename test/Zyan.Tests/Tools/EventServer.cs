using System;
using System.Threading.Tasks;
using Zyan.Communication;

namespace Zyan.Tests.Tools
{
	public class EventServer : IEventServer
    {
        public event EventHandler MyEvent;

        private Guid ClientID => ServerSession.CurrentSession.SessionID;

        public void OnMyEvent()
        {
            try
            {
                Console.WriteLine("Client {0} invoked the event (sync).", ClientID);
                MyEvent?.Invoke(null, EventArgs.Empty);
            }
            catch
            {
                Console.WriteLine("Exception caught!");
                throw;
            }
        }

        public async Task OnMyEventAsync()
        {
            await Task.Delay(1);
            Console.WriteLine("Client {0} invoked the event (async).", ClientID);
            MyEvent?.Invoke(null, EventArgs.Empty);
        }

        public void StressTest(int count)
        {
            Console.WriteLine("Client {0} invoked the event {1} times (sync).", ClientID, count);
            for (var i = 0; i < count; i++)
            {
                MyEvent?.Invoke(null, EventArgs.Empty);
            }
        }

        public async Task StressTestAsync(int count)
        {
	        await Task.Delay(1);
            Console.WriteLine("Client {0} invoked the event {1} times (async).", ClientID, count);
            for (var i = 0; i < count; i++)
            {
                MyEvent?.Invoke(null, EventArgs.Empty);
            }
        }
    }
}
