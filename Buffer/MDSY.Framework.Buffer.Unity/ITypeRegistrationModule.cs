using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity;

namespace MDSY.Framework.Buffer.Unity
{
    /// <summary>
    /// Defines the interaction points for providing dynamic, assembly-based 
    /// type registration with an IUnityContainer.
    /// </summary>
    /// <remarks> 
    /// Objects which implement <c>ITypeRegistrationModule</c> should implement 
    /// <c>LoadRegistrationsInto</c> to perform type registrations with the given IUnityContainer.
    /// </remarks>
    public interface ITypeRegistrationModule
    {
        /// <summary>
        /// Assigns Unity type registration mapping using the given <paramref name="container"/>. 
        /// </summary>
        /// <param name="container">The IUnityContainer with which type mappings will be registered.</param>
        void LoadRegistrationsInto(IUnityContainer container);
    }
}

