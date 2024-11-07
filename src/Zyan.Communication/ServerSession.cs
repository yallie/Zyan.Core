using System;
using CoreRemoting;

namespace Zyan.Communication
{
    /// <summary>
    /// Describes a server session.
    /// </summary>
    public class ServerSession
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSession"/> class.
        /// </summary>
        /// <param name="session">Remoting client session.</param>
        internal ServerSession(RemotingSession session) =>
            RemotingSession = session;

        /// <summary>
        /// Gets the remoting client session.
        /// </summary>
        public RemotingSession RemotingSession { get; }

        /// <summary>
        /// Gets the session ID.
        /// </summary>
        public Guid SessionID => RemotingSession.SessionId;

        /// <summary>
        /// Gets the session of the current logical server thread.
        /// </summary>
        public static ServerSession CurrentSession =>
            new ServerSession(RemotingSession.Current);
    }
}
