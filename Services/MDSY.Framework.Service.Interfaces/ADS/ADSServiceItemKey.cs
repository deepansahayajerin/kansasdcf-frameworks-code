using System.Diagnostics;
using System.Runtime.Serialization;

namespace MDSY.Framework.Service.Interfaces
{
    /// <summary>
    /// Defines an object which deals with item keys pressed. 
    /// </summary>
    [DataContract, DebuggerDisplay("{Name},{KeyPressed}, {FormName}")]
    
    public class ADSServiceItemKey : IAterasServiceItem
    {
        #region public properties
        /// <summary>
        /// Name of the item
        /// </summary>
        [DataMember]
        public string Name { get;  set; }

        /// <summary>
        /// Name of the UI Form
        /// </summary>
        [DataMember]
        public string FormName { get; set; }

        /// <summary>
        /// ID of the Application
        /// </summary>
        [DataMember]
        public string ApplicationID { get; set; }

        /// <summary>
        /// ADS Response
        /// </summary>
        [DataMember]
        public string ResponeName { get; set; }
        
        /// <summary>
        /// If this is a type KeyPress this will be the key pressed.
        /// </summary>
        [DataMember]
        public string KeyPressed { get; set; }

        /// <summary>
        /// Gets the value of any additional key that was pressed.
        /// </summary>
        [DataMember]
        public string HelperKeyPressed { get; set; }

        /// <summary>
        /// Gets the current focused Textbox.
        /// </summary>
        [DataMember]
        public string CurrentControl { get; set; }

        /// <summary>
        /// Gets the current position of cursor
        /// </summary>
        [DataMember]
        public string CurrentPosition { get; set; }

        /// <summary>
        /// Gets the Alarm setting
        /// </summary>
        [DataMember]
        public bool SetAlarm { get; set; }

        /// <summary>
        /// Set Keyboard to all caps
        /// </summary>
        [DataMember]
        public bool SetCaps { get; set; }

        /// <summary>
        /// Gets the correct field style
        /// </summary>
        [DataMember]
        public string CorrectFieldStyle { get; set; }

        /// <summary>
        /// Gets the incrorrect field style
        /// </summary>
        [DataMember]
        public string InCorrectFieldStyle { get; set; }

        /// <summary>
        /// Gets the respponse controls
        /// </summary>
        [DataMember]
        public string ResponseControls { get; set; }

        /// <summary>
        /// Gets the respponse controls
        /// </summary>
        [DataMember]
        public string MessagePosition { get; set; }

        [DataMember]
        public string HasCursorField { get; set; }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the ADSServiceItemKey class.
        /// </summary>
        /// <param name="name">Name of item</param>
        /// <param name="key">Value of key pressed</param>
        /// <param name="helperKey">Value of any additional key that was pressed</param>
        /// <param name="currentPosition">Cursor current position</param>
        public ADSServiceItemKey(string name, string key, string helperKey, string currentPosition)
        {
            Name = name;
            KeyPressed = key;
            HelperKeyPressed = helperKey;
            CurrentPosition = currentPosition;
            CurrentControl = currentPosition;
            SetAlarm = false;
        }

        /// <summary>
        /// Initializes a new instance of the ADSServiceItemKey class.
        /// </summary>
        /// <param name="name">Name of item</param>
        /// <param name="key">Value of key pressed</param>
        /// <param name="helperKey">Value of any additional key that was pressed</param>
        /// <param name="currentPosition">Cursor current position</param>
        /// <param name="setAlarm">True if Alarm is required</param>
        public ADSServiceItemKey(string name, string key, string helperKey, string currentPosition, bool setAlarm)
        {
            Name = name;
            KeyPressed = key;
            HelperKeyPressed = helperKey;
            CurrentPosition = currentPosition;
            CurrentControl = currentPosition;
            SetAlarm = setAlarm;
        }

        /// <summary>
        /// Initializes a new instance of the ADSServiceItemKey class.
        /// </summary>
        /// <param name="name">Name of item</param>
        /// <param name="key">Value of key pressed</param>
        /// <param name="helperKey">Value of any additional key that was pressed</param>
        /// <param name="currentPosition">Cursor current position</param>
        /// <param name="setAlarm">True if Alarm is required</param>
        /// <param name="correctFieldStyle">Correct field style</param>
        /// <param name="incorrectFeldStyle">Incorrect field style</param>
        public ADSServiceItemKey(string name, string key, string helperKey, string currentPosition, bool setAlarm, 
            string correctFieldStyle, string incorrectFeldStyle)
        {
            Name = name;
            KeyPressed = key;
            HelperKeyPressed = helperKey;
            CurrentPosition = currentPosition;
            CurrentControl = currentPosition;
            SetAlarm = setAlarm;
            CorrectFieldStyle = correctFieldStyle;
            InCorrectFieldStyle = incorrectFeldStyle;
        }

        /// <summary>
        /// Initializes a new instance of the ADSServiceItemKey class.
        /// </summary>
        /// <param name="name">Name of item</param>
        /// <param name="key">Value of key pressed</param>
        /// <param name="formName">Form name</param>
        /// <param name="helperKey">Value of any additional key that was pressed</param>
        /// <param name="currentPosition">Cursor current position</param>
        /// <param name="setAlarm">True if Alarm is required</param>
        /// <param name="correctFieldStyle">Correct field style</param>
        /// <param name="incorrectFeldStyle">Incorrect Field Style</param>
        /// <param name="responseControls">Response Controls</param>
        /// <param name="isAllCaps">True if text is in capital letters</param>
        public ADSServiceItemKey(string name, string key, string formName, string helperKey, string currentPosition, bool setAlarm, string correctFieldStyle,
            string incorrectFeldStyle, string responseControls, bool isAllCaps)
        {
            Name = name;
            KeyPressed = key;
            HelperKeyPressed = helperKey;
            CurrentPosition = currentPosition;
            CurrentControl = currentPosition;
            SetAlarm = setAlarm;
            CorrectFieldStyle = correctFieldStyle;
            InCorrectFieldStyle = incorrectFeldStyle;
            FormName = formName;
            ResponseControls = responseControls;
            SetCaps = isAllCaps;
        }
        #endregion
    }
}
