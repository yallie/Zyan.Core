using CoreRemoting;

namespace Zyan.Communication;

/// <inheritdoc/>
internal class ZyanProxyBuilder : RemotingProxyBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZyanProxyBuilder"/> class.
    /// </summary>
    /// <param name="connection"><see cref="ZyanConnection"/> instance</param>
    public ZyanProxyBuilder(ZyanConnection connection)
    {
        Connection = connection;
    }

    private ZyanConnection Connection { get; }

    /// <inheritdoc/>
    public override T CreateProxy<T>(RemotingClient remotingClient, string serviceName = "") =>
        (T)ProxyGenerator.CreateInterfaceProxyWithoutTarget(
            interfaceToProxy: typeof(T),
            interceptor: new ZyanServiceProxy<T>(
                connection: Connection,
                client: remotingClient,
                serviceName: serviceName));
}
