using System;
using System.Collections.Generic;
using System.Linq;
using Unity;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Linq;

namespace MDSY.Framework.Buffer.Unity
{
    /// <summary>
    /// Utility singleton class for maintaining a single Unity Container reference
    /// without storing a reference in each Resolved object.
    /// </summary>
    public sealed class UnitySingleton
    {
        #region private fields
        private static volatile IUnityContainer instance;
        private readonly static object syncRoot = new Object();
        #endregion

        #region private constructor
        private UnitySingleton() { }
        #endregion

        #region private methods
        private static void LoadTypeRegistrationsFromXml(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new Exception(string.Format("Specified type registration mappings file, {0}, does not exist in folder {1}.", filename,
                    System.Environment.CurrentDirectory));
            }

            XDocument xDoc = XDocument.Load(filename);
            Container.LoadRegistrationsFrom(xDoc);
        }
        #endregion


        #region public properties
        /// <summary> 
        /// Gets the global root injection container.
        /// </summary>
        public static IUnityContainer Container
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new UnityContainer();
                        }
                    }
                }

                return instance;
            }
        }
        #endregion

        #region public methods

        /// <summary>
        /// Clears and disposes of the current singleton IUnityContainer instance, forcing a new instance
        /// upon next reference.
        /// </summary>
        public static void ResetContainer()
        {
            if (instance != null)
            {
                instance.Dispose();
            }
            instance = null;
        }

        /// <summary>
        /// Loads type registrations from any assemblies listed in <paramref name="filename"/> which 
        /// also contain one or more ITypeRegistrationModule instances.
        /// </summary>
        /// <param name="filename"></param>
        public static void LoadRegistrationsFromXml(string filename)
        {
            LoadTypeRegistrationsFromXml(filename);
        }
        #endregion

    }

}

