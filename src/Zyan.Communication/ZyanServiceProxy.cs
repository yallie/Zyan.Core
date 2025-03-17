using System;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using CoreRemoting;
using stakx.DynamicProxy;
using Zyan.Communication.CallInterception;
using Zyan.Communication.Toolbox;

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

        // handle call interception
        var cfg = Connection.Config;
        if (cfg.EnableCallInterception && cfg.CallInterceptors.Count > 0 && !CallInterceptor.IsPaused)
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
    /// Helper class to convert an asynchronous interception into a Func{object} delegate.
    /// </summary>
    /// <param name="interceptAsync">Async interception method, i.e. base.InterceptAsync</param>
    /// <param name="invocation">Asynchronous invocation parameters.</param>
    private class AsyncInvocationHelper(Func<IAsyncInvocation, ValueTask> interceptAsync, IAsyncInvocation invocation)
    {
        /// <summary>
        /// Actual method that performs a remote invocation.
        /// </summary>
        private async Task<object> InvokeRemoteMethod()
        {
            await interceptAsync(invocation);
            return invocation.Result;
        }

        /// <summary>
        /// Async invocation method that returns a parameterless Task.
        /// Used for wrapping methods with return type of Task.
        /// This method can be saved as a delegate of type Func{object}.
        /// </summary>
        private async Task InvokeReturnTask()
        {
            await InvokeRemoteMethod();
        }

        /// <summary>
        /// Async invocation method that returns a parameterized Task.
        /// Used for wrapping methods with return type of Task{T}.
        /// This method can be saved as a delegate of type Func{object}.
        /// </summary>
        private async Task<TResult> InvokeReturnTaskT<TResult>()
        {
            var result = await InvokeRemoteMethod();
            return (TResult)result;
        }

        public Func<object> GetMakeRemoteCallFunction(Type taskType)
        {
            if (taskType == typeof(void) || taskType == typeof(Task))
            {
                // create InvokeFuncReturnTask as Func<object>
                var makeRemoteCall = new Func<object>(InvokeReturnTask);
                return makeRemoteCall;
            }

            if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                // get TResult type from Task<TResult>
                var taskReturnType = taskType.GetGenericArguments().First();

                // create InvokeFuncReturnTaskT<TResult> as Func<object>
                var genericMethod = new Func<object>(InvokeReturnTaskT<int>).Method;
                var invokeMethod = genericMethod.GetGenericMethodDefinition().MakeGenericMethod(taskReturnType);
                var invokeFunction = invokeMethod.CreateDelegate<Func<object>>(this);
                return invokeFunction;
            }

            throw new NotSupportedException($"Return type not supported: {taskType.Name}");
        }

        public object GetTaskResult(object taskValue)
        {
            if (taskValue == null || taskValue.GetType() == typeof(Task))
            {
                return null;
            }

            if (taskValue is Task &&
                taskValue.GetType() is Type taskType &&
                taskType.IsGenericType &&
                taskType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var resultInfo = taskType.GetProperty(nameof(Task<int>.Result));
                var result = resultInfo.GetValue(taskValue);
                return result;
            }

            // throw new NotSupportedException($"Task result type not supported: {taskValue.GetType().Name}");
            return taskValue;
        }
    }

    /// <summary>
    /// Intercepts an asynchronous call of a member on the proxy object.
    /// </summary>
    protected override async ValueTask InterceptAsync(IAsyncInvocation invocation)
    {
        // handle call interception
        var cfg = Connection.Config;
        if (cfg.EnableCallInterception && cfg.CallInterceptors.Count > 0 && !CallInterceptor.IsPaused)
        {
            var helper = new AsyncInvocationHelper(base.InterceptAsync, invocation);
            var makeRemoteCall = helper.GetMakeRemoteCallFunction(invocation.Method.ReturnType);

            var interceptor = cfg.CallInterceptors.FindMatchingInterceptor(
                ServiceInterfaceType, ServiceName,
                invocation.Method.MemberType, invocation.Method.Name,
                invocation.Method.GetParameters());

            if (interceptor != null)
            {
                var data = new CallInterceptionData(ServiceName,
                    invocation.Arguments.ToArray(), makeRemoteCall);

                interceptor.OnInterception(data);
                if (data.Intercepted)
                {
                    invocation.Result = helper.GetTaskResult(data.ReturnValue);
                    return;
                }
            }
        }

        // interception disabled, interceptor not found,
        // or the handler didn't intercept the invocation
        await base.InterceptAsync(invocation);
    }
}
