using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using CoreRemoting;
using stakx.DynamicProxy;
using Zyan.Communication.CallInterception;

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

    /// <summary>
    /// Intercepts a synchronous call of a member on the proxy object.
    /// </summary>
    /// <param name="invocation">Intercepted invocation details</param>
    protected override void Intercept(IInvocation invocation)
    {
        object MakeRemoteCall()
        {
            base.Intercept(invocation);
            return invocation.ReturnValue;
        }

        var cfg = Connection.Config;
        if (cfg.EnableCallInterception)
        {
            var interceptor = cfg.CallInterceptors.FindMatchingInterceptor(
                ServiceInterfaceType, ServiceName,
                invocation.Method.MemberType, invocation.Method.Name,
                invocation.Method.GetParameters());

            if (interceptor != null)
            {
                var data = new CallInterceptionData(ServiceName,
                    invocation.Arguments, MakeRemoteCall);

                interceptor.OnInterception(data);
                if (data.Intercepted)
                {
                    invocation.ReturnValue = data.ReturnValue;
                    return;
                }
            }
        }

        // interception disabled, interceptor not found,
        // or the handler didn't intercept the invocation
        base.Intercept(invocation);
    }

    /// <summary>
    /// Intercepts an asynchronous call of a member on the proxy object.
    /// </summary>
    protected override async ValueTask InterceptAsync(IAsyncInvocation invocation)
    {
        // not sure how to handle it
        async Task MakeRemoteCallAsync() =>
            await base.InterceptAsync(invocation);

        object MakeRemoteCall() =>
            MakeRemoteCallAsync();

        var cfg = Connection.Config;
        if (cfg.EnableCallInterception)
        {
            var interceptor = cfg.CallInterceptors.FindMatchingInterceptor(
                ServiceInterfaceType, ServiceName,
                invocation.Method.MemberType, invocation.Method.Name,
                invocation.Method.GetParameters());

            if (interceptor != null)
            {
                var data = new CallInterceptionData(ServiceName,
                    invocation.Arguments.ToArray(), MakeRemoteCall);

                interceptor.OnInterception(data);
                if (data.Intercepted)
                {
                    invocation.Result = data.ReturnValue;
                    return;
                }
            }
        }

        // interception disabled, interceptor not found,
        // or the handler didn't intercept the invocation
        await base.InterceptAsync(invocation);
    }
}
