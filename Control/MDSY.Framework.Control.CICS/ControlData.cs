using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace MDSY.Framework.Control.CICS
{
    /// <summary>
    /// Control for the BL level to hold general information
    /// </summary>
    public class ControlData
    {
        #region private attributes
        private string _program;
        private string _control;
        #endregion

        #region public properties
        /// <summary>
        /// Gets or sets if this item can be navigated to.
        /// </summary>
        public bool IsNavigate { get; set; }
        /// <summary>
        /// Gets or sets the program name
        /// </summary>
        public string Program
        {
            get
            {
                if (_program == null)
                    _program = string.Empty;
                return _program;
            }
            set { _program = value; }
        }
        /// <summary>
        /// Gets or sets the SET CONTROL from Natural
        /// </summary>
        public string Control
        {
            get
            {
                if (_control == null)
                    _control = string.Empty;
                return _control;
            }
            set { _control = value; }
        }
        #endregion
    }
}
