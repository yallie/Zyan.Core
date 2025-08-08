using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using CoreRemoting;
using CoreRemoting.RpcMessaging;
using stakx.DynamicProxy;
using Zyan.Communication.CallInterception;
using Zyan.Communication.Toolbox;
using Zyan.InterLinq;

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
        if (HandleCallInterception(invocation))
            return;

        if (HandleLinqQuery(invocation))
            return;

        base.Intercept(invocation);
    }

    private bool HandleCallInterception(IInvocation invocation)
    {
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
                object MakeRemoteCall()
                {
                    base.Intercept(invocation);
                    return invocation.ReturnValue;
                }

                var data = new CallInterceptionData(ServiceName,
                    invocation.Arguments, MakeRemoteCall);

                interceptor.InterceptionHandler(data);
                if (data.Intercepted)
                {
                    invocation.ReturnValue = data.ReturnValue;
                    return true;
                }
            }
        }

        // interception disabled, interceptor not found,
        // or the handler didn't intercept the invocation
        return false;
    }

    private bool HandleLinqQuery(IInvocation invocation)
    {
        // handle remote LINQ query
        var methodInfo = invocation.Method;
        if (methodInfo.GetParameters().Length == 0 &&
            methodInfo.GetGenericArguments().Length == 1 &&
            methodInfo.ReturnType.IsGenericType &&
            methodInfo.ReturnType.GetGenericTypeDefinition() is Type returnTypeDef &&
            (returnTypeDef == typeof(IEnumerable<>) || returnTypeDef == typeof(IQueryable<>)))
        {
            var elementType = methodInfo.GetGenericArguments().First();
            var serverHandlerName = ZyanMethodQueryHandler.GetMethodQueryHandlerName(ServiceName, methodInfo);
            var clientHandler = new ZyanClientQueryHandler(Connection, serverHandlerName);
            invocation.ReturnValue = clientHandler.Get(elementType);
            return true;
        }

        // This method call doesn't represent a LINQ query
        return false;
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

        public async Task<object> GetTaskResult(object taskValue)
        {
            if (taskValue == null || taskValue.GetType() == typeof(Task))
            {
                // run to completion
                if (taskValue is Task task)
                    await task;

                return null;
            }

            if (taskValue is Task taskWithResult &&
                taskValue.GetType() is Type taskType &&
                taskType.IsGenericType &&
                taskType.GetProperty(nameof(Task<int>.Result)) is PropertyInfo resultInfo)
            {
                // run to completion
                await taskWithResult;

                // get result
                var result = resultInfo.GetValue(taskValue);
                return result;
            }

            throw new NotSupportedException($"Task result type not supported: {taskValue.GetType().Name}");
            //return taskValue;
        }
    }

    /// <summary>
    /// Intercepts an asynchronous call of a member on the proxy object.
    /// </summary>
    protected override async ValueTask InterceptAsync(IAsyncInvocation invocation)
    {
        if (await HandleCallInterception(invocation))
            return;

        await base.InterceptAsync(invocation);
    }

    private async Task<bool> HandleCallInterception(IAsyncInvocation invocation)
    {
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
                var helper = new AsyncInvocationHelper(base.InterceptAsync, invocation);
                var makeRemoteCall = helper.GetMakeRemoteCallFunction(invocation.Method.ReturnType);

                var data = new CallInterceptionData(ServiceName,
                    invocation.Arguments.ToArray(), makeRemoteCall);

                interceptor.InterceptionHandler(data);
                if (data.Intercepted)
                {
                    invocation.Result = await helper.GetTaskResult(data.ReturnValue);
                    return true;
                }
            }
        }

        // interception disabled, interceptor not found,
        // or the handler didn't intercept the invocation
        return false;
    }
}
