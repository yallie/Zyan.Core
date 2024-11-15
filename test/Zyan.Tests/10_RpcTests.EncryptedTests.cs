using Zyan.Communication;

namespace Zyan.Tests
{
    public class RpcTests_EncryptedTests : RpcTests
    {
        protected override ZyanComponentHostConfig HostConfig =>
            Set(base.HostConfig, c => c.MessageEncryption = true);

        protected override ZyanConnectionConfig ConnConfig =>
            Set(base.ConnConfig, c => c.MessageEncryption = true);
    }
}
