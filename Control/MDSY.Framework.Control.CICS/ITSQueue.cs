using MDSY.Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Control.CICS
{
    public interface ITSQueue
    {
        byte[] ReadTemporaryQueue(string queueName, int queueLength, int queueItem, RowPosition itemPosition, QueueOption queueOption = QueueOption.None);

        void DeleteTemporaryQueue(string queueName);

        int WriteTemporaryQueue(string queueName, byte[] queueData, int queueLength, int queueItem, QueueOption queueOption = QueueOption.None);
    }
}
