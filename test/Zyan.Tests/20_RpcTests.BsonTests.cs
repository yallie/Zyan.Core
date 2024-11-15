using CoreRemoting.Serialization.Bson;
using Zyan.Communication;

namespace Zyan.Tests
{
    public class RpcTests_BsonTests : RpcTests
    {
        protected override ZyanComponentHostConfig HostConfig =>
            Set(base.HostConfig, c => c.Serializer = new BsonSerializerAdapter());

        protected override ZyanConnectionConfig ConnConfig =>
            Set(base.ConnConfig, c => c.Serializer = new BsonSerializerAdapter());
    }
}
