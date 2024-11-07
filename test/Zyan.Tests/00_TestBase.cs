using System.Threading.Tasks;
using System;
using Xunit;
using Zyan.Communication;

namespace Zyan.Tests
{
    public class TestBase
    {
        protected virtual int TestPort => 9092;

        protected ZyanComponentHostConfig HostConfig => new ZyanComponentHostConfig
        {
            NetworkPort = TestPort,
        };

        protected ZyanConnectionConfig ConnConfig => new ZyanConnectionConfig
        {
            // note: looks like RemotingClient keeps track
            // of all client instances created out of the same config
            // and disposes of the old clients automatically :-0
            ServerPort = TestPort,
            AuthenticationTimeout = 5,
            ConnectionTimeout = 120,
            InvocationTimeout = 5,
            SendTimeout = 5,
        };

        protected TimeSpan WaitTimeout = TimeSpan.FromSeconds(1);

        protected async Task<T> IsInTime<T>(Task<T> task, string message = "Timed out!")
        {
            var result = await Task.WhenAny(task, Task.Delay(WaitTimeout));
            if (ReferenceEquals(result, task))
            {
                return await task;
            }

            Assert.Fail(message);
            throw new NotSupportedException();
        }
    }
}
