using System;
using CoreRemoting;
using CoreRemoting.Serialization.Binary;

namespace Zyan.Communication
{
    /// <summary>
    /// Maintains a connection to a Zyan component host.
    /// </summary>
    public class ZyanConnection : IDisposable
    {
        private ClientConfig ClientConfig { get; set; }

        private RemotingClient RemotingClient { get; set; }

        private Action ConnectAsNeeded { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZyanConnection"/> class.
        /// </summary>
        public ZyanConnection(ClientConfig config = null)
        { 
            ClientConfig = config ?? new ClientConfig();
            ClientConfig.Serializer = new BinarySerializerAdapter();
            RemotingClient = new RemotingClient(ClientConfig);

            // automatically initialize the connection
            var nop = ConnectAsNeeded = () => { };
            ConnectAsNeeded = () =>
            {
                RemotingClient.Connect();
                ConnectAsNeeded = nop;
            };
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
        public T CreateProxy<T>()
        {
            ConnectAsNeeded();
            return RemotingClient.CreateProxy<T>();
        }

        /// <summary>
        /// Creates a local proxy object of a specified remote component.
        /// </summary>
        /// <typeparam name="T">Remote component interface type</typeparam>
        /// <param name="uniqueName">Unique component name</param>
        /// <returns>Proxy</returns>
        public T CreateProxy<T>(string uniqueName)
        {
            ConnectAsNeeded();
            return RemotingClient.CreateProxy<T>(uniqueName);
        }
    }
}
