using CoreRemoting.Channels.Websocket;
using CoreRemoting.Serialization.Bson;
using Zyan.Communication;

namespace Zyan.Tests;

public class MatrixTests_WsBsonEncrTests : RpcTests
{
    protected override ZyanComponentHostConfig HostConfig =>
        Set(base.HostConfig, c =>
        {
            c.Channel = new WebsocketServerChannel();
            c.Serializer = new BsonSerializerAdapter();
            c.MessageEncryption = true;
        });

    protected override ZyanConnectionConfig ConnConfig =>
        Set(base.ConnConfig, c =>
        {
            c.Channel = new WebsocketClientChannel();
            c.Serializer = new BsonSerializerAdapter();
            c.MessageEncryption = true;
        });
}