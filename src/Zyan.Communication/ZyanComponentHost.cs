﻿using System;
using CoreRemoting;
using CoreRemoting.DependencyInjection;
using CoreRemoting.Serialization.Binary;
using Zyan.Communication.DependencyInjection;
using Zyan.Communication.SessionMgmt;

namespace Zyan.Communication;

/// <summary>
/// Host for publishing components with Zyan.
/// </summary>
public class ZyanComponentHost : IDisposable
{
    public ZyanComponentHostConfig Config { get; private set; }

    private RemotingServer RemotingServer { get; set; }

    private IScopedContainer Container { get; set; }

    private ISessionManager SessionManager { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZyanComponentHost" /> class.
    /// </summary>
    /// <param name="config">Remoting configuration, optional</param>
    public ZyanComponentHost(ZyanComponentHostConfig config = null)
    {
        Config = config ?? new ZyanComponentHostConfig();
        Config.Serializer = Config.Serializer ?? new BinarySerializerAdapter();

        // TODO: handle custom config session repository?
        var sessionManager = new InProcSessionManager(
            config.KeySize,
            config.InactiveSessionSweepInterval,
            config.MaximumSessionInactivityTime,
            config.SessionRepository);

        SessionManager = sessionManager;
        Config.SessionRepository = sessionManager;

        // make sure we're using the scoped container
        Config.ScopedContainer = Container =
            Config.ScopedContainer ?? new DryIocAdapter();

        // start up the server as specified in the config
        RemotingServer = new RemotingServer(Config);
        RemotingServer.BeginCall += RemotingServer_BeginCall;
        RemotingServer.AfterCall += RemotingServer_AfterCall;
        if (Config.AutoStart)
        {
            RemotingServer.Start();
        }
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

        // set current server session
        SessionManager.SetCurrentSession(session);
    }

    private void RemotingServer_AfterCall(object sender, ServerRpcContext e)
    {
        SessionManager.SetCurrentSession(null);
    }

    /// <summary>
    /// Starts listening for incoming messages.
    /// </summary>
    public void Start() => RemotingServer.Start();

    /// <summary>
    /// Stops listening for incoming messages.
    /// </summary>
    public void Stop() => RemotingServer.Stop();

    /// <summary>
    /// Releases all managed resources.
    /// </summary>
    public void Dispose()
    {
        RemotingServer.AfterCall -= RemotingServer_AfterCall;
        RemotingServer.BeginCall -= RemotingServer_BeginCall;
        RemotingServer.Dispose();
    }

    /// <summary>
    /// Registers a component in the component catalog.
    /// </summary>
    /// <typeparam name="TInterface">Component interface type</typeparam>
    /// <typeparam name="TService">Component implementation type</typeparam>
    /// <param name="lifetime">Optional component lifetime</param>
    public ZyanComponentHost RegisterComponent<TInterface, TService>(ActivationType lifetime = ActivationType.SingleCall, string uniqueName = "")
        where TInterface : class
        where TService : class, TInterface, new()
    {
        var serviceLifetime = lifetime == ActivationType.SingleCall ? ServiceLifetime.SingleCall : ServiceLifetime.Singleton;
        Container.RegisterService<TInterface, TService>(serviceLifetime, uniqueName);
        return this;
    }
}