
#region Using Directives
using System;
#endregion

namespace MDSY.Framework.Core
{
    /// <summary>
    /// SecureBaseObject is the root of all objects requiring security checks in the DBS System
    /// </summary>
    public class SecureBaseObject : BaseObject
    {
        #region Private Members

        private readonly int _securityCode = 0;

        #endregion

        #region Public Properties
        /// <summary>
        /// 
        /// </summary>
        public int SecurityCode { get { return _securityCode; }   }

        #endregion

        #region Protected Constructors
        protected SecureBaseObject(string name, string description, string hashKey, string securityCode)
            : base(name, description, hashKey)
        {
            this._securityCode = CheckIntegerArgument(securityCode, "securityCode");
        }
        #endregion
    }
}
