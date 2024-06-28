using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreRemoting.DependencyInjection;
using DryIoc;
using DryIoc.MefAttributedModel;

namespace Zyan.Communication
{
    internal class DryIocAdapter : DependencyInjectionContainerBase, IDependencyInjectionContainer
    {
        private IContainer RootContainer { get; set; } = new Container().WithMef();

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

        public override bool IsRegistered<TServiceInterface>(string serviceName = "")
        {
            return RootContainer.IsRegistered<TServiceInterface>(serviceName);
        }

        private IReuse GetReuse(ServiceLifetime lifetime) =>
            (lifetime == ServiceLifetime.SingleCall) ? Reuse.ScopedOrSingleton : Reuse.Singleton;

        protected override void RegisterServiceInContainer<TServiceInterface, TServiceImpl>(ServiceLifetime lifetime, string serviceName = "")
        {
            RootContainer.Register<TServiceInterface, TServiceImpl>(GetReuse(lifetime), serviceKey: serviceName);
        }

        protected override void RegisterServiceInContainer<TServiceInterface>(Func<TServiceInterface> factoryDelegate, ServiceLifetime lifetime, string serviceName = "")
        {
            RootContainer.RegisterDelegate(factoryDelegate, GetReuse(lifetime), serviceKey: serviceName);
        }

        protected override object ResolveServiceFromContainer(ServiceRegistration registration)
        {
            return RootContainer.Resolve(registration.ImplementationType ?? registration.InterfaceType, serviceKey: registration.ServiceName);
        }

        protected override TServiceInterface ResolveServiceFromContainer<TServiceInterface>(ServiceRegistration registration)
        {
            return ResolveServiceFromContainer(registration) as TServiceInterface;
        }
    }
}
