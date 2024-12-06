using System;
using System.Threading;
using CoreRemoting;
using Zyan.Communication.SessionMgmt;

namespace Zyan.Communication;

/// <summary>
/// Describes a server session.
/// </summary>
public class ServerSession : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerSession"/> class.
    /// </summary>
    /// <param name="session">Remoting client session.</param>
    internal ServerSession(RemotingSession session, IRemotingServer server, ISessionManager sessionManager)
    {
        RemotingSession = session;
        RemotingServer = server;
        RemotingServer.BeforeCall += RemotingServer_BeforeCall;
        RemotingServer.AfterCall += RemotingServer_AfterCall;
        SessionManager = sessionManager;
    }

    private void RemotingServer_BeforeCall(object sender, ServerRpcContext e)
    {
        var sessionId = RemotingSession.Current.SessionId;
        Current.Value = SessionManager.GetSessionBySessionID(sessionId);
    }

    private void RemotingServer_AfterCall(object sender, ServerRpcContext e)
    {
        Current.Value = null;
    }

    /// <summary>
    /// Gets the remoting client session.
    /// </summary>
    public RemotingSession RemotingSession { get; }

    private IRemotingServer RemotingServer { get; }

    private ISessionManager SessionManager { get; }

    /// <summary>
    /// Gets the session ID.
    /// </summary>
    public Guid SessionID => RemotingSession.SessionId;

    private static AsyncLocal<ServerSession> Current { get; } =
        new AsyncLocal<ServerSession>();

    /// <summary>
    /// Gets the session of the current logical server thread.
    /// </summary>
    public static ServerSession CurrentSession
    {
        get => Current.Value;
        internal set => Current.Value = value;
    }

    public void Dispose()
    { 
        RemotingServer.BeforeCall -= RemotingServer_BeforeCall;
        RemotingServer.AfterCall -= RemotingServer_AfterCall;
    }
}