﻿using Zyan.Communication;

namespace Zyan.Tests
{
    public class EncryptedBasicTests : BasicTests
    {
        protected override ZyanComponentHostConfig HostConfig =>
            Set(base.HostConfig, c => c.MessageEncryption = true);

        protected override ZyanConnectionConfig ConnConfig =>
            Set(base.ConnConfig, c => c.MessageEncryption = true);
    }
}