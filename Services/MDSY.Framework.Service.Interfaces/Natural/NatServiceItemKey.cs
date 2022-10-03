using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDSY.Framework.Service.Interfaces
{
    [Serializable]
    [DebuggerDisplay("{Name},{KeyPressed}, {FormName}")]
    public class NatServiceItemKey : IAterasServiceItem
    {
        #region public properties
        /// <summary>
        /// Name of the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name of the UI Form
        /// </summary>
        public string FormName { get; set; }

        /// <summary>
        /// ID of the Application
        /// </summary>
        public string ApplicationID { get; set; }

        /// <summary>
        /// ADS Response
        /// </summary>
        public string ResponeName { get; set; }

        /// <summary>
        /// If this is a type KeyPress this will be the key pressed.
        /// </summary>
        public string KeyPressed { get; set; }

        /// <summary>
        /// Gets the value of any additional key that was pressed.
        /// </summary>
        public string HelperKeyPressed { get; set; }

        /// <summary>
        /// Gets the current focused Textbox.
        /// </summary>
        public string CurrentControl { get; set; }

        /// <summary>
        /// Gets the current position of cursor
        /// </summary>
        public string CurrentPosition { get; set; }

        public int CursorLine { get; set; }
        public int CursorColumn { get; set; }

        /// <summary>
        /// Gets the Alarm setting
        /// </summary>
        public bool SetAlarm { get; set; }

        /// <summary>
        /// Set Keyboard to all caps
        /// </summary>
        public bool SetCaps { get; set; }

        public string CorrectFieldStyle { get; set; }

        public string InCorrectFieldStyle { get; set; }

        public string ResponseControls { get; set; }

        public string SaveArea { get; set; }

        public string MessagePosition { get; set; }

        public bool Autoskip { get; set; }

        public bool NonConversationalMode { get; set; }

        public bool ControlTypeIsScreen { get; set; }

        public string PFKeys11 { get; set; }
        public string PFKeys12 { get; set; }
        public string PFKeys21 { get; set; }
        public string PFKeys22 { get; set; }
        #endregion

        #region constructors
        public NatServiceItemKey(string name, string key, string helperKey, string currentPosition)
        {
            Name = name;
            KeyPressed = key;
            HelperKeyPressed = helperKey;
            CurrentPosition = currentPosition;
            CurrentControl = "";
            SetAlarm = false;
        }

        public NatServiceItemKey(string name, string key, string helperKey, string currentPosition, bool setAlarm)
        {
            Name = name;
            KeyPressed = key;
            HelperKeyPressed = helperKey;
            CurrentPosition = currentPosition;
            CurrentControl = "";
            SetAlarm = setAlarm;
        }
        public NatServiceItemKey(string name, string key, string helperKey, string currentPosition, bool setAlarm, bool autoskip, string messagePosition, bool nonConversationalMode, bool controlTypeIsScreen = false)
        {
            Name = name;
            KeyPressed = key;
            HelperKeyPressed = helperKey;
            CurrentPosition = currentPosition;
            CurrentControl = "";
            SetAlarm = setAlarm;
            MessagePosition = messagePosition;
            Autoskip = autoskip;
            NonConversationalMode = nonConversationalMode;
            ControlTypeIsScreen = controlTypeIsScreen;
        }
        public NatServiceItemKey(string name, string key, string helperKey, string currentPosition, bool setAlarm, string correctFieldStyle, string incorrectFeldStyle)
        {
            Name = name;
            KeyPressed = key;
            HelperKeyPressed = helperKey;
            CurrentPosition = currentPosition;
            CurrentControl = "";
            SetAlarm = setAlarm;
            CorrectFieldStyle = correctFieldStyle;
            InCorrectFieldStyle = incorrectFeldStyle;
        }
        public NatServiceItemKey(string name, string key, string formName, string helperKey, string currentPosition, bool setAlarm, string correctFieldStyle, string incorrectFeldStyle, string responseControls)
        {
            Name = name;
            KeyPressed = key;
            HelperKeyPressed = helperKey;
            CurrentPosition = currentPosition;
            CurrentControl = "";
            SetAlarm = setAlarm;
            CorrectFieldStyle = correctFieldStyle;
            InCorrectFieldStyle = incorrectFeldStyle;
            FormName = formName;
            ResponseControls = responseControls;
        }
        #endregion
    }
}
