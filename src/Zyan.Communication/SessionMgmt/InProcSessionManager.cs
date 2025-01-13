using CoreRemoting;
using CoreRemoting.Channels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Principal;

namespace Zyan.Communication.SessionMgmt;

/// <summary>
/// Slim in-process session manager.
/// Uses <see cref="SessionRepository"/> behind the scenes.
/// </summary>
public class InProcSessionManager : ISessionManager, ISessionRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InProcSessionManager"/> class.
    /// </summary>
    public InProcSessionManager(int keySize, int sweepInterval, int inactivityTime, ISessionRepository sessionRepository)
    {
        SessionRepository = sessionRepository ?? new SessionRepository(keySize, sweepInterval, inactivityTime);
    }

    private ISessionRepository SessionRepository { get; set; }

    /// <inheritdoc/>
    public int KeySize => SessionRepository.KeySize;

    /// <inheritdoc/>
    public IEnumerable<RemotingSession> Sessions => SessionRepository.Sessions;

    /// <inheritdoc/>
    public RemotingSession CreateSession(byte[] clientPublicKey, string clientAddress, IRemotingServer server, IRawMessageTransport rawMessageTransport)
    {
        var rs = SessionRepository.CreateSession(clientPublicKey, clientAddress, server, rawMessageTransport);
        var ss = new ServerSession(rs, server, this);
        ServerSessions[ss.SessionID] = ss;
        return rs;
    }

    /// <inheritdoc/>
    public RemotingSession GetSession(Guid sessionId) =>
        SessionRepository.GetSession(sessionId);

    /// <inheritdoc/>
    void ISessionRepository.RemoveSession(Guid sessionId)
    {
        SessionRepository.RemoveSession(sessionId);
        if (ServerSessions.TryRemove(sessionId, out var ss))
        {
            ss.Dispose();
        }

        SessionVars.TryRemove(sessionId, out _);
    }

    /// <inheritdoc/>
    public void Dispose() =>
        SessionRepository.Dispose();

    /// <inheritdoc/>
    private ConcurrentDictionary<Guid, ServerSession> ServerSessions { get; } =
        new ConcurrentDictionary<Guid, ServerSession>();

    private ConcurrentDictionary<Guid, ConcurrentDictionary<string, object>> SessionVars { get; } =
        new ConcurrentDictionary<Guid, ConcurrentDictionary<string, object>>();

    /// <inheritdoc/>
    public bool ExistSession(Guid sessionId) =>
        ServerSessions.ContainsKey(sessionId);

    /// <inheritdoc/>
    public ServerSession GetSessionBySessionID(Guid sessionId) =>
        ServerSessions.TryGetValue(sessionId, out var result) ? result : null;

    /// <inheritdoc/>
    public ServerSession CreateServerSession(Guid sessionID, DateTime timeStamp, IIdentity identity) =>
        throw new NotImplementedException("This method is kept for compatibility");

    public ServerSession CreateServerSession(RemotingSession rs, IRemotingServer server) =>
        new(rs, server, this);

    /// <inheritdoc/>
    public void SetCurrentSession(ServerSession session) =>
        ServerSession.CurrentSession = session;

    /// <inheritdoc/>
    public void StoreSession(ServerSession session) =>
        ServerSessions[session.SessionID] = session;

    /// <inheritdoc/>
    public void RenewSession(ServerSession session) =>
        throw new NotImplementedException("This method is kept for compatibility");

    /// <inheritdoc/>
    public void RemoveSession(Guid sessionId)
    {
        SessionRepository.RemoveSession(sessionId);
        ServerSessions.TryRemove(sessionId, out _);
    }

    /// <inheritdoc/>
    public void TerminateSession(Guid sessionId)
    {
        RemoveSession(sessionId);
        ClientSessionTerminated?.Invoke(this, new LoginEventArgs(LoginEventType.Logoff, null, DateTime.Now));
    }

    /// <inheritdoc/>
    event EventHandler<LoginEventArgs> ClientSessionTerminated;

    private ConcurrentDictionary<string, object> GetSessionVars(Guid sessionId) =>
        SessionVars.GetOrAdd(sessionId, g => new ConcurrentDictionary<string, object>());

    /// <inheritdoc/>
    public object GetSessionVariable(Guid sessionId, string name) =>
        GetSessionVars(sessionId).TryGetValue(name, out var result) ? result : null;

    /// <inheritdoc/>
    public void SetSessionVariable(Guid sessionId, string name, object value) =>
        GetSessionVars(sessionId)[name] = value;
}
