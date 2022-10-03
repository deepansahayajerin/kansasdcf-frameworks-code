using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Collections.Generic;
using MDSY.Framework.Buffer.Interfaces;
using Unity;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Buffer.Services
{
    /// <summary>
    /// Primary interface point for MDSY.Framework.Buffer assemblies. Clients should interact with BufferServices and 
    /// its available services. 
    /// </summary>
    public class BufferServices
    {
        #region static constructor
        /// <summary>
        /// Static constructor for the Services class.
        /// </summary>
        static BufferServices()
        {
            ContainerIsInitialized = false;
        }

        #endregion

        #region Unity container
        private static IUnityContainer unityContainer;

        private static void InitializeDIContainer(string typeRegXmlFilename)
        {
            // note: if typeRegXmlFilename is empty, reset the Singleton without loading any registrations.

            UnitySingleton.ResetContainer();
            if (!string.IsNullOrEmpty(typeRegXmlFilename))
            {
                UnitySingleton.LoadRegistrationsFromXml(typeRegXmlFilename);
            }
            unityContainer = UnitySingleton.Container;

            ContainerIsInitialized = (unityContainer != null);
        }

        /// <summary>
        /// Gets the root DI container singleton for Buffer services. 
        /// </summary>
        /// <remarks>
        /// <warning>
        /// Do not surface this property as public. Converted code should not have access to the Framework's 
        /// DI infrastructure. 
        /// </warning>
        /// </remarks>
        private static IUnityContainer UnityDIContainer
        {
            get
            {
                if (unityContainer == null)
                {
                    string typeRegXml = ConfigSettings.GetAppSettingsString("FrameworkTypeRegMaps");
                    
                    if (String.IsNullOrEmpty(typeRegXml))
                        typeRegXml = "InjectionTypeReg.xml";

                    InitializeDIContainer(typeRegXml);
                    if (unityContainer == null)
                    {
                        throw new Exception("Attempted to access UnityDIContainer without initialization. Call Services.InitializeInversionContainer() before accessing any services.");
                    }
                }

                return unityContainer;
            }
        }


        #endregion

        #region private methods
        private static T GetService<T>()
        {
            return UnityDIContainer.ResolveType<T>();
        }
        #endregion

        #region public methods
        /// <summary>
        /// Causes the internal dependency injection container to load type registration mappings as 
        /// specified by the given XML file. 
        /// </summary>
        /// <param name="filename">The TypeReg.xml file from which to load type mappings.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IUnityContainer InitializeInversionContainer(string filename)
        {
            InitializeDIContainer(filename);
            return UnitySingleton.Container;
        }
        #endregion

        #region private fields
        [ThreadStatic]
        private static IRecordCollectionService _records;
        [ThreadStatic]
        private static IBufferAddressCollectionService _bufferAddresses;
        [ThreadStatic]
        private static IRecordBufferCollectionService _initialRecordBuffers;
        [ThreadStatic]
        private static IDirectiveServices _directives;
        private static Dictionary<string, IRecord> _recordDefinitions = null;
        #endregion

        #region public properties

        /// <summary>
        /// Returns <c>true</c> if the InitializeInversionContainer() method has been called, thus initializing 
        /// the internal IoC container. 
        /// </summary>
        /// <remarks>
        /// If ContainerIsInitialized returns <c>false</c>, then the system's Dependency Injection architecture 
        /// is in an un-initialized state and cannot be used. 
        /// </remarks>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool ContainerIsInitialized { get; set; }

        #endregion

        #region Services
        /// <summary>
        /// Gets an IArrayUtilitiesService object which can be used to manipulate arrays and array elements.
        /// </summary>
        public static IArrayUtilitiesService ArrayUtilities
        {
            get { return GetService<IArrayUtilitiesService>(); }
        }

        /// <summary>
        /// Gets an IFactoryService object which can be used to create new object instances. 
        /// </summary>
        public static IFactoryService Factory
        {
            get { return GetService<IFactoryService>(); }
        }

        /// <summary>
        /// Gets an ILanguageService object which can be used for legacy language emulation.
        /// </summary>
        public static ILanguageService Languages
        {
            get { return GetService<ILanguageService>(); }
        }

        /// <summary>
        /// Gets an ILoggingService object which can be used for logging.
        /// </summary>
        public static ILoggingService Logging
        {
            get { return GetService<ILoggingService>(); }
        }

        /// <summary>
        /// Gets an IRecordCollectionService object which contains a collection of
        /// record objects currently in the system.
        /// </summary>
        public static IRecordCollectionService Records
        {
            get
            {
                if (_records == null)
                {
                    _records = GetService<IRecordCollectionService>();
                }
                return _records;
            }
        }

        /// <summary>
        /// Gets an IBufferAddressCollectionService object which contains a collection of
        /// IBufferAddress objects.
        /// </summary>
        public static IBufferAddressCollectionService BufferAddresses
        {
            get
            {
                if (_bufferAddresses == null)
                {
                    _bufferAddresses = GetService<IBufferAddressCollectionService>();
                }
                return _bufferAddresses;
            }
        }

        /// <summary>
        /// Gets an IRecordBufferCollectionService object which contains a 
        /// collection of references to IDataBuffers keyed by original owning record.
        /// </summary>
        public static IRecordBufferCollectionService InitialRecordBuffers
        {
            get
            {
                if (_initialRecordBuffers == null)
                {
                    _initialRecordBuffers = GetService<IRecordBufferCollectionService>();
                }
                return _initialRecordBuffers;
            }
        }

        /// <summary>
        /// Gets an IIndexBaseServices object which contains methods for 
        /// dealing with zero-based or one-based indexes.
        /// </summary>
        public static IIndexBaseServices Indexes
        {
            get { return GetService<IIndexBaseServices>(); }
        }

        /// <summary>
        /// Gets an IDirectiveServices object which supports system directive settings.
        /// </summary>
        public static IDirectiveServices Directives
        {
            get
            {
                if (_directives == null)
                {
                    _directives = GetService<IDirectiveServices>();
                }
                return _directives;
            }
        }

        /// <summary>
        /// Returns a reference to the collection of the application records definitions.
        /// </summary>
        public static Dictionary<string, IRecord> RecordDefinitions
        {
            get
            {
                if (_recordDefinitions == null)
                {
                    _recordDefinitions = new Dictionary<string, IRecord>();
                }
                return _recordDefinitions;
            }
        }


        #endregion


    }
}
