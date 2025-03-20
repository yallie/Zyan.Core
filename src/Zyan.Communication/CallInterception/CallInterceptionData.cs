using System;

namespace Zyan.Communication.CallInterception;

/// <summary>
/// Describes a single call interception action.
/// </summary>
public class CallInterceptionData
{
    /// <summary>
    /// Creates a new instance of the CallInterceptionData class.
    /// </summary>
    /// <param name="invokerName">Inform interceptor about proxy unique name.</param>
    /// <param name="parameters">Parameter values of the intercepted call</param>
    /// <param name="remoteInvoker">Delegate for remote invocation</param>
    public CallInterceptionData(string invokerName, object[] parameters, Func<object> remoteInvoker)
    {
        InvokerUniqueName = invokerName;
        Intercepted = false;
        ReturnValue = null;
        Parameters = parameters;
        MakeRemoteCall = remoteInvoker;
    }

    /// <summary>
    /// Makes a remote call.
    /// </summary>
    /// <returns>Return value of the remotely called method.</returns>
    public Func<object> MakeRemoteCall { get; }

    /// <summary>
    /// Proxy caller name.
    /// </summary>
    public string InvokerUniqueName { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the call was intercepted.
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