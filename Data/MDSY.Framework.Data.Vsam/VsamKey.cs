using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Data.Vsam
{
    public class VsamKey
    {
        public VsamKey()
        {
            VsamKeyLength = 0;
            VsamKeyOffset = 0;
        }

        public VsamKey(IBufferValue bufferValue)
        {
            BinaryKey = bufferValue.AsBytes;
            StringKey = bufferValue.BytesAsString;
            VsamKeyLength = 0;
            VsamKeyOffset = 0;
        }

        public VsamKey(IBufferValue bufferValue, int vsamKeyLength)
        {
            BinaryKey = bufferValue.AsBytes;
            StringKey = bufferValue.BytesAsString;
            VsamKeyLength = vsamKeyLength;
        }

        public VsamKey(IBufferValue bufferValue, int vsamKeyLength, int vsamKeyOffset)
        {
            BinaryKey = bufferValue.AsBytes;
            StringKey = bufferValue.BytesAsString;
            VsamKeyLength = vsamKeyLength;
            VsamKeyOffset = vsamKeyOffset;
        }

        public VsamKey(int vsamKeyLength, int vsamKeyOffset)
        {
            VsamKeyLength = vsamKeyLength;
            VsamKeyOffset = vsamKeyOffset;
        }

        public int VsamKeyLength { get; set; }
        public int VsamKeyOffset { get; set; }
        public byte[] BinaryKey { get; set; }
        public string StringKey { get; set; }
    }
}
