using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Data.SqlClient;
using System.Data.Common;
using MDSY.Framework.Service.Interfaces;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.BaseClasses;
using MDSY.Utilities.Security;
using MDSY.Framework.Buffer;
using MDSY.Framework.Core;
using MDSY.Framework.Buffer.Services;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Control.CICS
{
    /// <summary>
    /// Online Control Services 
    /// </summary>
    [Serializable]
    public class OnlineControl : PredefinedRecordBase
    {
        #region Name constants
        /// <summary>
        /// Name constants.
        /// </summary>
        internal static class Names
        {
            internal const string RecordName = "OnlineControl";
            internal const string EIBAID = "EIBAID";
            internal const string EIBATT = "EIBATT";
            internal const string EIBCALEN = "EIBCALEN";
            internal const string EIBCOMPL = "EIBCOMPL";
            internal const string EIBCONF = "EIBCONF";
            internal const string EIBCPOSN = "EIBCPOSN";
            internal const string EIBDATE = "EIBDATE";
            internal const string EIBDS = "EIBDS";
            internal const string EIBEOC = "EIBEOC";
            internal const string EIBERR = "EIBERR";

            internal const string EIBERRCD = "EIBERRCD";
            internal const string EIBFMH = "EIBFMH";
            internal const string EIBFN = "EIBFN";
            internal const string EIBFREE = "EIBFREE";
            internal const string EIBNODAT = "EIBNODAT";
            internal const string EIBRCODE = "EIBRCODE";
            internal const string EIBRECV = "EIBRECV";
            internal const string EIBREQID = "EIBREQID";
            internal const string EIBRESP = "EIBRESP";

            internal const string EIBRESP2 = "EIBRESP2";
            internal const string EIBRLDBK = "EIBRLDBK";
            internal const string EIBRSRCE = "EIBRSRCE";
            internal const string EIBSIG = "EIBSIG";
            internal const string EIBSYNC = "EIBSYNC";
            internal const string EIBSYNRB = "EIBSYNRB";
            internal const string EIBTASKN = "EIBTASKN";
            internal const string EIBTIME = "EIBTIME";
            internal const string EIBTRMID = "EIBTRMID";
            internal const string EIBTRNID = "EIBTRNID";
            internal const string RESP = "RESP";
        }

        #endregion

        #region direct-access properties
        public IField EIBAID { get { return GetElementByName<IField>(Names.EIBAID); } }
        public IField EIBATT { get { return GetElementByName<IField>(Names.EIBATT); } }
        public IField EIBCALEN { get { return GetElementByName<IField>(Names.EIBCALEN); } }
        public IField EIBCOMPL { get { return GetElementByName<IField>(Names.EIBCOMPL); } }
        public IField EIBCONF { get { return GetElementByName<IField>(Names.EIBCONF); } }
        public IField EIBCPOSN { get { return GetElementByName<IField>(Names.EIBCPOSN); } }
        private IField _EIBDATE { get { return GetElementByName<IField>(Names.EIBDATE); } }
        public IField EIBDS { get { return GetElementByName<IField>(Names.EIBDS); } }
        public IField EIBEOC { get { return GetElementByName<IField>(Names.EIBEOC); } }
        public IField EIBERR { get { return GetElementByName<IField>(Names.EIBERR); } }

        public IField EIBERRCD { get { return GetElementByName<IField>(Names.EIBERRCD); } }
        public IField EIBFMH { get { return GetElementByName<IField>(Names.EIBFMH); } }
        public IField EIBFN { get { return GetElementByName<IField>(Names.EIBFN); } }
        public IField EIBFREE { get { return GetElementByName<IField>(Names.EIBFREE); } }
        public IField EIBNODAT { get { return GetElementByName<IField>(Names.EIBNODAT); } }
        public IField EIBRCODE { get { return GetElementByName<IField>(Names.EIBRCODE); } }
        public IField EIBRECV { get { return GetElementByName<IField>(Names.EIBRECV); } }
        public IField EIBREQID { get { return GetElementByName<IField>(Names.EIBREQID); } }
        public IField EIBRESP { get { return GetElementByName<IField>(Names.EIBRESP); } }

        public IField EIBRESP2 { get { return GetElementByName<IField>(Names.EIBRESP2); } }
        public IField EIBRLDBK { get { return GetElementByName<IField>(Names.EIBRLDBK); } }
        public IField EIBRSRCE { get { return GetElementByName<IField>(Names.EIBRSRCE); } }
        public IField EIBSIG { get { return GetElementByName<IField>(Names.EIBSIG); } }
        public IField EIBSYNC { get { return GetElementByName<IField>(Names.EIBSYNC); } }
        public IField EIBSYNRB { get { return GetElementByName<IField>(Names.EIBSYNRB); } }
        public IField EIBTASKN { get { return GetElementByName<IField>(Names.EIBTASKN); } }
        private IField _EIBTIME { get { return GetElementByName<IField>(Names.EIBTIME); } }
        public IField EIBTRMID { get { return GetElementByName<IField>(Names.EIBTRMID); } }
        public IField EIBTRNID { get { return GetElementByName<IField>(Names.EIBTRNID); } }
        public IField RESP { get { return GetElementByName<IField>(Names.RESP); } }

        #endregion

        #region Public Properties
        public byte[] DFHCOMMAREA { get; set; }

        public bool ExitProgram { get; set; }

        public bool isMapWaitingSend { get; set; }

        public string ReturnTransID
        {
            get { return _returnTransID == null ? "" : _returnTransID; }
            set { _returnTransID = value; }
        }
        public IField EIBDATE
        {
            get
            {
                GetLatestDateTime();
                return _EIBDATE;
            }
        }
        public IField EIBTIME
        {
            get
            {
                GetLatestDateTime();
                return _EIBTIME;
            }
        }

        public string TransferProgram { get; set; }
        public bool isStartTransaction { get; set; }
        public DbConnection AppConnection { get; set; }
        public DbTransaction AppTransaction { get; set; }

        #endregion

        #region private variables

        private readonly IDictionary<string, OnlineProgramBase> programInstanceCache;
        private readonly IDictionary<string, BatchBase> commonInstanceCache;

        [ThreadStatic]
        private static string _returnTransID;

        private static Dictionary<string, object> resourceLockColl = new Dictionary<string, object>();

        public Dictionary<string, DataChannel> DataChannels = new Dictionary<string, DataChannel>();

        private Dictionary<string, DataSpool> DataSpools = new Dictionary<string, DataSpool>();
        #endregion

        #region Constructors
        public OnlineControl()
            : base()
        {
            programInstanceCache = new Dictionary<string, OnlineProgramBase>();
            commonInstanceCache = new Dictionary<string, BatchBase>();
            this.ResetToInitialValue();
            DBSUtil.SetLoggingSessionID(ServiceControl.UserID);
        }
        #endregion

        #region overrides
        protected override void DefineRecordStructure(IStructureDefinition recordDef)
        {

            recordDef.CreateNewField(Names.EIBAID, Framework.Buffer.Common.FieldType.String, 1);
            recordDef.CreateNewField(Names.EIBATT, Framework.Buffer.Common.FieldType.String, 1);
            recordDef.CreateNewField(Names.EIBCALEN, Framework.Buffer.Common.FieldType.CompShort, 4);
            recordDef.CreateNewField(Names.EIBCOMPL, Framework.Buffer.Common.FieldType.String, 1);
            recordDef.CreateNewField(Names.EIBCONF, Framework.Buffer.Common.FieldType.String, 1);
            recordDef.CreateNewField(Names.EIBCPOSN, Framework.Buffer.Common.FieldType.CompShort, 4);
            recordDef.CreateNewField(Names.EIBDATE, Framework.Buffer.Common.FieldType.PackedDecimal, 7, Convert.ToInt32(string.Format("1{0:yy}{1:000}", DateTime.Today, DateTime.Now.DayOfYear)));
            recordDef.CreateNewField(Names.EIBDS, Framework.Buffer.Common.FieldType.String, 8);
            recordDef.CreateNewField(Names.EIBEOC, Framework.Buffer.Common.FieldType.String, 1);
            recordDef.CreateNewField(Names.EIBERR, Framework.Buffer.Common.FieldType.String, 1);
            recordDef.CreateNewField(Names.EIBERRCD, Framework.Buffer.Common.FieldType.String, 8);
            recordDef.CreateNewField(Names.EIBFMH, Framework.Buffer.Common.FieldType.String, 1);
            recordDef.CreateNewField(Names.EIBFN, Framework.Buffer.Common.FieldType.String, 2);
            recordDef.CreateNewField(Names.EIBFREE, Framework.Buffer.Common.FieldType.String, 1);
            recordDef.CreateNewField(Names.EIBNODAT, Framework.Buffer.Common.FieldType.String, 1);
            recordDef.CreateNewField(Names.EIBRCODE, Framework.Buffer.Common.FieldType.String, 6);
            recordDef.CreateNewField(Names.EIBRECV, Framework.Buffer.Common.FieldType.String, 1);
            recordDef.CreateNewField(Names.EIBREQID, Framework.Buffer.Common.FieldType.String, 8);
            recordDef.CreateNewField(Names.EIBRESP, Framework.Buffer.Common.FieldType.CompInt, 8);
            recordDef.CreateNewField(Names.EIBRESP2, Framework.Buffer.Common.FieldType.CompInt, 8);
            recordDef.CreateNewField(Names.EIBRLDBK, Framework.Buffer.Common.FieldType.String, 1);
            recordDef.CreateNewField(Names.EIBRSRCE, Framework.Buffer.Common.FieldType.String, 8);
            recordDef.CreateNewField(Names.EIBSIG, Framework.Buffer.Common.FieldType.String, 1);
            recordDef.CreateNewField(Names.EIBSYNC, Framework.Buffer.Common.FieldType.String, 1);
            recordDef.CreateNewField(Names.EIBSYNRB, Framework.Buffer.Common.FieldType.String, 1);
            recordDef.CreateNewField(Names.EIBTASKN, Framework.Buffer.Common.FieldType.PackedDecimal, 7);
            recordDef.CreateNewField(Names.EIBTIME, Framework.Buffer.Common.FieldType.PackedDecimal, 7, Convert.ToInt32(DateTime.Now.ToString("HHmmss")));
            recordDef.CreateNewField(Names.EIBTRMID, Framework.Buffer.Common.FieldType.String, 4);
            recordDef.CreateNewField(Names.EIBTRNID, Framework.Buffer.Common.FieldType.String, 4);
            recordDef.CreateNewField(Names.RESP, Framework.Buffer.Common.FieldType.CompShort, 4, 0);

        }
        #endregion

        #region Public Methods

        #region Transfer

        /// <summary>
        /// Transfer to new program on same level.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="saveArea"></param>
        /// <param name="saveAreaLength"></param>
        /// <param name="respCode"></param>
        public void Transfer(string programName, IBufferValue saveArea, int saveAreaLength, IField respCode = null)
        {
            TransferProgram = programName;
            if (saveArea.AsBytes.Length < saveAreaLength)
                DFHCOMMAREA = saveArea.AsBytes;
            else
            {
                DFHCOMMAREA = new byte[saveAreaLength];
                Array.Copy(saveArea.AsBytes, DFHCOMMAREA, saveAreaLength);
            }
            ExitProgram = true;
        }
        /// <summary>
        /// Transfer to new program on same level.
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="saveArea"></param>
        /// <param name="saveAreaLength"></param>
        /// <param name="respCode"></param>
        public void Transfer(IBufferValue programName, IBufferValue saveArea, int saveAreaLength, IField respCode = null)
        {
            Transfer(programName.DisplayValue, saveArea, saveAreaLength, respCode);
        }
        public void Transfer(IBufferValue programName, IBufferValue saveArea, IField respCode = null)
        {
            Transfer(programName.DisplayValue, saveArea, saveArea.AsBytes.Length, respCode);
        }
        public void Transfer(string programName, IField respCode = null)
        {
            DFHCOMMAREA = null;
            TransferProgram = programName;
            ExitProgram = true;
        }
        public void Transfer(IBufferValue programName, IField respCode = null)
        {
            DFHCOMMAREA = null;
            TransferProgram = programName.DisplayValue;
            ExitProgram = true;
        }
        public void Transfer(IBufferValue programName, SaveData saveDataType, IBufferValue saveArea, IField respCode = null)
        {
            TransferProgram = programName.DisplayValue;
            if (saveDataType == SaveData.Channel)
            {

            }
            ExitProgram = true;
        }
        public void Transfer(string programName, SaveData saveDataType, IBufferValue saveArea, IField respCode = null)
        {
            TransferProgram = programName;
            if (saveDataType == SaveData.Channel)
            {

            }
            ExitProgram = true;
        }
        #endregion

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
            programName = programName.Trim();
            if (saveArea.AsBytes.Length < saveAreaLength)
                DFHCOMMAREA = saveArea.AsBytes;
            else
            {
                DFHCOMMAREA = new byte[saveAreaLength];
                Array.Copy(saveArea.AsBytes, DFHCOMMAREA, saveAreaLength);
            }
            OnlineProgramBase linkProgramInstance;

            if (!programInstanceCache.ContainsKey(programName))
            {
                Type programType = DBSUtil.GetBLType(programName);
                if (programType == null) throw new ApplicationControlException(string.Format("Link Program not found: {0}", programName));
                programInstanceCache.Add(programName, (OnlineProgramBase)Activator.CreateInstance(programType, this));
            }

            linkProgramInstance = programInstanceCache[programName];

            linkProgramInstance.Control.AppConnection = AppConnection;
            linkProgramInstance.Control.AppTransaction = AppTransaction;

            DBSUtil.CheckProgramLogging(programName, "Enter Program ");
            if (linkProgramInstance.ExecuteMain() == 12 && ServiceControl.CurrentException != null)
                throw new ApplicationControlException("LINK " + programName + " failed. Please see inner exception for more details", ServiceControl.CurrentException);
            if (AppConnection == null) { AppConnection = linkProgramInstance.Control.AppConnection; }
            if (AppTransaction == null) { AppTransaction = linkProgramInstance.Control.AppTransaction; }
            DBSUtil.CheckProgramLogging(programName, "Exit Program ");

            saveArea.AssignFrom(DFHCOMMAREA);
            this.EIBCALEN.Assign(DFHCOMMAREA.Length);
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

        public void Link(IBufferValue programName, DataChannel dataChannel)
        {
            Link(programName.DisplayValue, dataChannel);
        }

        /// <summary>
        /// Link to new program down one level; Expect return back to same place in currecnt program.
        /// </summary>
        /// <param name="programName"></param>
        public void Link(string programName)
        {
            programName = programName.Trim();
            OnlineProgramBase linkProgramInstance;
            if (!programInstanceCache.ContainsKey(programName))
            {
                Type programType = DBSUtil.GetBLType(programName);
                if (programType == null) throw new ApplicationControlException(string.Format("Link Program not found: {0}", programName));
                programInstanceCache.Add(programName, (OnlineProgramBase)Activator.CreateInstance(programType, this));
            }
            linkProgramInstance = programInstanceCache[programName];

            linkProgramInstance.Control.AppConnection = AppConnection;
            linkProgramInstance.Control.AppTransaction = AppTransaction;

            DBSUtil.CheckProgramLogging(programName, "Enter Program ");
            if (linkProgramInstance.ExecuteMain() == 12 && ServiceControl.CurrentException != null)
            {
                DBSUtil.CheckProgramLogging(programName, "LINK failed in program " + programName + ". \r\n" + CatchError(ServiceControl.CurrentException), LogMessageType.Error);
                throw new ApplicationControlException("LINK " + programName + " failed. Please see inner exception for more details", ServiceControl.CurrentException);
            }
            if (AppConnection == null) { AppConnection = linkProgramInstance.Control.AppConnection; }
            if (AppTransaction == null) { AppTransaction = linkProgramInstance.Control.AppTransaction; }
            DBSUtil.CheckProgramLogging(programName, "Exit Program ");
            ExitProgram = false;
        }

        public void Link(string programName, DataChannel dataChannel)
        {
            programName = programName.Trim();
            OnlineProgramBase linkProgramInstance;
            if (!programInstanceCache.ContainsKey(programName))
            {
                Type programType = DBSUtil.GetBLType(programName);
                if (programType == null) throw new ApplicationControlException(string.Format("Link Program not found: {0}", programName));
                programInstanceCache.Add(programName, (OnlineProgramBase)Activator.CreateInstance(programType, this));
            }
            linkProgramInstance = programInstanceCache[programName];

            linkProgramInstance.Control.AppConnection = AppConnection;
            linkProgramInstance.Control.AppTransaction = AppTransaction;
            if (linkProgramInstance.Control.DataChannels.ContainsKey(dataChannel.ChannelName))
                linkProgramInstance.Control.DataChannels[dataChannel.ChannelName] = dataChannel;
            else
                linkProgramInstance.Control.DataChannels.Add(dataChannel.ChannelName, dataChannel);

            DBSUtil.CheckProgramLogging(programName, "Enter Program ");
            if (linkProgramInstance.ExecuteMain() == 12 && ServiceControl.CurrentException != null)
            {
                DBSUtil.CheckProgramLogging(programName, "LINK failed in program " + programName + ". \r\n" + CatchError(ServiceControl.CurrentException), LogMessageType.Error);
                throw new ApplicationControlException("LINK " + programName + " failed. Please see inner exception for more details", ServiceControl.CurrentException);
            }
            if (AppConnection == null) { AppConnection = linkProgramInstance.Control.AppConnection; }
            if (AppTransaction == null) { AppTransaction = linkProgramInstance.Control.AppTransaction; }
            DBSUtil.CheckProgramLogging(programName, "Exit Program ");
            ExitProgram = false;
        }

        public void Link(IBufferValue programName, SaveData saveDataType, IBufferValue saveDataName,
            IField respCode = null)
        {
            //DFHCOMMAREA = saveArea.AsBytes;
            if (saveDataType == SaveData.Channel)
            {
                throw new NotImplementedException("Link with Channel not implemented yet");
            }

            OnlineProgramBase linkProgramInstance;

            if (!programInstanceCache.ContainsKey(programName.DisplayValue.Trim()))
            {
                Type programType = DBSUtil.GetBLType(programName.DisplayValue);
                if (programType == null) throw new ApplicationControlException(string.Format("Link Program not found: {0}", programName));
                programInstanceCache.Add(programName.DisplayValue.Trim(),
                    (OnlineProgramBase)Activator.CreateInstance(programType, this));
            }
            linkProgramInstance = programInstanceCache[programName.DisplayValue.Trim()];

            linkProgramInstance.Control.AppConnection = AppConnection;
            linkProgramInstance.Control.AppTransaction = AppTransaction;

            DBSUtil.CheckProgramLogging(programName.DisplayValue, "Enter Program ");
            if (linkProgramInstance.ExecuteMain() == 12 && ServiceControl.CurrentException != null)
            {
                DBSUtil.CheckProgramLogging(programName.DisplayValue.Trim(), "LINK failed in program " + programName.DisplayValue.Trim() + ". \r\n" + CatchError(ServiceControl.CurrentException), LogMessageType.Error);
                throw new ApplicationControlException("LINK " + programName.DisplayValue.Trim() + " failed. Please see inner exception for more details", ServiceControl.CurrentException);
            }
            if (AppConnection == null) { AppConnection = linkProgramInstance.Control.AppConnection; }
            if (AppTransaction == null) { AppTransaction = linkProgramInstance.Control.AppTransaction; }
            DBSUtil.CheckProgramLogging(programName.DisplayValue, "Exit Program ");

            //saveArea.AssignFrom(DFHCOMMAREA);
            //this.EIBCALEN.Assign(DFHCOMMAREA.Length);
            ExitProgram = false;
        }

        #endregion

        #region Call
        /// <summary>
        /// Call another new program
        /// </summary>
        public void Call(string programName, params object[] parms)
        {
            Type programType = DBSUtil.GetBLType(programName);
            OnlineProgramBase programInstance;
            BatchBase commonInstance;

            if (programType == null) throw new ApplicationControlException(string.Format("Called SubProgram not found: {0}", programName));
            if (programType is OnlineProgramBase)
            {
                if (!programInstanceCache.ContainsKey(programName.Trim()))
                    programInstanceCache.Add(programName.Trim(), (OnlineProgramBase)Activator.CreateInstance(programType, this));
                programInstance = programInstanceCache[programName.Trim()];
                programInstance.Main(parms);
            }

            else
            {
                if (!commonInstanceCache.ContainsKey(programName.Trim()))
                    commonInstanceCache.Add(programName.Trim(), (BatchBase)Activator.CreateInstance(programType));
                commonInstance = commonInstanceCache[programName.Trim()];
                commonInstance.ExecuteMain(parms);
            }

            //TBD:  Move data back to parms

            ExitProgram = false;
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
            DFHCOMMAREA = saveArea;
            this.EIBCALEN.Assign(saveArea.Length);
            ExitProgram = true;
        }
        public void Return(IBufferValue saveArea)
        {
            DFHCOMMAREA = saveArea.AsBytes;
            this.EIBCALEN.Assign(saveArea.AsBytes.Length);
            resourceLockColl.Clear();
            ExitProgram = true;
        }
        public void Return()
        {
            resourceLockColl.Clear();
            ExitProgram = true;
        }

        public void Return(string nextTransaction)
        {
            ReturnTransID = nextTransaction.Trim();
            TransferProgram = string.Empty;
            CICSServiceItemKey CICSServiceItemKey = null;
            if (DBSUtil.ServiceThreadShareData != null)
                CICSServiceItemKey = (CICSServiceItemKey)DBSUtil.ServiceThreadShareData[0];

            if (CICSServiceItemKey != null)
            {
                CICSServiceItemKey.KeyPressed = "RETURN";
            }
            resourceLockColl.Clear();
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
            ReturnTransID = nextTransaction.Trim();
            TransferProgram = string.Empty;
            //if (saveArea.AsBytes.Length < saveAreaLength)
            //    DFHCOMMAREA = saveArea.AsBytes;
            //else
            //{
            if (saveArea != null)
            {
                byte[] saveAreaAsBytes = saveArea.AsBytes;
                DFHCOMMAREA = new byte[saveAreaLength];
                Array.Copy(saveAreaAsBytes, DFHCOMMAREA, Math.Min(saveAreaLength, saveAreaAsBytes.Length));
            }
            CICSServiceItemKey CICSServiceItemKey = null;
            if (DBSUtil.ServiceThreadShareData != null)
                CICSServiceItemKey = (CICSServiceItemKey)DBSUtil.ServiceThreadShareData[0];

            if (CICSServiceItemKey != null)
            {
                CICSServiceItemKey.KeyPressed = "RETURN";
            }
            resourceLockColl.Clear();
            ExitProgram = true;

        }

        public void Return(string nextTransaction, SaveData saveDataType, IBufferValue saveArea)
        {
            ReturnTransID = nextTransaction.Trim();
            TransferProgram = string.Empty;
            if (saveDataType == SaveData.Channel)
            {
                //throw new NotImplementedException("Return with Channel not implemented yet");
            }
            DFHCOMMAREA = saveArea.AsBytes;

            CICSServiceItemKey CICSServiceItemKey = (CICSServiceItemKey)DBSUtil.ServiceThreadShareData[0];
            if (CICSServiceItemKey != null)
            {
                CICSServiceItemKey.KeyPressed = "RETURN";
            }
            resourceLockColl.Clear();
            ExitProgram = true;

        }

        /// <summary>
        /// Return to start level with next transaction ID
        /// </summary>
        /// <param name="nextTransaction"></param>
        /// <param name="saveArea"></param>
        /// <param name="saveAreaLength"></param>
        public void Return(string nextTransaction, IBufferValue saveArea)
        {
            if (saveArea != null)
                Return(nextTransaction, saveArea, saveArea.Buffer.Length);
            else
                Return(nextTransaction, saveArea, 0);
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

        /// <summary>
        /// Return to top menu
        /// </summary>
        public void ReturnToTop()
        {
            DBSUtil.ServiceThreadShareData.Clear();
            DBSUtil.ServiceThreadShareData.Add(new CICSServiceItemKey("default", string.Empty, string.Empty, string.Empty, false, string.Empty));
            DBSUtil.ExternalThreadHolder.Set();
            isMapWaitingSend = true;
            resourceLockColl.Clear();
            ExitProgram = true;
        }

        public void ReturnException(Exception ex)
        {
            ServiceControl.CurrentException = ex;
            SimpleLogging.LogMandatoryMessageToFile(ex.Message + Environment.NewLine + ex.StackTrace);
        }
        #endregion

        #region Misc.

        public void ThrowException(IField abcode = null)
        {
            ThrowException(abcode == null ? "" : abcode.DisplayValue);
        }

        public void ThrowException(string abcode)
        {
            resourceLockColl.Clear();
            throw new ApplicationControlException("CICS ABEND" + (abcode == null ? "" : (" ABCODE = " + abcode)));
        }

        /// <summary>
        /// Determine DFHRESP error codes showng codes returned from mainframe CICS services
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public int DFHRESP(string errorCode)
        {
            switch (errorCode)
            {
                case "NORMAL": return 0;
                case "ERROR": return 1;
                case "RDATT": return 2;
                case "WRBRK": return 3;
                case "EOF": return 4;
                case "EODS": return 5;
                case "EOC": return 6;
                case "INBFMH": return 7;
                case "ENDINPT": return 8;
                case "NONVAL": return 9;
                case "NOSTART": return 10;
                case "TERMIDERR": return 11;
                case "FILENOTFOUND": return 12;
                case "NOTFND": return 13;
                case "DUPREC": return 14;
                case "DUPKEY": return 15;
                case "INVREQ": return 16;
                case "IOERR": return 17;
                case "NOSPACE": return 18;
                case "NOTOPEN": return 19;
                case "ENDFILE": return 20;
                case "ILLOGIC": return 21;
                case "LENGERR": return 22;
                case "QZERO": return 23;
                case "SIGNAL": return 24;
                case "QBUSY": return 25;
                case "ITEMERR": return 26;
                case "PGMIDERR": return 27;
                case "TRANSIDERR": return 28;
                case "ENDDATA": return 29;
                case "EXPIRED": return 31;
                case "RETPAGE": return 32;
                case "RTEFAIL": return 33;
                case "RTESOME": return 34;
                case "TSIOERR": return 35;
                case "MAPFAIL": return 36;
                case "INVERRTERM": return 37;
                case "INVMPSZ": return 38;
                case "IGREQID": return 39;
                case "OVERFLOW": return 40;
                case "INVLDC": return 41;
                case "NOSTG": return 42;
                case "JIDERR": return 43;
                case "QIDERR": return 44;
                case "NOJBUFSP": return 45;
                case "DSSTAT": return 46;
                case "SELNERR": return 47;
                case "FUNCERR": return 48;
                case "UNEXPIN": return 49;
                case "NOPASSBKRD": return 50;
                case "NOPASSBKWR": return 51;
                case "SYSIDERR": return 53;
                case "ISCINVREQ": return 54;
                case "ENQBUSY": return 55;
                case "IGREQCD": return 57;
                case "SESSIONERR": return 58;
                case "SYSBUSY": return 59;
                case "SESSBUSY": return 60;
                case "NOTALLOC": return 61;
                case "CBIDERR": return 62;
                case "INVEXITREQ": return 63;
                case "INVPARTNSET": return 64;
                case "INVPARTN": return 65;
                case "PARTNFAIL": return 66;
                case "USERIDERR": return 69;
                case "NOTAUTH": return 70;
                case "SUPPRESSED": return 72;
                case "NOSPOOL": return 80;
                case "TERMERR": return 81;
                case "ROLLEDBACK": return 82;
                case "END": return 83;
                case "DISABLED": return 84;
                case "ALLOCERR": return 85;
                case "STRELERR": return 86;
                case "OPENERR": return 87;
                case "SPOLBUSY": return 88;
                case "SPOLERR": return 89;
                case "NODEIDERR": return 90;
                case "TASKIDERR": return 91;
                case "TCIDERR": return 92;
                case "DSNNOTFOUND": return 93;
                case "LOADING": return 94;
                case "MODELIDERR": return 95;
                case "OUTDESCRERR": return 96;
                case "PARTNERIDERR": return 97;
                case "PROFILEIDERR": return 98;
                case "NETNAMERR	": return 99;
                case "LOCKED": return 100;
                case "RECORDBUSY": return 101;
                case "UOWNOTFOUND": return 102;
                case "UOWLNOTFOUND": return 103;
                case "LINKABEND": return 104;
                case "CHANGED": return 105;
                case "PROCESSBUSY": return 106;
                case "ACTIVITYBUSY": return 107;
                case "PROCESSERR": return 108;
                case "ACTIVITYERR": return 109;
                case "CONTAINERERR": return 110;
                case "EVENTERR": return 111;
                case "TOKENERR": return 112;
                case "NOTFINISHED": return 113;
                case "POOLERR": return 114;
                case "TIMERERR": return 115;
                case "SYMBOLERR": return 116;
                case "TEMPLATERR": return 117;
                case "RESUNAVAIL": return 121;
                case "CHANNELERR": return 122;
                case "CCSIDERR": return 123;



                default: return 0;
            }
        }

        /// <summary>
        /// Set handle condition based on error code.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public static HandleCondition GetCondition(int errorCode)
        {
            switch (errorCode)
            {
                case 0: return HandleCondition.NORMAL;
                case 1: return HandleCondition.ERROR;
                case 84: return HandleCondition.DISABLED;
                case 12: return HandleCondition.FILENOTFOUND;
                case 13: return HandleCondition.NOTFND;
                case 17: return HandleCondition.IOERR;
                case 19: return HandleCondition.NOTOPEN;
                case 20: return HandleCondition.ENDFILE;
                case 36: return HandleCondition.MAPFAIL;
                default: return HandleCondition.ERROR;
            }

        }

        /// <summary>
        /// Retrieves current date time 
        /// </summary>
        /// <param name="dateTimeField"></param>
        public void GetLatestDateTime(IBufferValue dateTimeField)
        {
            DateTime centuryBegin = new DateTime(1900, 1, 1);
            DateTime currentDate = DateTime.Now;
            if (!string.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("ApplicationDate")))
                currentDate = DateTime.ParseExact(ConfigSettings.GetAppSettingsString("ApplicationDate"), "yyyy-MM-dd", null) + DateTime.Now.TimeOfDay;

            long elapsedTicks = (currentDate.Ticks - centuryBegin.Ticks) / 10000;
            dateTimeField.Assign(elapsedTicks);

            _EIBDATE.AssignFrom(string.Format("1{0:yy}{1:000}", DateTime.Today, DateTime.Now.DayOfYear));
            _EIBTIME.AssignFrom(currentDate.ToString("HHmmss"));
        }

        /// <summary>
        /// Retrieves current date time into EIBDATE and EIBTIME
        /// </summary>
        public void GetLatestDateTime()
        {
            DateTime currentDate = DateTime.Now;
            if (!string.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("ApplicationDate")))
                currentDate = DateTime.ParseExact(ConfigSettings.GetAppSettingsString("ApplicationDate"), "yyyy-MM-dd", null) + DateTime.Now.TimeOfDay;

            _EIBDATE.AssignFrom(string.Format("1{0:yy}{1:000}", DateTime.Today, DateTime.Now.DayOfYear));
            _EIBTIME.AssignFrom(currentDate.ToString("HHmmss"));
        }

        public string GetCurrentTimeStamp()
        {
            DateTime currentDate = DateTime.Now;
            if (!string.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("ApplicationDate")))
                currentDate = DateTime.ParseExact(ConfigSettings.GetAppSettingsString("ApplicationDate"), "yyyy-MM-dd", null) + DateTime.Now.TimeOfDay;

            return currentDate.ToString("yyyy-MM-dd HH:mm:ss.FFFFFF");
        }


        /// <summary>
        /// Causes a delay for a specified interval "hhmmss"
        /// </summary>
        /// <param name="interval">integer containing "hhmmss" format</param>
        /// <param name="delayName"></param>
        public void Delay(int interval, string delayName = null)
        {
            Thread.Sleep(interval * 1000);
        }

        /// <summary>
        /// Causes a delay for a specified interval "hhmmss"
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="delayName"></param>
        public void Delay(IBufferValue interval, IBufferValue delayName = null)
        {
            Delay(Convert.ToInt32(interval.BytesAsString), delayName.BytesAsString);
        }

        public void Delay(IBufferValue interval, string delayName = null)
        {
            Delay(Convert.ToInt32(interval.BytesAsString), delayName);
        }

        public void Delay(int interval, IBufferValue delayName = null)
        {
            Delay(interval, delayName.BytesAsString);
        }

        /// <summary>
        /// Create a resource lock based on lock name
        /// </summary>
        /// <param name="lockName"></param>
        /// <param name="lockNameLength"></param>
        public void SetResourceLock(IBufferValue lockName, int lockNameLength)
        {
            string lockKey = lockName.BytesAsString.Substring(0, lockNameLength);
            object resourceDefLock = new object();

            while (resourceLockColl.ContainsKey(lockKey))
            {
                Thread.Sleep(250);
            }

            lock (resourceDefLock)
            {
                resourceLockColl.Add(lockKey, resourceDefLock);
            }
        }
        public void SetResourceLock(IBufferValue lockName, int lockNameLength, IField responseField)
        {
            string lockKey = lockName.BytesAsString.Substring(0, lockNameLength);
            object resourceDefLock = new object();

            while (resourceLockColl.ContainsKey(lockKey))
            {
                Thread.Sleep(250);
            }

            lock (resourceDefLock)
            {
                resourceLockColl.Add(lockKey, resourceDefLock);
            }
            DBSUtil.Condition = HandleCondition.NORMAL;
            SetEIBRCODE();
            responseField.SetValue(RESP);

        }


        /// <summary>
        /// Release a resource lock based on lock name
        /// </summary>
        /// <param name="lockName"></param>
        /// <param name="lockNameLength"></param>
        public void RemoveResourceLock(IBufferValue lockName, int lockNameLength)
        {
            string lockKey = lockName.BytesAsString.Substring(0, lockNameLength);
            if (resourceLockColl.ContainsKey(lockKey))
            {
                resourceLockColl.Remove(lockKey);
            }
        }

        /// <summary>
        /// Start transaction on new thread
        /// </summary>
        /// <param name="transId"></param>
        /// <param name="startInterval"></param>
        /// <param name="requestID"></param>
        /// <param name="sendData"></param>
        /// <param name="dataLength"></param>
        /// <param name="termId"></param>
        public void StartTransaction(string transId, int startInterval, string requestID, IBufferValue sendData,
            IBufferValue dataLength, IBufferValue termId)
        {
            int length = 0;
            if (dataLength != null)
                length = int.Parse(dataLength.DisplayValue);
            StartTransaction(transId, startInterval, requestID, sendData, length, termId);
        }

        public void StartTransaction(string transId, int startInterval, string requestID, IBufferValue sendData,
            int dataLength, IBufferValue termId)
        {
            StartTransaction(transId, startInterval, requestID, sendData, dataLength, termId.BytesAsString);
        }

        public void StartTransaction(string transId, int startInterval, string requestID, IBufferValue sendData,
    int dataLength, string termId)
        {
            ReturnTransID = transId.ToUpper();
            EIBTRMID.SetValue(ServiceControl.TERMID);
            EIBTRNID.SetValue(ReturnTransID);
            isStartTransaction = true;
            isMapWaitingSend = false;

            if (startInterval > 0)
            {
                System.Timers.Timer timer = new System.Timers.Timer(startInterval * 1000);
                timer.Elapsed += (sender, e) => { HandleTimer(transId, termId, sendData.AsBytes, dataLength); };
                timer.Start();
            }
            else
                TriggerTransaction(transId, termId, sendData == null ? null : sendData.AsBytes, dataLength);
        }

        private void HandleTimer(string transId, string termId, byte[] sendData, int dataLength)
        {
            TriggerTransaction(transId, termId, sendData, dataLength);
        }

        private void TriggerTransaction(string transId, string termId, byte[] sendData, int datLength)
        {
            DBSUtil.LogInformationMessage("Triggering transaction: transId=" + transId + " termId=" + termId + " - " + DateTime.Now.Ticks.ToString());

            object[] parms = new object[] { transId, termId, ServiceControl.UserID, ServiceControl.OPID, sendData, datLength };
            NamedBackgroundWorker backgroundWorker = new NamedBackgroundWorker(transId + "-" + termId + "-" + DateTime.Now.Ticks.ToString());
            backgroundWorker.DoWork += new DoWorkEventHandler(BackgroundWorker_RunProcess);
            // probably dont need the RunWorkerCompleted but for now put in here to see if we ever do finish.
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunProcessCompleted);
            backgroundWorker.RunWorkerAsync(parms);
        }

        private void BackgroundWorker_RunProcessCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
