using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Interfaces;
using MDSY.Framework.Core;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.BaseClasses;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Online Control Services 
    /// </summary>
    [Serializable]
    public class BatchControl
    {

        #region Public Properties

        public bool ExitProgram { get; set; }

        public bool CancelProgram { get; set; }

        public string ReturnTransID { get; set; }

        public string TransferProgram { get; set; }

        public static string CurrentSchema
        {
            get { return currentSchema; }
            set { currentSchema = value; }
        }

        public static string StoredProcSchema
        {
            get { return storedProcSchema; }
            set { storedProcSchema = value; }
        }

        public string ServerID
        {
            get
            {
                return Environment.GetEnvironmentVariable("COMPUTERNAME");
            }
        }

        #endregion

        #region private variables

        private readonly IDictionary<string, BatchBase> programInstanceCache;
        [ThreadStatic]
        private static string currentSchema;
        [ThreadStatic]
        private static string storedProcSchema;

        #endregion

        #region Constructors
        public BatchControl()
            : base()
        {
            programInstanceCache = new Dictionary<string, BatchBase>();

        }
        #endregion

        #region Public Methods

        #region Link
        /// <summary>
        /// Link to new program down one level; Expect return back to same place in currecnt program.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="saveArea"></param>
        /// <param name="saveAreaLength"></param>
        /// <param name="respCode"></param>
        public void Link(string programName, IBufferValue saveArea, int saveAreaLength, IField respCode = null)
        {

            BatchBase programInstance;

            if (!programInstanceCache.ContainsKey(programName.Trim()))
            {
                Type programType = ProgramUtilities.GetBLType(programName);
                if (programType == null) throw new Exception(string.Format("Link Program not found: {0}", programName));
                programInstanceCache.Add(programName.Trim(), (BatchBase)Activator.CreateInstance(programType, this));
            }

            programInstance = programInstanceCache[programName.Trim()];
            programInstance.ExecuteMain();

            ExitProgram = false;
        }
        /// <summary>
        /// Link to new program down one level; Expect return back to same place in currecnt program.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="saveArea"></param>
        /// <param name="saveAreaLength"></param>
        /// <param name="respCode"></param>
        public void Link(IBufferValue programName, IBufferValue saveArea, int saveAreaLength, IField respCode = null)
        {
            Link(programName.DisplayValue, saveArea, saveAreaLength, respCode);
        }

        /// <summary>
        /// Link to new program down one level; Expect return back to same place in currecnt program.
        /// </summary>
        /// <param name="programName"></param>
        public void Link(IBufferValue programName)
        {
            Link(programName.DisplayValue);
        }

        /// <summary>
        /// Link to new program down one level; Expect return back to same place in currecnt program.
        /// </summary>
        /// <param name="programName"></param>
        public void Link(string programName)
        {
            BatchBase programInstance;
            if (!programInstanceCache.ContainsKey(programName))
            {
                Type programType = ProgramUtilities.GetBLType(programName);
                if (programType == null) throw new Exception(string.Format("Link Program not found: {0}", programName));
                programInstanceCache.Add(programName.Trim(), (BatchBase)Activator.CreateInstance(programType, this));
            }
            programInstance = programInstanceCache[programName.Trim()];
            programInstance.ExecuteMain();
            ExitProgram = false;
        }
        #endregion

        #region Call
        /// <summary>
        /// Call another new program
        /// </summary>
        public void Call(string programName, params object[] parms)
        {
            BatchBase programInstance;

            if (!programInstanceCache.ContainsKey(programName.Trim()))
            {
                Type programType = ProgramUtilities.GetBLType(programName);
                if (programType == null) throw new Exception(string.Format("Called SubProgram not found: {0}", programName));
                programInstanceCache.Add(programName.Trim(), (BatchBase)Activator.CreateInstance(programType)); //(programType, this));
            }

            programInstance = programInstanceCache[programName.Trim()];
            programInstance.Control.CancelProgram = false;
            programInstance.Control.ExitProgram = false;
            if (parms == null || parms.Length == 0)
                programInstance.ExecuteMain();
            else
                programInstance.ExecuteMain(parms);

            if (programInstance.Control.CancelProgram)
            {
                ExitProgram = true;
                CancelProgram = true;
            }
            else
            {
                ExitProgram = false;
            }
        }

        /// <summary>
        /// Call another new program
        /// </summary>
        public void Call(IBufferValue programName, params object[] parms)
        {
            Call(programName.DisplayValue, parms);
        }
        #endregion

        #region Return
        /// <summary>
        /// Return to calling program
        /// </summary>
        /// <param name="saveArea"></param>
        public void Return(byte[] saveArea)
        {
            ExitProgram = true;
        }

        /// <summary>
        /// Return to start level with next transaction ID
        /// </summary>
        /// <param name="nextTransaction"></param>
        /// <param name="saveArea"></param>
        /// <param name="saveAreaLength"></param>
        public void Return(string nextTransaction, IBufferValue saveArea, int saveAreaLength)
        {
            ReturnTransID = nextTransaction;
            TransferProgram = string.Empty;
            ExitProgram = true;

        }
        /// <summary>
        /// Return to start level with next transaction ID
        /// </summary>
        /// <param name="nextTransaction"></param>
        /// <param name="saveArea"></param>
        /// <param name="saveAreaLength"></param>
        public void Return(IBufferValue nextTransaction, IBufferValue saveArea, int saveAreaLength)
        {
            Return(nextTransaction.DisplayValue, saveArea, saveAreaLength);
        }

        /// <summary>
        /// Return to start level with next transaction ID
        /// </summary>
        /// <param name="nextTransaction"></param>
        /// <param name="saveArea"></param>
        /// <param name="saveAreaLength"></param>
        public void Return(IBufferValue nextTransaction, IBufferValue saveArea)
        {
            Return(nextTransaction.DisplayValue, saveArea, saveArea.AsBytes.Length);
        }


        public void ReturnException(Exception ex)
        {
            Console.WriteLine("Batch runtime error: " + ex.Message);
            Console.WriteLine("         Stacktrace: " + ex.StackTrace);
            ExitProgram = true;

        }
        #endregion

        #region Misc.
        public void ThrowException()
        {

        }


        /// <summary>
        /// Retrieves current date time 
        /// </summary>
        /// <param name="dateTimeField"></param>
        public void GetLatestDateTime(IBufferValue dateTimeField)
        {
            DateTime centuryBegin = new DateTime(1900, 1, 1);
            DateTime currentDate = DateTime.Now;

            long elapsedTicks = (currentDate.Ticks - centuryBegin.Ticks) / 10000;
            dateTimeField.Assign(elapsedTicks);
        }

        public string GetCurrentTimeStamp()
        {
            DateTime currentDate = DateTime.Now;
            if (!string.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("ApplicationDate")))
                currentDate = DateTime.ParseExact(ConfigSettings.GetAppSettingsString("ApplicationDate"), "yyyy-MM-dd", null) + DateTime.Now.TimeOfDay;

            return currentDate.ToString("yyyy-MM-dd HH:mm:ss.FFFFFF");
        }


        #endregion

        #endregion
    }
}
