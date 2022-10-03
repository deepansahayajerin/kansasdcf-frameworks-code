using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MDSY.Framework.Service.Interfaces
{
    public interface ICustomData
    {
        /// <summary>
        /// Invoked on the client side before establishing the Service Session
        /// </summary>
        /// <param name="aterasCollection"></param>
        void FillDataCollection(IDictionary<string, object> aterasCollection);

        /// <summary>
        /// Invoked on the server side at session establishment, once per thread (Service and Ateras process)
        /// </summary>
        /// <param name="aterasCollection"></param>
        void ReadDataCollection(IDictionary<string, object> aterasCollection);

    }
}
