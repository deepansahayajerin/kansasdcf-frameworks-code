using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MDSY.Framework.Core;

namespace MDSY.Framework.Control.CICS
{
    public class TSQueueInMemory : ITSQueue
    {

        private static IDictionary<string, QueueDetail> _queueArea;

        public static IDictionary<string, QueueDetail> QueueArea
        {
            get
            {
                if (_queueArea == null)
                    _queueArea = new Dictionary<string, QueueDetail>();
                return _queueArea;
            }
            set { _queueArea = value; }
        }

        public byte[] ReadTemporaryQueue(string queueName, int queueLength, int queueItem, RowPosition itemPosition, QueueOption queueOption = QueueOption.None)
        {
            DBSUtil.Condition = HandleCondition.NORMAL;
            byte[] record = null;

            if (QueueArea.Keys.Contains(queueName))
            {
                if (queueOption == QueueOption.Next)
                {
                    itemPosition = RowPosition.Next;
                }
                record = QueueArea[queueName].ReadQueueDetailRecord(itemPosition, queueItem);
                if (record == null)
                    record = new byte[queueLength];
            }
            else
                DBSUtil.Condition = HandleCondition.QIDERR;

            return record;
        }

        public int WriteTemporaryQueue(string queueName, byte[] queueData, int queueLength, int queueItem, QueueOption queueOption = QueueOption.None)
        {
            bool isRewrite = (queueOption == QueueOption.Rewrite);
            DBSUtil.Condition = HandleCondition.NORMAL;
            int currentRecordID;

            if (QueueArea.Keys.Contains(queueName))
            {
                currentRecordID = QueueArea[queueName].WriteQueueDetailRecord(queueItem, queueData, isRewrite);
            }
            else
            {
                QueueArea.Add(queueName, new QueueDetail());
                currentRecordID = QueueArea[queueName].WriteQueueDetailRecord(queueItem, queueData, isRewrite);
            }

            return currentRecordID;
        }

        public void DeleteTemporaryQueue(string queueName)
        {
            if (!QueueArea.Keys.Contains(queueName))
            {
                DBSUtil.Condition = HandleCondition.QIDERR;
                return;
            }
            QueueArea.Remove(queueName);
            DBSUtil.Condition = HandleCondition.NORMAL;
        }

    }

    public class QueueDetail
    {
        //[ThreadStatic]
        //private static int _currentItem = 0;
        private int _currentItem = 0;

        internal List<byte[]> QueueDetailRecords { get; set; }
        internal int CurrentRecordID { get; set; }

        public QueueDetail()
        {
            QueueDetailRecords = new List<byte[]>();
            CurrentRecordID = 0;
        }


        internal int WriteQueueDetailRecord(int recordID, byte[] queueData, bool isRewrite)
        {
            CurrentRecordID = recordID == 0
                ? QueueDetailRecords.Count + 1
                : recordID;

            if (QueueDetailRecords.Count < CurrentRecordID || !isRewrite)
            {
                QueueDetailRecords.Add(queueData);
                CurrentRecordID = QueueDetailRecords.Count;
            }
            else
                QueueDetailRecords[CurrentRecordID - 1] = queueData;

            return CurrentRecordID;
        }

        internal byte[] ReadQueueDetailRecord(RowPosition rowPosition, int recordID)
        {
            int readRowID = 0;

            if (recordID == 0)
            {
                if (_currentItem > 0)
                    readRowID = _currentItem - 1;
                else
                    readRowID = 0;
            }
            else
            {
                readRowID = recordID - 1;
            }

            if (rowPosition == RowPosition.Next)
            {
                readRowID++;
            }

            if (QueueDetailRecords.Count <= readRowID)
            {
                DBSUtil.Condition = HandleCondition.ITEMERR;
                return null;
            }
            else
            {
                DBSUtil.Condition = HandleCondition.NORMAL;
                _currentItem = readRowID + 1;
                return QueueDetailRecords[readRowID];
            }
        }

        internal int EraseQueueDetailRecord()
        {
            QueueDetailRecords.Clear();
            return 0;
        }
    }
}
