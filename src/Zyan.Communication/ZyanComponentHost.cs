using System;
using CoreRemoting;
using CoreRemoting.DependencyInjection;
using CoreRemoting.Serialization.Binary;
using Zyan.Communication.DependencyInjection;

namespace Zyan.Communication
{
    /// <summary>
    /// Host for publishing components with Zyan.
    /// </summary>
    public class ZyanComponentHost : IDisposable
    {
        private ServerConfig ServerConfig { get; set; }

        private RemotingServer RemotingServer { get; set; }

        private IScopedContainer DiContainer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZyanComponentHost" /> class.
        /// </summary>
        /// <param name="config">Remoting configuration, optional</param>
        public ZyanComponentHost(ServerConfig config = null)
        {
            ServerConfig = config ?? new ServerConfig();
            ServerConfig.Serializer = ServerConfig.Serializer ?? new BinarySerializerAdapter();

            // make sure we're using the scoped container
            ServerConfig.DependencyInjectionContainer = DiContainer =
                (ServerConfig.DependencyInjectionContainer as IScopedContainer) ??
                new DryIocAdapter();

            RemotingServer = new RemotingServer(ServerConfig);
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
        public void Dispose() => Stop();

        /// <summary>
        /// Registers a component in the component catalog.
        /// </summary>
        /// <typeparam name="TInterface">Component interface type</typeparam>
        /// <typeparam name="TService">Component implementation type</typeparam>
        /// <param name="lifetime">Optional component lifetime</param>
        public void RegisterComponent<TInterface, TService>(ActivationType lifetime = ActivationType.SingleCall, string uniqueName = "")
            where TInterface : class
            where TService : class, TInterface, new()
        {
            var serviceLifetime = lifetime == ActivationType.SingleCall ? ServiceLifetime.SingleCall : ServiceLifetime.Singleton;
            DiContainer.RegisterService<TInterface, TService>(serviceLifetime, uniqueName);
        }
    }
}
