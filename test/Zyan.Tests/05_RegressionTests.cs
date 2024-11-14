using System.Threading.Tasks;
using WatsonTcp;
using Xunit;
using Test = Xunit.FactAttribute;
using TestFixture = System.SerializableAttribute;

namespace Zyan.Tests
{
    [TestFixture]
    public class RegressionTests : TestBase
    {
        [Test]
        public async Task WatsonTcpServerAndClientInstantDisposal()
        {
            void messageReceived(object sender, MessageReceivedEventArgs e)
            {
            };

            using var server = new WatsonTcpServer("127.0.0.1", TestPort);
            server.Events.MessageReceived += messageReceived;
            server.Start();

            using var client = new WatsonTcpClient("127.0.0.1", TestPort);
            var clientConnected = new TaskCompletionSource<bool>();
            client.Events.MessageReceived += messageReceived;
            client.Events.ServerConnected += (s, e) => clientConnected.TrySetResult(true);
            client.Connect();

            await clientConnected.Task;
            Assert.True(client.Connected);

            await Task.Delay(100);
            client.Disconnect();
        }
    }
}
