using CoreRemoting;
using System;
using System.Collections.Generic;

namespace Zyan.Communication;

/// <summary>
/// Describes arguments for events raised after remote method invocation.
/// </summary>
public class BeforeInvokeEventArgs(ServerRpcContext context) : EventArgs
{
    /// <summary>
    /// Gets or sets the server RPC context.
    /// </summary>
    public ServerRpcContext Context { get; private set; } =
        context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Gets or sets a unique ID for call tracking.
    /// </summary>
    public Guid TrackingID => Context.UniqueCallKey;

    /// <summary>
    /// Gets or sets a cancel flag.
    /// </summary>
    public bool Cancel
    { 
        get => Context.Cancel;
        set => Context.Cancel = value;
    }

    /// <summary>
    /// Gets or sets the exception in case of cancellation.
    /// </summary>
    public Exception CancelException
    {
        get => Context.Exception;
        set => Context.Exception = value;
    }

    /// <summary>
    /// Gets or sets the interface name of the remote component.
    /// </summary>
    public string InterfaceName
    {
        get => Context.MethodCallMessage.ServiceName;
        set => Context.MethodCallMessage.ServiceName = value;
    }

    // <summary>
    // Gets or sets the correlation set for wiring remote delegates.
    // </summary>
    //public List<DelegateCorrelationInfo> DelegateCorrelationSet { get; set; }

    /// <summary>
    /// Gets or sets the name of the remote method to be invoked.
    /// </summary>
    public string MethodName
    {
        get => Context.MethodCallMessage.ServiceName;
        set => Context.MethodCallMessage.ServiceName = value;
    }

    /// <summary>
    /// Gets or sets method arguments (parameters).
    /// </summary>
    public object[] Arguments
    {
        get => Context.MethodCallParameterValues;
        set => Context.MethodCallParameterValues = value;
    }

    /// <summary>
    /// Returns a string representation of this event arguments.
    /// </summary>
    /// <returns>String representation of data</returns>
    public override string ToString()
    {
        var argsAsString = new List<string>();

        if (Arguments != null)
        {
            foreach (object arg in Arguments)
            {
                if (arg == null)
                    argsAsString.Add("null");
                else
                    argsAsString.Add(arg.ToString());
            }
        }

        string argChain = string.Join(", ", argsAsString.ToArray());

        return string.Format("{0}.{1}({2})", InterfaceName, MethodName, argChain);
    }
}
