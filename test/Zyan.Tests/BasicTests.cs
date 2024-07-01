using System.Threading.Tasks;
using Xunit;
using Zyan.Communication;
using Zyan.Tests.Tools;
using TestFixture = System.SerializableAttribute;
using Test = Xunit.FactAttribute;

namespace Zyan.Tests
{
    [TestFixture]
    public class BasicTests
    {
        private const int TestPort = 9092;

        private ZyanComponentHostConfig HostConfig { get; } = new ZyanComponentHostConfig
        {
            NetworkPort = TestPort,
        };

        private ZyanConnectionConfig ConnConfig { get; } = new ZyanConnectionConfig
        {
            ServerPort = TestPort,
            AuthenticationTimeout = 5,
            ConnectionTimeout = 5,
            InvocationTimeout = 5,
            SendTimeout = 5,
        };

        [Test]
        public void SyncRpcTest()
        {
            using (var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>())
            using (var conn = new ZyanConnection(ConnConfig))
            {
                var proxy = conn.CreateProxy<IHelloServer>();
                var result = proxy.Hello("Hello");
                Assert.Equal("Hello World!", result);
            }
        }

        [Test]
        public async Task AsyncRpcTest()
        {
            using (var host = new ZyanComponentHost(HostConfig).RegisterComponent<IHelloServer, HelloServer>())
            using (var conn = new ZyanConnection(ConnConfig))
            {
                var proxy = conn.CreateProxy<IHelloServer>();
                var result = await proxy.HelloAsync("Hello");
                Assert.Equal("Hello World!", result);
            }
        }
    }
}
