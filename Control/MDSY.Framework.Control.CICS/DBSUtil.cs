using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using MDSY.Framework.Service.Interfaces;
using System.Threading;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer;
using MDSY.Framework.Core;
using MDSY.Framework.Interfaces;


namespace MDSY.Framework.Control.CICS
{
    #region enum
    /// <summary>
    /// When we have only one DBS parameter we need to say if it is
    /// a giving of position or number
    /// </summary>
    public enum ExamineMode
    {
        Number,
        Position
    }

    public enum LogMessageType
    {
        Information,
        Error
    }
    #endregion

    /// <summary>
    /// Utility class.
    /// </summary>
    public static class DBSUtil
    {
        #region static properties
        /// <summary>
        /// Gets or sets the assembly name to be used in the reflection methods found on this class.
        /// </summary>
        public static string TypeAssemblyName { get; set; }
        /// <summary>
        /// Gets or sets the name space to be used in the reflection methods found on this class.
        /// </summary>
        public static string TypeNameSpace { get; set; }

        [ThreadStatic]
        private static EventWaitHandle _internalThreadHolder;

        [ThreadStatic]
        private static EventWaitHandle _externalThreadHolder;

        [ThreadStatic]
        private static List<IAterasServiceItem> _inServiceThreadShareData;

        [ThreadStatic]
        private static HandleCondition _condition;

        private static ITSQueue _tsQueueHandler;
        private static ITDQueue _tdQueueHandler;
        private static IDisplayHandler _displayHandler;

        public static HandleCondition Condition
        {
            get { return _condition; }
            set { _condition = value; }
        }

        #endregion

        #region internal static methods
        /// <summary>
        /// Return a are reference to a named loaded assembly from the current application domain (running app).
        /// </summary>
        /// <param name="assemblyShortName"></param>
        /// <returns></returns>
        internal static Assembly FindAssembly(string assemblyShortName)
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
            Assembly localAssembly = Assembly.Load(assemblyShortName);

            return localAssembly;
        }
        #endregion

        #region private static methods

