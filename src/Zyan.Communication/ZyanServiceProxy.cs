using CoreRemoting;

namespace Zyan.Communication;

/// <inheritdoc/>
internal class ZyanServiceProxy<T> : ServiceProxy<T>
{
    /// <summary>
    /// Creates a new instance of the ServiceProxy class.
    /// </summary>
    /// <param name="connection"><see cref="ZyanConnection"/> instance.</param>
    /// <param name="client">CoreRemoting client to be used for client/server communication</param>
    /// <param name="serviceName">Unique name of the remote service</param>
    public ZyanServiceProxy(ZyanConnection connection, RemotingClient client, string serviceName = "")
        : base(client, serviceName)
    {
        Connection = connection;
    }

    private ZyanConnection Connection { get; }
}
