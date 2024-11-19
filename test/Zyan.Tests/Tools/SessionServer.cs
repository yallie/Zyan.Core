using System;
using System.Threading.Tasks;
using Zyan.Communication;

namespace Zyan.Tests.Tools
{
    internal class SessionServer : ISessionServer
    {
        public Guid GetSessionID() =>
            ServerSession.CurrentSession.SessionID;

        public Task<Guid> GetSessionIDAsync() =>
            Task.FromResult(ServerSession.CurrentSession.SessionID);
    }
}
