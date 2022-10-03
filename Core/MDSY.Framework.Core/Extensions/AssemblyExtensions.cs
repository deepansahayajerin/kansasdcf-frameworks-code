using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MDSY.Framework.Interfaces;

namespace MDSY.Framework.Core
{
    public static class AssemblyExtensions
    {
        #region private

        private static IEnumerable<Type> GetImplementingProgramTypes<T>(this Assembly instance)
                       where T : ILegacyProgram
        {
            return instance.GetExportedTypes().Where(TypeImplementsProgram<T>);
        }

        /// <summary>
        /// Returns <c>true</c> if the given type is an ILegacyProgram implementor (of <typeparamref name="T"/>)
        /// which can be created via a parameterless constructor.
        /// </summary>
        private static bool TypeImplementsProgram<T>(Type type)
            where T : ILegacyProgram
        {
            return typeof(T).IsAssignableFrom(type)
                && !type.IsAbstract
                && !type.IsInterface
                && type.GetConstructor(Type.EmptyTypes) != null;
        }


        #endregion

        #region public methods

        /// <summary>
        /// Returns <c>true</c> if the assembly contains an object which implements 
        /// the given <c>ILegacyProgram</c>-descendant interface (<typeparamref name="T"/>), 
        /// and which has the given <paramref name="programName"/>.
        /// </summary>
        /// <param name="programName">The name of the routine for which to search.</param>
        /// <returns><c>true</c> if a callable routine with the specified name is found.</returns>
        public static bool ContainsProgram<T>(this Assembly instance, string programName)
            where T : ILegacyProgram
        {
            return instance.GetProgramNames<T>().Contains(programName);
        }

        /// <summary>
        /// Returns the single instance of <typeparamref name="T"/> that has the given 
        /// <paramref name="programName"/>, if it exists. Otherwise, returns <c>null</c>.
        /// </summary>
        public static T GetProgram<T>(this Assembly instance, string programName)
            where T : ILegacyProgram
        {
            return instance
                .GetImplementingProgramTypes<T>()
                .Where(type => String.Compare(type.Name, programName, true) == 0)
                .Select(type => (T)Activator.CreateInstance(type))
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns a list of names of all the types within the assembly which implement 
        /// ILegacyProgram.
        /// </summary>
        /// <typeparam name="T">The ILegacyProgram or descendant type for which to search.</typeparam>
        /// <returns>A list of type names.</returns>
        public static IEnumerable<string> GetProgramNames<T>(this Assembly instance)
            where T : ILegacyProgram
        {
            return instance
                .GetImplementingProgramTypes<T>()
                .Select(type => type.Name);
        }

        /// <summary>
        /// Returns a collection of instances of all the types within the assembly 
        /// that implement <typeparamref name="T"/>.
        /// </summary>
        public static IEnumerable<T> GetProgram<T>(this Assembly instance)
            where T : ILegacyProgram
        {
            return instance
                .GetImplementingProgramTypes<T>()
                .Select(type => (T)Activator.CreateInstance(type));
        }

        /// <summary>
        /// Returns a collection of instances of all the types within the assembly 
        /// that implement <typeparamref name="T"/> and that are of the specified
        /// <paramref name="sourceType"/>.
        /// </summary>
        public static IEnumerable<T> GetPrograms<T>(this Assembly instance, string sourceType)
            where T : ILegacyProgram
        {
            return instance
                .GetImplementingProgramTypes<T>()
                .Select(type => (T)Activator.CreateInstance(type))
                .Where(pgm => String.Compare(pgm.SourceType, sourceType) == 0);
        }



        /// <summary>
        /// Returns <c>true</c> if the Assembly contains any objects implementing the given 
        /// <c>ILegacyProgram</c>-descendant interface types.
        /// </summary>
        /// <typeparam name="T">The ILegacyProgram-descendant for which to search.</typeparam>
        /// <returns><c>true</c>, if the assembly contains one, or more, 
        /// implementations of <typeparamref name="T"/></returns>
        public static bool HasLegacyPrograms<T>(this Assembly instance)
            where T : ILegacyProgram
        {
            return (instance.GetImplementingProgramTypes<T>().Count() > 0);
        }
        #endregion

    }

}
