﻿using System;
using System.Collections.Generic;
using CoreRemoting.DependencyInjection;
using DryIoc;
using DryIoc.MefAttributedModel;

namespace Zyan.Communication.DependencyInjection
{
    /// <summary>
    /// Dependency injection container based on DryIoc container implementation.
    /// </summary>
    public class DryIocAdapter : DependencyInjectionContainerBase, IDependencyInjectionContainer, IScopedContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DryIocAdapter"/> class.
        /// </summary>
        /// <param name="container">Optional container instance.</param>
        public DryIocAdapter(IContainer container = null) =>
            RootContainer = container ?? new Container().WithMef();

        private IContainer RootContainer { get; }

        public override void Dispose()
        {
            base.Dispose();
            RootContainer.Dispose();
        }

        public override IEnumerable<Type> GetAllRegisteredTypes()
        {
            // TODO
            return Array.Empty<Type>();
        }

        public override bool IsRegistered<TServiceInterface>(string serviceName = "") =>
            RootContainer.IsRegistered<TServiceInterface>(serviceName);

        private IReuse GetReuse(ServiceLifetime lifetime) =>
            lifetime == ServiceLifetime.SingleCall ? Reuse.ScopedOrSingleton : Reuse.Singleton;

        protected override void RegisterServiceInContainer<TServiceInterface, TServiceImpl>(ServiceLifetime lifetime, string serviceName = "") =>
            RootContainer.Register<TServiceInterface, TServiceImpl>(GetReuse(lifetime), serviceKey: serviceName);

        protected override void RegisterServiceInContainer<TServiceInterface>(Func<TServiceInterface> factoryDelegate, ServiceLifetime lifetime, string serviceName = "") =>
            RootContainer.RegisterDelegate(factoryDelegate, GetReuse(lifetime), serviceKey: serviceName);

        protected override object ResolveServiceFromContainer(ServiceRegistration registration) =>
            RootContainer.Resolve(registration.InterfaceType ?? registration.ImplementationType, serviceKey: registration.ServiceName);

        protected override TServiceInterface ResolveServiceFromContainer<TServiceInterface>(ServiceRegistration registration) =>
            ResolveServiceFromContainer(registration) as TServiceInterface;

        public IScopedContainer OpenScope(string name = null, bool track = false)
        {
            RootContainer.OpenScope(name, track);
            return this;
        }
    }
}
