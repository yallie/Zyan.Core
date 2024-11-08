﻿using System;
using CoreRemoting;
using CoreRemoting.Serialization.Binary;

namespace Zyan.Communication
{
    /// <summary>
    /// Maintains a connection to a Zyan component host.
    /// </summary>
    public class ZyanConnection : IDisposable
    {
        private ZyanConnectionConfig Config { get; set; }

        private RemotingClient RemotingClient { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZyanConnection"/> class.
        /// </summary>
        public ZyanConnection(ZyanConnectionConfig config = null)
        {
            Config = config ?? new ZyanConnectionConfig();
            Config.Serializer = Config.Serializer ?? new BinarySerializerAdapter();

            RemotingClient = new RemotingClient(Config);
            if (Config.AutoConnect)
            {
                RemotingClient.Connect();
            }
        }

        /// <summary>
        /// Connects to the remote server.
        /// </summary>
        public void Connect() => RemotingClient.Connect();

        /// <summary>
        /// Release managed resources.
        /// </summary>
        public void Dispose() => RemotingClient.Dispose();

        /// <summary>
        /// Creates a local proxy object of a specified remote component.
        /// </summary>
        /// <typeparam name="T">Remote component interface type</typeparam>
        /// <returns>Proxy</returns>
        public T CreateProxy<T>() =>
            RemotingClient.CreateProxy<T>();

        /// <summary>
        /// Creates a local proxy object of a specified remote component.
        /// </summary>
        /// <typeparam name="T">Remote component interface type</typeparam>
        /// <param name="uniqueName">Unique component name</param>
        /// <returns>Proxy</returns>
        public T CreateProxy<T>(string uniqueName) =>
            RemotingClient.CreateProxy<T>(uniqueName);

        /// <summary>
        /// Shut down the proxy and disconnect its event handlers from server.
        /// </summary>
        /// <param name="proxy">Proxy to disconnect.</param>
        public void ShutdownProxy(object proxy) =>
            RemotingClient.ShutdownProxy(proxy);
    }
}
