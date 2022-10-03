using System;
using System.Collections.Generic;
using System.Linq;
using Unity;
using System.Collections.Specialized;
using System.Reflection;
using System.IO;
using System.Xml.Linq;
using System.Runtime.Serialization;
using Unity.Exceptions;

namespace MDSY.Framework.Buffer.Unity
{
    /// <summary>
    /// Extenion methods for IUnityContainer objects.
    /// </summary>
    public static class UnityContainerExtensions
    {
        #region constants
        private const string XName_AssembliesPath = "path";
        private const string XName_AssmPathAttribute = "value";
        private const string XName_FrameworkAssembliesDirectory = "frameworkAssembliesDirectory";
        #endregion

        #region private
        private static readonly string ResolutionNameNone = "(none)";
        private static readonly string xmlElementRegisterFrom = "registerFrom";
        private static readonly string xmlAttribEnabled = "enabled";
        private static readonly string xmlAttribFilename = "filename";
        private static readonly string xmlAttribThrowOnLoadError = "throwOnLoadError";

        private static void ThrowNullEmptyArgException(string paramName)
        {
            throw new ArgumentException(String.Format("{0} is null or empty.", paramName));
        }


        /// <summary>
        /// Returns a collection of assembly filenames which should contain IDefineTypeRegistration
        /// instances.
        /// </summary>
        private static IEnumerable<RegistrationAssemblyInfo> GetRegisteringAssembliesFrom(XElement xmlRoot)
        {
            return from node in xmlRoot.Descendants(xmlElementRegisterFrom)
                   select new RegistrationAssemblyInfo(node.Attribute(xmlAttribFilename).Value,
                       Convert.ToBoolean(node.Attribute(xmlAttribEnabled).Value),
                       Convert.ToBoolean((node.Attribute(xmlAttribThrowOnLoadError) != null) ?
                                      node.Attribute(xmlAttribThrowOnLoadError).Value :
                                      "true"));
        }
        #endregion


        #region public methods
        /// <summary>
        /// Loads bindings from the given collection of type registration modules.
        /// </summary>
        internal static IUnityContainer LoadRegistrationsFrom(this IUnityContainer instance, IEnumerable<ITypeRegistrationModule> modules)
        {
            if (instance == null)
                throw new ArgumentNullException("instance is null");
            if (modules == null)
                throw new ArgumentNullException("modules is null");

            foreach (ITypeRegistrationModule module in modules)
            {
                module.LoadRegistrationsInto(instance);
            }
            return instance;
        }

        /// <summary>
        /// Searches the given collection of Assembly objects and loads any type registration modules found within.
        /// </summary>
        internal static IUnityContainer LoadRegistrationsFrom(this IUnityContainer instance, IEnumerable<Assembly> assemblies)
        {
            if (instance == null)
                throw new ArgumentNullException("instance is null");
            if (assemblies == null)
                throw new ArgumentNullException("assemblies is null");

            foreach (Assembly assembly in assemblies)
            {
                IEnumerable<ITypeRegistrationModule> modules = assembly.GetInjectionModules();
                if ((modules != null) && (modules.Count() > 0))
                {
                    LoadRegistrationsFrom(instance, modules);
                }
            }
            return instance;
        }


        /// <summary>
        /// Examines the given assembly object and loads any type registration modules found within.
        /// </summary>
        internal static IUnityContainer LoadRegistrationsFrom(this IUnityContainer instance, string path, string assemblyFilename)
        {
            if (instance == null)
                throw new ArgumentNullException("instance is null");

            if (String.IsNullOrEmpty(path))
                ThrowNullEmptyArgException(XName_AssembliesPath);

            if (String.IsNullOrEmpty(assemblyFilename))
                ThrowNullEmptyArgException("assemblyFilename");

            string filename = Path.Combine(path, assemblyFilename);

            if (File.Exists(filename))
            {
                IList<Assembly> assemblies = new List<Assembly>();
                assemblies.Add(Assembly.LoadFile(filename));

                LoadRegistrationsFrom(instance, assemblies);
            }

            return instance;
        }



