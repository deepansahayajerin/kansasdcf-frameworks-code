using MDSY.Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Control.CICS
{
    public interface ITDQueue
    {
        byte[] ReadTransientQueue(string queueName, int queueLength);

        void DeleteTransientQueue(string queueName);

        void WriteTransientQueue(string queueName, byte[] queueData, int queueLength);

    }
}
