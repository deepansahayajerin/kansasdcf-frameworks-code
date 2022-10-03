using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Xml.Serialization;



namespace MDSY.Framework.Control.CICS
{
    /// <summary>
    /// Represents the state of a user
    /// </summary>
    [Serializable]
    public class ProcessState
    {
        #region public members
        /// <summary>
        /// Gets the control state of the running program
        /// </summary>
        public ControlData Control {get; private set; }
        /// <summary>
        /// Gets the current connection to the database
        /// </summary>
        //public DbConversation DBConv {get; private set; }

        #endregion

        #region constructors
        public ProcessState()
        {
            Control = new ControlData();

            //DBConv = new DbConversation();         

        }
        #endregion
    }
}