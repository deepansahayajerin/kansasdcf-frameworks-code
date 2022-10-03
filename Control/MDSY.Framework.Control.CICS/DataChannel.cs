using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Control.CICS
{
    public class DataChannel
    {
        private IDictionary<string, byte[]> dataContainer = new Dictionary<string, byte[]> { };

        public DataChannel (string channelName)
        {
            ChannelName = channelName;
        }
        public string ChannelName { get; private set; }

        public byte[] GetContainer(string containerName)
        {
            if (!dataContainer.ContainsKey(containerName))
            {
                //set channelerr
                return null;
            }
            
            return dataContainer[containerName];
        }

        public void PutContainer(string containerName, byte[] containerData)
        {
            if (!dataContainer.ContainsKey(containerName))
            {
                dataContainer.Add(containerName, containerData);
            }
            else
            {
                dataContainer[containerName] = containerData;
            }
        }

        public void DeleteContainer(string containerName)
        {
            if (dataContainer.ContainsKey(containerName))
            {
                dataContainer.Remove(containerName);
            }

        }
    }
}
