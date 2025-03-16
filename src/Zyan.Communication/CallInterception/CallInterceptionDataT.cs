using System;
using System.Reflection;
using Castle.DynamicProxy;
using BaseCallInterceptionData = Zyan.Communication.CallInterception.CallInterceptionData;

namespace Zyan.Communication.CallInterception;

// Delegate for remote method invocation.
using InvokeRemoteMethodDelegate = Func<IInvocation, MethodInfo, object>;

/// <summary>
/// Describes a single call interception action.
/// </summary>
public class CallInterceptionData<T> : BaseCallInterceptionData
{
    /// <summary>
    /// Creates a new instance of the CallInterceptionData{T} class.
    /// </summary>
    /// <param name="invokerName">Inform interceptor about proxy unique name.</param>
    /// <param name="parameters">Parameter values of the intercepted call</param>
    /// <param name="remoteInvoker">Delegate for remote invocation</param>
    /// <param name="methodInvocation">Remoting method invocation</param>
    public CallInterceptionData(string invokerName, object[] parameters, InvokeRemoteMethodDelegate remoteInvoker, IInvocation methodInvocation)
        : base(invokerName, parameters, remoteInvoker, methodInvocation)
    {
    }

    /// <summary>
    /// Creates a new instance of the CallInterceptionData{T} class.
    /// </summary>
    public CallInterceptionData(BaseCallInterceptionData data)
        : this(data.InvokerUniqueName, data.Parameters, data._remoteInvoker, data._methodInvocation)
    { 
    }

    /// <summary>
    /// Makes a remote call.
    /// </summary>
    /// <returns>Return value of the remotely called method.</returns>
    public new T MakeRemoteCall() => (T)base.MakeRemoteCall();
}