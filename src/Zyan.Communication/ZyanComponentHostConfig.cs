using CoreRemoting;
using CoreRemoting.DependencyInjection;
using Zyan.Communication.DependencyInjection;

namespace Zyan.Communication
{
    /// <summary>
    /// Configuration class for the <see cref="ZyanComponentHost"/>.
    /// </summary>
    public class ZyanComponentHostConfig : ServerConfig
    {
        /// <summary>
        /// Gets a value indicating whether the host should start automatically.
        /// </summary>
        public bool AutoStart { get; set; } = true;

        /// <summary>
        /// Gets or sets the dependency injection container to be used for service registration.
        /// DryIoc Container is used, if set to null.
        /// </summary>
        public override IDependencyInjectionContainer DependencyInjectionContainer
        {
            get => base.DependencyInjectionContainer;
            set => base.DependencyInjectionContainer = ScopedContainerHelper.Get(value);
        }

        /// <summary>
        /// Gets or sets the scoped dependency injection container.
        /// DryIoc Container is used, if set to null.
        /// </summary>
        public IScopedContainer ScopedContainer
        {
            get => DependencyInjectionContainer as IScopedContainer;
            set => DependencyInjectionContainer = value;
        }
    }
}
