using System;

namespace Zyan.Communication
{
    /// <summary>
    /// Maintains a connection to a Zyan component host.
    /// </summary>
    public class ZyanConnection : IDisposable
    {
        /// <summary>
        /// Release managed resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Creates a local proxy object of a specified remote component.
        /// </summary>
        /// <typeparam name="T">Remote component interface type</typeparam>
        /// <returns>Proxy</returns>
        public T CreateProxy<T>() => CreateProxy<T>(null);

        /// <summary>
        /// Creates a local proxy object of a specified remote component.
        /// </summary>
        /// <typeparam name="T">Remote component interface type</typeparam>
        /// <param name="uniqueName">Unique component name</param>
        /// <returns>Proxy</returns>
        public T CreateProxy<T>(string uniqueName)
        {
            throw new NotImplementedException();
        }
    }
}
