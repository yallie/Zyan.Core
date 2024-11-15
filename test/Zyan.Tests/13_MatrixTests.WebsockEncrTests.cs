using CoreRemoting.Channels.Websocket;
using Zyan.Communication;

namespace Zyan.Tests
{
    public class MatrixTests_WebsockEncrTests : RpcTests
    {
        protected override ZyanComponentHostConfig HostConfig =>
            Set(base.HostConfig, c =>
            {
            	c.Channel = new WebsocketServerChannel();
            	c.MessageEncryption = true;
            });

        protected override ZyanConnectionConfig ConnConfig =>
            Set(base.ConnConfig, c =>
            {
            	c.Channel = new WebsocketClientChannel();
            	c.MessageEncryption = true;
            });
    }
}
