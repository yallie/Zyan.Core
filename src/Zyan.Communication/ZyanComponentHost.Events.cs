using System;
using CoreRemoting;

namespace Zyan.Communication;

partial class ZyanComponentHost
{
    private void AttachRemotingServerEvents()
    {
        RemotingServer.BeginCall += RemotingServer_BeginCall;
        RemotingServer.AfterCall += RemotingServer_AfterCall;
        RemotingServer.RejectCall += RemotingServer_RejectCall;
        RemotingServer.Logon += RemotingServer_Logon;
        RemotingServer.Logoff += RemotingServer_Logoff;
    }

    private void DetachRemotingServerEvents()
    {
        RemotingServer.BeginCall -= RemotingServer_BeginCall;
        RemotingServer.AfterCall -= RemotingServer_AfterCall;
        RemotingServer.RejectCall -= RemotingServer_RejectCall;
        RemotingServer.Logon += RemotingServer_Logon;
        RemotingServer.Logoff += RemotingServer_Logoff;
    }

    private void RemotingServer_BeginCall(object sender, ServerRpcContext e)
    {
        // TODO: thread safety on session creation
        var sessionId = e.Session.SessionId;
        var session = SessionManager.GetSessionBySessionID(sessionId);
        if (session == null)
        {
            session = SessionManager.CreateServerSession(sessionId, DateTime.Now, e.Session.Identity);
            SessionManager.StoreSession(session);
        }

        SessionManager.SetCurrentSession(session);
        BeforeInvoke?.Invoke(this, new BeforeInvokeEventArgs(e));
    }

    private void RemotingServer_AfterCall(object sender, ServerRpcContext e)
    {
        if (e.Exception == null)
            AfterInvoke?.Invoke(this, new AfterInvokeEventArgs(e));
        else
            InvokeCanceled?.Invoke(this, new InvokeCanceledEventArgs(e));

        SessionManager.SetCurrentSession(null);
    }

    private void RemotingServer_RejectCall(object sender, ServerRpcContext e) =>
        InvokeRejected?.Invoke(this, new InvokeCanceledEventArgs(e));

    private void RemotingServer_Logon(object sender, EventArgs e) =>
        ClientLoggedOn?.Invoke(this,
            new LoginEventArgs(LoginEventType.Logon,
                RemotingSession.Current?.Identity,
                RemotingSession.Current?.ClientAddress,
                DateTime.Now));

    private void RemotingServer_Logoff(object sender, EventArgs e) =>
        ClientLoggedOff?.Invoke(this,
            new LoginEventArgs(LoginEventType.Logoff,
                RemotingSession.Current?.Identity,
                RemotingSession.Current?.ClientAddress,
                DateTime.Now));

    /// <summary>
    /// Occurs when new client is logged on.
    /// </summary>
    public event EventHandler<LoginEventArgs> ClientLoggedOn;

    /// <summary>
    /// Occurs when the client is logged off.
    /// </summary>
    public event EventHandler<LoginEventArgs> ClientLoggedOff;

    /// <summary>
    /// Occurs before the component call is initiated.
    /// </summary>
    public event EventHandler<BeforeInvokeEventArgs> BeforeInvoke;

    /// <summary>
    /// Occurs after the component call is completed.
    /// </summary>
    public event EventHandler<AfterInvokeEventArgs> AfterInvoke;

    /// <summary>
    /// Occurs when the component call is canceled due to exception.
    /// </summary>
    public event EventHandler<InvokeCanceledEventArgs> InvokeCanceled;

    /// <summary>
    /// Occurs when the component call is rejected due to security reasons.
    /// </summary>
    public event EventHandler<InvokeCanceledEventArgs> InvokeRejected;
}