using System;
using System.Threading.Tasks;
using Xunit;
using Zyan.Communication;
using Zyan.Tests.Tools;
using Test = Xunit.FactAttribute;
using TestFixture = System.SerializableAttribute;

namespace Zyan.Tests
{
    [TestFixture]
    public class EventTests : TestBase
    {
        protected override int TestPort => 9093;

        [Test]
        public async Task SyncSelfEvent()
        {
            using (var host = new ZyanComponentHost(HostConfig).RegisterComponent<IEventServer, EventServer>())
            using (var conn1 = new ZyanConnection(ConnConfig))
            {
                var tcs1 = new TaskCompletionSource<bool>();
                var proxy1 = conn1.CreateProxy<IEventServer>();
                proxy1.MyEvent += (s, e) => tcs1.TrySetResult(true);
                proxy1.OnMyEvent();

                var result = await IsInTime(tcs1.Task);
                Assert.True(result);
            }
        }

        [Test]
        public async Task SyncTwoClientEvents()
        {
            using (var host = new ZyanComponentHost(HostConfig).RegisterComponent<IEventServer, EventServer>())
            using (var conn1 = new ZyanConnection(ConnConfig))
            using (var conn2 = new ZyanConnection(ConnConfig))
            {
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
        }

        [Test]
        public async Task AsyncSelfEvent()
        {
            using (var host = new ZyanComponentHost(HostConfig).RegisterComponent<IEventServer, EventServer>())
            using (var conn1 = new ZyanConnection(ConnConfig))
            {
                var tcs1 = new TaskCompletionSource<bool>();
                var proxy1 = conn1.CreateProxy<IEventServer>();
                proxy1.MyEvent += (s, e) => tcs1.TrySetResult(true);
                await proxy1.OnMyEventAsync();

                var result = await IsInTime(tcs1.Task);
                Assert.True(result);
            }
        }

        [Test]
        public async Task AsyncTwoClientEvents()
        {
            using (var host = new ZyanComponentHost(HostConfig).RegisterComponent<IEventServer, EventServer>())
            using (var conn1 = new ZyanConnection(ConnConfig))
            using (var conn2 = new ZyanConnection(ConnConfig))
            {
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
        }
    }
}