        /// <summary>
        /// Launches a generic method
        /// </summary>
        /// <param name="targetObject"></param>
        /// <param name="targetType"></param>
        /// <param name="methodName"></param>
        /// <param name="argumentArray"></param>
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
                    sb.Append(ex.ToString());
                    sb.Append("\r\n");
                    ex = ex.InnerException;
                }

                string errorMessageString = sb.ToString();
                //   SimpleLogging.LogMandatoryMessageToFile(errorMessageString;

                throw new ApplicationException(String.Format("Exception for [{0}][{1}]\r\n{2}", targetType.Name, targetMethod.Name, errorMessageString), ex);
            }
        }

        public static string ConvertPFKey(string passKey)
        {
            switch (passKey)
            {
                case "CLR": return "_";
                case "CLEAR": return "_";
                case "ESCAPE": return "|";
                case "ENTR": return @"'";
                case "ENTER": return @"'";
                case "PA1": return "%";
                case "PA2": return ">";
                case "PA3": return ",";
                case "PF1": return "1";
                case "PF2": return "2";
                case "PF3": return "3";
                case "PF4": return "4";
                case "PF5": return "5";
                case "PF6": return "6";
                case "PF7": return "7";
                case "PF8": return "8";
                case "PF9": return "9";
                case "PF10": return ":";
                case "PF11": return "#";
                case "PF12": return "@";
                case "PF13": return "A";
                case "PF14": return "B";
                case "PF15": return "C";
                case "PF16": return "D";
                case "PF17": return "E";
                case "PF18": return "F";
                case "PF19": return "G";
                case "PF20": return "H";
                case "PF21": return "I";
                case "PF22": return "¢";
                case "PF23": return ".";
                case "PF24": return "<";
                default: return "";
            }
        }

        public static string ConvertToPFKey(string passKey)
        {
            switch (passKey)
            {
                case "_": return "CLEAR";
                //case "_": return"CLR";
                case "|": return "ESCAPE";
                case @"'": return "ENTER";
                //case @"'": return"ENTR'";
                case "%": return "PA1";
                case ">": return "PA2";
                case ",": return "PA3";
                case "1": return "PF1";
                case "2": return "PF2";
                case "3": return "PF3";
                case "4": return "PF4";
                case "5": return "PF5";
                case "6": return "PF6";
                case "7": return "PF7";
                case "8": return "PF8";
                case "9": return "PF9";
                case ":": return "PF10";
                case "#": return "PF11";
                case "@": return "PF12";
                case "A": return "PF13";
                case "B": return "PF14";
                case "C": return "PF15";
                case "D": return "PF16";
                case "E": return "PF17";
                case "F": return "PF18";
                case "G": return "PF19";
                case "H": return "PF20";
                case "I": return "PF21";
                case "¢": return "PF22";
                case ".": return "PF23";
                case "<": return "PF24";
                default: return " ";
            }
        }

        public static ITSQueue TSQueueHandler
        {
            get
            {
                if (_tsQueueHandler == null)
                {
                    _tsQueueHandler = GetTSQueueHandler();
                }

                return _tsQueueHandler;
            }
        }

        public static ITDQueue TDQueueHandler
        {
            get
            {
                if (_tdQueueHandler == null)
                {
                    _tdQueueHandler = GetTDQueueHandler();
                }

                return _tdQueueHandler;
            }
        }

        public static IDisplayHandler DisplayHandler
        {
            get
            {
                if (_displayHandler == null)
                {
                    _displayHandler = GetDisplayHandler();
                }

                return _displayHandler;
            }
        }

        /// <summary>
        /// Gets implementation of Queue
        /// </summary>
        /// <returns></returns>
        private static ITSQueue GetTSQueueHandler()
        {
            ITSQueue tsqueueHandler = InversionContainer.GetImplementingObject<ITSQueue>();
            if (tsqueueHandler == null)
                tsqueueHandler = new TSQueueInMemory();
            return tsqueueHandler;
        }
        private static ITDQueue GetTDQueueHandler()
        {
            ITDQueue tdqueueHandler = InversionContainer.GetImplementingObject<ITDQueue>();
            if (tdqueueHandler == null)
                tdqueueHandler = new TDQueueInUtilityDB();
            return tdqueueHandler;
        }

        private static IDisplayHandler GetDisplayHandler()
        {
            IDisplayHandler displayHandler = InversionContainer.GetImplementingObject<IDisplayHandler>();
            if (displayHandler == null)
                displayHandler = new DisplayHandler();
            return displayHandler;
        }
        #endregion

        #region public static methods

        #region stop execution
        /// <summary>
        /// Completly gets out of the application.
        /// </summary>
        public static void StopExecution()
        {
            StopExecution(0);
            //Do not put any more code in this overload. All code should be in StopExecution(int).
        }
        /// <summary>
        /// Completly gets out of the application with a returnCode.
        /// </summary>
        /// <param name="returnCode"></param>
        public static void StopExecution(int returnCode)
        {
            System.Environment.Exit(returnCode);
        }
        #endregion

        #region Service
        public static List<IAterasServiceItem> ServiceThreadShareData
        {
            get { return DBSUtil._inServiceThreadShareData; }
            set { DBSUtil._inServiceThreadShareData = value; }
        }
        public static bool IsService
        {
            get
            {
                bool isService = false;

                if (_internalThreadHolder != null)
                {
                    isService = true;
                }

                return isService;
            }
        }
        /// <summary>
        /// Gets or sets the thread for the internal system holder
        /// </summary>
        public static EventWaitHandle InternalThreadHolder
        {
            get { return DBSUtil._internalThreadHolder; }
            set { DBSUtil._internalThreadHolder = value; }
        }
        /// <summary>
        /// Gets or sets the thread for the external Service system holder
        /// </summary>
        public static EventWaitHandle ExternalThreadHolder
        {
            get { return DBSUtil._externalThreadHolder; }
            set { DBSUtil._externalThreadHolder = value; }
        }
        #endregion

        #region Misc
        public static string ApplyMask(string text, string mask)
        {
            return Mask.ApplyMask(text, mask);
        }

        public static string DEEDIT(IBufferValue text, int length)
        {
            //Functionality so that alphabetic and special characters are removed from a data field, 
            // and the remaining digits right-aligned and padded to the left with zeros as necessary.
            byte[] newValue = new byte[length];
            for (int v = 0; v < newValue.Length; v++)
            {
                newValue[v] = 0x20;
            }

            int editCntr = length - 1;
            int newValueCtr = editCntr;
            byte[] editValue = text.AsBytes;

            // Check if the last byte is a '-' or 'A' to 'F'.  If so keep them, otherwise just keep numerics 
            if ((editValue[editCntr] == 45) || (editValue[editCntr] >= 65 && editValue[editCntr] <= 70))
            {
                newValue[newValueCtr] = editValue[editCntr];
                editCntr--;
                newValueCtr--;
            }
            else
            {
                int j = newValueCtr;
                for (int i = editCntr; i >= 0; i--)
                {
                    if (editValue[i] >= 48 && editValue[i] <= 57)
                    {
                        newValue[j] = editValue[i];
                        j--;
                    }
                }
            }
            string newText = string.Empty;
            foreach (byte b in newValue)
            {
                newText += (Char)b;
            }
            newText = newText.Trim();
            if (newText.Length > 0)
                newText = newText.PadLeft(length, '0');
            return newText;
        }

        public static string DEEDIT(IBufferValue text, IBufferValue length)
        {
            //Functionality so that alphabetic and special characters are removed from a data field, 
            // and the remaining digits right-aligned and padded to the left with zeros as necessary.
            int iLength = 0;
            Int32.TryParse(length.DisplayValue.AsString(), out iLength);
            return DEEDIT(text, iLength);
        }

        /// <summary>
        /// Check for logging option
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="loggingDescription"></param>
        public static void CheckProgramLogging(string programName, string loggingDescription, LogMessageType messageType = LogMessageType.Information)
        {
            if (UISettings.LogFileErrorOnly && messageType != LogMessageType.Error) return;

            if (UISettings.LogFileEnabled)
            {
                SimpleLogging.LogMandatoryMessageToFile(string.Concat(loggingDescription, programName));
            }
        }

        /// <summary>
        /// Check for logging option
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="loggingDescription"></param>
        public static void CheckProgramLogging(string programName, string loggingDescription, string userID, LogMessageType messageType = LogMessageType.Information)
        {
            if (UISettings.LogFileErrorOnly && messageType != LogMessageType.Error) return;

            if (UISettings.LogFileEnabled)
            {
                SimpleLogging.LogMandatoryMessageToFile(string.Concat(loggingDescription, programName), userID, true);
            }
        }

        public static void LogInformationMessage(string message)
        {
            if (UISettings.LogFileErrorOnly) return;

            if (UISettings.LogFileEnabled)
            {
                SimpleLogging.LogMandatoryMessageToFile(message);
            }
        }

        /// <summary>
        /// Returns the the latest lines form te message log
        /// </summary>
        /// <param name="lineCount"></param>
        /// <returns></returns>
        public static string GetLatestLogMessages(int lineCount)
        {
            if (UISettings.LogFileEnabled)
            {
                return SimpleLogging.RetrieveLatestLogMessages(lineCount);
            }
            else
                return string.Empty;

        }
        /// <summary>
        /// Sets teh Seeion ID used for logging
        /// </summary>
        /// <param name="sessionID"></param>
        public static void SetLoggingSessionID(string sessionID)
        {
            if (UISettings.LogFileEnabled || UISettings.LogFileErrorOnly)
            {
                SimpleLogging.SessionID = sessionID;
            }
        }
        #endregion

        #region invoke
        /// <summary>
        /// Allows getting a program reference via a Field. The Natural program would set the variable to contain the program's name.
        /// </summary>
        /// <param name="fieldName">The Field containing the program's name as text.</param>
        /// <returns></returns>
        public static Type GetBLType(IField fieldName)
        {
            return (GetBLType(fieldName.AsString()));
        }

        /// <summary>
        /// This Object type based assmebly name found in config UISettings
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetBLType(string typeName)
        {
            Type type = null;

            Assembly targetAsm;
            if (TypeAssemblyName == null)
            {
                TypeAssemblyName = UISettings.BLAssemblyName;
            }
            if (TypeNameSpace == null)
            {
                TypeNameSpace = UISettings.BLNamespace;
            }

            // Go get a reference to the assembly
            targetAsm = FindAssembly(TypeAssemblyName);
            if (targetAsm == null)
            {
                throw new ApplicationException(String.Format("InvokeHasMainMethod: Assembly [{0}] not found", TypeAssemblyName));
            }

            // Get the type reference using a name of "NameSpace.TypeName"
            type = targetAsm.GetType(String.Format("{0}.{1}", TypeNameSpace, typeName));
            if (type == null)
            {
                // Check for Common namepspace
                if (UISettings.CommonAssemblyName == string.Empty)
                    throw new ApplicationException(String.Format("InvokeHasMainMethod: Type class [{0}] not found in assembly [{1}]", typeName, TypeAssemblyName));
                else
                {
                    targetAsm = FindAssembly(UISettings.CommonAssemblyName);

                    if (targetAsm != null)
                    {
                        type = targetAsm.GetType(String.Format("{0}.{1}", UISettings.CommonNamespace, typeName));
                    }
                    if (type == null || targetAsm == null)
                        throw new ApplicationException(String.Format("InvokeHasMainMethod: Type class [{0}] not found in assembly [{1}]", typeName, TypeAssemblyName));

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
        public static Type GetBLType(string assemblyName, string nameSpace, string typeName)
        {
            Type type = null;
            Assembly targetAsm;

            TypeAssemblyName = assemblyName;
            TypeNameSpace = nameSpace;


            // Go get a reference to the assembly
            targetAsm = FindAssembly(TypeAssemblyName);
            if (targetAsm == null)
            {
                throw new ApplicationException(String.Format("InvokeHasMainMethod: Assembly [{0}] not found", TypeAssemblyName));
            }

            // Get the type reference using a name of "NameSpace.TypeName"
            type = targetAsm.GetType(TypeNameSpace + "." + typeName);
            if (type == null)
            {
                throw new ApplicationException(String.Format("InvokeHasMainMethod: Type class [{0}] not found in assembly [{1}]", typeName, TypeAssemblyName));
            }

            return type;
        }

        /// <summary>
        /// Check if a class
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static bool DoesPLTypeExist(string typeName)
        {
            Assembly targetAsm;
            bool exist = false;

            // Go get a reference to the assembly
            targetAsm = FindAssembly(UISettings.PLAssemblyName);
            if (targetAsm != null)
            {
                // Get the type reference using a name of "NameSpace.TypeName"
                if (targetAsm.GetType(UISettings.PLNamespace + "." + typeName) != null)
                {
                    exist = true;
                }
            }

            return exist;
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

            // Go get a reference to the assembly
            targetAsm = FindAssembly(UISettings.PLAssemblyName);
            if (targetAsm == null)
            {
                throw new ApplicationException(String.Format("InvokeHasMainMethod: Assembly [{0}] not found", UISettings.PLAssemblyName));
            }
            type = targetAsm.GetType(typeName.Trim());
            if (type == null)
                type = targetAsm.GetType(UISettings.PLNamespace + "." + typeName.Trim());
            if (type == null)
            {
                throw new ApplicationException(String.Format("InvokeHasMainMethod: Type class [{0}] not found in assembly [{1}]", typeName, UISettings.PLAssemblyName));
            }

            return type;
        }

        /// <summary>
        /// This DAL Object type based assmebly name found in config UISettings
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetDALType(string typeName)
        {
            Type type = null;

            Assembly targetAsm;

            string dalTypeAssemblyName = UISettings.DALAssemblyName;
            string dalTypeNameSpace = UISettings.DALNamespace;


            // Go get a reference to the assembly
            targetAsm = FindAssembly(dalTypeAssemblyName);
            if (targetAsm == null)
            {
                throw new ApplicationException(String.Format("InvokeHasMainMethod: Assembly [{0}] not found", dalTypeAssemblyName));
            }

            // Get the type reference using a name of "NameSpace.TypeName"
            type = targetAsm.GetType(dalTypeNameSpace + "." + typeName);
            if (type == null)
            {
                // Check for Common namepspace
                if (UISettings.CommonAssemblyName == string.Empty)
                    throw new ApplicationException(String.Format("InvokeHasMainMethod: Type class [{0}] not found in assembly [{1}]", typeName, TypeAssemblyName));
                else
                {
                    targetAsm = FindAssembly(UISettings.CommonAssemblyName);

                    if (targetAsm != null)
                    {
                        type = targetAsm.GetType(UISettings.CommonNamespace + "." + typeName);
                    }
                    if (type == null || targetAsm == null)
                        throw new ApplicationException(String.Format("InvokeHasMainMethod: Type class [{0}] not found in assembly [{1}]", typeName, TypeAssemblyName));

                }
            }

            return type;
        }

        public static string GetMapSet(string mapName)
        {
            string mapSet = "";

            Assembly targetAsm;
            if (TypeAssemblyName == null)
            {
                TypeAssemblyName = UISettings.BLAssemblyName;
            }
            if (TypeNameSpace == null)
            {
                TypeNameSpace = UISettings.BLNamespace;
            }

            // Go get a reference to the assembly
            targetAsm = FindAssembly(TypeAssemblyName);
            if (targetAsm == null)
            {
                throw new ApplicationException(String.Format("InvokeHasMainMethod: Assembly [{0}] not found", TypeAssemblyName));
            }
            foreach (Type type in targetAsm.GetTypes())
            {
                if (type.Name.Contains("_" + mapName + "_Map"))
                {
                    mapSet = type.Name.Substring(0, type.Name.IndexOf('_'));
                    break;
                }
            }
            if (mapSet == "")
            {
                foreach (Type type in targetAsm.GetTypes())
                {
                    if (type.Name.StartsWith(mapName + "_") && type.Name.Contains("_Map"))
                    {
                        mapSet = "_" + type.Name.Substring(type.Name.IndexOf('_') + 1, type.Name.IndexOf('_'));
                        break;
                    }
                }
            }
            return mapSet;
        }
        #endregion

        #region Temporary Queue
        public static void WriteTemporaryQueue(IBufferValue queueName, IBufferValue queueData, IField dataLength, IField queueItem, QueueOption qOption)
        {
            queueItem.SetValue(TSQueueHandler.WriteTemporaryQueue(queueName.DisplayValue, queueData.AsBytes, dataLength.AsInt(), queueItem.AsInt(), qOption));
        }

        public static void WriteTemporaryQueue(IBufferValue queueName, IBufferValue queueData, IField dataLength)
        {
            TSQueueHandler.WriteTemporaryQueue(queueName.DisplayValue, queueData.AsBytes, dataLength.AsInt(), 1);
        }

        public static void WriteTemporaryQueue(string queueName, IBufferValue queueData, IField dataLength, IField queueItem, QueueOption qOption)
        {
            queueItem.SetValue(TSQueueHandler.WriteTemporaryQueue(queueName, queueData.AsBytes, dataLength.AsInt(), queueItem.AsInt(), qOption));
        }

        public static void WriteTemporaryQueue(string queueName, IBufferValue queueData, IField dataLength)
        {
            TSQueueHandler.WriteTemporaryQueue(queueName, queueData.AsBytes, dataLength.AsInt(), 1);
        }

        public static void WriteTemporaryQueue(IBufferValue queueName, IBufferValue queueData, int dataLength, IField queueItem, QueueOption qOption)
        {
            queueItem.SetValue(TSQueueHandler.WriteTemporaryQueue(queueName.DisplayValue, queueData.AsBytes, dataLength, queueItem.AsInt(), qOption));
        }

        public static void WriteTemporaryQueue(IBufferValue queueName, IBufferValue queueData, int dataLength)
        {
            TSQueueHandler.WriteTemporaryQueue(queueName.DisplayValue, queueData.AsBytes, dataLength, 1);
        }

        public static void WriteTemporaryQueue(string queueName, IBufferValue queueData, int dataLength, IField queueItem, QueueOption qOption)
        {
            queueItem.SetValue(TSQueueHandler.WriteTemporaryQueue(queueName, queueData.AsBytes, dataLength, queueItem.AsInt(), qOption));
        }

        public static void WriteTemporaryQueue(string queueName, IBufferValue queueData, int dataLength)
        {
            TSQueueHandler.WriteTemporaryQueue(queueName, queueData.AsBytes, dataLength, 1);
        }

        public static byte[] ReadTemporaryQueue(string queueName, int dataLength, int queueItem, QueueOption queueOption = QueueOption.None)
        {
            return TSQueueHandler.ReadTemporaryQueue(queueName, dataLength, queueItem, Core.RowPosition.RowID, queueOption);
        }

        public static byte[] ReadTemporaryQueue(string queueName, int dataLength, QueueOption queueOption = QueueOption.None)
        {
            return TSQueueHandler.ReadTemporaryQueue(queueName, dataLength, 0, Core.RowPosition.RowID, queueOption);
        }

        public static void DeleteTemporaryQueue(IBufferValue queueName)
        {
            TSQueueHandler.DeleteTemporaryQueue(queueName.DisplayValue);
        }

        public static void DeleteTemporaryQueue(string queueName)
        {
            TSQueueHandler.DeleteTemporaryQueue(queueName);
        }
        #endregion

        #region Transient Queue
        public static void WriteTransientQueue(IBufferValue queueName, IBufferValue queueData, int dataLength)
        {
            TDQueueHandler.WriteTransientQueue(queueName.DisplayValue, queueData.AsBytes, dataLength);
        }

        public static void WriteTransientQueue(string queueName, IBufferValue queueData)
        {
            TDQueueHandler.WriteTransientQueue(queueName, queueData.AsBytes, queueData.AsBytes.Length);
        }

        public static void WriteTransientQueue(string queueName, IBufferValue queueData, int dataLength)
        {
            TDQueueHandler.WriteTransientQueue(queueName, queueData.AsBytes, dataLength);
        }

        public static byte[] ReadTransientQueue(IBufferValue queueName, int dataLength)
        {
            return TDQueueHandler.ReadTransientQueue(queueName.DisplayValue, dataLength);
        }
        #endregion

        #endregion
    }
}