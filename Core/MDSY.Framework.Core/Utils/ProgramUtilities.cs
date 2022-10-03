using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Provides program utilities.
    /// </summary>
    public static class ProgramUtilities
    {
        #region static properties
        /// <summary>
        /// Gets or sets the assembly name to be used in the reflection methods found on this class.
        /// </summary>
        public static string TypeAssemblyName { get; set; }
        /// <summary>
        /// Gets or sets the Common assembly name to be used in the reflection methods found on this class.
        /// </summary>
        public static string CommonTypeAssemblyName { get; set; }
        /// <summary>
        /// Gets or sets the name space to be used in the reflection methods found on this class.
        /// </summary>
        public static string TypeNameSpace { get; set; }
        /// <summary>
        /// Gets or sets the Common name space to be used in the reflection methods found on this class.
        /// </summary>
        public static string CommonTypeNameSpace { get; set; }
        /// <summary>
        /// Gets or sets the DAL assembly name to be used in the reflection methods found on this class.
        /// </summary>
        public static string DalTypeAssemblyName { get; set; }
        /// <summary>
        /// Gets or sets the DAL name space to be used in the reflection methods found on this class.
        /// </summary>
        public static string DalTypeNameSpace { get; set; }


        #endregion

        #region internal static methods
        /// <summary>
        /// Returns a reference to a named loaded assembly from the current application domain (running app).
        /// </summary>
        /// <param name="assemblyShortName">Assembly name</param>
        /// <returns>assembly name</returns>
        public static Assembly FindAssembly(string assemblyShortName)
        {
            Assembly localAssembly = null;
            try
            {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.GetName(false).Name == assemblyShortName)
                {
                    return assembly;
                }
            }
            // If not in application domain, try to load from base directory (For batch operations)
                localAssembly = Assembly.Load(assemblyShortName);
            }
            catch
            {
                //Return null
            }

            return localAssembly;
        }
        #endregion

        #region private static methods

        /// <summary>
        /// Launches a generic method
        /// </summary>
        /// <param name="targetObject">The object where the method is invoked from</param>
        /// <param name="targetType">The object type</param>
        /// <param name="methodName">The method name</param>
        /// <param name="argumentArray">an array of parameters list</param>
        private static void InvokeGenericMethod(object targetObject, Type targetType, string methodName, params object[] argumentArray)
        {
            /// build the parameter list to find the method
            List<Type> setDataParameterList = new List<Type>();

            if (argumentArray != null)
            {
                foreach (object argument in argumentArray)
                {
                    setDataParameterList.Add(argument.GetType());
                }
            }

            // Get a Method ref using the method name
            MethodInfo targetMethod = targetType.GetMethod(methodName, setDataParameterList.ToArray());
            if (targetMethod == null)
            {
                throw new ApplicationException(String.Format("InvokeInput: Unable to get reference to method [{0}] in class [{1}], assembly [{2}])", methodName, targetType.Name, targetType.Assembly.FullName));
            }

            try
            {
                System.Diagnostics.Debug.WriteLine(String.Format("About to invoke [{0}][{1}]", targetType.Name, targetMethod.Name));

                // Invoke the method with the parameters sent in?
                targetMethod.Invoke(targetObject, argumentArray);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                while (ex != null)
                {
                    sb.AppendLine(ex.ToString());
                    //sb.Append("\r\n");
                    ex = ex.InnerException;
                }

                string errorMessageString = sb.ToString();
                //   SimpleLogging.LogMandatoryMessageToFile(errorMessageString;

                throw new ApplicationException(String.Format("Exception for [{0}][{1}]\r\n{2}", targetType.Name, targetMethod.Name, errorMessageString), ex);
            }
        }

        #endregion

        #region public static methods

        #region invoke

        /// <summary>
        /// Returns this object type
        /// </summary>
        /// <param name="fieldName">The field name</param>
        /// <returns>Object type</returns>
        public static Type GetBLType(IField fieldName)
        {
            return (GetBLType(fieldName.GetValue<string>()));
        }

        /// <summary>
        /// This Object type based assembly name found in config UISettings
        /// </summary>
        /// <param name="typeName">The type name</param>
        /// <returns>the assemblyname and type name</returns>
        public static Type GetBLType(string typeName)
        {
            Type type = null;

            Assembly targetAsm;
            if (TypeAssemblyName == null)
            {
                TypeAssemblyName = ConfigSettings.GetAppSettingsStringFromSection("Namespaces", "BLAssemblyName");
            }
            if (TypeNameSpace == null)
            {
                TypeNameSpace = ConfigSettings.GetAppSettingsStringFromSection("Namespaces", "BLNamespace");
            }

            // Go get a reference to the assembly
            targetAsm = FindAssembly(TypeAssemblyName);
            if (targetAsm != null)
            {
            // Get the type reference using a name of "NameSpace.TypeName"
            type = targetAsm.GetType(String.Format("{0}.{1}", TypeNameSpace, typeName));
            }
            if (type == null)
            {
                // Check for Common namepspace
                if (CommonTypeAssemblyName == null)
                    CommonTypeAssemblyName = ConfigSettings.GetAppSettingsStringFromSection("Namespaces", "CommonAssemblyName");
                targetAsm = FindAssembly(CommonTypeAssemblyName);

                if (CommonTypeNameSpace == null)
                    CommonTypeNameSpace = ConfigSettings.GetAppSettingsStringFromSection("Namespaces", "CommonNamespace");

                if (targetAsm != null)
                {
                    type = targetAsm.GetType(String.Format("{0}.{1}", CommonTypeNameSpace, typeName));
                }
                if (targetAsm == null || type == null)
                    throw new ApplicationException(String.Format("InvokeHasMainMethod: Type class [{0}] not found in assembly [{1}]", typeName, TypeAssemblyName));

            }
            Console.WriteLine(String.Format("** Invoking Program {0} at {1} - From Assembly '{2}' ",
            type.Name,
            DateTime.Now,
            targetAsm.FullName));

            return type;
        }

        /// <summary>
        /// Get Object type based on passed in Assembly name
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="nameSpace">namespace</param>
        /// <param name="typeName">type name</param>
        /// <returns></returns>
        public static Type GetType(string assemblyName, string nameSpace, string typeName)
        {
            Type type = null;
            Assembly targetAsm;

            string typeAssemblyName = assemblyName;
            string typeNameSpace = nameSpace;


            // Go get a reference to the assembly
            targetAsm = FindAssembly(typeAssemblyName);
            if (targetAsm == null)
            {
                throw new ApplicationException(String.Format("InvokeHasMainMethod: Assembly [{0}] not found", typeAssemblyName));
            }

            // Get the type reference using a name of "NameSpace.TypeName"
            type = targetAsm.GetType(String.Format("{0}.{1}", typeNameSpace, typeName));
            if (type == null)
            {
                throw new ApplicationException(String.Format("InvokeHasMainMethod: Type class [{0}] not found in assembly [{1}]", typeName, typeAssemblyName));
            }

            return type;
        }

        /// <summary>
        /// This Object gets the DAL type based assmebly name found in config UISettings
        /// </summary>
        /// <param name="typeName">Type name</param>
        /// <returns>DAL type based assembly name</returns>
        public static Type GetDALType(string typeName)
        {
            Type type = null;

            Assembly targetAsm;
            if (DalTypeAssemblyName == null)
            {
                DalTypeAssemblyName = ConfigSettings.GetAppSettingsStringFromSection("Namespaces", "DALAssemblyName");
            }
            if (DalTypeNameSpace == null)
            {
                DalTypeNameSpace = ConfigSettings.GetAppSettingsStringFromSection("Namespaces", "DALNamespace");
            }

            // Go get a reference to the assembly
            targetAsm = FindAssembly(DalTypeAssemblyName);
            if (targetAsm == null)
            {
                throw new ApplicationException(String.Format("InvokeHasMainMethod: Assembly [{0}] not found", DalTypeAssemblyName));
            }

            // Get the type reference using a name of "NameSpace.TypeName"
            type = targetAsm.GetType(String.Format("{0}.{1}", DalTypeNameSpace, typeName));
            if (type == null)
            {
                // Check for Common namepspace

                targetAsm = FindAssembly(ConfigSettings.GetAppSettingsStringFromSection("Namespaces", "CommonAssemblyName"));
                DalTypeNameSpace = ConfigSettings.GetAppSettingsStringFromSection("Namespaces", "CommonNamespace");
                if (targetAsm != null)
                {
                    type = targetAsm.GetType(String.Format("{0}.{1}", DalTypeNameSpace, typeName));
                }
                if (targetAsm == null)
                    throw new ApplicationException(String.Format("InvokeHasMainMethod: Type class [{0}] not found in assembly [{1}]", typeName, DalTypeAssemblyName));

            }
            Console.WriteLine(String.Format("** Invoking Program {0} at {1} - From Assembly '{2}' ",
            type.Name,
            DateTime.Now,
            targetAsm.FullName));

            return type;
        }

        public static Decimal GetRandomNumber()
        {
            Random rnd = new Random();
            return (decimal)rnd.NextDouble();
        }

        public static Decimal GetRandomNumber(int seed)
        {
            Random rnd = new Random(seed);
            return (decimal)rnd.NextDouble();

        }

        public static DateTime GetHighestValue(DateTime date1, DateTime date2)
        {
            if (date1 < date2)
                return date2;
            else
                return date1;
        }

        public static decimal GetHighestValue(decimal dec1, decimal dec2)
        {
            if (dec1 < dec2)
                return dec2;
            else
                return dec1;
        }

        #endregion

        #endregion
    }
}
