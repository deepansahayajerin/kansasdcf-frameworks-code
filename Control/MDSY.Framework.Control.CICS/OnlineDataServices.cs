using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using MDSY.Framework.Data.Vsam;
using MDSY.Framework.Core;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer;
using MDSY.Framework.Interfaces;
using System.Xml;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Control.CICS
{
    public class OnlineDataServices : IDataServices
    {
        #region Private variables
        private OnlineControl _control;
        private DbProviderFactory dbFactory { get; set; }
        private IDictionary<string, VsamDalBase> dalCache = new Dictionary<string, VsamDalBase>();
        private static IDictionary<string, string> OnlineFiles = new Dictionary<string, string>();
        private static IDictionary<string, string> OnlineSegments = new Dictionary<string, string>();
        private static IDictionary<string, string> OnlineAIX = new Dictionary<string, string>();
        #endregion

        public OnlineDataServices(OnlineControl control)
        {
            _control = control;
            SetNewDBConnection();
            SetOnlineFiles();
        }

        public DataTable CurrentDataTable { get; set; }
        #region Public Methods
        /// <summary>
        /// Read record into buffer based on logical key.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="targetBuffer"></param>
        /// <param name="targetBufferLength"></param>
        /// <param name="recordKey"></param>
        /// <param name="readOptions"></param>
        public void Read(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, params ReadOption[] readOptions)
        {
            Read(fileName, targetBuffer, targetBufferLength, recordKey, recordKey.DisplayValue.Length, null, readOptions);
        }
        /// <summary>
        /// Read record into buffer based on logical key.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="targetBuffer"></param>
        /// <param name="targetBufferLength"></param>
        /// <param name="recordKey"></param>
        /// <param name="recordKeyLength"></param>
        /// <param name="readOptions"></param>
        public void Read(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, int recordKeyLength, params ReadOption[] readOptions)
        {
            Read(fileName, targetBuffer, targetBufferLength, recordKey, recordKeyLength, null, readOptions);
        }
        /// <summary>
        /// Read record into buffer based on logical key.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="targetBuffer"></param>
        /// <param name="targetBufferLength"></param>
        /// <param name="recordKey"></param>
        /// <param name="respCode"></param>
        /// <param name="readOptions"></param>
        public void Read(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, IBufferValue respCode, params ReadOption[] readOptions)
        {
            Read(fileName, targetBuffer, targetBufferLength, recordKey, recordKey.DisplayValue.Length, respCode, readOptions);
        }
        /// <summary>
        /// Read record into buffer based on logical key.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="targetBuffer"></param>
        /// <param name="recordKey"></param>
        /// <param name="respCode"></param>
        /// <param name="readOptions"></param>
        public void Read(string fileName, IBufferValue targetBuffer, IBufferValue recordKey, IBufferValue respCode, params ReadOption[] readOptions)
        {
            Read(fileName, targetBuffer, targetBuffer.BytesAsString.Length, recordKey, recordKey.DisplayValue.Length, respCode, readOptions);
        }
        /// <summary>
        /// Read record into buffer based on logical key.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="targetBuffer"></param>
        /// <param name="recordKey"></param>
        /// <param name="respCode"></param>
        /// <param name="readOptions"></param>
        public void Read(string fileName, IBufferValue targetBuffer, IBufferValue recordKey, params ReadOption[] readOptions)
        {
            Read(fileName, targetBuffer, targetBuffer.BytesAsString.Length, recordKey, recordKey.DisplayValue.Length, null, readOptions);
        }
        /// <summary>
        /// Read record into buffer based on logical key.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="targetBuffer"></param>
        /// <param name="targetBufferLength"></param>
        /// <param name="recordKey"></param>
        /// <param name="RecordKeyLength"></param>
        /// <param name="respCode"></param>
        /// <param name="readOptions"></param>
        public void Read(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, int RecordKeyLength, IBufferValue respCode, params ReadOption[] readOptions)
        {
            bool isPartialSearch = false;
            bool isSetPointer = false;
            //bool isSetRowLock = false;
            if (readOptions != null)
            {
                foreach (ReadOption ro in readOptions)
                {
                    if (ro == ReadOption.PartialSearch)
                        isPartialSearch = true;
                    else if (ro == ReadOption.SetPointer)
                        isSetPointer = true;
                    //else if (ro == ReadOption.Update)
                    //    isSetRowLock = true;
                }
            }

            int retCode = 0;
            try
            {
                VsamDalBase dalInstance = GetDalInstance(fileName);
                CheckTransaction(dalInstance);
                if (isPartialSearch)
                {
                    retCode = dalInstance.ReadPartialKey(new VsamKey(recordKey), RecordKeyLength);
                }
                else
                {
                    retCode = dalInstance.ReadByKey(new VsamKey(recordKey));
                }
                if (retCode == 0)
                {
                    if (isSetPointer)
                    {
                        IField ptrField = (IField)targetBuffer;
                        ptrField.SetValue(dalInstance.GetRecord().GetBufferAddressKey());
                    }
                    else
                    {
                        targetBuffer.AssignFrom(dalInstance.AsBytes);
                        if (targetBuffer is IGroup)
                        {
                            IGroup target = (IGroup)targetBuffer;
                            foreach (IBufferElement element in target.Elements)
                            {
                                element.Parent = target;
                                element.Record = target.Record;

                                if (element is IBufferValue)
                                {
                                    (element as IBufferValue).Buffer = target.Buffer;
                                }

                                if (element is IElementCollection)
                                {
                                    (element as IElementCollection).AssignDataBufferRecursive(target.Buffer);
                                }

                                IBufferElement otherElement = element.Record.StructureElementByName(element.Name);
                                otherElement = element;
                            }
                        }

                    }
                    if (dalInstance.VsamDalDataTable.Rows.Count == 0)
                    {
                        retCode = 13;
                    }
                    else
                    {
                        if (dalInstance.IsBinaryKey)
                            recordKey.SetValue(dalInstance.LastKey.BinaryKey);
                        else
                            recordKey.SetValue(dalInstance.LastKey.StringKey);
                    }
                }
                else
                {
                    if (isSetPointer)
                    {
                        IField ptrField = (IField)targetBuffer;
                        ptrField.SetValue(-1);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("connection"))
                {
                    if (ex.Message.ToUpper().Contains("TRANSACTION") || ex.Message.ToUpper().Contains("LOCKED") || ex.Message.ToUpper().Contains("TIMEOUT"))
                    {
                        retCode = 20;
                    }
                }
                else if (ex.Message.Contains("No OnlineFile entry exists for"))
                {
                    retCode = 13;
                }
                else
                {
                    SimpleLogging.LogMandatoryMessageToFile(string.Concat("Data services Read Exception: ", ex.Message, ex.StackTrace));
                    retCode = 05;
                }
            }
            SetHandleCondition(retCode);
            if (!object.Equals(respCode, null))
            {
                respCode.Assign(_control.RESP.AsInt());
            }
        }

        /// <summary>
        /// Start read of multiple records based on logical key
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="recordKey"></param>
        /// <param name="RecordKeyLength"></param>
        /// <param name="respCode"></param>
        public void StartRead(string fileName, IBufferValue recordKey, int RecordKeyLength, IBufferValue respCode = null)
        {
            VsamDalBase dalInstance = GetDalInstance(fileName);
            CheckTransaction(dalInstance);
            int retCode = 0;
            try
            {

                retCode = dalInstance.StartRead(new VsamKey(recordKey));
                if (retCode == 0 && dalInstance.VsamDalDataTable.Rows.Count == 0)
                {
                    retCode = 13;
                }
            }
            catch
            {
                retCode = 84;
                throw;
            }
            SetHandleCondition(retCode);
            if (!object.Equals(respCode, null))
            {
                respCode.Assign(_control.RESP.AsInt());
            }
        }
        /// <summary>
        /// Start read of multiple records based on logical key
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="recordKey"></param>
        /// <param name="respCode"></param>
        /// <param name="readOptions"></param>
        public void StartRead(string fileName, IBufferValue recordKey, IBufferValue respCode, params ReadOption[] readOptions)
        {

            VsamDalBase dalInstance = GetDalInstance(fileName);
            CheckTransaction(dalInstance);
            int respCodeInt = 0;
            try
            {
                respCode.Assign(dalInstance.StartRead(new VsamKey(recordKey), readOptions));
                respCodeInt = int.Parse(respCode.DisplayValue);
                if (respCodeInt == 0 && dalInstance.VsamDalDataTable.Rows.Count == 0)
                {
                    respCode.Assign(13);
                }
            }
            catch (Exception ex)
            {
                respCode.Assign(84);
                SimpleLogging.LogMandatoryMessageToFile(String.Concat("SQl Exception:", ex.Message));
            }
            SetHandleCondition(respCodeInt);
        }
        /// <summary>
        /// Start read of multiple records based on logical key
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="recordKey"></param>
        /// <param name="respCode"></param>
        /// <param name="readOptions"></param>
        public void StartRead(string fileName, IBufferValue recordKey, params ReadOption[] readOptions)
        {
            StartRead(fileName, new VsamKey(recordKey), readOptions);
        }

        private void StartRead(string fileName, VsamKey recordKey, params ReadOption[] readOptions)
        {

            VsamDalBase dalInstance = GetDalInstance(fileName);
            CheckTransaction(dalInstance);
            int respCodeInt = 0;
            try
            {
                dalInstance.StartRead(recordKey, readOptions);

                if (respCodeInt == 0 && dalInstance.VsamDalDataTable.Rows.Count == 0)
                {
                    respCodeInt = 23;
                }
            }
            catch
            {

                throw;
            }
            SetHandleCondition(respCodeInt);
        }
        public void StartRead(string fileName, IBufferValue recordKey, int recordKeyLength, params ReadOption[] readOptions)
        {
            StartRead(fileName, new VsamKey(recordKey, recordKeyLength), readOptions);
        }

        /// <summary>
        /// Read next record in key sequence
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="targetBuffer"></param>
        /// <param name="targetBufferLength"></param>
        /// <param name="recordKey"></param>
        /// <param name="RecordKeyLength"></param>
        /// <param name="respCode"></param>
        public void ReadNext(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, int RecordKeyLength, IBufferValue respCode = null, params ReadOption[] readOptions)
        {
            VsamDalBase dalInstance = GetDalInstance(fileName);
            CheckTransaction(dalInstance);
            int retCode = dalInstance.ReadNext(new VsamKey(recordKey));
            bool isSetPointer = false;
            if (readOptions != null)
            {
                foreach (ReadOption ro in readOptions)
                {
                    if (ro == ReadOption.SetPointer) { isSetPointer = true; }
                }
            }
            if (retCode == 0)
            {
                if (isSetPointer)
                {
                    IField ptrField = (IField)targetBuffer;
                    ptrField.SetValue(dalInstance.GetRecord().GetBufferAddressKey());
                }
                else
                {
                    targetBuffer.AssignFrom(dalInstance.AsBytes);
                }
                if (dalInstance.UseAlternateIndex)
                {
                    if (dalInstance.AlternateIndexes[dalInstance.AlternateIndex].IsbinaryKey)
                        recordKey.SetValue(dalInstance.AlternateIndexes[dalInstance.AlternateIndex].LastKey.BinaryKey);
                    else
                        recordKey.SetValue(dalInstance.AlternateIndexes[dalInstance.AlternateIndex].LastKey.StringKey);
                }
                else
                {
                    if (dalInstance.IsBinaryKey)
                        recordKey.SetValue(dalInstance.LastKey.BinaryKey);
                    else
                        recordKey.SetValue(dalInstance.LastKey.StringKey);
                }
            }
            SetHandleCondition(retCode);
            if (!object.Equals(respCode, null))
            {
                respCode.Assign(_control.RESP.AsInt());
            }
        }
        public void ReadNext(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, IBufferValue respCode = null, params ReadOption[] readOptions)
        {
            ReadNext(fileName, targetBuffer, targetBufferLength, recordKey, recordKey.DisplayValue.Length, respCode, readOptions);
        }
        public void ReadNext(string fileName, IBufferValue targetBuffer, IBufferValue recordKey, IBufferValue respCode = null, params ReadOption[] readOptions)
        {
            ReadNext(fileName, targetBuffer, targetBuffer.BytesAsString.Length, recordKey, recordKey.DisplayValue.Length, respCode, readOptions);
        }

        /// <summary>
        /// Read previous record
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="targetBuffer"></param>
        /// <param name="targetBufferLength"></param>
        /// <param name="recordKey"></param>
        /// <param name="RecordKeyLength"></param>
        /// <param name="respCode"></param>
        public void ReadPrev(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, int RecordKeyLength, IBufferValue respCode = null, params ReadOption[] readOptions)
        {
            VsamDalBase dalInstance = GetDalInstance(fileName);
            CheckTransaction(dalInstance);
            int retCode = dalInstance.ReadPrev(new VsamKey(recordKey));
            bool isSetPointer = false;
            if (readOptions != null)
            {
                foreach (ReadOption ro in readOptions)
                {
                    if (ro == ReadOption.SetPointer) { isSetPointer = true; }
                }
            }

            if (retCode == 0)
            {
                if (isSetPointer)
                {
                    IField ptrField = (IField)targetBuffer;
                    ptrField.SetValue(dalInstance.GetRecord().GetBufferAddressKey());
                }
                else
                {
                    targetBuffer.AssignFrom(dalInstance.AsBytes);
                }

                if (dalInstance.UseAlternateIndex)
                {
                    if (dalInstance.AlternateIndexes[dalInstance.AlternateIndex].IsbinaryKey)
                        recordKey.SetValue(dalInstance.AlternateIndexes[dalInstance.AlternateIndex].LastKey.BinaryKey);
                    else
                        recordKey.SetValue(dalInstance.AlternateIndexes[dalInstance.AlternateIndex].LastKey.StringKey);
                }
                else
                {
                    if (dalInstance.IsBinaryKey)
                        recordKey.SetValue(dalInstance.LastKey.BinaryKey);
                    else
                        recordKey.SetValue(dalInstance.LastKey.StringKey);
                }

            }

            SetHandleCondition(retCode);
            if (!object.Equals(respCode, null))
            {
                respCode.Assign(_control.RESP.AsInt());
            }
        }
        public void ReadPrev(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, IBufferValue respCode = null, params ReadOption[] readOptions)
        {
            ReadPrev(fileName, targetBuffer, targetBufferLength, recordKey, recordKey.DisplayValue.Length, respCode, readOptions);
        }
        public void ReadPrev(string fileName, IBufferValue targetBuffer, IBufferValue recordKey, IBufferValue respCode = null, params ReadOption[] readOptions)
        {
            ReadPrev(fileName, targetBuffer, targetBuffer.BytesAsString.Length, recordKey, recordKey.DisplayValue.Length, respCode, readOptions);
        }

        /// <summary>
        /// Write new record
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sourceBuffer"></param>
        /// <param name="sourceBufferLength"></param>
        /// <param name="recordKey"></param>
        /// <param name="RecordKeyLength"></param>
        /// <param name="respCode"></param>
        public void Write(string fileName, IBufferValue sourceBuffer, int sourceBufferLength, IBufferValue recordKey, int RecordKeyLength, IBufferValue respCode = null)
        {
            VsamDalBase dalInstance = GetDalInstance(fileName);
            SetTransaction(dalInstance);
            dalInstance.AssignFrom(sourceBuffer);
            int returnCode = dalInstance.Write();
            SetHandleCondition(returnCode);
            if (!object.Equals(respCode, null))
            {
                respCode.Assign(_control.RESP.AsInt());
            }
        }
        public void Write(string fileName, IBufferValue sourceBuffer, int sourceBufferLength, IBufferValue recordKey, IBufferValue respCode = null)
        {
            Write(fileName, sourceBuffer, sourceBufferLength, recordKey, recordKey.DisplayValue.Length, respCode);
        }
        public void Write(string fileName, IBufferValue sourceBuffer, IBufferValue recordKey, IBufferValue respCode = null)
        {
            Write(fileName, sourceBuffer, sourceBuffer.BytesAsString.Length, recordKey, recordKey.DisplayValue.Length, respCode);
        }

        /// <summary>
        /// Rewrite (update) existing record based on logical key
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sourceBuffer"></param>
        /// <param name="sourceBufferLength"></param>
        /// <param name="respCode"></param>
        public void Rewrite(string fileName, IBufferValue sourceBuffer, int sourceBufferLength, IBufferValue respCode = null)
        {
            VsamDalBase dalInstance = GetDalInstance(fileName);
            SetTransaction(dalInstance);
            dalInstance.AssignFrom(sourceBuffer);
            int returnCode = dalInstance.ReWrite();
            SetHandleCondition(returnCode);
            if (!object.Equals(respCode, null))
            {
                respCode.Assign(_control.RESP.AsInt());
            }
        }
        public void Rewrite(string fileName, IBufferValue sourceBuffer, IBufferValue respCode = null)
        {
            Rewrite(fileName, sourceBuffer, sourceBuffer.BytesAsString.Length, respCode);
        }

        /// <summary>
        /// Delete record based on logical key
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="recordKey"></param>
        /// <param name="RecordKeyLength"></param>
        /// <param name="respCode"></param>
        public void Delete(string fileName, IBufferValue recordKey, int RecordKeyLength, IBufferValue respCode = null)
        {
            VsamDalBase dalInstance = GetDalInstance(fileName);
            SetTransaction(dalInstance);
            int returnCode = dalInstance.Delete(new VsamKey(recordKey));
            SetHandleCondition(returnCode);
            if (!object.Equals(respCode, null))
            {
                respCode.Assign(_control.RESP.AsInt());
            }
        }
        public void Delete(string fileName, IBufferValue recordKey, IBufferValue respCode = null)
        {
            Delete(fileName, recordKey, recordKey.DisplayValue.Length, respCode);
        }

        public void Delete(string fileName, IBufferValue respCode = null)
        {
            VsamDalBase dalInstance = GetDalInstance(fileName);
            SetTransaction(dalInstance);
            int returnCode = dalInstance.Delete();
            SetHandleCondition(returnCode);
            if (!object.Equals(respCode, null))
            {
                respCode.Assign(_control.RESP.AsInt());
            }
        }

        public void Unlock(string fileName)
        {

        }

        /// <summary>
        /// End read on multiple records
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="respCode"></param>
        public void EndRead(string fileName, IBufferValue respCode = null)
        {
            VsamDalBase dalInstance = GetDalInstance(fileName);
            if (dalInstance.VsamDalDataTable != null)
                dalInstance.VsamDalDataTable.Clear();
            dalInstance.DataTableCurrentRow = -1;
            if (!object.Equals(respCode, null))
            {
                respCode.Assign(0);
            }
            else
            {
                SetHandleCondition(0);
            }

        }

        public void Rollback()
        {
            try
            {

                if (_control.AppTransaction != null)
                {
                    _control.AppTransaction.Rollback();
                    SimpleLogging.LogMandatoryMessageToFile("****  Rollback() -  Rolledback Transaction: " + (_control.AppTransaction == null ? 0 : _control.AppTransaction.GetHashCode()) + " ****\r\n");
                }
            }
            catch
            {
                //Write Log rollback error
            }
            finally
            {
                if (_control.AppTransaction != null)
                {
                    _control.AppTransaction.Dispose();
                    _control.AppTransaction = null;
                }
            }

        }

        public void SavePoint()
        {
            try
            {
                if (_control.AppTransaction != null && _control.AppTransaction.Connection != null)
                {
                    SimpleLogging.LogMandatoryMessageToFile("*** Commit Transaction: " + (_control.AppTransaction == null ? 0 : _control.AppTransaction.GetHashCode()));
                    _control.AppTransaction.Commit();
                    System.Threading.Thread.Sleep(250);
                    _control.AppConnection.Close();
                    if (_control.AppConnection.State == ConnectionState.Closed)
                        _control.AppConnection.Open();
                    _control.AppTransaction = _control.AppConnection.BeginTransaction();
                    SimpleLogging.LogMandatoryMessageToFile("*** Begin Transaction: " + (_control.AppTransaction == null ? 0 : _control.AppTransaction.GetHashCode()));
                }
            }
            catch
            {
                //Write Log syncpoint error
                _control.AppTransaction.Dispose();
                _control.AppTransaction = null;
            }
        }

        /// <summary>
        /// Close file/database connect
        /// </summary>
        public void CloseConnection()
        {
            if (_control.isMapWaitingSend || ServiceControl.CurrentException != null)
            {
                if (_control.AppTransaction != null)
                {
                    if (_control.AppTransaction.Connection != null)
                    {
                        if (ServiceControl.CurrentException == null)
                        {
                            SimpleLogging.LogMandatoryMessageToFile("*** Commit Transaction: " + (_control.AppTransaction == null ? 0 : _control.AppTransaction.GetHashCode()));
                            _control.AppTransaction.Commit();
                            System.Threading.Thread.Sleep(250);
                        }
                        else
                            _control.AppTransaction.Rollback();
                    }
                    _control.AppTransaction.Dispose();
                    _control.AppTransaction = null;
                }
                if (_control.AppConnection.State == ConnectionState.Open)
                    _control.AppConnection.Close();
            }
        }

        /// <summary>
        /// Forces transaction and connection to be closed
        /// </summary>
        public void ForceDbClose()
        {

            if (_control.AppTransaction != null)
            {
                if (_control.AppTransaction.Connection != null)
                {
                    SimpleLogging.LogMandatoryMessageToFile("*** Commit Transaction: " + (_control.AppTransaction == null ? 0 : _control.AppTransaction.GetHashCode()));
                    _control.AppTransaction.Commit();
                    System.Threading.Thread.Sleep(250);
                }
                _control.AppTransaction.Dispose();
                _control.AppTransaction = null;
            }

            if (_control.AppConnection.State == ConnectionState.Open)
                _control.AppConnection.Close();
        }

        public void ExecuteSqlQuery(string sqlString, params Object[] parms)
        {
            throw new NotImplementedException("ExecuteSqlQuery not yet implemented");
        }

        public void ExecuteSql(string sqlString, params Object[] parms)
        {
            throw new NotImplementedException("ExecuteSqlQuery not yet implemented");
        }

        public string GetSqlca()
        {
            throw new NotImplementedException("GetSqlca not yet implemented");
        }
        public int GetSqlCode()
        {
            throw new NotImplementedException("GetSqlCode not yet implemented");
        }
        public void OpenConnection()
        {
            throw new NotImplementedException("Open Connection not yet implemented");
        }
        public void Commit()
        {
            SavePoint();
        }

        public int GetNextSequence(string fileForSequence)
        {
            string connString = ConfigSettings.GetConnectionStrings("DataConnectionString", "connectionString");
            string nextSequence = "";
            using (SqlConnection sqlConnection = new SqlConnection(connString))
            {
                try
                {
                    sqlConnection.Open();
                    SqlCommand cmd = sqlConnection.CreateCommand();
                    nextSequence = "StoredProc_GetNext" + fileForSequence;
                    cmd.CommandText = nextSequence;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    var result = cmd.ExecuteScalar();
                    sqlConnection.Close();
                    return Convert.ToInt32(result);
                }
                catch (Exception ex)
                {
                    throw new ApplicationControlException(" GetNextSequence Stored Proc Problem: " + ex.Message + "  " + ex.StackTrace);
                }
            }
        }

        public int GetCurrentSequence(string fileForSequence)
        {
            string connString = ConfigSettings.GetConnectionStrings("DataConnectionString", "connectionString");
            string nextSequence = "";
            using (SqlConnection sqlConnection = new SqlConnection(connString))
            {
                try
                {
                    sqlConnection.Open();
                    SqlCommand cmd = sqlConnection.CreateCommand();
                    nextSequence = "StoredProc_GetCurrent" + fileForSequence;
                    cmd.CommandText = nextSequence;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    var result = cmd.ExecuteScalar();
                    sqlConnection.Close();
                    return Convert.ToInt32(result);
                }
                catch (Exception ex)
                {
                    throw new ApplicationControlException(" GetCurrentSequence Stored Proc Problem: " + ex.Message + "  " + ex.StackTrace);
                }
            }
        }
        #endregion

        #region Private methods
        private VsamDalBase GetDalInstance(string dalName)
        {
            if (OnlineSegments.ContainsKey(dalName.Trim()))
            {
                string dalSegment = OnlineSegments[dalName.Trim()];
                if (!dalCache.ContainsKey(dalName.Trim()))
                {
                    try
                    {
                        Type dalType = DBSUtil.GetDALType(string.Concat("DAL_", dalSegment));
                        if (dalType == null)
                            throw new DataAccessLayerException("No data access layer program exists for DAL_" + dalSegment);

                        VsamDalBase dalInstance = (VsamDalBase)Activator.CreateInstance(dalType, _control.AppConnection);
                        if (OnlineFiles[dalName.Trim()] != string.Empty)
                        {
                            dalInstance.TableName = OnlineFiles[dalName.Trim()];
                        }
                        if (OnlineAIX.ContainsKey(dalName.Trim()))
                        {
                            dalInstance.AlternateIndex = OnlineAIX[dalName.Trim()];
                            dalInstance.UseAlternateIndex = true;
                        }
                        dalCache.Add(dalName.Trim(), dalInstance);
                    }
                    catch (Exception exc)
                    {
                        SimpleLogging.LogMandatoryMessageToFile(String.Concat("There was a problem loading data access layer program DAL_" + dalSegment, exc.StackTrace));
                        throw new DataAccessLayerException("There was a problem loading data access layer program DAL_" + dalSegment, exc);
                    }
                }
            }
            else
            {
                throw new DataAccessLayerException("No OnlineFile entry exists for " + dalName);
            }

            return dalCache[dalName.Trim()];
        }

        private void SetHandleCondition(int returnCode)
        {

            int onlineVsamCode = 0;
            // IO ERROR (catch all)
            string hexEIBRCODE = GetStringFromBytes(new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00 });
            switch (returnCode)
            {
                case 0: onlineVsamCode = 0;
                    hexEIBRCODE = GetStringFromBytes(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;
                // End of file
                case 10: onlineVsamCode = 20;
                    hexEIBRCODE = GetStringFromBytes(new byte[] { 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;
                // File Not Found
                case 13: onlineVsamCode = 12;
                    hexEIBRCODE = GetStringFromBytes(new byte[] { 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;
                // Record not found
                case 23: onlineVsamCode = 13;
                    hexEIBRCODE = GetStringFromBytes(new byte[] { 0x81, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;
                // Duplicate Key - DUPKEY/DUPREC
                case 22: onlineVsamCode = 15;
                    hexEIBRCODE = GetStringFromBytes(new byte[] { 0x84, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;
                // Not Open
                case 05: onlineVsamCode = 19;
                    hexEIBRCODE = GetStringFromBytes(new byte[] { 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;
                // Lock or Timeout error
                case 20: onlineVsamCode = 17;
                    hexEIBRCODE = GetStringFromBytes(new byte[] { 0x05, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    break;

                default: break;
            }

            DBSUtil.Condition = OnlineControl.GetCondition(onlineVsamCode);
            _control.RESP.Assign(onlineVsamCode);
            _control.EIBRESP.Assign(onlineVsamCode);
            if (onlineVsamCode == 0)
                _control.EIBRCODE.SetMinValue();
            else
                _control.EIBRCODE.SetValue(hexEIBRCODE);
        }

        private string GetStringFromBytes(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private void SetNewDBConnection()
        {
            try
            {
                if (_control.AppConnection == null)
                {
                    string providerName = ConfigSettings.GetAppSettingsString("DbConnFactory");
                    string connectionString = ConfigSettings.GetConnectionStrings("DataConnectionString", "connectionString");
                    DbProviderFactories.RegisterFactory(providerName, SqlClientFactory.Instance);
                    dbFactory = DbProviderFactories.GetFactory(providerName);

                    _control.AppConnection = dbFactory.CreateConnection();
                    _control.AppConnection.ConnectionString = connectionString;
                }
            }
            catch (Exception ex)
            {
                SimpleLogging.LogMandatoryMessageToFile(ex.Message + " " + ex.StackTrace.ToString());
                throw new DataAccessLayerException("Error with DBFactory", ex);
            }
        }

        private void SetOnlineFiles()
        {
            if (OnlineFiles.Count == 0)
            {
                string OnlineFilesXMLPath = ConfigSettings.GetAppSettingsString("OnlineFilesXmlFile");
                if (String.IsNullOrEmpty(OnlineFilesXMLPath))
                {
                    throw new DataAccessLayerException("OnlineFilesXmlFile entry does not exist in the app.config");
                }
                XmlDocument xd = new XmlDocument();

                xd.Load(OnlineFilesXMLPath);

                foreach (XmlElement programNode in xd.SelectNodes("/OnlineFiles/OnlineFile"))
                {
                    if (!OnlineFiles.ContainsKey(programNode.Attributes["FileName"].Value))
                    {
                        OnlineFiles.Add(programNode.Attributes["FileName"].Value, programNode.Attributes["TableName"].Value);
                        OnlineSegments.Add(programNode.Attributes["FileName"].Value, programNode.Attributes["SegmentName"].Value);
                        if (programNode.Attributes["AlternateIndex"].Value != null && programNode.Attributes["AlternateIndex"].Value != "")
                        {
                            OnlineAIX.Add(programNode.Attributes["FileName"].Value, programNode.Attributes["AlternateIndex"].Value);
                        }
                    }
                }
            }

        }

        private void SetTransaction(VsamDalBase dalInstance)
        {
            if (_control.AppConnection.State == ConnectionState.Closed)
                _control.AppConnection.Open();
            if (_control.AppTransaction == null || (_control.AppTransaction != null && _control.AppTransaction.Connection == null))
            {
                _control.AppTransaction = _control.AppConnection.BeginTransaction();
                SimpleLogging.LogMandatoryMessageToFile("*** Begin Transaction: " + (_control.AppTransaction == null ? 0 : _control.AppTransaction.GetHashCode()));
            }
            dalInstance.DalTransaction = _control.AppTransaction;
        }

        private void CheckTransaction(VsamDalBase dalInstance)
        {
            if (_control.AppTransaction != null)
                dalInstance.DalTransaction = _control.AppTransaction;
        }

        #endregion

    }
}
