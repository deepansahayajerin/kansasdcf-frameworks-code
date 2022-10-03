
#region Using Directives
using System;
#endregion

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Base Object is the root of all objects in the DBS system
    /// </summary>
    [Serializable]
    public abstract class BaseObject
    {
        #region Private Members

        private readonly string _name = string.Empty;
        private readonly string _hashKey = string.Empty;
        private readonly string _description = string.Empty;

        #endregion

        #region Public Properties

        public string HashKey
        {
            get
            {
                return _hashKey;
            }

        }
        public string Name
        {
            get
            {
                return _name;
            }
        }
        public string Description
        {
            get
            {
                return _description;
            }
        }
        #endregion

        #region Protected Constructors
        public BaseObject()
        {
        }
        protected BaseObject(string name, string description, string hashKey)
        {
            //System.Diagnostics.Debug.WriteLine(" in BaseObject " + name);
            this._description = description;

            if (name == string.Empty)
            {
                throw new ArgumentException("must contain a value", "name");
            }
            else
            {
                this._name = name;
            }

            if (hashKey == string.Empty)
            {
                throw new ArgumentException("must contain a value", "hashKey");
            }
            else
            {
                this._hashKey = hashKey;
            }
        }
        #endregion

        #region Protected Methods
        protected int CheckIntegerArgument(string number, string argumentName)
        {
            try
            {
                return int.Parse(number);
            }
            catch
            {
                throw new ArgumentException("must be an integer", argumentName);
            }
        }
        #endregion
    }
}
