using System;
using CoreRemoting;
using CoreRemoting.DependencyInjection;
using CoreRemoting.Serialization.Binary;
using Zyan.Communication.DependencyInjection;
using Zyan.Communication.SessionMgmt;
using Zyan.Communication.Toolbox;

namespace Zyan.Communication;

/// <summary>
/// Host for publishing components with Zyan.
/// </summary>
public partial class ZyanComponentHost : IDisposable
{
    public ZyanComponentHostConfig Config { get; private set; }

    private RemotingServer RemotingServer { get; set; }

    private ISessionManager SessionManager { get; set; }

    private IDependencyInjectionContainer Container =>
        RemotingServer.ServiceRegistry;

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
            Config.KeySize,
            Config.InactiveSessionSweepInterval,
            Config.MaximumSessionInactivityTime,
            Config.SessionRepository);

        SessionManager = sessionManager;
        Config.SessionRepository = sessionManager;
        Config.DependencyInjectionContainer ??= new DryIocAdapter();

        // start up the server as specified in the config
        RemotingServer = new RemotingServer(Config);
        AttachRemotingServerEvents();
        if (Config.AutoStart)
        {
            RemotingServer.Start();
        }
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
        DetachRemotingServerEvents();
        RemotingServer.Dispose();
    }

    /// <summary>
    /// Registers a component in the component catalog.
    /// </summary>
    /// <typeparam name="TInterface">Component interface type</typeparam>
    /// <typeparam name="TService">Component implementation type</typeparam>
    /// <param name="serviceLifetime">Optional component lifetime</param>
    public ZyanComponentHost RegisterComponent<TInterface, TService>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped, string uniqueName = "")
        where TInterface : class
        where TService : class, TInterface, new()
    {
        Container.RegisterService<TInterface, TService>(serviceLifetime, uniqueName);
        Container.RegisterQueryableMethods<TInterface, TService>(serviceLifetime, uniqueName);
        return this;
    }
}