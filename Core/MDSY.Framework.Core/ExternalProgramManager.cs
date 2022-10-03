using MDSY.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// A static manager which provides an access point to external ILegacyProgams.
    /// </summary>
    /// <remarks>
    /// <para>Retreives ILegacyProgram descendants from other assemblies and can optionally keep ILegacyProgram
    /// implementing objects "resident" for repeated access.</para>
    /// <para>If a bit of converted code in one assembly needs to call a routine or access a programmatic 
    /// object in another converted code assembly, the calling code should access ExternalProgramManager.
    /// Call GetProgram() for an istance of the sought after ILegacyProgram descendant.</para>
    /// </remarks>
    public static class ExternalProgramManager
    {
        #region private fields
        private readonly static IDictionary<string, ILegacyProgram> _residentPrograms = new Dictionary<string, ILegacyProgram>();
        #endregion

        #region private methods

        private static string GetAssemblyShortName(Assembly assembly)
        {
            return assembly.GetName(false).Name;
        }

        /// <summary>
        /// Returns <c>true</c> if an ILegacyProgram with the specified <paramref name="key"/>
        /// already exists in the internal program list.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool ProgramIsResident(string key)
        {
            return _residentPrograms.ContainsKey(key);
        }

        /// <summary>
        /// Adds the given program to the manager's internal program list if it does not already exist there. 
        /// </summary>
        /// <param name="assemblyName">Name of the assembly which contains the program.</param>
        /// <param name="programName">Class name of the program.</param>
        /// <param name="program">The ILegacyProgram instance to add.</param>
        private static void AddProgramToResidentList(string assemblyName, string programName, ILegacyProgram program)
        {
            string key = ConstructKey(assemblyName, programName);
            if (!ProgramIsResident(key))
            {
                _residentPrograms.Add(key, program);
            }
        }

        /// <summary>
        /// Generates the key value for the manager's internal dictionary of "resident" programs.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly which contains the program.</param>
        /// <param name="programName">Class name of the program.</param>
        /// <returns>The generated key value.</returns>
        private static string ConstructKey(string assemblyName, string programName)
        {
            return string.Format("{0}_{1}", assemblyName, programName);
        }

        /// <summary>
        /// Creates an instance of the specified ILegacyProgram type from the given <paramref name="assemlbyName"/>.
        /// </summary>
        /// <typeparam name="T">The type of ILegacyProgram to retrieve.</typeparam>
        /// <param name="assemblyName">Name of the assembly which contains the program.</param>
        /// <param name="programName">Class name of the program.</param>
        /// <returns>The new object instance or <c>null</c></returns>
        private static T LoadExternalProgramFrom<T>(Assembly assembly, string programName)
            where T: ILegacyProgram
        {
            return assembly.ContainsProgram<T>(programName) ?
                assembly.GetProgram<T>(programName) :
                default(T);
        }

        /// <summary>
        /// Returns an instance of the specified ILegacyProgram, searching first within 
        /// the manager's internal list of "resident" programs, or, if not found, retrieving 
        /// a new instance of the object. 
        /// </summary>
        /// <typeparam name="T">The type of the ILegacyProgram to retrieve.</typeparam>
        /// <param name="assemblyName">Name of the assembly which contains the program.</param>
        /// <param name="programName">Class name of the program.</param>
        /// <returns>The program object instnace, if found.</returns>
        private static T GetExternalProgram<T>(Assembly assembly, string programName)
            where T: ILegacyProgram
        {
            T result = default(T);

            string assemblyName = GetAssemblyShortName(assembly);
            string key = ConstructKey(assemblyName, programName);

            result = ProgramIsResident(key) ?
                (T)_residentPrograms[key] :
                LoadExternalProgramFrom<T>(assembly, programName);

            return result;
        }

        private static void RemoveProgram(string assemblyName, string programName)
        {
            _residentPrograms.Remove(ConstructKey(assemblyName, programName));
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets a count of the ILegacyProgram objects in the manager's internal "resident" program list.
        /// </summary>
        public static int ResidentProgramCount
        {
            get { return _residentPrograms.Count; }
        }
        #endregion

        #region public methods

        /// <summary>
        /// Returns the ILegacyProgram with the specified <paramref name="programName"/>
        /// from the given <paramref name="assembly"/> without loading the program into
        /// the "resident" list.
        /// </summary>
        /// <remarks>Note that if a program with the given <paramref name="programName"/>
        /// already exists in the manager's list, that existing instance will be returned.</remarks>
        /// <typeparam name="T">The ILegacyProgram descendant type to return.</typeparam>
        /// <param name="assembly">The Assembly which contains the program.</param>
        /// <param name="programName">Class name of the program.</param>
        /// <returns>An object of the given type <typeparamref name="T"/></returns>
        public static T GetProgram<T>(Assembly assembly, string programName)
            where T : ILegacyProgram
        {
            return GetExternalProgram<T>(assembly, programName);
        }

        /// <summary>
        /// Returns the ILegacyProgram with the specified <paramref name="programName"/>
        /// from the given <paramref name="assembly"/> and loads the program into
        /// the "resident" list.
        /// </summary>
        /// <remarks>Note that if a program with the given <paramref name="programName"/>
        /// already exists in the manager's list, that existing instance will be returned.</remarks>
        /// <typeparam name="T">The ILegacyProgram descendant type to return.</typeparam>
        /// <param name="assembly">The Assembly which contains the program.</param>
        /// <param name="programName">Class name of the program.</param>
        /// <returns>An object of the given type <typeparamref name="T"/></returns>
        public static T GetProgramAndKeepResident<T>(Assembly assembly, string programName)
            where T: ILegacyProgram
        {
            string assemblyName = GetAssemblyShortName(assembly);

            T result = GetExternalProgram<T>(assembly, programName);
            AddProgramToResidentList(assemblyName, programName, result);
            return result;
        }

        /// <summary>
        /// Loads the ILegacyProgram with the specified <paramref name="programName"/>
        /// from the given <paramref name="assembly"/> into the manager's internal 
        /// "resident" program list.
        /// </summary>
        /// <param name="assembly">The Assembly which contains the program.</param>
        /// <param name="programName">Class name of the program.</param>
        public static void LoadProgram(Assembly assembly, string programName)
        {
            string assemblyName = GetAssemblyShortName(assembly);
            if (!ProgramIsResident(ConstructKey(assemblyName, programName)))
            {
            	GetExternalProgram<ILegacyProgram>(assembly, programName);
            }
        }

        public static void LoadPrograms(Assembly assembly, params string[] programNames)
        {
            Array.ForEach(programNames, name => LoadProgram(assembly, name));
        }

        /// <summary>
        /// Causes the given ILegacyProgram to be removed from the manager's internal list of
        /// ILegacyPrograms kept "resident". 
        /// </summary>
        /// <param name="program">The ILegacyProgram to remove.</param>
        public static void UnloadProgram(ILegacyProgram program)
        {
            RemoveProgram(program.AssemblyName, program.Name);
        }

        /// <summary>
        /// Causes the specified program to be removed from the manager's internal list of
        /// ILegacyPrograms kept "resident". 
        /// </summary>
        /// <param name="assemblyName">Name of the assembly which contains the program.</param>
        /// <param name="programName">Class name of the program.</param>
        public static void UnloadProgram(Assembly assembly, string programName)
        {
            string assemblyName = GetAssemblyShortName(assembly);
            RemoveProgram(assemblyName, programName);
        }

        /// <summary>
        /// Clears the manager's internal list of ILegacyPrograms kept "resident". 
        /// </summary>
        public static void ClearResidentPrograms()
        {
            _residentPrograms.Clear();
        }
        #endregion
    }
}
