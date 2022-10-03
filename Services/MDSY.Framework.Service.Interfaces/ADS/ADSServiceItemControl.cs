using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using MDSY.Framework.Core;


namespace MDSY.Framework.Service.Interfaces
{
    /// <summary>
    /// Defines an object which deals with item control.
    /// </summary>
    [DataContract]
    //[DebuggerDisplay("{Name} |T:{Text} |RO:{ReadOnly} |C:{Column} |R:{Row} |L:{Length} |BC:{BackColor} |FC:{ForeColor} |F:{Focused}")]
    
    public class ADSServiceItemControl : IAterasServiceItem
    {
        #region public properties
        /// <summary>
        /// Name of the item
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Text in the field
        /// </summary>
        [DataMember]
        public string Text { get; set; }

        /// <summary>
        /// If the item is read only and cannot be edited
        /// </summary>
        [DataMember]
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Permanent setting of is read only  
        /// </summary>
        [DataMember]
        public bool PermReadOnly { get; set; }

        /// <summary>
        /// Gets the column that the item is located
        /// </summary>
        [DataMember]
        public int LocationRow { get; private set; }

        /// <summary>
        /// Gets the row that the item is located
        /// </summary>
        [DataMember]
        public int LocationColumn { get; private set; }

        /// <summary>
        /// The size of the field in number of characters.
        /// </summary>
        [DataMember]
        public int Length { get; private set; }

        /// <summary>
        /// Gets the style of the field
        /// </summary>
        [DataMember]
        public string Style { get; set; }

        /// <summary>
        /// Gets the Permanent style of the field
        /// </summary>
        [DataMember]
        public string PermStyle { get; set; }

        /// <summary>
        /// Gets the style of the field
        /// </summary>
        [DataMember]
        public string EditMask { get; set; }

        /// <summary>
        /// Sets a field as upper case only.
        /// </summary>
        [DataMember]
        public bool ForceUpperCase { get; private set; }

        /// <summary>
        /// Display an empty sting when the value is zero.
        /// </summary>
        [DataMember]
        public bool IsBlankWhenZero { get; set; }

        /// <summary>
        /// Is field MDT set.
        /// </summary>
        [DataMember]
        public bool IsModified { get; set; }

        /// <summary>
        /// Permanent setting of Is field MDT .
        /// </summary>
        [DataMember]
        public bool PermIsModified { get; set; }

        /// <summary>
        /// Is field value changed set.
        /// </summary>
        [DataMember]
        public bool IsChanged { get; set; }

        /// <summary>
        /// Has EraseEof been pressed?
        /// </summary>
        [DataMember]
        public bool isErased { get; set; }

        /// <summary>
        /// Permanent setting of Erased
        /// </summary>
        [DataMember]
        public bool PermIsErased { get; set; }

        /// <summary>
        /// Field has error condition
        /// </summary>
        [DataMember]
        public bool isInError { get; set; }

        /// <summary>
        /// Sets a field as fill character.
        /// </summary>
        [DataMember]
        public char FillCharacter { get; set; }

        /// <summary>
        /// Sets the OutputData setting
        /// </summary>
        [DataMember]
        public FieldAttributeOutputData OutputSetting { get; set; }

        [DataMember]
        public InputAttributes InputAttributes { get ; set; }

        [DataMember]
        public OutputAttributes OutputAttributes { get; set; }

        [DataMember]
        public bool IsSkip { get; set; }


        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the ADSServiceItemControl class.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <param name="text">Item text</param>
        /// <param name="readOnly">Specifies if item is read only</param>
        /// <param name="length">Item's length</param>
        /// <param name="style">Item's style</param>
        /// <param name="row">Item's row position</param>
        /// <param name="col">Item's column position</param>
        /// <param name="isModified">Specifies if the item has been modified</param>
        /// <param name="editMask">Item's edit mask</param>
        /// <param name="forceUpperCase">Specifies if item's text is to be in upper case or not</param>
        /// <param name="fillCharacter">Fill character for the item</param>
        public ADSServiceItemControl(string name, string text, bool readOnly, int length, string style, int row, int col,
            bool isModified, string editMask, bool forceUpperCase, bool isBlankWhenZero, char fillCharacter, InputAttributes inputAttributes, OutputAttributes outputAttributes, bool isSkip)
        {
            Name = name;
            Text = text;
            ReadOnly = readOnly;
            Length = length;
            Style = style;
            LocationRow = row;
            LocationColumn = col;
            IsModified = isModified;
            EditMask = editMask;
            ForceUpperCase = forceUpperCase;
            IsBlankWhenZero = isBlankWhenZero;
            FillCharacter = fillCharacter;
            PermReadOnly = ReadOnly;
            PermIsModified = IsModified;
            PermStyle = Style;
            InputAttributes = inputAttributes;
            OutputAttributes = outputAttributes;
            IsSkip = isSkip;
        }

        /// <summary>
        /// Initializes a new instance of the ADSServiceItemControl class.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <param name="text">Item text</param>
        /// <param name="readOnly">Specifies if item is read only</param>
        /// <param name="permReadOnly">Specifies if item is read only is permanent</param>
        /// <param name="length">Item's length</param>
        /// <param name="style">Item's style</param>
        /// <param name="permStyle">The permanebt style to be used</param>
        /// <param name="row">Item's row position</param>
        /// <param name="col">Item's column position</param>
        /// <param name="isModified">Specifies if the item has been modified</param>
        /// <param name="permIsModified">Specifies if the item has been permanently modified</param>
        /// <param name="editMask">Item's edit mask</param>
        /// <param name="forceUpperCase">Specifies if item's text is to be in upper case or not</param>
        /// <param name="fillCharacter">Fill character for the item</param>
        public ADSServiceItemControl(string name, string text, bool readOnly, bool permReadOnly, int length, string style, string permStyle, int row, int col,
            bool isModified, bool permIsModified, string editMask, bool forceUpperCase, bool isBlankWhenZero, char fillCharacter, InputAttributes inputAttributes, OutputAttributes outputAttributes, bool isSkip)
        {
            Name = name;
            Text = text;
            ReadOnly = readOnly;
            PermReadOnly = permReadOnly;
            Length = length;
            Style = style;
            PermStyle = permStyle;
            LocationRow = row;
            LocationColumn = col;
            IsModified = isModified;
            PermIsModified = permIsModified;
            EditMask = editMask;
            ForceUpperCase = forceUpperCase;
            IsBlankWhenZero = isBlankWhenZero;
            FillCharacter = fillCharacter;
            InputAttributes = inputAttributes;
            OutputAttributes = outputAttributes;
            IsSkip = isSkip;
        }

        public ADSServiceItemControl(string name, string text, bool readOnly, int length, string style, InputAttributes inputAttributes, OutputAttributes outputAttributes)
        {
            Name = name;
            Text = text;
            ReadOnly = readOnly;
            Length = length;
            Style = style;
            LocationRow = 0;
            LocationColumn = 0;
            IsModified = false;
            ForceUpperCase = false;
            PermReadOnly = ReadOnly;
            PermStyle = Style;
            InputAttributes = inputAttributes;
            OutputAttributes = outputAttributes;
        }
        #endregion
    }
}
