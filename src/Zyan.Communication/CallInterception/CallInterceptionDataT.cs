using System;
using System.Reflection;
using Castle.DynamicProxy;
using BaseCallInterceptionData = Zyan.Communication.CallInterception.CallInterceptionData;

namespace Zyan.Communication.CallInterception;

/// <summary>
/// Describes a single call interception action.
/// </summary>
public class CallInterceptionData<T>
{
    /// <summary>
    /// Creates a new instance of the CallInterceptionData{T} class.
    /// </summary>
    public CallInterceptionData(BaseCallInterceptionData data) => Data = data;

    /// <summary>
    /// Actual call interception data.
    /// </summary>
    public BaseCallInterceptionData Data { get; private set; }

    /// <summary>
    /// Gets or sets a value whether the remote call is intercepted.
    /// </summary>
    public bool Intercepted
    { 
        get => Data.Intercepted;
        set => Data.Intercepted = value;
    }

    /// <summary>
    /// Makes a remote call.
    /// </summary>
    /// <returns>
    /// Return value of the remotely called method.
    /// </returns>
    public T MakeRemoteCall() => (T)Data.MakeRemoteCall();

    /// <summary>
    /// Implicit conversion operator for <see cref="BaseCallInterceptionData"/>.
    /// </summary>
    public static implicit operator CallInterceptionData<T>(BaseCallInterceptionData data) =>
        new CallInterceptionData<T>(data);

    /// <summary>
    /// Implicit conversion operator for <see cref="CallInterceptionData{T}"/>.
    /// </summary>
    public static implicit operator BaseCallInterceptionData(CallInterceptionData<T> data) =>
        data.Data;
}