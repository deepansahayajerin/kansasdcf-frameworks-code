using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MDSY.Framework.Service.Interfaces;
using System.Reflection;
using System.ServiceModel;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.BaseClasses;
using System.Data.Common;
using MDSY.Framework.Core;

namespace MDSY.Framework.Control.CICS
{
    public static class ServiceControl
    {
        #region static properties

        [ThreadStatic]
        private static ProcessState _processState;

        [ThreadStatic]
        private static EventWaitHandle _internalThreadHolder;

        [ThreadStatic]
        private static EventWaitHandle _externalThreadHolder;

        [ThreadStatic]
        private static List<IAterasServiceItem> _inServiceThreadShareData;

        [ThreadStatic]
        private static bool _isUpperCase;

        [ThreadStatic]
        private static string _transferProgram;

        [ThreadStatic]
        private static string _currentProgram;

        [ThreadStatic]
        private static string _currentKeyPressed;

        [ThreadStatic]
        private static string _userID;

        [ThreadStatic]
        private static string _oPID;

        [ThreadStatic]
        private static string _termID;

        [ThreadStatic]
        private static string _applID;

        [ThreadStatic]
        private static string _sessionID;

        [ThreadStatic]
        private static string _t_classA;

        [ThreadStatic]
        private static string _t_classB;

        [ThreadStatic]
        private static string _t_cfacu;

        [ThreadStatic]
        private static string _t_cwhse;

        [ThreadStatic]
        private static byte[] _t_pcipl;

        [ThreadStatic]
        private static byte[] _t_pcilp;

        [ThreadStatic]
        private static OperationContext _operationContext;

        [ThreadStatic]
        private static Exception _currentException;

        [ThreadStatic]
        private static bool _isDisplayCommand;

        [ThreadStatic]
        private static int _clientSessionNumber;

        [ThreadStatic]
        private static CustomServiceData _customData;

        [ThreadStatic]
        private static PredefinedRecordBase _TWARecord;

        private static string _serviceName;

        #endregion

        #region Public static Properties

        public static ProcessState ProcessState
        {
            get
            {
                if (_processState == null)
                    _processState = new ProcessState();
                return _processState;
            }
            set { _processState = value; }
        }


        public static List<IAterasServiceItem> ServiceThreadShareData
        {
            get { return _inServiceThreadShareData; }
            set { _inServiceThreadShareData = value; }
        }

        /// <summary>
        /// Gets or sets the thread for the internal system holder
        /// </summary>
        public static EventWaitHandle InternalThreadHolder
        {
            get { return _internalThreadHolder; }
            set { _internalThreadHolder = value; }
        }

        /// <summary>
        /// Gets or sets the thread for the external Service system holder
        /// </summary>
        public static EventWaitHandle ExternalThreadHolder
        {
            get { return _externalThreadHolder; }
            set { _externalThreadHolder = value; }
        }

        public static PredefinedRecordBase TWARecord
        {
            get { return _TWARecord; }
            set { _TWARecord = value; }
        }

        public static string TransferProgram
        {
            get { return _transferProgram; }
            set { _transferProgram = value; }
        }

        public static bool IsUpperCase
        {
            get { return _isUpperCase; }
            set { _isUpperCase = value; }
        }

        public static string CurrentProgram
        {
            get { return _currentProgram; }
            set { _currentProgram = value; }
        }

        public static string CurrentKeyPressed
        {
            get { return _currentKeyPressed; }
            set { _currentKeyPressed = value; }
        }

        public static bool IsDisplayCommand
        {
            get { return _isDisplayCommand; }
            set { _isDisplayCommand = value; }
        }

        public static string UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }

        public static string OPID
        {
            get { return _oPID; }
            set { _oPID = value; }
        }

         public static string TERMID
        {
            get { return _termID; }
            set { _termID = value; }
        }

         public static string APPLID
         {
             get { return _applID; }
             set { _applID = value; }
         }

        public static string SessionID
        {
            get { return _sessionID; }
            set { _sessionID = value; }
        }

        public static string T_CLASSA
        {
            get { return _t_classA; }
            set { _t_classA = value; }
        }

        public static string T_CLASSB
        {
            get { return _t_classB; }
            set { _t_classB = value; }
        }
        public static string T_CFACU
        {
            get { return _t_cfacu; }
            set { _t_cfacu = value; }
        }

        public static string T_CWHSE
        {
            get { return _t_cwhse; }
            set { _t_cwhse = value; }
        }

        public static byte[] T_PCIPL
        {
            get { return _t_pcipl; }
            set { _t_pcipl = value; }
        }

