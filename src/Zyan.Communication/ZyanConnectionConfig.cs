using CoreRemoting;
using Zyan.Communication.CallInterception;

namespace Zyan.Communication;

public class ZyanConnectionConfig : ClientConfig
{
    /// <summary>
    /// Automatically connects the client.
    /// </summary>
    public bool AutoConnect { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether call interception is enabled.
    /// </summary>
    public bool EnableCallInterception { get; set; } = true;

    /// <summary>
    /// Gets the call interceptors.
    /// </summary>
    public CallInterceptorCollection CallInterceptors { get; set; } = new();
}