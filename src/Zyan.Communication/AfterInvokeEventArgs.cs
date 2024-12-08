using CoreRemoting;
using System;
using System.Collections.Generic;

namespace Zyan.Communication;

/// <summary>
/// Describes arguments for events raised after remote method invocation.
/// </summary>
public class AfterInvokeEventArgs(ServerRpcContext context) : EventArgs
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
    /// Gets or sets the return value of the invoked method.
    /// </summary>
    public object ReturnValue { get; set; } // TODO
    //{
    //    get => Context.Re;
    //    set => Context.MethodCallParameterValues = value;
    //}

    /// <summary>
    /// Returns a string representation of this event arguments.
    /// </summary>
    /// <returns>String representation of data</returns>
    public override string ToString()
    {
        List<string> argsAsString = new List<string>();

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

        if (ReturnValue != null)
            return string.Format("{0}.{1}({2}) = {3}", InterfaceName, MethodName, argChain, ReturnValue.ToString());
        else
            return string.Format("{0}.{1}({2}) = null", InterfaceName, MethodName, argChain);
    }
}