        private static string GetDefaultFrameworkDirectory(XDocument xmlFile)
        {
            string defaultDirectory = AppDomain.CurrentDomain.BaseDirectory;

            XElement assembliesDir = xmlFile.Root.Descendants(XName_FrameworkAssembliesDirectory).FirstOrDefault();
            if (assembliesDir != null)
            {
                XElement assmPath = assembliesDir.Element(XName_AssembliesPath);
                if (assmPath != null)
                {
                    XAttribute assmPathAttribute = assmPath.Attribute(XName_AssmPathAttribute);
                    if (assmPathAttribute != null)
                    {
                        // take into account relative paths
                        string path = Path.Combine(defaultDirectory, assmPathAttribute.Value);
                        if (Directory.Exists(path))
                        {
                            defaultDirectory = path;
                        }
                    }
                }
            }
            return defaultDirectory;
        }
        /// <summary>
        /// Loads the assemblies listed in the given xml file and loads any type registrations found there. 
        /// </summary>
        /// <remarks>
        /// Note, the xml schema for the given file is simple, but must match:
        /// <code>
        /// &lt;xs:schema attributeFormDefault="unqualified" elementFormDefault="qualified" xmlns:xs="http://www.w3.org/2001/XMLSchema"&gt;
        ///  &lt;xs:element name="ImplementationAssemblies"&gt;
        ///    &lt;xs:complexType&gt;
        ///      &lt;xs:sequence&gt;
        ///        &lt;xs:element maxOccurs="unbounded" name="registerFrom"&gt;
        ///          &lt;xs:complexType&gt;
        ///            &lt;xs:attribute name="filename" type="xs:string" use="required" /&gt;
        ///            &lt;xs:attribute name="enabled" type="xs:boolean" use="required" /&gt;
        ///            &lt;xs:attribute name="throwOnLoadError" type="xs:boolean" use="optional" /&gt;
        ///          &lt;/xs:complexType&gt;
        ///        &lt;/xs:element&gt;
        ///      &lt;/xs:sequence&gt;
        ///    &lt;/xs:complexType&gt;
        ///  &lt;/xs:element&gt;
        /// &lt;/xs:schema&gt;
        /// </code>
        /// <note>The <c>throwOnLoadError</c> behavior is not yet fully implemented.</note>
        /// </remarks>
        public static IUnityContainer LoadRegistrationsFrom(this IUnityContainer instance, XDocument xmlFile)
        {
            IEnumerable<RegistrationAssemblyInfo> regAssemblies = GetRegisteringAssembliesFrom(xmlFile.Root);
            string defaultDirectory = GetDefaultFrameworkDirectory(xmlFile);

            foreach (RegistrationAssemblyInfo asmInfo in regAssemblies)
            {
                if (asmInfo.Enabled)
                {
                    //string path = string.IsNullOrWhiteSpace(Path.GetDirectoryName(asmInfo.Filename)) ?
                    //    defaultDirectory :
                    //    Path.GetDirectoryName(asmInfo.Filename);

                    string fullPath = Path.Combine(defaultDirectory, asmInfo.Filename);
                    if (File.Exists(fullPath))
                    {
                        instance.LoadRegistrationsFrom(defaultDirectory, asmInfo.Filename);
                    }
                    else if (asmInfo.Throw)
                    {
                        throw new InvalidTypeRegistrationException(
                            string.Format("Attempted to load registrations from xml content, specified assembly '{0}' was not found."
                               , fullPath), fullPath);
                    }
                }
            }

            return instance;
        }


        /// <summary>
        /// Performs a guarded DI type resolution and returns the new object of type <typeparamref name="T"/>. 
        /// </summary>
        /// <remarks>
        /// If a matching resolution type for the given <typeparamref name="T"/>, has not been mapped, 
        /// a <c>ResolutionFailedException</c> will be thrown by Unity and will be caught here. 
        /// Currently, only a console message is dispatched. This is enough for debugging but needs 
        /// to be more robust for production code.
        /// </remarks>
        /// <typeparam name="T">The type for which to resolve a new object.</typeparam>
        /// <returns>The newly resolved object.</returns>
        /// <param name="mappingName = """></param>
        public static T ResolveType<T>(this IUnityContainer instance, string mappingName = "")
        {
            T result = default(T);

            try
            {
                result = String.IsNullOrEmpty(mappingName) ?
                             instance.Resolve<T>() :
                             instance.Resolve<T>(mappingName);
            }
            catch (ResolutionFailedException ex)
            {
                if (ex.Message.Contains("is an interface and cannot be constructed."))
                {
                    string nameRequested = ex.NameRequested ?? ResolutionNameNone;
                    throw new InvalidTypeRegistrationException(
                        String.Format("No type mapping for type: {0}, name: {1}.", ex.TypeRequested, nameRequested), ex);
                }
                else
                    throw;
            }

            return result;
        }
        #endregion

    }
}
