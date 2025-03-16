using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace Zyan.Communication.CallInterception;

// Delegate for remote method invocation.
using InvokeRemoteMethodDelegate = Func<IInvocation, MethodInfo, object>;

/// <summary>
/// Describes a single call interception action.
/// </summary>
public class CallInterceptionData
{
    // Delegate for remote invocation
    internal InvokeRemoteMethodDelegate _remoteInvoker = null;

    // Remoting message invocation
    internal IInvocation _methodInvocation = null;

    /// <summary>
    /// Creates a new instance of the CallInterceptionData class.
    /// </summary>
    /// <param name="invokerName">Inform interceptor about proxy unique name.</param>
    /// <param name="parameters">Parameter values of the intercepted call</param>
    /// <param name="remoteInvoker">Delegate for remote invocation</param>
    /// <param name="methodInvocation">Remoting method invocation</param>
    public CallInterceptionData(string invokerName, object[] parameters, InvokeRemoteMethodDelegate remoteInvoker, IInvocation methodInvocation)
    {
        InvokerUniqueName = invokerName;
        Intercepted = false;
        ReturnValue = null;
        Parameters = parameters;
        _remoteInvoker = remoteInvoker;
        _methodInvocation = methodInvocation;
    }

    /// <summary>
    /// Makes a remote call.
    /// </summary>
    /// <returns>Return value of the remotely called method.</returns>
    public object MakeRemoteCall() =>
        _remoteInvoker(_methodInvocation, _methodInvocation.Method);

    /// <summary>
    /// Proxy caller name.
    /// </summary>
    public string InvokerUniqueName { get; }

    /// <summary>
    /// Gets or sets wether the call was intercepted.
    /// </summary>
    public bool Intercepted { get; set; }

    /// <summary>
    /// Gets or sets the return value to be used.
    /// </summary>
    public object ReturnValue { get; set; }

    /// <summary>
    /// Gets or sets the parameters which are passed to the call.
    /// </summary>
    public object[] Parameters { get; set; }
}