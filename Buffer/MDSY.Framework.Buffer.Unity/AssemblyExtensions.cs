using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MDSY.Framework.Buffer.Unity
{
    /// <summary>
    /// Provides extended functionality for Assemblies specific to managing IDefineTypeRegistration-bearing modules.
    /// </summary>
    internal static class AssemblyExtensions
    {
        #region private methods
        /// <summary>
        /// Returns <c>true</c> if the given type is an IDefineTypeRegistration implementor 
        /// which can be created via a parameterless constructor.
        /// </summary>
        private static bool IsTypeRegistrationDefiningModule(Type type)
        {
            return typeof(ITypeRegistrationModule).IsAssignableFrom(type)
                && !type.IsAbstract
                && !type.IsInterface
                && type.GetConstructor(Type.EmptyTypes) != null;
        }
        #endregion

        #region extension methods
        /// <summary>
        /// Returns a list of an IDefineTypeRegistration implementers contained within the assembly.
        /// </summary>
        public static IEnumerable<ITypeRegistrationModule> GetInjectionModules(this Assembly instance)
        {
            return instance.GetExportedTypes()
                    .Where(IsTypeRegistrationDefiningModule)
                    .Select(type => Activator.CreateInstance(type) as ITypeRegistrationModule);
        }

        /// <summary>
        /// Returns <c>true</c> if the Assembly contains at least one implementor of IDefineTypeRegistration.
        /// </summary>
        public static bool HasInjectionModules(this Assembly instance)
        {
            return instance.GetExportedTypes().Any(IsTypeRegistrationDefiningModule);
        }
        #endregion

    }
}