#if DEBUG
                throw new ApplicationControlException("Background worker process failed. Please see inner exception for more details", e.Error);
#endif
            }
        }

        private void BackgroundWorker_RunProcess(object sender, DoWorkEventArgs e)
        {
            String programName = "";
            try
            {
                ServiceControl.TERMID = (string)((object[])e.Argument)[1];
                ServiceControl.UserID = (string)((object[])e.Argument)[2];
                ServiceControl.OPID = (string)((object[])e.Argument)[3];
                DBSUtil.SetLoggingSessionID(ServiceControl.UserID);
                string transId = (string)((object[])e.Argument)[0];
                programName = TransactionControl.Instance.GetProgramName(transId.ToUpper());
                Type programType = DBSUtil.GetBLType(programName);
                OnlineProgramBase programInstance;
                if (programType == null) return;
                programInstance = (OnlineProgramBase)Activator.CreateInstance(programType, null);

                programInstance.StartTransTermId = ServiceControl.TERMID;
                byte[] sendData = (byte[])((object[])e.Argument)[4];
                if (sendData != null)
                {
                    int dataLength = (int)((object[])e.Argument)[5];
                    programInstance.SendData = new byte[dataLength];
                    Array.Copy(sendData, 0, programInstance.SendData, 0, (long)Math.Min(sendData.Length, dataLength));
                }

                DBSUtil.CheckProgramLogging(programType.Name, "Enter Program ");
                if (programInstance.ExecuteMain() == 12 && ServiceControl.CurrentException != null)
                {
                    DBSUtil.CheckProgramLogging(programName, "Start Transaction failed in program " + programName + ". \r\n" + CatchError(ServiceControl.CurrentException), LogMessageType.Error);
                    throw new ApplicationControlException("Start Transaction program " + programName + " failed. Please see inner exception for more details", ServiceControl.CurrentException);
                }

                programInstance.Data.ForceDbClose();
                DBSUtil.CheckProgramLogging(programType.Name, "Exit Program ");
            }
            catch (Exception ex)
            {
                if (ServiceControl.CurrentException == null)
                    CatchError(ex);
                ServiceControl.CurrentException = ex;
                //throw new Exception("TDQueue " + programName + " failed. Please see inner exception for more details", ex);
            }
        }

        /// <summary>
        /// Cancel transaction  
        /// </summary>
        /// <param name="transId"></param>
        /// <param name="startInterval"></param>
        /// <param name="requestID"></param>
        public void CancelTransaction(string transId, string requestID)
        {
            EIBRCODE.SetMaxValue();
        }

        public IBufferElement GetCommonWorkArea()
        {
            throw new NotImplementedException("GetCommonWorkArea Not yet implemented!");
        }

        public IBufferElement GetTransactionWorkArea()
        {
            throw new NotImplementedException("GetTransactionnWorkArea Not yet implemented!");
        }


        protected override string GetRecordName()
        {
            return Names.RecordName;
        }

        public void PutContainerData(IBufferValue containerName, IBufferValue channelName, IBufferValue data,
            int dataLength, IField respCode = null)
        {
            PutContainerData(containerName.DisplayValue, channelName, data, dataLength, respCode);
        }

        public void PutContainerData(string containerName, IBufferValue channelName, IBufferValue data,
    int dataLength, IField respCode = null)
        {
            try
            {
                string chanName = "SYSTEM";
                if (channelName != null)
                    chanName = channelName.DisplayValue;
                if (!DataChannels.ContainsKey(chanName))
                {
                    DataChannels.Add(chanName, new DataChannel(chanName));
                }

                DataChannel dataChannel = DataChannels[chanName];
                dataChannel.PutContainer(containerName, data.AsBytes);
                respCode.SetValue(0);
            }
            catch
            {
                respCode.SetValue(122);
            }
        }

        /// <summary>
        /// Get Channel Container data
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="channelName"></param>
        /// <param name="respCode"></param>
        /// <returns></returns>
        public void GetContainerData(IBufferValue containerName, IBufferValue channelName, IBufferValue saveData, IField respCode = null)
        {

            GetContainerData(containerName.DisplayValue, channelName, saveData, respCode);
        }

        public void GetContainerData(string containerName, IBufferValue channelName, IBufferValue saveData, IField respCode = null)
        {
            string chanName = "SYSTEM";
            if (channelName != null)
                chanName = channelName.DisplayValue;
            byte[] containerData = null;
            respCode.SetValue(0);
            if (!DataChannels.ContainsKey(chanName))
            {
                RESP.SetValue(122);
                if (respCode != null)
                {
                    respCode.SetValue(122);
                }
            }
            else
            {
                containerData = DataChannels[chanName].GetContainer(containerName);
                if (containerData == null)
                {
                    RESP.SetValue(110);
                    if (respCode != null)
                    {
                        respCode.SetValue(110);
                    }
                }
            }

            if (containerData != null)
            {
                saveData.SetValue(containerData);
            }

        }

        public string GetChannelName()
        {
            if (DataChannels.Count > 0)
                return DataChannels.Keys.First();
            else
                return string.Empty;
        }
        public void DeleteContainerData(IBufferValue containerName, IBufferValue channelName, IField respCode = null)
        {
            DeleteContainerData(containerName.DisplayValue, channelName, respCode);
        }

        public void DeleteContainerData(string containerName, IBufferValue channelName, IField respCode = null)
        {
            string chanName = "SYSTEM";
            if (channelName != null)
                chanName = channelName.DisplayValue;
            if (!DataChannels.ContainsKey(chanName))
            {
                RESP.SetValue(122);
                if (respCode != null)
                {
                    respCode.SetValue(122);
                }
            }
            else
            {
                DataChannels[chanName].DeleteContainer(containerName);
                respCode.SetValue(0);
            }

        }

        public void OpenSpool(IBufferValue spoolToken, IBufferValue spoolUserID, IBufferValue spoolNode, IBufferValue spoolClass, int recordLength, IField respCode = null)
        {
            OpenSpool(spoolToken, spoolUserID.BytesAsString, spoolNode, spoolClass, recordLength, respCode);
        }
        public void OpenSpool(IBufferValue spoolToken, string spoolUserID, IBufferValue spoolNode, IBufferValue spoolClass, int recordLength, IField respCode = null)
        {
            string spNode = string.Empty;
            string spClass = string.Empty;
            if (spoolNode != null)
            {
                spNode = spoolNode.BytesAsString;
            }
            if (spoolClass != null)
            {
                spClass = spoolClass.BytesAsString;
            }
            if (DataSpools.ContainsKey(spoolToken.BytesAsString.Trim()))
            {
                DataSpools[spoolToken.BytesAsString.Trim()] = null;
                DataSpools[spoolToken.BytesAsString.Trim()] = new DataSpool(spoolUserID, spNode, spClass);
            }
            else
            {
                DataSpools.Add(spoolToken.BytesAsString.Trim(), new DataSpool(spoolUserID, spNode, spClass));
            }
            if (respCode != null)
                respCode.SetValue(0);

        }

        public void WriteSpool(IBufferValue spoolToken, IBufferValue spoolData, IField respCode = null)
        {
            if (DataSpools.ContainsKey(spoolToken.BytesAsString.Trim()))
            {
                DataSpools[spoolToken.BytesAsString.Trim()].AddDataToSpool(spoolData.BytesAsString);
                if (respCode != null)
                    respCode.SetValue(0);
            }
            else
            {
                if (respCode != null)
                    respCode.SetValue(89);
            }

        }

        public void WriteSpool(IBufferValue device, IBufferValue spoolToken, IBufferValue spoolData, IField respCode = null)
        {
            WriteSpool(spoolToken, spoolData, respCode);
        }

        public void CloseSpool(IBufferValue spoolToken, IField respCode = null)
        {
            if (DataSpools.ContainsKey(spoolToken.BytesAsString.Trim()))
            {
                DataSpool spool = DataSpools[spoolToken.BytesAsString.Trim()];

                if (spool.UserID.Trim() == "INTRDR")
                {
                    StringBuilder jclStream = new StringBuilder();
                    foreach (string jclLine in DataSpools[spoolToken.BytesAsString.Trim()].DataList)
                    {
                        jclStream.AppendLine(jclLine);
                    }
                    string submitJCL = jclStream.ToString();
                    SubmitToBatch(submitJCL);
                }
                DataSpools.Remove(spoolToken.BytesAsString.Trim());
                if (respCode != null)
                    respCode.SetValue(0);
            }
            else
            {
                if (respCode != null)
                    respCode.SetValue(89);
            }
        }

        /// <summary>
        /// Gets New Buffer for an IGroup by cloning the Record  buffer and initializing the Group
        /// </summary>
        /// <param name="bufferGroup"></param>
        /// <param name="bufferLength"></param>
        /// <param name="initialValue"></param>
        public void GetNewBuffer(IGroup bufferGroup, int bufferLength, IBufferValue initialValue)
        {
            IRecord recordClone = (IRecord)bufferGroup.Record.Clone();
            bufferGroup.SetReferenceTo(recordClone);
            BufferServices.Records.Add(recordClone);
            if (initialValue == null)
                bufferGroup.FillWithByte(0);
            else
                bufferGroup.SetValueAll(initialValue.BytesAsString[0]);
        }

        /// <summary>
        /// Gets New Buffer for a Field by cloning the Record buffer and initializing the Field
        /// </summary>
        /// <param name="bufferField"></param>
        /// <param name="bufferLength"></param>
        /// <param name="initialValue"></param>
        public void GetNewBuffer(IField bufferField, int bufferLength, IBufferValue initialValue)
        {
            IRecord recordClone = (IRecord)bufferField.Record.Clone();
            bufferField.SetReferenceTo(recordClone);
            BufferServices.Records.Add(recordClone);
            if (initialValue == null)
                bufferField.FillWithByte(0);
            else
                bufferField.SetValueAll(initialValue.BytesAsString[0]);
        }

        /// <summary>
        /// Gets New Buffer for a Record by cloning the Record buffer and initializing  
        /// </summary>
        /// <param name="record"></param>
        /// <param name="bufferLength"></param>
        /// <param name="initialValue"></param>
        public void GetNewBuffer(IRecord record, int bufferLength, IField initialValue)
        {
            record = (IRecord)record.Clone();
            BufferServices.Records.Add(record);
            if (initialValue.IsSpaces())
                record.SetValueWithSpaces();
            else if (initialValue.IsMinValue())
                record.SetMinValue();
        }

        /// <summary>
        /// Checks every given number of seconds to see if all background tasks for the user are completed before continuing
        /// </summary>
        /// <param name="seconds"></param>
        public async void CheckBackgroundProcess(int seconds)
        {
            int ctr = 0;
            while (Security.GetUserThreadCount(GetUserID()) > 0)
            {
                if (ctr > 25)
                {
                    break;
                }
                await Task.Delay(TimeSpan.FromSeconds(seconds));
                ctr++;
            }
        }
        #endregion

        #region Queue
        public void WriteTemporaryQueue(IBufferValue queueName, IBufferValue queueData, IField dataLength, IField queueItem, QueueOption queueOption = QueueOption.None)
        {
            DBSUtil.WriteTemporaryQueue(queueName, queueData, dataLength, queueItem, queueOption);
            SetEIBRCODE();
        }
        public void WriteTemporaryQueue(string queueName, IBufferValue queueData, IField dataLength, IField queueItem, QueueOption queueOption = QueueOption.None)
        {
            DBSUtil.WriteTemporaryQueue(queueName, queueData, dataLength, queueItem, queueOption);
            SetEIBRCODE();
        }
        public void WriteTemporaryQueue(IBufferValue queueName, IBufferValue queueData, IField dataLength)
        {
            DBSUtil.WriteTemporaryQueue(queueName, queueData, dataLength);
            SetEIBRCODE();
        }
        public void WriteTemporaryQueue(string queueName, IBufferValue queueData, IField dataLength)
        {
            DBSUtil.WriteTemporaryQueue(queueName, queueData, dataLength);
            SetEIBRCODE();
        }
        public void WriteTemporaryQueue(IBufferValue queueName, IBufferValue queueData, int dataLength, IField queueItem, QueueOption queueOption = QueueOption.None)
        {
            DBSUtil.WriteTemporaryQueue(queueName, queueData, dataLength, queueItem, queueOption);
            SetEIBRCODE();
        }
        public void WriteTemporaryQueue(string queueName, IBufferValue queueData, int dataLength, IField queueItem, QueueOption queueOption = QueueOption.None)
        {
            DBSUtil.WriteTemporaryQueue(queueName, queueData, dataLength, queueItem, queueOption);
            SetEIBRCODE();
        }
        public void WriteTemporaryQueue(string queueName, IBufferValue queueData, int dataLength, IField queueItem, IField responseField, QueueOption queueOption = QueueOption.None)
        {
            DBSUtil.WriteTemporaryQueue(queueName, queueData, dataLength, queueItem, queueOption);
            SetEIBRCODE();
            responseField.SetValue(RESP);
        }
        public void WriteTemporaryQueue(IBufferValue queueName, IBufferValue queueData, int dataLength, IField queueItem, IField responseField, QueueOption queueOption = QueueOption.None)
        {
            DBSUtil.WriteTemporaryQueue(queueName, queueData, dataLength, queueItem, queueOption);
            SetEIBRCODE();
            responseField.SetValue(RESP);
        }
        public void WriteTemporaryQueue(IBufferValue queueName, IBufferValue queueData, int dataLength)
        {
            DBSUtil.WriteTemporaryQueue(queueName, queueData, dataLength);
            SetEIBRCODE();
        }
        public void WriteTemporaryQueue(IBufferValue queueName, IBufferValue queueData, int dataLength, IField responseField)
        {
            DBSUtil.WriteTemporaryQueue(queueName, queueData, dataLength);
            SetEIBRCODE();
            responseField.SetValue(RESP);
        }
        public void WriteTemporaryQueue(string queueName, IBufferValue queueData, int dataLength)
        {
            DBSUtil.WriteTemporaryQueue(queueName, queueData, dataLength);
            SetEIBRCODE();
        }

        public void ReadTemporaryQueue(IBufferValue queueData, IBufferValue queueName, IField dataLength, IField queueItem, QueueOption queueOption = QueueOption.None)
        {
            byte[] tempBytes;
            tempBytes = DBSUtil.ReadTemporaryQueue(queueName.BytesAsString, dataLength.AsInt(), queueItem.AsInt(), queueOption);
            if (DBSUtil.Condition == HandleCondition.NORMAL)
                queueData.Assign(tempBytes);
            SetEIBRCODE();
        }
        public void ReadTemporaryQueue(IBufferValue queueData, IBufferValue queueName, IField dataLength, IField queueItem, IField responseField, QueueOption queueOption = QueueOption.None)
        {
            byte[] tempBytes;
            tempBytes = DBSUtil.ReadTemporaryQueue(queueName.BytesAsString, dataLength.AsInt(), queueItem.AsInt(), queueOption);
            if (DBSUtil.Condition == HandleCondition.NORMAL)
                queueData.Assign(tempBytes);
            SetEIBRCODE();
            responseField.SetValue(RESP);
        }
        public void ReadTemporaryQueue(IBufferValue queueData, IBufferValue queueName, IField dataLength, int queueItem, QueueOption queueOption = QueueOption.None)
        {
            byte[] tempBytes;
            tempBytes = DBSUtil.ReadTemporaryQueue(queueName.BytesAsString, dataLength.AsInt(), queueItem, queueOption);
            if (DBSUtil.Condition == HandleCondition.NORMAL)
                queueData.Assign(tempBytes);
            SetEIBRCODE();
        }
        public void ReadTemporaryQueue(IBufferValue queueData, IBufferValue queueName, IField dataLength, int queueItem, IField responseField, QueueOption queueOption = QueueOption.None)
        {
            byte[] tempBytes;
            tempBytes = DBSUtil.ReadTemporaryQueue(queueName.BytesAsString, dataLength.AsInt(), queueItem, queueOption);
            if (DBSUtil.Condition == HandleCondition.NORMAL)
                queueData.Assign(tempBytes);
            SetEIBRCODE();
            responseField.SetValue(RESP);
        }
        public void ReadTemporaryQueue(IBufferValue queueData, IBufferValue queueName, int dataLength, IField queueItem, IField responseField, QueueOption queueOption = QueueOption.None)
        {
            byte[] tempBytes;
            tempBytes = DBSUtil.ReadTemporaryQueue(queueName.BytesAsString, dataLength, queueItem.AsInt(), queueOption);
            if (DBSUtil.Condition == HandleCondition.NORMAL)
                queueData.Assign(tempBytes);
            SetEIBRCODE();
            responseField.SetValue(RESP);
        }
        public void ReadTemporaryQueue(IBufferValue queueData, IBufferValue queueName, int dataLength, IField queueItem, IField responseField, IField responseField2, QueueOption queueOption = QueueOption.None)
        {
            byte[] tempBytes;
            tempBytes = DBSUtil.ReadTemporaryQueue(queueName.BytesAsString, dataLength, queueItem.AsInt(), queueOption);
            if (DBSUtil.Condition == HandleCondition.NORMAL)
                queueData.Assign(tempBytes);
            SetEIBRCODE();
            responseField.SetValue(RESP);
            responseField2.SetValue(RESP);
        }
        public void ReadTemporaryQueue(IBufferValue queueData, IBufferValue queueName, int dataLength, int queueItem, IField responseField, IField responseField2, QueueOption queueOption = QueueOption.None)
        {
            byte[] tempBytes;
            tempBytes = DBSUtil.ReadTemporaryQueue(queueName.BytesAsString, dataLength, queueItem, queueOption);
            if (DBSUtil.Condition == HandleCondition.NORMAL)
                queueData.Assign(tempBytes);
            SetEIBRCODE();
            responseField.SetValue(RESP);
            responseField2.SetValue(RESP);
        }
        public void ReadTemporaryQueue(IBufferValue queueData, IBufferValue queueName, IField dataLength, QueueOption queueOption = QueueOption.None)
        {
            byte[] tempBytes;
            tempBytes = DBSUtil.ReadTemporaryQueue(queueName.BytesAsString, dataLength.AsInt(), queueOption);
            if (DBSUtil.Condition == HandleCondition.NORMAL)
                queueData.Assign(tempBytes);
            SetEIBRCODE();
        }
        public void ReadTemporaryQueue(IBufferValue queueData, string queueName, IField dataLength, IField responseField, QueueOption queueOption = QueueOption.None)
        {
            byte[] tempBytes;
            tempBytes = DBSUtil.ReadTemporaryQueue(queueName, dataLength.AsInt(), queueOption);
            if (DBSUtil.Condition == HandleCondition.NORMAL)
                queueData.Assign(tempBytes);
            SetEIBRCODE();
            responseField.SetValue(RESP);
        }
        public void ReadTemporaryQueue(IBufferValue queueData, string queueName, int dataLength, int queueItem, IField responseField, QueueOption queueOption = QueueOption.None)
        {
            byte[] tempBytes;
            tempBytes = DBSUtil.ReadTemporaryQueue(queueName, dataLength, queueItem, queueOption);
            if (DBSUtil.Condition == HandleCondition.NORMAL)
                queueData.Assign(tempBytes);
            SetEIBRCODE();
            responseField.SetValue(RESP);
        }
        public void DeleteTemporaryQueue(IBufferValue queueName)
        {
            DBSUtil.DeleteTemporaryQueue(queueName);
            SetEIBRCODE();
        }
        public void DeleteTemporaryQueue(string queueName)
        {
            DBSUtil.DeleteTemporaryQueue(queueName);
            SetEIBRCODE();
        }

        public void WriteTransientQueue(IBufferValue queueName, IBufferValue queueData, IField dataLength)
        {
            DBSUtil.WriteTransientQueue(queueName, queueData, dataLength.AsInt());
            SetEIBRCODE();
        }

        public void WriteTransientQueue(IBufferValue queueName, IBufferValue queueData, int dataLength, IField responseField)
        {
            DBSUtil.WriteTransientQueue(queueName, queueData, dataLength);
            SetEIBRCODE();
            responseField.SetValue(RESP);
        }

        public void WriteTransientQueue(string queueName, IBufferValue queueData, IField dataLength)
        {
            DBSUtil.WriteTransientQueue(queueName, queueData, dataLength.AsInt());
            SetEIBRCODE();
        }

        public void WriteTransientQueue(string queueName, IBufferValue queueData)
        {
            DBSUtil.WriteTransientQueue(queueName, queueData);
            SetEIBRCODE();
        }


        public void ReadTransientQueue(IBufferValue bufferValue, IBufferValue queueName, int dataLength)
        {
            byte[] tempBytes;
            tempBytes = DBSUtil.ReadTransientQueue(queueName, dataLength);
            if (DBSUtil.Condition == HandleCondition.NORMAL)
                bufferValue.Assign(tempBytes);
            SetEIBRCODE();
        }
        public void ReadTransientQueue(IBufferValue bufferValue, IBufferValue queueName)
        {
            byte[] tempBytes;
            tempBytes = DBSUtil.ReadTransientQueue(queueName, bufferValue.BytesAsString.Length);
            if (DBSUtil.Condition == HandleCondition.NORMAL)
                bufferValue.Assign(tempBytes);
            SetEIBRCODE();
        }

        public void ReadTransientQueue(IBufferValue bufferValue, IBufferValue queueName, IField dataLength, IField responseField)
        {
            byte[] tempBytes;
            tempBytes = DBSUtil.ReadTransientQueue(queueName, dataLength.AsInt());
            if (DBSUtil.Condition == HandleCondition.NORMAL)
                bufferValue.Assign(tempBytes);
            SetEIBRCODE();
            responseField.SetValue(RESP);
        }
        public void ReadTransientQueue(IBufferValue bufferValue, IBufferValue queueName, IField responseField)
        {
            byte[] tempBytes;
            tempBytes = DBSUtil.ReadTransientQueue(queueName, bufferValue.BytesAsString.Length);
            if (DBSUtil.Condition == HandleCondition.NORMAL)
                bufferValue.Assign(tempBytes);
            SetEIBRCODE();
            responseField.SetValue(RESP);
        }

        public string GetUserID()
        {
            if (ServiceControl.UserID == null || ServiceControl.UserID == "")
                return string.Empty;
            else
                return ServiceControl.UserID; ;
        }
        public void SetUserID(string userID)
        {
            ServiceControl.UserID = userID;
        }
        public void SetUserID(IField userID)
        {
            ServiceControl.UserID = userID.AsString().Trim();
        }
        #endregion


        #endregion

        #region Private Methods
        private void SubmitToBatch(string submitJCL)
        {
            string BatchEnvironment = string.Empty;

            if (!(String.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("BatchEnvironment"))))
            {
                BatchEnvironment = ConfigSettings.GetAppSettingsString("BatchEnvironment");
            }
            if (string.IsNullOrEmpty(BatchEnvironment) || BatchEnvironment.ToUpper() == "EAVJES")
            {
                #region EavJes Job Submission
                #region Send file to EavJes RJE folder
                string submissionFolder; string jesConnection; string jesTarget;
                if (!(String.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("RJEFolderPath"))))
                {
                    submissionFolder = ConfigSettings.GetAppSettingsString("RJEFolderPath");
                }
                else
                {
                    return;
                }

                string fileName = string.Concat(submissionFolder, @"\JCL", DateTime.Now.ToString("yyyyMMddHHmmssff"), ".txt");

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                // Now create the file
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    Byte[] jclBytes = new UTF8Encoding(true).GetBytes(submitJCL);
                    fs.Write(jclBytes, 0, jclBytes.Length);
                }
                #endregion

                #region Insert Row in Jes Execition table
                if (!(String.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("RJETarget"))))
                {
                    jesTarget = ConfigSettings.GetAppSettingsString("RJETarget");
                }
                else
                {
                    return;
                }

                SqlConnection sqlConnection;
                try
                {
                    jesConnection = ConfigSettings.GetConnectionStrings("JesConnectionString", "connectionString");
                    string commandText = string.Format("INSERT INTO rte_execution_request(target, path, type, target_name) VALUES ('{0}', '{1}', 'R', '{0}') ", jesTarget, fileName);

                    sqlConnection = new SqlConnection(jesConnection);
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand(commandText, sqlConnection);

                    int rowsInserted = sqlCommand.ExecuteNonQuery();
                    sqlConnection.Close();
                }
                catch (Exception ex)
                {
                    throw new ApplicationControlException(" Online Job Subission Problem: " + ex.Message);
                }
                finally
                {

                }
                #endregion
                #endregion
            }
            else if (BatchEnvironment.ToUpper() == "ZAMS")
            {
                #region ZAMS Job Submission
                SqlConnection sqlConnection;
                try
                {
                    string batchConnection = ConfigSettings.GetConnectionStrings("BatchConnectionString", "connectionString");
                    string commandText = string.Format("ZAMS_RJE_SUBMISSION");

                    sqlConnection = new SqlConnection(batchConnection);
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand(commandText, sqlConnection);
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@deck", submitJCL);

                    int rowsInserted = sqlCommand.ExecuteNonQuery();
                    sqlConnection.Close();
                }
                catch (Exception ex)
                {
                    throw new ApplicationControlException(" Online Job Subission Problem: " + ex.Message);
                }
                finally
                {

                }
                #endregion
            }

        }

        private void SetEIBRCODE()
        {
            string hexEIBRCODE = GetStringFromBytes(new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00 });
            switch (DBSUtil.Condition)
            {
                case HandleCondition.NORMAL:
                    hexEIBRCODE = GetStringFromBytes(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    RESP.Assign(0);
                    EIBRESP.Assign(0);
                    break;
                // End data, Item Error
                case HandleCondition.ENDDATA:
                case HandleCondition.QZERO:
                case HandleCondition.ITEMERR:
                    RESP.Assign(23);
                    EIBRESP.Assign(23);
                    hexEIBRCODE = GetStringFromBytes(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;
                // End of file
                case HandleCondition.ENDFILE:
                    hexEIBRCODE = GetStringFromBytes(new byte[] { 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;
                // Record not found
                case HandleCondition.NOTFND:
                    hexEIBRCODE = GetStringFromBytes(new byte[] { 0x81, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;
                // Not Open
                case HandleCondition.NOTOPEN:
                    hexEIBRCODE = GetStringFromBytes(new byte[] { 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;
                // INVREQ
                case HandleCondition.INVREQ:
                    hexEIBRCODE = GetStringFromBytes(new byte[] { 0x10, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;
                // LENERR
                case HandleCondition.LENGERR:
                    hexEIBRCODE = GetStringFromBytes(new byte[] { 16, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;
                // QUE ID error
                case HandleCondition.QIDERR:
                    hexEIBRCODE = GetStringFromBytes(new byte[] { 0x2C, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    RESP.Assign(44);
                    EIBRESP.Assign(44);
                    break;
                default: break;
            }

            EIBRCODE.SetValue(hexEIBRCODE);

            ;
        }

        private string GetStringFromBytes(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public void SendControl(string passedData)
        {
            int indx = passedData.IndexOf(',');
            if (indx >= 0)
            {
                DBSUtil.ServiceThreadShareData.Clear();
                // Add Service control
                DBSUtil.ServiceThreadShareData.Add(new CICSServiceItemKey(string.Concat("EntryPoint:", passedData.Substring(0, indx)), string.Empty, string.Empty, string.Empty));
                DBSUtil.ServiceThreadShareData.Add(new CICSServiceItemControl("StartData", passedData.Substring(indx), false, 25, "BRIGHT"));
                StartTransaction(passedData.Substring(0, indx), 0, null, null, null, EIBTRMID);
            }
            else
                throw new ApplicationControlException(string.Format("Invalid XCLR Format: {0}", passedData));
        }

        private string CatchError(Exception ex)
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
            while (ex.InnerException != null)
            {
                Exception exi = ex.InnerException;
                errMessage.AppendLine("== Inner Exception Message ==");
                errMessage.AppendLine(exi.Message);
                errMessage.AppendLine("******************************************************************");
                errMessage.AppendLine("== StackTrace ==");
                errMessage.AppendLine(exi.StackTrace);
                errMessage.AppendLine("******************************************************************");

                ex = ex.InnerException;
            }

            return errMessage.ToString();
        }
        #endregion
    }
}
