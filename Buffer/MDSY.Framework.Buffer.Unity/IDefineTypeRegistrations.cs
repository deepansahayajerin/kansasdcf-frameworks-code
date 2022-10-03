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
    /// Objects which implement <c>IDefineTypeRegistrations</c> should implement 
    /// <c>LoadRegistrationsInto</c> to perform
    /// type registrations with the given IUnityContainer.
    /// Note: any object implementing <c>IDefineTypeRegistrations</c> found in 
    /// the application's working path at registration load-up time
    /// will have <c>LoadRegistrationsInto()</c> called automatically unless <c>LoadAutomatically</c> is <c>false</c>.
    /// </remarks>
    [Obsolete("Use ITypeRegistrationModule instead", true)]
    public interface IDefineTypeRegistrations
    {
        /// <summary>
        /// Assigns Unity type registration mapping using the given <paramref name="container"/>. 
        /// </summary>
        /// <param name="container">The IUnityContainer with which type mappings will be registered.</param>
        void LoadRegistrationsInto(IUnityContainer container);

        /// <summary>
        /// Gets whether the type mappings set up by LoadRegistrationsInto should be automatically registered.
        /// This value is not currently regarded. 
        /// </summary>
        bool LoadAutomatically { get; }
    }



}