        public static byte[] T_PCILP
        {
            get { return _t_pcilp; }
            set { _t_pcilp = value; }
        }
        public static string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }

        public static OperationContext CurrentContext
        {
            get { return _operationContext; }
            set { _operationContext = value; }
        }

        public static int GlobalSessionNumber { get; set; }

        public static int ClientSessionNumber
        {
            get { return _clientSessionNumber; }
            set { _clientSessionNumber = value; }
        }

        public static Exception CurrentException
        {
            get { return _currentException; }
            set { _currentException = value; }
        }

        public static string CultureInfo { get; set; }

        public static CustomServiceData CustomData
        {
            get
            {
                if (_customData == null)
                {
                    _customData = new CustomServiceData();
                }
                return _customData;
            }
            set { _customData = value; }
        }

        //public static DbTransaction AppDbTransaction
        //{
        //    get { return _appDbTransaction; }
        //    set { _appDbTransaction = value; }
        //}
        #endregion

        #region Public Static Methods

        public static Type GetBLType(string assemblyName, string typeName)
        {
            Type type = null;
            Assembly targetAsm;

            // Go get a reference to the assembly
            targetAsm = FindAssembly(assemblyName);
            if (targetAsm == null)
            {
                targetAsm = FindAssembly(UISettings.BLAssemblyName);
                if (targetAsm == null)
                {
                    throw new ApplicationControlException(String.Format("InvokeHasMainMethod: Assembly [{0}] not found", assemblyName));
                }
            }

            // Get the type reference using a name of "NameSpace.TypeName"
            type = targetAsm.GetType(typeName);
            if (type == null)
            {
                // Check for Common namepspace
                if (UISettings.CommonAssemblyName == string.Empty)
                    throw new ApplicationControlException(String.Format("InvokeHasMainMethod: Type class [{0}] not found in assembly [{1}]", typeName, assemblyName));
                else
                {
                    targetAsm = FindAssembly(UISettings.CommonAssemblyName);

                    if (targetAsm != null)
                    {
                        type = targetAsm.GetType(UISettings.CommonNamespace + "." + typeName);
                    }
                    if (targetAsm == null)
                        throw new ApplicationControlException(String.Format("InvokeHasMainMethod: Type class [{0}] not found in assembly [{1}]", typeName, assemblyName));

                }
            }

            return type;
        }
        /// <summary>
        /// Get Object type based on passed in Assembly name
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="nameSpace"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        /// 

        public static Type GetBLType(string assemblyName, string nameSpace, string typeName)
        {
            Type type = null;
            Assembly targetAsm;

            // Go get a reference to the assembly
            targetAsm = FindAssembly(assemblyName);
            if (targetAsm == null)
            {
                throw new ApplicationControlException(String.Format("InvokeHasMainMethod: Assembly [{0}] not found", assemblyName));
            }

            // Get the type reference using a name of "NameSpace.TypeName"
            type = targetAsm.GetType(nameSpace + "." + typeName);
            if (type == null)
            {
                throw new ApplicationControlException(String.Format("InvokeHasMainMethod: Type class [{0}] not found in assembly [{1}]", typeName, assemblyName));
            }

            return type;
        }

        /// <summary>
        /// This w
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetPLType(string typeName)
        {
            Type type = null;
            Assembly targetAsm;
            string assemblyName = string.Concat(UISettings.PLAssemblyName, ".dll");
            // Go get a reference to the assembly
            if (typeName.Contains("."))
            {
                assemblyName = string.Concat(typeName.Substring(0, typeName.LastIndexOf(".")), ".dll");
            }

            try
            {
                targetAsm = Assembly.LoadFrom(string.Concat(UISettings.PLAssemblyPath, @"\", assemblyName));
            }
            catch
            {
                targetAsm = FindAssembly(assemblyName);
            }

            if (targetAsm == null)
            {
                targetAsm = FindAssembly(assemblyName);
            }

            if (targetAsm == null)
            {
                throw new ApplicationControlException(String.Format("InvokeHasMainMethod: Assembly [{0}] not found", assemblyName));
            }
            type = targetAsm.GetType(typeName.Trim());
            if (type == null)
                type = targetAsm.GetType(typeName.Substring(typeName.LastIndexOf(".") + 1));
            if (type == null)
            {
                throw new ApplicationControlException(String.Format("InvokeHasMainMethod: Type class [{0}] not found in assembly [{1}]", typeName, assemblyName));
            }

            return type;
        }

        public static Type GetWPFType(string typeName)
        {
            Type type = null;
            Assembly targetAsm;
            string assemblyName = string.Concat(UISettings.PLAssemblyName, ".dll");
            if (typeName.Contains("."))
            {
                assemblyName = string.Concat(typeName.Substring(0, typeName.LastIndexOf(".")), ".dll");
            }

            try
            {
                targetAsm = Assembly.LoadFrom(string.Concat(UISettings.PLAssemblyPath, @"\", assemblyName));
            }
            catch
            {
                targetAsm = FindAssembly(assemblyName);
            }

            //TBD Check if PLAssembly name is null and then get from typeName
            if (targetAsm == null)
            {
                throw new ApplicationControlException(String.Format("InvokeHasMainMethod: Assembly [{0}] not found", UISettings.PLAssemblyName));
            }
            type = targetAsm.GetType(typeName.Trim());
            if (type == null)
                type = targetAsm.GetType(typeName.Substring(typeName.LastIndexOf(".") + 1));

            if (type == null)
            {
                throw new ApplicationControlException(String.Format("InvokeHasMainMethod: Type class [{0}] not found in assembly [{1}]", typeName, UISettings.PLAssemblyName));
            }

            return type;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Return a are reference to a named loaded assembly from the current application domain (running app).
        /// </summary>
        /// <param name="assemblyShortName"></param>
        /// <returns></returns>
        private static Assembly FindAssembly(string assemblyShortName)
        {
            Assembly localAssembly;
            try
            {
                string asmPath = string.Concat(UISettings.BLAssemblyPath, @"\", assemblyShortName, ".dll");
                localAssembly = Assembly.LoadFrom(asmPath);
            }
            catch
            {
                localAssembly = null;
            }

            if (localAssembly != null)
            {
                return localAssembly;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.GetName(false).Name == assemblyShortName)
                {
                    return assembly;
                }
            }
            // If not in application domain, try to load from base directory (For batch operations)
            try
            {
                localAssembly = Assembly.Load(assemblyShortName);
                return localAssembly;
            }
            catch
            {
                return null;
            }
        }

        internal static void ClearVarTS()
        {
            _internalThreadHolder = null;
            _externalThreadHolder = null;
            _inServiceThreadShareData = null;
            _transferProgram = null;
            _currentProgram = null;
            _currentKeyPressed = null;
            _userID = null;
            _sessionID = null;
            _operationContext = null;
            _currentException = null;
            _customData = null;
        }
        #endregion
    }
}
