using CoreRemoting;

namespace Zyan.Communication
{
    public class ZyanConnectionConfig : ClientConfig
    {
        /// <summary>
        /// Automatically connects the client.
        /// </summary>
        public bool AutoConnect { get; set; } = true;
    }
}
