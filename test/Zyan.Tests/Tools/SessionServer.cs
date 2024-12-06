using System;
using System.Threading.Tasks;
using Zyan.Communication;

namespace Zyan.Tests.Tools
{
    internal class SessionServer : ISessionServer
    {
        private ServerSession CurrentSession { get; } =
            ServerSession.CurrentSession;

        public Guid GetSessionID() =>
            ServerSession.CurrentSession.SessionID;

        public Task<Guid> GetSessionIDAsync() =>
            Task.FromResult(ServerSession.CurrentSession.SessionID);

        public bool SessionsAreSame() =>
            ReferenceEquals(CurrentSession, ServerSession.CurrentSession);
    }
}
