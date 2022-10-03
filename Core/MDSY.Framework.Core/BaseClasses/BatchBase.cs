using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MDSY.Framework.Core;
using MDSY.Framework.Buffer.Interfaces;
using System.Text;
using System.Xml;
using MDSY.Framework.Buffer;
using MDSY.Framework.Buffer.BaseClasses;
using MDSY.Framework.Interfaces;
using System.Data.Common;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Core
{
    public class BatchBase : IDisposable
    {
        #region Private fields

        //private string _dbConnectionString;
        private IRecord _holdRecord;
        private IDisplayHandler displayHandler = null;
        private IDataServices dataServices = null;
        IDictionary<string, BatchBase> programInstanceCache = new Dictionary<string, BatchBase>();
        public List<string> UPSIFlags;
        public List<string> AcceptData = null;
        private Dictionary<string, string> filepathSubstitutions = new Dictionary<string, string>();
        #endregion

        #region public Fields
        public const int ZEROES = 0;
        public const int ZEROS = 0;
        public const int ZERO = 0;
        public const char CHAR_LOW_VALUE = '\0';
        public const string SPACES = " ";
        public const string SPACE = " ";
        public const string Goto = "Goto";
        public const string QUOTE = "'";
        public string LOW_VALUES = Convert.ToString(CHAR_LOW_VALUE);
        public string LOW_VALUE = Convert.ToString(CHAR_LOW_VALUE);
        public string MinValue = Convert.ToString(CHAR_LOW_VALUE);
        public string HIGH_VALUE = Convert.ToString(0xFF);
        public string HIGH_VALUES = Convert.ToString(0xFF);
        public int TALLY;

        #endregion

        #region Constructors
        public BatchBase()
        {
            SetUpProgramVariables();
            Control = new BatchControl();
        }
        #endregion

        private IDisplayHandler DisplayHandler
        {
            get
            {
                if (displayHandler == null)
                {
                    displayHandler = GetDisplayHandlerObject();
                }

                return displayHandler;
            }
        }

        #region Public Properties
        public IField Return_Code { get { return _holdRecord.GetFieldByName("_return_code"); } }
        public IField SORT_RETURN { get { return _holdRecord.GetFieldByName("SORT_RETURN"); } }
        public IField SORT_FILE_SIZE { get { return _holdRecord.GetFieldByName("SORT_FILE_SIZE"); } }
        public IField WHEN_COMPILED { get { return _holdRecord.GetFieldByName("WHEN_COMPILED"); } }
        public IField ProgramName { get { return _holdRecord.GetFieldByName("ProgramName"); } }
        public bool IsSizeError { get; set; }
        public IGroup CURRENT_DATE { get; set; }
        public BatchControl Control { get; set; }
        public static string SourceType
        {
            get { return "COBOL"; }
        }
        public IDataServices Data
        {
            get
            {
                if (dataServices == null)
                {
                    dataServices = GetDataServicesObject();
                }

                return dataServices;
            }
        }

        public bool ExitProgram { get; set; }
        public IField XML_EVENT { get { return _holdRecord.GetFieldByName("XML_EVENT"); } }
        public IField XML_TEXT { get { return _holdRecord.GetFieldByName("XML_TEXT"); } }
        public IField XML_CODE { get { return _holdRecord.GetFieldByName("XML_Code"); } }
        public IField XML_NAMESPACE { get { return _holdRecord.GetFieldByName("XML_NAMESPACE"); } }
        public IField XML_NAMESPACE_PREFIX { get { return _holdRecord.GetFieldByName("XML_NAMESPACE_PREFIX"); } }
        public DateTime ApplicationDate
        {
            get
            {
                DateTime today = DateTime.Now;
                if (System.Environment.GetEnvironmentVariable("ApplicationDate") != null)
                    try { today = DateTime.Parse(System.Environment.GetEnvironmentVariable("ApplicationDate")) + DateTime.Now.TimeOfDay; }
                    catch { }
                else if (!string.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("ApplicationDate")))
                    try { today = DateTime.ParseExact(ConfigSettings.GetAppSettingsString("ApplicationDate"), "yyyy-MM-dd", null) + DateTime.Now.TimeOfDay; }
                    catch { }
                return today;
            }
        }
        public bool CommandOverflow { get; set; }
        #endregion

        #region events
        public event EventHandler<LogMessageEventArgs> LogMessageSent;
        #endregion

        #region event triggers
        /// <summary>
        /// Triggers the LogMessageSent event.
        /// </summary>
        private void OnLogMessageSent(string message)
        {
            EventHandler<LogMessageEventArgs> handler = LogMessageSent;
            if (handler != null)
            {
                handler(this, new LogMessageEventArgs(message));
            }
        }
        #endregion

        #region public Methods
        public virtual int ExecuteMain()
        {
            return 12;
        }

        public virtual int ExecuteMain(params object[] parms)
        {
            return 12;
        }

        protected virtual int ExecuteAsSubroutine(string inValue, out string outValue)
        {
            outValue = "No Method Found to Override";
            return 12;
        }

        protected virtual void RunMain(string startLabel, string returnLabel)
        {

        }

        protected void Perform(string startLabel, string returnLabel)
        {
            RunMain(startLabel, returnLabel);
        }

        public void Call(string programName, params object[] parms)
        {

            BatchBase programInstance;

            if (!programInstanceCache.ContainsKey(programName.Trim()))
            {
                Type programType = ProgramUtilities.GetBLType(programName);
                if (programType == null) throw new Exception(string.Concat("Called SubProgram not found: ", programName));
                programInstanceCache.Add(programName.Trim(), (BatchBase)Activator.CreateInstance(programType, this));
            }

            programInstance = programInstanceCache[programName.Trim()];
            programInstance.ExecuteMain(parms);

            //TBD:  Move data back to parms

            ExitProgram = false;
        }

        /// <summary>
        /// Execute Program with In string and out string for parameters
        /// </summary>
        /// <param name="inValue"></param>
        /// <param name="outValue"></param>
        /// <returns></returns>
        public int Execute(string inValue, out string outValue)
        {
            return ExecuteAsSubroutine(inValue, out outValue);
        }

        /// <summary>
        /// Stops execution of the program by throwing exception and setting return code to the error code.
        /// </summary>
        /// <param name="ErrorNumber"></param>
        public void StopExecution(int ErrorNumber)
        {
            // Environment.Exit(ErrorNumber);
            Return_Code.Assign(ErrorNumber);
            throw new Exception(string.Concat("Execution Stopped! Return Code:", ErrorNumber));
        }

        /// <summary>
        /// Signals the end of program execution. 
        /// </summary>
        public void StopExecution()
        {
            Control.ExitProgram = true;
            int rtnCodeValue = Return_Code.GetValue<int>();
            if (rtnCodeValue <= 8)
                return;

            throw new Exception(string.Concat("Execution Stopped! Return Code:", rtnCodeValue));
        }

        /// <summary>
        /// Stops execution of programs by throwing exception with the reason text.
        /// </summary>
        /// <param name="reason"></param>
        public void StopExecution(string reason)
        {
            throw new Exception(reason);
        }

        /// <summary>
        /// Writes a message to the console.
        /// </summary>
        /// <param name="text"></param>
        public void DisplayToLog(string text)
        {
            if (text.Length > 36000)
                text = text.Substring(0, 36000);
            if (DisplayHandler != null)
            {
                DisplayHandler.Display(text);
            }
            else
            {
                EventHandler<LogMessageEventArgs> handler = LogMessageSent;

                if (handler != null)
                {
                    Console.WriteLine(text);
                    OnLogMessageSent(text);
                }
                else
                {
                    Console.WriteLine(text);
                }
            }
        }


        public void GetAcceptData(string name, IBufferValue targetField)
        {
            targetField.SetValue(GetAcceptData(name));
        }



        public string GetAcceptData(string name)
        {
            string value = "";
            if (AcceptData == null)
            {
                AcceptData = new List<string>();
                value = System.Environment.GetEnvironmentVariable(name);
                if (value == null)
                {
                    value = "";
                    throw new Exception("Accept From " + name + " problem; Env Var does not exist with that name.");
                }
                if (value.Contains("\\") && System.IO.File.Exists(value))   //filename to file that contains the inline data
                    try
                    {
                        string[] values = System.IO.File.ReadAllLines(value);
                        value = values[0];
                        if (values.Length > 1)
                        {
                            for (int i = 1; i < values.Length; i++)
                                AcceptData.Add(values[i]);
                        }
                    }
                    catch { }
            }
            else if (AcceptData.Count > 0)
            {
                value = AcceptData[0];
                AcceptData.RemoveAt(0);
            }

            return value;
        }


        /// <summary>
        /// Moves passed data into the Linking Section class buffer.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="parms"></param>
        //protected void SetCobolParmData(IRecord record, string[] parms)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    foreach (string parm in parms)
        //    {
        //        builder.Append(parm.Replace("^", " "));
        //    }

        //    //string parmString = string.Empty;
        //    //foreach (string parm in parms)
        //    //{
        //    //    parmString = string.Concat(parmString, parm.Replace("^", " "));
        //    //}

        //    record.DBSBufferReplace(parmString, 0, parmString.Length);


        //}

        //protected void SetCobolParmData(IRecord record, IField[] parms)
        //{
        //    for (int i = 0; i < parms.Length; i++)
        //    {
        //        if (parms[i] is Field_Old)
        //        {
        //            record.FieldList[i].Text = parms[i].Text;
        //        }
        //        else if (parms[i] is Group_Old)
        //        {
        //            record.FieldList[i].Text = parms[i].Text;
        //        }

        //    }

        //}

        //protected void SetCobolParmData(Record_Old record, object[] parms)
        //{
        //    //string parmString = string.Empty;
        //    for (int i = 0; i < parms.Length; i++)
        //    {
        //        if (parms[i] is Field_Old)
        //        {
        //            record.FieldList[i].SetValue((Field_Old)parms[i]);
        //        }
        //        else if (parms[i] is Group_Old)
        //        {
        //            record.FieldList[i].SetValue((Group_Old)parms[i]);
        //        }
        //        else if (parms[i] is string)
        //        {
        //            record.FieldList[i].SetValue((string)parms[i]);
        //        }

        //    }

        //}

        //protected void SetCobolParmDataWithLength(IRecord record, string[] parms)
        //{
        //    string parmString = string.Empty;
        //    foreach (string parm in parms)
        //    {
        //        parmString = string.Concat(parmString, parm.Replace("^", " "));
        //    }

        //    record.DBSBufferReplace(record.DBSBuffer.ToString(2, parmString.Length), parmString, 2, parmString.Length);
        //    record.FieldList[1].Int = parmString.Length;

        //}

        //protected void SetCobolParmDataWithLength(Record_Old lsRec, string parm)
        //{
        //    string parmString = string.Empty;

        //    parmString = parm;

        //    lsRec.DBSBufferReplace(lsRec.DBSBuffer.ToString(2, parmString.Length), parmString, 2, parmString.Length);
        //    lsRec.FieldList[1].Int = parmString.Length;

        //}

        //protected void SetCobolParmData(Record_Old lsRec, string parm)
        //{
        //    string parmString = string.Empty;

        //    parmString = parm;

        //    lsRec.DBSBufferReplace(lsRec.DBSBuffer.ToString(), parmString, 0, parmString.Length);

        //}

        protected void SetPassedParm(object passedVariable, object calledProgramVariable)
        {
            if (passedVariable == null)
                calledProgramVariable = null;
            else if (calledProgramVariable is IBufferValue)
            {
                var pgmVar = (IBufferValue)calledProgramVariable;
                if (passedVariable is IBufferValue)
                    pgmVar.SetValue((IBufferValue)passedVariable);
                else if (passedVariable is string)
                    pgmVar.SetValue((string)passedVariable);
                else if (passedVariable is decimal)
                    pgmVar.SetValue((decimal)passedVariable);
                else if (passedVariable is int)
                    pgmVar.SetValue((int)passedVariable);
                else if (passedVariable is DateTime)
                    pgmVar.SetValue((DateTime)passedVariable);
                else if (passedVariable is byte[])
                    pgmVar.SetValue((byte[])passedVariable);
                return;
            }
            else if (calledProgramVariable is string)
            {
                var pgmVar = (string)calledProgramVariable;
                if (passedVariable is IBufferValue)
                {
                    IBufferValue pVAr = (IBufferValue)passedVariable;
                    pgmVar = pVAr.DisplayValue;
                }
                else
                    pgmVar = (string)passedVariable;

            }
            else if (calledProgramVariable is int)
            {
                var pgmVar = (int)calledProgramVariable;
                if (passedVariable is IBufferValue)
                {
                    IBufferValue pVAr = (IBufferValue)passedVariable;
                    pgmVar = Convert.ToInt32(pVAr.DisplayValue);
                }
                else
                    pgmVar = (int)passedVariable;
            }
            else if (calledProgramVariable is decimal)
            {
                var pgmVar = (decimal)calledProgramVariable;
                if (passedVariable is IBufferValue)
                {
                    IBufferValue pVAr = (IBufferValue)passedVariable;
                    pgmVar = Convert.ToDecimal(pVAr.DisplayValue);
                }
                else
                    pgmVar = (decimal)passedVariable;
            }
            else if (calledProgramVariable is DateTime)
            {
                var pgmVar = (DateTime)calledProgramVariable;
                if (passedVariable is IBufferValue)
                {
                    IBufferValue pVAr = (IBufferValue)passedVariable;
                    pgmVar = Convert.ToDateTime(pVAr.DisplayValue);
                }
                else
                    pgmVar = (DateTime)passedVariable;
            }
            else if (calledProgramVariable is byte[])
            {
                var pgmVar = (byte[])calledProgramVariable;
                if (passedVariable is IBufferValue)
                {
                    IBufferValue pVAr = (IBufferValue)passedVariable;
                    pgmVar = pVAr.AsBytes;
                }
                else
                    pgmVar = (byte[])passedVariable;
            }


        }

        protected void UpdateReturnParm(ref object passedVariable, object calledProgramVariable)
        {
            if (calledProgramVariable == null)
                passedVariable = null;
            else if (passedVariable is IBufferValue)
            {
                var pgmVar = (IBufferValue)passedVariable;
                if (calledProgramVariable is IBufferValue)
                    pgmVar.SetValue((IBufferValue)calledProgramVariable);
                else if (calledProgramVariable is string)
                    pgmVar.SetValue((string)calledProgramVariable);
                else if (calledProgramVariable is decimal)
                    pgmVar.SetValue((decimal)calledProgramVariable);
                else if (calledProgramVariable is int)
                    pgmVar.SetValue((int)calledProgramVariable);
                else if (calledProgramVariable is DateTime)
                    pgmVar.SetValue((DateTime)calledProgramVariable);
                else if (calledProgramVariable is byte[])
                    pgmVar.SetValue((byte[])calledProgramVariable);
                return;
            }
            else if (passedVariable is string)
            {
                if (calledProgramVariable is IBufferValue)
                {
                    IBufferValue pVAr = (IBufferValue)calledProgramVariable;
                    passedVariable = pVAr.DisplayValue;
                }
                else
                    passedVariable = (string)calledProgramVariable;
            }
            else if (passedVariable is int)
            {
                if (calledProgramVariable is IBufferValue)
                {
                    IBufferValue pVAr = (IBufferValue)calledProgramVariable;
                    passedVariable = Convert.ToInt32(pVAr.DisplayValue);
                }
                else
                    passedVariable = (int)calledProgramVariable;
            }
            else if (passedVariable is decimal)
            {
                if (calledProgramVariable is IBufferValue)
                {
                    IBufferValue pVAr = (IBufferValue)calledProgramVariable;
                    passedVariable = Convert.ToDecimal(pVAr.DisplayValue);
                }
                else
                    passedVariable = (decimal)calledProgramVariable;
            }
            else if (passedVariable is DateTime)
            {
                if (calledProgramVariable is IBufferValue)
                {
                    IBufferValue pVAr = (IBufferValue)calledProgramVariable;
                    passedVariable = Convert.ToDateTime(pVAr.DisplayValue);
                }
                else
                    passedVariable = (DateTime)calledProgramVariable;
            }
            else if (passedVariable is byte[])
            {
                if (calledProgramVariable is IBufferValue)
                {
                    IBufferValue pVAr = (IBufferValue)calledProgramVariable;
                    passedVariable = pVAr.AsBytes;
                }
                else
                    passedVariable = (byte[])calledProgramVariable;
            }
        }

        /// <summary>
        /// Writes error abort message to the console.
        /// </summary>
        /// <param name="ex"></param>
        public void DisplayError(Exception ex)
        {
            string errMessage = string.Empty;
            errMessage = string.Concat(errMessage, "\n\n**********************************************\n\n");
            errMessage = string.Concat(errMessage, "    ", ex.Source, " ABORTING DUE TO THE FOLLOWING ERROR\n\n");
            errMessage = string.Concat(errMessage, ex.Message, "\n\n");
            errMessage = string.Concat(errMessage, ex.StackTrace, "\n\n");
            errMessage = string.Concat(errMessage, "**********************************************");
            DisplayToLog(errMessage);
        }

        /// <summary>
        /// Dispose 
        /// </summary>
        public void Dispose()
        {
        }

        public string CheckGotoReturn(string returnMethod)
        {
            if (returnMethod == string.Empty)
                return Goto;
            else
                //{
                //    // check for perform within perform, etc
                //    try
                //    {
                //        object x = new System.Diagnostics.StackFrame(1).GetMethod();
                //        object y = new System.Diagnostics.StackFrame(2).GetMethod();
                //    }
                //    catch { }
                return returnMethod;
            //}
        }

        //public int GetNextSequence(string fileForSequence)
        //{
        //    string segmentName = fileForSequence;
        //    if (fileForSequence == "TFL")
        //        segmentName = GetSegmentName(fileForSequence);

        //    string connString = ConfigurationManager.ConnectionStrings["DataConnectionString"].ConnectionString;
        //    string nextSequence = "";
        //    using (DbConnection sqlConnection = new DbConnection(connString))
        //    {
        //        try
        //        {
        //            sqlConnection.Open();
        //            DbCommand cmd = sqlConnection.CreateCommand();
        //            nextSequence = "StoredProc_GetNext" + segmentName;
        //            cmd.CommandText = nextSequence;
        //            cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //            var result = cmd.ExecuteScalar();
        //            sqlConnection.Close();
        //            return Convert.ToInt32(result);
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new ApplicationControlException(" GetNextSequence Stored Proc Problem: " + ex.Message + "  " + ex.StackTrace);
        //        }
        //    }
        //}

        //public int ResetSequence(string fileForSequence)
        //{
        //    string segmentName = fileForSequence;
        //    if (fileForSequence == "TFL")
        //        segmentName = GetSegmentName(fileForSequence);

        //    string connString = ConfigurationManager.ConnectionStrings["DataConnectionString"].ConnectionString;
        //    string nextSequence = "";
        //    using (SqlConnection sqlConnection = new SqlConnection(connString))
        //    {
        //        try
        //        {
        //            sqlConnection.Open();
        //            SqlCommand cmd = sqlConnection.CreateCommand();
        //            nextSequence = "StoredProc_Reset" + segmentName;
        //            cmd.CommandText = nextSequence;
        //            cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //            var result = cmd.ExecuteScalar();
        //            sqlConnection.Close();
        //            return Convert.ToInt32(result);
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new ApplicationControlException(" ResetSequence Stored Proc Problem: " + ex.Message + "  " + ex.StackTrace);
        //        }
        //    }
        //}

        protected void GetXmlNodeInformation(XmlTextReader xmlReader)
        {
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    XML_EVENT.SetValue("START-OF-ELEMENT");
                    XML_TEXT.SetValue(xmlReader.Name);
                    break;
                case XmlNodeType.Text:
                    XML_EVENT.SetValue("CONTENT-CHARACTERS");
                    XML_TEXT.SetValue(xmlReader.Value);
                    break;
                case XmlNodeType.EndElement:
                    XML_EVENT.SetValue("END-OF-ELEMENT");
                    XML_TEXT.SetValue(xmlReader.Name);
                    break;
                case XmlNodeType.Attribute:
                    XML_EVENT.SetValue("ATTRIBUTE-NAME");
                    XML_TEXT.SetValue(xmlReader.Name);
                    break;
                case XmlNodeType.Document:
                    XML_EVENT.SetValue("START-OF-DOCUMENT");
                    XML_TEXT.SetValue(xmlReader.Name);
                    break;
                case XmlNodeType.XmlDeclaration:
                    XML_EVENT.SetValue("STANDALONE-DECLARATION");
                    XML_TEXT.SetValue(xmlReader.Value);
                    break;
                case XmlNodeType.Comment:
                    XML_EVENT.SetValue("COMMENT");
                    XML_TEXT.SetValue(xmlReader.Value);
                    break;
                case XmlNodeType.Whitespace:
                    if (XML_EVENT.IsEqualTo("START-OF-ELEMENT"))
                        XML_EVENT.SetValue("CONTENT-CHARACTERS");
                    else
                        XML_EVENT.SetValue("WHITESPACE");
                    XML_TEXT.SetValue(xmlReader.Value);
                    break;
                default:
                    XML_EVENT.SetValue(xmlReader.NodeType.ToString());
                    XML_TEXT.SetValue(xmlReader.Value);
                    break;

            }


        }

        #endregion

        #region Private Methods

        private string GetSegmentName(string segmentFile)
        {
            string fileName = "";
            string filePath = "";
            string segmentName = "";
            fileName = Environment.GetEnvironmentVariable(segmentFile);
            fileName = FilePathSubstitutions(fileName);

            if (fileName.Contains(","))
                filePath = fileName.Substring(0, fileName.IndexOf(','));
            else
                filePath = Environment.GetEnvironmentVariable(segmentFile);

            if (filePath.StartsWith("#VSAM#"))
            {
                segmentName = filePath.Split('#')[2];
            }
            return segmentName;
        }

        private string FilePathSubstitutions(string filepath)
        {
            string newDd = filepath.Replace("|", ",");
            newDd = newDd.Replace("NNN", DateTime.Now.Ticks.ToString()).Replace("?D", DateTime.Now.ToString("yyyyMMdd"));

            try
            {
                foreach (string sub in filepathSubstitutions.Keys)
                {
                    if (newDd.Contains(sub))
                        newDd = newDd.Replace(sub, filepathSubstitutions[sub]);
                }
            }
            catch { }

            //in case not covered by filesubstitutions file.
            if (newDd.StartsWith("IP="))
                newDd = @"uncroot\" + newDd.Substring(newDd.LastIndexOf('\\') + 1);
            if (newDd.ToLower().StartsWith("uncroot"))
                newDd = newDd.ToLower().Replace("uncroot", Environment.GetEnvironmentVariable("UNCROOT"));

            return newDd;
        }

        private static IDisplayHandler GetDisplayHandlerObject()
        {
            try
            {
                return InversionContainer.GetImplementingObject<IDisplayHandler>();
            }
            catch
            {
                return null;
            }
        }

        private static IDataServices GetDataServicesObject()
        {
            try
            {
                return InversionContainer.GetImplementingObject<IDataServices>();
            }
            catch
            {
                return null;
            }
        }

        private void SetUpProgramVariables()
        {
            _holdRecord = MDSY.Framework.Buffer.Services.BufferServices.Factory.NewRecord("holdRecord", rec =>
            {
                rec.CreateNewField("_return_code", FieldType.UnsignedNumeric, 4, 0);
                rec.CreateNewField("WHEN_COMPILED", FieldType.String, 14);
                rec.CreateNewField("SORT_RETURN", FieldType.UnsignedNumeric, 4);
                rec.CreateNewField("SORT_FILE_SIZE", FieldType.UnsignedNumeric, 5);
                rec.CreateNewField("ProgramName", Buffer.Common.FieldType.String, 8);
                rec.CreateNewGroup("CURRENT_DATE", (CURRENT_DATE) =>
                {
                    CURRENT_DATE.CreateNewField("Current_Year", FieldType.UnsignedNumeric, 4);
                    CURRENT_DATE.CreateNewField("Current_Month", FieldType.UnsignedNumeric, 2);
                    CURRENT_DATE.CreateNewField("Current_Day", FieldType.UnsignedNumeric, 2);
                });
                rec.CreateNewField("XML_EVENT", FieldType.String, 30, FillWith.Spaces);
                rec.CreateNewField("XML_TEXT", FieldType.String, 500, FillWith.Spaces);
                rec.CreateNewField("XML_Code", FieldType.SignedNumeric, 5, ZERO);
                rec.CreateNewField("XML_NAMESPACE", FieldType.String, 100, FillWith.Spaces);
                rec.CreateNewField("XML_NAMESPACE_PREFIX", FieldType.String, 100, FillWith.Spaces);
            });

            System.IO.FileInfo fi = new System.IO.FileInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            WHEN_COMPILED.Assign(fi.LastWriteTime.ToString("yyyyMMddhhmmss"));

        }

        protected void SetUPSIFlags()
        {

        }
        #endregion

    }
}
