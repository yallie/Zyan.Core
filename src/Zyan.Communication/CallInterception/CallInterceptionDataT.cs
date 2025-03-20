namespace Zyan.Communication.CallInterception;

/// <summary>
/// Describes a single call interception action.
/// </summary>
public class CallInterceptionData<T>
{
    /// <summary>
    /// Actual call interception data.
    /// </summary>
    public CallInterceptionData Data { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the remote call was intercepted.
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
    /// Implicit conversion operator for <see cref="CallInterceptionData"/>.
    /// </summary>
    public static implicit operator CallInterceptionData<T>(CallInterceptionData data) => new ()
    {
        Data = data,
    };
}