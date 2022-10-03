using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using MDSY.Framework.Interfaces;
using MDSY.Framework.Service.Interfaces;
using MDSY.Framework.Core;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.BaseClasses;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer;
using System.Collections.Generic;
using System.Reflection;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Control.CICS
{
    public class OnlineProgramBase
    {
        #region Private fields

        //private string _dbConnectionString;
        private HoldRecord _holdRecord;
        private Dictionary<HandleCondition, string> _handleConditions = new Dictionary<HandleCondition, string>();
        private string _fileName = null;
        #endregion

        #region public Fields
        public const int ZEROES = 0;
        public const int ZEROS = 0;
        public const int ZERO = 0;
        public const string SPACES = " ";
        public const string SPACE = " ";
        public const string Goto = "Goto";
        public const char CHAR_LOW_VALUE = '\0';
        public string LOW_VALUES = Convert.ToString(CHAR_LOW_VALUE);
        public string LOW_VALUE = Convert.ToString(CHAR_LOW_VALUE);
        public string HIGH_VALUE = Convert.ToString(0xFF);
        public string HIGH_VALUES = Convert.ToString(0xFF);
        public string ProgramName = "";
        public string STARTCODE = "TD";
        #endregion

        #region Public Properties
        public IField Return_Code { get { return _holdRecord.Return_Code; } }
        public IField SORT_RETURN { get { return _holdRecord.SORT_RETURN; } }
        public IField WHEN_COMPILED { get { return _holdRecord.WHEN_COMPILED; } }
        public OnlineControl Control { get; set; }
        public IDataServices Data { get; set; }
        public IMapServices Map { get; set; }
        public bool IsSizeError { get; set; }
        public int TALLY;
        public Dictionary<HandleCondition, string> HandleConditions
        {
            get { return _handleConditions; }
        }
        public byte[] SendData { get; set; }
        public string StartTransTermId { get; set; }
        public string StartTransTransId { get; set; }

        #endregion

        #region Constructors
        public OnlineProgramBase(OnlineControl controlData)
        {
            if (object.ReferenceEquals(controlData, null))
                Control = new OnlineControl();
            else
                Control = controlData;

            SetUpProgramVariables();
        }

        public OnlineProgramBase()
        {

            Control = new OnlineControl();

            SetUpProgramVariables();
        }
        #endregion

        #region public Methods
        public virtual int Main()
        {
            return 0;
        }

        public virtual int Main(params object[] passedParms)
        {
            return 0;
        }

        public virtual int ExecuteMain()
        {
            return 0;
        }

        public virtual int ExecuteMain(params object[] passedParms)
        {
            return 0;
        }

        public void CallSubProgram(string subProgramName, params IField[] passedParms)
        {

        }

        /// <summary>
        /// Stops execution of the program by throwing exception and setting return code to the error code.
        /// </summary>
        /// <param name="ErrorNumber"></param>
        public void StopExecution(int ErrorNumber)
        {
            Return_Code.Assign(ErrorNumber);
            throw new Exception(string.Concat("Execution Stopped! Return Code:", ErrorNumber));
        }

        /// <summary>
        /// Signals the end of program execution. 
        /// </summary>
        public void StopExecution()
        {
        }

        /// <summary>
        /// Stops execution of programs by throwing exception with the reason text.
        /// </summary>
        /// <param name="reason"></param>
        public static void StopExecution(string reason)
        {
            throw new Exception(reason);
        }

        protected virtual void RunMain(string startLabel, string returnLabel)
        {

        }

        protected void Perform(string startLabel, string returnLabel)
        {
            RunMain(startLabel, returnLabel);
        }

        /// <summary>
        /// Writes a message to the console.
        /// </summary>
        /// <param name="text"></param>
        public static void DisplayToLog(string text)
        {
            Console.WriteLine(text);
        }

        public string CheckGotoReturn(string returnMethod)
        {
            if (returnMethod == string.Empty)
                return Goto;
            else
                return returnMethod;
        }

        public void ReceiveData(IBufferValue receivedData, IBufferValue dataLength)
        {
            if (SendData != null)
            {
                receivedData.AssignFrom(SendData);
                SendData = null;
            }
        }

        public void ReceiveData(IBufferValue receivedData, int dataLength)
        {
            if (SendData != null)
            {
                receivedData.AssignFrom(SendData);
                SendData = null;
            }
        }

        public void SendFrom(IBufferValue mapRecordSource, int sendLength, params SendOption[] sendOptions)
        {
            string filePath = "c:\\Jetro_output\\";
            if (!string.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("PrintOutputPath")))
                filePath = ConfigSettings.GetAppSettingsString("PrintOutputPath");

            if (!System.IO.Directory.Exists(filePath))
                System.IO.Directory.CreateDirectory(filePath);

            if (_fileName == null)
                _fileName = StartTransTermId + "_" + ServiceControl.UserID + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt";

            string text = Encoding.ASCII.GetString(mapRecordSource.AsBytes);
            if (text.Length > sendLength)
                text = text.Substring(0, sendLength);

            int lineLength = 0;
            if (!string.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("PrintLineLength")))
                int.TryParse(ConfigSettings.GetAppSettingsString("PrintLineLength"), out lineLength);

            if (lineLength > 0)
            {
                StringBuilder lines = new StringBuilder();

                if (text.Length <= lineLength)
                    lines.Append(text);
                else
                {
                    int startPos = 0;
                    while (true)
                    {
                        if (startPos + lineLength < text.Length)
                        {
                            lines.Append(text.Substring(startPos, lineLength) + Environment.NewLine);
                            startPos += lineLength;
                        }
                        else
                        {
                            lines.Append(text.Substring(startPos));
                            break;
                        }

                    }
                }

                System.IO.File.AppendAllText(filePath + _fileName, lines.ToString() + Environment.NewLine);
            }
            else
                System.IO.File.AppendAllText(filePath + _fileName, text + Environment.NewLine);
        }

        /// <summary>
        /// Writes error abort message to the console.
        /// </summary>
        /// <param name="ex"></param>
        public void DisplayError(System.Exception ex)
        {


            StringBuilder errMessage = new StringBuilder();
            errMessage.AppendLine("******************************************************************");
            errMessage.AppendLine("== Exception Message ==");
            errMessage.AppendLine(string.Concat("    ", ex.Source, " ABORTING DUE TO THE FOLLOWING ERROR"));
            errMessage.AppendLine(ex.Message);
            errMessage.AppendLine("******************************************************************");
            errMessage.AppendLine("== StackTrace ==");
            errMessage.AppendLine(ex.StackTrace);
            errMessage.AppendLine("******************************************************************");
            if (ex.InnerException != null)
            {
                Exception exi = ex.InnerException;
                errMessage.AppendLine("== Inner Exception Message ==");
                errMessage.AppendLine(exi.Message);
                errMessage.AppendLine("******************************************************************");
                errMessage.AppendLine("== StackTrace ==");
                errMessage.AppendLine(exi.StackTrace);
                errMessage.AppendLine("******************************************************************");
            }

            if (DBSUtil.IsService)
            {

                DBSUtil.ServiceThreadShareData.Clear();
                DBSUtil.ServiceThreadShareData.Add(new CICSServiceItemKey("Error", "QUIT", null, "0", true, string.Empty));
                DBSUtil.ServiceThreadShareData.Add(new CICSServiceItemControl("Message", errMessage.ToString(), true, 80, "Red"));

                return;
            }
            else
            {
                DisplayToLog(errMessage.ToString());

            }


        }

        #endregion

        protected void SetHandleCondtions(params object[] condtionParms)
        {
            for (int ctr = 0; ctr < condtionParms.Length; ctr++)
            {
                if (condtionParms[ctr] is HandleCondition)
                {
                    HandleCondition handleCondition = (HandleCondition)condtionParms[ctr];
                    if (_handleConditions.ContainsKey(handleCondition))
                    {
                        _handleConditions[handleCondition] = (string)condtionParms[ctr + 1];
                    }
                    else
                    {
                        _handleConditions.Add(handleCondition, (string)condtionParms[ctr + 1]);
                    }
                }
            }
        }

        protected void SetIgnoreConditions(params HandleCondition[] condtionParms)
        {
            foreach (HandleCondition hCond in condtionParms)
            {
                if (_handleConditions.ContainsKey(hCond))
                {
                    _handleConditions[hCond] = string.Empty;
                }
            }

        }

        protected string CheckHandleCondition()
        {
            if (HandleConditions.ContainsKey(DBSUtil.Condition) && HandleConditions[DBSUtil.Condition] != string.Empty)
            {
                return HandleConditions[DBSUtil.Condition];
            }
            return "";
        }

        protected bool CheckHandleConditions()
        {
            if (HandleConditions.ContainsKey(DBSUtil.Condition) && HandleConditions[DBSUtil.Condition] != string.Empty)
            {
                Type thisInstance = this.GetType();
                thisInstance.InvokeMember(HandleConditions[DBSUtil.Condition], BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, this, new object[] { "Goto" });
                return true;
            }
            return false;
        }

        protected bool CheckHandleConditions(string returnMethod)
        {
            if (HandleConditions.ContainsKey(DBSUtil.Condition) && HandleConditions[DBSUtil.Condition] != string.Empty)
            {
                    returnMethod = CheckGotoReturn(returnMethod);
                    Type thisInstance = this.GetType();
                    thisInstance.InvokeMember(HandleConditions[DBSUtil.Condition], BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, this, new object[] { returnMethod });
                    return true;
            }
            return false;
        }


        protected IMapDefinition GetMapDataInstance(IField mapset, IField mapname)
        {
            IMapDefinition mapdef = (IMapDefinition)DBSUtil.GetBLType(string.Concat(mapset.AsString().Trim(), "_", mapname.AsString().Trim()));
            return mapdef;
        }

        protected void Dump(IField dumpCode)
        {
            Dump(dumpCode.DisplayValue);
        }

        protected void Dump(string dumpCode)
        {
            DumpTask(dumpCode);
        }

        protected void DumpTask(IField dumpCode)
        {
            DumpTask(dumpCode.DisplayValue);
        }

        protected void DumpTask(string dumpCode)
        {
            if (ServiceControl.CurrentException != null)
            {
                SimpleLogging.LogMandatoryMessageToFile("DUMPCODE: " + dumpCode + "\r\n" + ServiceControl.CurrentException.ToString());
                ServiceControl.CurrentException = null;
            }
            else
            {
                string[] stackTmp = Environment.StackTrace.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                bool startCopying = false;
                StringBuilder stackTrace = new StringBuilder();
                for (int i = 0; i < stackTmp.Length; i++)
                {
                    if (startCopying)
                        stackTrace.Append(stackTmp[i] + "\r\n");
                    else if (stackTmp[i].Contains("DumpTask"))
                        startCopying = true;
                }

                SimpleLogging.LogMandatoryMessageToFile("DUMPCODE: " + dumpCode + "\r\n" + stackTrace.ToString());
            }
        }


        #region Private Methods
        private void SetUpProgramVariables()
        {
            try
            {
                Map = new MapServices(Control);
                Data = new OnlineDataServices(Control);
                _holdRecord = new HoldRecord();

                System.IO.FileInfo fi = new System.IO.FileInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                WHEN_COMPILED.Assign(fi.LastWriteTime.ToString("yyyyMMddhhmm"));

                LOW_VALUES = LOW_VALUE.ToString();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
        #endregion
    }


    internal class HoldRecord : PredefinedRecordBase
    {
        #region Name constants
        /// <summary>
        /// Name constants.
        /// </summary>
        internal static class Names
        {
            internal const string RecordName = "HoldRecord";
            internal const string Return_Code = "Return_Code";
            internal const string WHEN_COMPILED = "WHEN_COMPILED";
            internal const string SORT_RETURN = "SORT_RETURN";
        }
        #endregion

        #region direct-access
        public IField Return_Code { get { return GetElementByName<IField>(Names.Return_Code); } }
        public IField WHEN_COMPILED { get { return GetElementByName<IField>(Names.WHEN_COMPILED); } }
        public IField SORT_RETURN { get { return GetElementByName<IField>(Names.SORT_RETURN); } }

        #endregion
        protected override void DefineRecordStructure(IStructureDefinition recordDef)
        {
            recordDef
                .NewField(Names.Return_Code, FieldType.UnsignedNumeric, 4)
                .NewField(Names.WHEN_COMPILED, FieldType.String, 12)
                .NewField(Names.SORT_RETURN, FieldType.UnsignedNumeric, 4);
        }

        protected override string GetRecordName()
        {
            return Names.RecordName;
        }
    }

}
