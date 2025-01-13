using System;
using System.Reflection.Emit;
using System.Reflection;

namespace Zyan.InterLinq
{
    public static class AppDomainExtensions
    {
        /// <summary>
        /// Backwards compatibility method to define dynamic assembly.
        /// </summary>
        /// <param name="ad">Application domain (ignored).</param>
        /// <param name="name">Assembly name.</param>
        /// <param name="access">Assembly access.</param>
        public static AssemblyBuilder DefineDynamicAssembly(this AppDomain ad,
            AssemblyName name, AssemblyBuilderAccess access)
        {
            return AssemblyBuilder.DefineDynamicAssembly(name, access);
        }
    }

    namespace Communication
    {
        namespace Wcf.NetDataContractSerializer
        {
            internal class NetDataContractFormatAttribute : Attribute { }
        }
    }
}

namespace System.ServiceModel
{
    internal class ServiceContractAttribute : Attribute { }
    internal class OperationContractAttribute : Attribute { }
    internal class FaultContractAttribute : Attribute
    {
        public FaultContractAttribute(Type type) { }
    }
}