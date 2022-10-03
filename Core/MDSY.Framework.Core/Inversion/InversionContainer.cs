using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Provides simple Dependency Inversion support through interface-based type mapping defined in an app.config file.
    /// </summary>
    /// <remarks>
    /// <para>InversionContainer provides inversion facilities, decoupled from a specific DI provider.</para>
    /// <para>Currently support is provided through simple type mapping in an app.config file. When DI needs grow, we 
    /// can replace this with Unity (or preferably Ninject), etc.</para>
    /// <para>
    /// The app.config settings are as follows:
    /// To specify a implementing type mapping for an interface, three app.config keys are required, and two are optional.
    /// All keys should be prefixed with the name of the interface to be implemented, the word "Implementer", and an underscore. 
    /// The keys are:
    /// <list type="table">
    /// <listheader>
    /// <term>Key</term>
    /// <term>Description</term>
    /// <term>Optional?</term>
    /// </listheader>
    /// <item>
    ///     <term>[InterfaceName]Implementer</term>
    ///     <term>A boolean value to indicate whether the type mapping is enabled.</term>
    ///     <term>No</term>
    /// </item>
    /// <item>
    ///     <term>[InterfaceName]Implementer_ImplementingType</term>
    ///     <term>The name of the class that implements the interface.</term>
    ///     <term>No</term>
    /// </item>
    /// <item>
    ///     <term>[InterfaceName]Implementer_Assembly</term>
    ///     <term>The short name (filename) of the assembly which contains the implementing type.</term>
    ///     <term>No</term>
    /// </item>
    /// <item>
    ///     <term>[InterfaceName]Implementer_AssemblyPath</term>
    ///     <term>If the assembly is not local, provides the path to the assembly.</term>
    ///     <term>Yes</term>
    /// </item>
    /// <item>
    ///     <term>[InterfaceName]Implementer_FullAssemblyName</term>
    ///     <term>If the assembly is located in the GAC, provides the Version, Culture, and PublicKeyToken
    ///     parts of the fully qualified assembly name (see example).</term>
    ///     <term>Yes</term>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// To load an implementing type from a local assembly:
    /// <code>
    /// &lt;add key="IMyInterfaceImplementer" value="true" /&gt;
    /// &lt;add key="IMyInterfaceImplementer_ImplementingType" value="MyImplementation" /&gt;
    /// &lt;add key="IMyInterfaceImplementer_Assembly" value="myCustomImpl.IO.dll" /&gt;
    /// </code>
    /// </example>
    /// <example>
    /// To load an implementing type from an assembly in another location:
    /// <code>
    /// &lt;add key="IMyInterfaceImplementer" value="true" /&gt;
    /// &lt;add key="IMyInterfaceImplementer_ImplementingType" value="MyImplementation" /&gt;
    /// &lt;add key="IMyInterfaceImplementer_Assembly" value="myCustomImpl.IO.dll" /&gt;
    /// &lt;add key="IMyInterfaceImplementer_AssemblyPath" value="c:\myDir\" /&gt;
    /// </code>
    /// </example>
    /// <example>
    /// To load an implementing type from a GAC assembly:
    /// <code>
    /// &lt;add key="IMyInterfaceImplementer" value="true" /&gt;
    /// &lt;add key="IMyInterfaceImplementer_ImplementingType" value="MyImplementation" /&gt;
    /// &lt;add key="IMyInterfaceImplementer_Assembly" value="myCustomImpl.IO.dll" /&gt;
    /// &lt;add key="IMyInterfaceImplementer_FullAssemblyName" value="Version=1.0.2013.0, Culture=neutral, PublicKeyToken=8742b27f8da049c3" /&gt;
    /// </code>
    /// </example>
    public static class InversionContainer
    {
        #region static readonly
        private static readonly string STR_EX_NoTypeFound = "No type was found to implement interface type {0}.";
        private static readonly string STR_EX_DoesNotImplementType = "Type {0} does not implement type {1}.";
        private static readonly string STR_EX_HandlerIsAbstract = "{0}-implementing type {1} cannot be constructed because it is abstract.";
        private static readonly string STR_EX_HandlerIsInterface = "{0}-implementing type {1} cannot be constructed because it is itself an interface.";
        private static readonly string STR_EX_NoParameterlessConstructor = "{0}-implementing type {1} cannot be constructed because it does not have a parameterless constructor.";
        private static readonly string STR_EX_AssmFileNotFound = "Specified load assembly for implementation of interface {0} not found.";
        private static readonly string STR_EX_CannotLocateAssemblyPath = "Unable to locate implementation assembly";
        private static readonly string STR_EX_AssemblyNameNotInSettings = "Implementing assembly not found in appSettings.";
        private static readonly string STR_EX_ImplementingTypeNotInSettings = "Implementing type not found in appSettings.";
        #endregion

        #region private methods
        private static Assembly LoadFromGac(string fullName)
        {
            Assembly assembly;
            try
            {
                AssemblyName aName = new AssemblyName(fullName);
                assembly = Assembly.Load(aName);
            }
            catch (FileLoadException ex)
            {
                throw new InversionContainerException(string.Format("The assembly with the given name, '{0}', could not be found.", fullName), ex);
            }
            return assembly;
        }
        /// <summary>
        /// Returns from config file the implementing class type to use for interfaces of the given <paramref name="interfaceType"/>.
        /// </summary>
        private static Type GetImplementingType(Type interfaceType)
        {
            Type result = null;
            string keyName = GetKeyName(interfaceType);
            string implementingType = string.Empty;
            string assemblyName = string.Empty;
            string assemblyPath = string.Empty;
            string gacAssemblyName = string.Empty;

            if (TryGetMappingSettings(keyName, out implementingType, out assemblyName, out assemblyPath, out gacAssemblyName))
            {
                if (String.IsNullOrEmpty(implementingType))
                    throw new InversionContainerException(String.Concat(STR_EX_ImplementingTypeNotInSettings, " ", keyName), interfaceType, null);

                if (String.IsNullOrEmpty(assemblyName))
                    throw new InversionContainerException(STR_EX_AssemblyNameNotInSettings, interfaceType, null);

                if (!Path.HasExtension(assemblyName))
                {
                    assemblyName = Path.ChangeExtension(assemblyName, "dll");
                }

                Assembly assembly = null;

                // prefer loading from GAC
                if (!String.IsNullOrEmpty(gacAssemblyName))
                {
                    assembly = LoadFromGac(String.Format("{0}, {1}", assemblyName, gacAssemblyName));
                }
                else if (String.IsNullOrEmpty(assemblyPath))
                {
                    try
                    {
                        assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyName);
                    }
                    catch (Exception ex)
                    {
                        throw new InversionContainerException(STR_EX_CannotLocateAssemblyPath, interfaceType, null, ex);
                    }
                }

                if (!File.Exists(assemblyPath))
                {
                    throw new FileNotFoundException(string.Format(STR_EX_AssmFileNotFound, interfaceType), assemblyPath);
                }
                else
                {
                    assembly = Assembly.LoadFrom(assemblyPath);
                }

                try
                {
                    if (assembly != null)
                    {
                        result = assembly.GetTypes()
                            .Where(x => x.Name == implementingType)
                            //.Where(x => interfaceType.IsAssignableFrom(x))
                            .Single();
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    StringBuilder sbException = new StringBuilder();
                    foreach (var item in ex.LoaderExceptions)
                    {
                        sbException.AppendLine(string.Concat(item.Message.ToString(), " "));
                    }
                    throw new Exception(String.Concat("Inversion reflection load error: ", sbException.ToString()));
                }

            }

            return result;
        }

        private static string GetKeyName(Type interfaceType)
        {
            return String.Format("{0}Implementer", interfaceType.Name);
        }

        private static bool TryGetMappingSettings(string keyName, out string implementingType, out string assemblyName, out string assemblyPath, out string gacAssemblyName)
        {
            bool result = false;
            implementingType = string.Empty;
            assemblyName = string.Empty;
            assemblyPath = string.Empty;
            gacAssemblyName = string.Empty;

            string keyNameImplType = keyName + "_ImplementingType";
            string keyNameAssmName = keyName + "_Assembly";
            string keyNameAssmPath = keyName + "_AssemblyPath";
            string keyNameFullAssmName = keyName + "_FullAssemblyName";

            try
            {
                implementingType = ConfigSettings.GetAppSettingsString(keyNameImplType);
                assemblyName = ConfigSettings.GetAppSettingsString(keyNameAssmName);
                // if the assemblyPath key doesn't exist, this returns null:
                assemblyPath = ConfigSettings.GetAppSettingsString(keyNameAssmPath);
                gacAssemblyName = ConfigSettings.GetAppSettingsString(keyNameFullAssmName);

                result = true;
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        #endregion

        #region public methods
        /// <summary>
        /// Returns <c>true</c> if the InversionContainer has a type mapping for the given interface type.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public static bool ContainsMapping<TInterface>()
        {
            bool result = false;

            string keyName = GetKeyName(typeof(TInterface));
            try
            {
                string value = ConfigSettings.GetAppSettingsString(keyName);
                bool temp;
                result = bool.TryParse(value, out temp) ? temp : false;
            }
            catch 
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Returns an object which implements the given <typeparamref name="TInterface"/> type. 
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public static TInterface GetImplementingObject<TInterface>()
        where TInterface : class // TInterface should be an interface
        {
            Type interfaceType = typeof(TInterface);
            Type handlerType = GetImplementingType(interfaceType);
            TInterface result = default(TInterface);

            if (handlerType != null)
            {

                // must implement TInterface & have a parameterless constructor...
                bool interfaceTypeIsAssignableFrom = interfaceType.IsAssignableFrom(handlerType);
                bool hasParameterlessConstructor = handlerType.GetConstructor(Type.EmptyTypes) != null;

                if (interfaceTypeIsAssignableFrom && !handlerType.IsAbstract &&
                !handlerType.IsInterface && hasParameterlessConstructor)
                {
                    result = Activator.CreateInstance(handlerType) as TInterface;
                }
                else
                {
                    string msg = string.Empty;
                    if (!interfaceTypeIsAssignableFrom)
                    {
                        msg = string.Format(STR_EX_DoesNotImplementType, handlerType, interfaceType);
                    }
                    else if (handlerType.IsAbstract)
                    {
                        msg = string.Format(STR_EX_HandlerIsAbstract, interfaceType, handlerType);
                    }
                    else if (handlerType.IsInterface)
                    {
                        msg = string.Format(STR_EX_HandlerIsInterface, interfaceType, handlerType);
                    }
                    else if (!hasParameterlessConstructor)
                    {
                        msg = string.Format(STR_EX_NoParameterlessConstructor, interfaceType, handlerType);
                    }

                    throw new InversionContainerException(msg, interfaceType, handlerType);
                }
            }
            else
            {
                throw new InversionContainerException(string.Format(STR_EX_NoTypeFound, interfaceType), interfaceType, null);
            }

            return result;
        }

        #endregion
    }
}
