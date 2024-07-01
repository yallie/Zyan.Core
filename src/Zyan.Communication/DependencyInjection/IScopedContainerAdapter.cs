using CoreRemoting.DependencyInjection;

namespace Zyan.Communication.DependencyInjection
{
    /// <summary>
    /// Adds the ability to create scopes.
    /// </summary>
    public interface IScopedContainer : IDependencyInjectionContainer
    {
        /// <summary>
        /// Opens the child scope.
        /// </summary>
        /// <param name="name">Optional name.</param>
        /// <param name="track">Track the scope in the parent container.</param>
        IScopedContainer OpenScope(string name = null, bool track = false);
    }
}
