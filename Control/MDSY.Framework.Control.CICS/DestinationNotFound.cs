using System;

namespace MDSY.Framework.Control.CICS
{
    public class DestinationNotFound : Exception
    {
        public DestinationNotFound(String _msg) : base(_msg) { }
    }
}
