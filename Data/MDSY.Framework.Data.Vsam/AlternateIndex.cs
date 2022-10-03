using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Data.Vsam
{
    public class AlternateIndex
    {
        public string Name { get; set; }

        public int KeyLength { get; set; }

        public bool IsbinaryKey { get; set; }

        public bool DuplicatesAllowed { get; set; }

        public VsamKey LastKey { get; set; }

        public AlternateIndex(string name, int length, bool isBinaryKey)
        {
            Name = name;
            KeyLength = length;
            IsbinaryKey = isBinaryKey;
            LastKey = new VsamKey();
            DuplicatesAllowed = false;
        }

        public AlternateIndex(string name, int length, bool isBinaryKey, bool duplicatesAllowed)
        {
            Name = name;
            KeyLength = length;
            IsbinaryKey = isBinaryKey;
            LastKey = new VsamKey();
            DuplicatesAllowed = duplicatesAllowed;
        }
    }
}
