using CoreRemoting;

namespace Zyan.Communication;

/// <summary>
/// Configuration class for the <see cref="ZyanComponentHost"/>.
/// </summary>
public class ZyanComponentHostConfig : ServerConfig
{
    /// <summary>
    /// Gets a value indicating whether the host should start automatically.
    /// </summary>
    public bool AutoStart { get; set; } = true;
}