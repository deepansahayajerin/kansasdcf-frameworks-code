using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Service.Interfaces
{
    [Serializable]
    //[DebuggerDisplay("{Name} |T:{Text} |RO:{ReadOnly} |C:{Column} |R:{Row} |L:{Length} |BC:{BackColor} |FC:{ForeColor} |F:{Focused}")]

    public class CICSServiceItemControl : IAterasServiceItem
    {
        #region public properties
        /// <summary>
        /// Name of the item
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Text in the field
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// If the item is read only and cannot be edited
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Gets the column that the item is located
        /// </summary>
        public int NaturalLocationX { get; private set; }

        /// <summary>
        /// Gets the row that the item is located
        /// </summary>
        public int NaturalLocationY { get; private set; }

        /// <summary>
        /// Gets the column that the item is located
        /// </summary>
        public int LocationRow { get; private set; }

        /// <summary>
        /// Gets the row that the item is located
        /// </summary>
        public int LocationColumn { get; private set; }

        /// <summary>
        /// The size of the field in number of characters.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Gets the style of the field
        /// </summary>
        public string Style { get; set; }

        /// <summary>
        /// Gets the style of the field
        /// </summary>
        public string EditMask { get; set; }

        /// <summary>
        /// Sets a field as upper case only.
        /// </summary>
        public bool ForceUpperCase { get; private set; }

        /// <summary>
        /// Is field MDT set.
        /// </summary>
        public bool IsModified { get; set; }

        /// <summary>
        /// Has EraseEof been pressed?
        /// </summary>
        public bool isErased { get; set; }

        /// <summary>
        /// Field has error condition
        /// </summary>
        public bool isInError { get; set; }

        /// <summary>
        /// Sets a field as fill character.
        /// </summary>
        public char FillCharacter { get; set; }

        /// <summary>
        /// Sets a field as upper case only.
        /// </summary>
        public bool Modified { get; set; }

        /// <summary>
        /// Has autoskip attribute?
        /// </summary>
        public bool Autoskip { get; private set; }

        #endregion

        #region constructors


        public CICSServiceItemControl(string name, string text, bool readOnly, int length, string style, int row, int col,
            bool isModified, string editMask, bool forceUpperCase, char fillCharacter)
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
            FillCharacter = fillCharacter;
        }

        public CICSServiceItemControl(string name, string text, bool readOnly, int length, string style)
        {
            Name = name;
            Text = text;
            ReadOnly = readOnly;
            Length = length;
            Style = style;
            ForceUpperCase = true;
            Modified = false;
            FillCharacter = ' ';
            EditMask = string.Empty;
        }

        public CICSServiceItemControl(string name, string text, bool readOnly, int length, string style, bool modified, bool autoskip)
        {
            Name = name;
            Text = text;
            ReadOnly = readOnly;
            Length = length;
            Style = style;
            ForceUpperCase = true;
            Modified = modified;
            FillCharacter = ' ';
            EditMask = string.Empty;
            Autoskip = autoskip;
        }

        public CICSServiceItemControl(string name, string text, bool readOnly, int length, string style, bool modified, bool autoskip, int row, int col,
           bool isModified, string editMask, bool forceUpperCase, char fillCharacter)
        {
            Name = name;
            Text = text;
            ReadOnly = readOnly;
            Length = length;
            Style = style;
            Modified = modified;
            Autoskip = autoskip;
            LocationRow = row;
            LocationColumn = col;
            IsModified = isModified;
            EditMask = editMask;
            ForceUpperCase = forceUpperCase;
            FillCharacter = fillCharacter;
        }
        #endregion
    }
}
