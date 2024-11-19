using System;
using System.Collections.Generic;
using CoreRemoting.DependencyInjection;

namespace Zyan.Communication.DependencyInjection
{
    /// <summary>
    /// Upgrades a <see cref="IDependencyInjectionContainer"/> instance to <see cref="IScopedContainer"/>.
    /// </summary>
    internal class ScopedContainerHelper : IScopedContainer
    {
        public static IScopedContainer Get(IDependencyInjectionContainer container) =>
            container is IScopedContainer scoped ? scoped :
            container is IDependencyInjectionContainer normal ? new ScopedContainerHelper(normal) :
            null;

        public ScopedContainerHelper(IDependencyInjectionContainer container) =>
            Container = container;

        private IDependencyInjectionContainer Container { get; }

        public IScopedContainer OpenScope(string name = null, bool track = false) => this;

        public void Dispose() => Container.Dispose();

        public IEnumerable<Type> GetAllRegisteredTypes() =>
            Container.GetAllRegisteredTypes();

        public object GetService(string serviceName) =>
            Container.GetService(serviceName);

        public TServiceInterface GetService<TServiceInterface>(string serviceName = "")
            where TServiceInterface : class =>
            Container.GetService<TServiceInterface>(serviceName);

        public Type GetServiceInterfaceType(string serviceName) =>
            Container.GetServiceInterfaceType(serviceName);

        public ServiceRegistration GetServiceRegistration(string serviceName) =>
            Container.GetServiceRegistration(serviceName);

        public IEnumerable<ServiceRegistration> GetServiceRegistrations(bool includeHiddenSystemServices = false) =>
            Container.GetServiceRegistrations(includeHiddenSystemServices);

        public bool IsRegistered<TServiceInterface>(string serviceName = "")
            where TServiceInterface : class =>
            Container.IsRegistered<TServiceInterface>(serviceName);

        public void RegisterService<TServiceInterface>(Func<TServiceInterface> factoryDelegate, ServiceLifetime lifetime, string serviceName = "", bool asHiddenSystemService = false)
            where TServiceInterface : class =>
            Container.RegisterService(factoryDelegate, lifetime, serviceName, asHiddenSystemService);

        void IDependencyInjectionContainer.RegisterService<TServiceInterface, TServiceImpl>(ServiceLifetime lifetime, string serviceName, bool asHiddenSystemService) =>
            Container.RegisterService<TServiceInterface, TServiceImpl>(lifetime, serviceName, asHiddenSystemService);
    }
}
