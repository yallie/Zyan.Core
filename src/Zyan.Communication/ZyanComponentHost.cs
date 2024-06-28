using System;

namespace Zyan.Communication
{
    /// <summary>
    /// Host for publishing components with Zyan.
    /// </summary>
    public class ZyanComponentHost : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZyanComponentHost" /> class.
        /// </summary>
        public ZyanComponentHost()
        {
        }

        /// <summary>
        /// Starts listening for incoming messages.
        /// </summary>
        public void Start()
        {
        }


        /// <summary>
        /// Stops listening for incoming messages.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Releases all managed resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Registers a component in the component catalog.
        /// </summary>
        /// <typeparam name="TInterface">Component interface type</typeparam>
        /// <typeparam name="TService">Component implementation type</typeparam>
        /// <param name="lifetime">Optional component lifetime</param>
        public void RegisterComponent<TInterface, TService>(ActivationType lifetime = ActivationType.SingleCall)
        {
            // TODO
        }
    }
}
