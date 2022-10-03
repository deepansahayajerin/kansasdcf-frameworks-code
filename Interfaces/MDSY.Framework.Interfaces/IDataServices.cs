using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Data;

namespace MDSY.Framework.Interfaces
{
    public interface IDataServices
    {
        void Read(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, int RecordKeyLength, IBufferValue respCode, params ReadOption[] readOptions);
        void Read(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, IBufferValue respCode, params ReadOption[] readOptions);
        void Read(string fileName, IBufferValue targetBuffer, IBufferValue recordKey, IBufferValue respCode, params ReadOption[] readOptions);
        void Read(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, params ReadOption[] readOptions);
        void Read(string fileName, IBufferValue targetBuffer, IBufferValue recordKey, params ReadOption[] readOptions);
        void Read(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, int RecordKeyLength, params ReadOption[] readOptions);
        void StartRead(string fileName, IBufferValue recordKey, int RecordKeyLength, IBufferValue respCode = null);
        void StartRead(string fileName, IBufferValue recordKey, IBufferValue respCode = null, params ReadOption[] readOptions);
        void StartRead(string fileName, IBufferValue recordKey, params ReadOption[] readOptions);
        void StartRead(string fileName, IBufferValue recordKey, int RecordKeyLength, params ReadOption[] readOptions);
        void ReadNext(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, int RecordKeyLength, IBufferValue respCode = null, params ReadOption[] readOptions);
        void ReadNext(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, IBufferValue respCode = null, params ReadOption[] readOptions);
        void ReadNext(string fileName, IBufferValue targetBuffer, IBufferValue recordKey, IBufferValue respCode = null, params ReadOption[] readOptions);
        void ReadPrev(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, int RecordKeyLength, IBufferValue respCode = null, params ReadOption[] readOptions);
        void ReadPrev(string fileName, IBufferValue targetBuffer, int targetBufferLength, IBufferValue recordKey, IBufferValue respCode = null, params ReadOption[] readOptions);
        void ReadPrev(string fileName, IBufferValue targetBuffer, IBufferValue recordKey, IBufferValue respCode = null, params ReadOption[] readOptions);
        void Write(string fileName, IBufferValue sourceBuffer, int sourceBufferLength, IBufferValue recordKey, int RecordKeyLength, IBufferValue respCode = null);
        void Write(string fileName, IBufferValue sourceBuffer, int sourceBufferLength, IBufferValue recordKey, IBufferValue respCode = null);
        void Write(string fileName, IBufferValue sourceBuffer, IBufferValue recordKey, IBufferValue respCode = null);
        void Rewrite(string fileName, IBufferValue sourceBuffer, int sourceBufferLength, IBufferValue respCode = null);
        void Rewrite(string fileName, IBufferValue sourceBuffer, IBufferValue respCode = null);
        void Delete(string fileName, IBufferValue recordKey, int RecordKeyLength, IBufferValue respCode = null);
        void Delete(string fileName, IBufferValue recordKey, IBufferValue respCode = null);
        void Delete(string fileName, IBufferValue respCode = null);
        void Unlock(string fileName);
        void Rollback();
        void Commit();
        void SavePoint();
        void CloseConnection();
        void OpenConnection();
        void EndRead(string fileName, IBufferValue respCode = null);
        void ForceDbClose();
        void ExecuteSqlQuery(string query, params object[] parms);
        void ExecuteSql(string query, params object[] parms);
        string GetSqlca();
        int GetSqlCode();
        int GetNextSequence(string fileForSequence);
        int GetCurrentSequence(string fileForSequence);
        DataTable CurrentDataTable { get; set; }
    }
}
