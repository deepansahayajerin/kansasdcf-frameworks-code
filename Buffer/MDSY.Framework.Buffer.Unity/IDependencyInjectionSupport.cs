using System;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace MDSY.Framework.Buffer.Unity
{
    /// <summary>
    /// Defines an object which has access to the dependency injection container which created it. 
    /// </summary>
    /// <remarks>
    /// <para>Objects which need to create instances of other objects should implement IDependencyInjectionSupport.</para>
    /// <note>
    /// To allow the Unity container to inject itself into the created object, the <see cref="Container"/> property
    /// must be decorated with the <c>[Microsoft.Practices.Unity.Dependency]</c> attribute.
    /// </note>
    /// </remarks>
    [Obsolete("No longer supported", true)]
    public interface IDependencyInjectionSupport
    {
        /// <summary>
        /// Gets the IOC (dependency injection) container which created this object instance.
        /// </summary>
        /// <remarks>
        /// The setter should only be used by the Unity container to inject itself into the instantiated object. 
        /// Treat this property as read-only.
        /// </remarks>
        IUnityContainer Container { get; set; }
    }
}
