using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDSY.Framework.Service.Interfaces
{
    [Serializable]
    //[DebuggerDisplay("{Name} |T:{Text} |RO:{ReadOnly} |C:{Column} |R:{Row} |L:{Length} |BC:{BackColor} |FC:{ForeColor} |F:{Focused}")]

    public class NatServiceItemControl : IAterasServiceItem
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
        public KeyValuePair<string, string>[] ReinputWithTextFields { get; private set; }

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
        public bool ForceUpperCase { get; set; }

        /// <summary>
        /// Sets whether Zero Printing is ON or OFF.
        /// </summary>
        public bool ZeroPrinting { get; private set; }

        public string ValidChars { get; set; }

        /// <summary>
        /// Is field MDT set.
        /// </summary>
        public bool IsModified { get; set; }

        /// <summary>
        /// Sets a field as upper case only.
        /// </summary>
        public bool Modified { get; set; }

        public string NumericFormat { get; set; }

        public bool IsInputFullLength { get; set; }

        /// <summary>
        /// Has EraseEof been pressed?
        /// </summary>
        public bool IsErased { get; set; }

        /// <summary>
        /// Field has error condition
        /// </summary>
        public bool IsInError { get; set; }

        /// <summary>
        /// Sets a field as fill character.
        /// </summary>
        public char FillCharacter { get; set; }

        /// <summary>
        /// Defines whether the control is label or not.
        /// </summary>
        public bool IsLabel { get; set; }

        public bool HasHelp { get; set; }
 
        public bool cssProtected = false;
        public bool cssIntensified = false;
        public string cssColor = "";
        public bool cssUnderlined = false;
        public bool cssBlinking = false;
        public bool cssHidden = false;
        public bool cssItalic = false;
        public bool cssReverse = false;
        #endregion

        #region constructors

        public NatServiceItemControl(string name, string text, bool readOnly, int length, string style)
        {
            Name = name;
            Text = text == null ? "" : text;
            ReadOnly = readOnly;
            Length = length;
            Style = style;
            ForceUpperCase = false;
            Modified = false;
            FillCharacter = ' ';
            EditMask = string.Empty;
        }

        public NatServiceItemControl(string name, string text, bool readOnly, int length, string style, bool forceUpperCase, bool zeroPrinting, string validChars, char fillCharacter, bool isLabel, bool hasHelp)
        {
            Name = name;
            if (text == null)
                text = "";
            else if (forceUpperCase)
                text = text.ToUpper();
            Text = text;
            ReadOnly = readOnly;
            Length = length;
            Style = style;
            ForceUpperCase = forceUpperCase;
            ZeroPrinting = zeroPrinting;
            ValidChars = validChars;
            Modified = false;
            FillCharacter = fillCharacter;
            IsLabel = isLabel;
            HasHelp = hasHelp;
        }

        public NatServiceItemControl(string name, KeyValuePair<string, string>[] text, bool readOnly, int length, string style, bool forceUpperCase, bool zeroPrinting, string validChars, char fillCharacter, bool isLabel)
        {
            Name = name;
            ReadOnly = readOnly;
            Length = length;
            Style = style;
            ForceUpperCase = forceUpperCase;
            ZeroPrinting = zeroPrinting;
            ValidChars = validChars;
            Modified = false;
            FillCharacter = fillCharacter;
            IsLabel = isLabel;
            ReinputWithTextFields = text;
            string tmpText = "";
            if (text != null && text[0].Key != null)
            {
                if (forceUpperCase)
                    tmpText = text[0].Key.ToString().ToUpper();
                else
                    tmpText = text[0].Key.ToString();
            }
            Text = tmpText;
        }

        public NatServiceItemControl(string name, string text, bool readOnly, int naturalLocationX, int naturalLocationY, int length, string style, bool focused, bool forceUpperCase, bool zeroPrinting, string validChars, bool isLabel, string editMask, bool hasHelp)
        {
            Name = name;
            if (text == null)
                text = "";
            else if (forceUpperCase)
                text = text.ToUpper();
            Text = text;
            ReadOnly = readOnly;
            NaturalLocationX = naturalLocationX;
            NaturalLocationY = naturalLocationY;
            Length = length;
            Style = style;
            ForceUpperCase = forceUpperCase;
            ZeroPrinting = zeroPrinting;
            ValidChars = validChars;
            Modified = false;
            FillCharacter = ' ';
            EditMask = editMask;
            IsLabel = isLabel;
            HasHelp = hasHelp;
        }

        public NatServiceItemControl(string name, string text, bool readOnly, int length, string style, int row, int col,
            bool isModified, string editMask, bool forceUpperCase, bool zeroPrinting, char fillCharacter, bool isLabel)
        {
            Name = name;
            if (text == null)
                text = "";
            else if (forceUpperCase)
                text = text.ToUpper();
            Text = text;
            ReadOnly = readOnly;
            Length = length;
            Style = style;
            LocationRow = row;
            LocationColumn = col;
            IsModified = isModified;
            EditMask = editMask;
            ForceUpperCase = forceUpperCase;
            ZeroPrinting = zeroPrinting;
            FillCharacter = fillCharacter;
            IsLabel = isLabel;
        }

        public NatServiceItemControl(string name, string text, bool readOnly, int naturalLocationX, int naturalLocationY, int length, Color backColor, Color foreColor, bool focused, bool forceUpperCase, bool zeroPrinting, bool isLabel)
        {
            Name = name;
            if (text == null)
                text = "";
            else if (forceUpperCase)
                text = text.ToUpper();
            Text = text;
            ReadOnly = readOnly;
            NaturalLocationX = naturalLocationX;
            NaturalLocationY = naturalLocationY;
            Length = length;
            Style = foreColor.ToString();
            ForceUpperCase = forceUpperCase;
            ZeroPrinting = zeroPrinting;
            Modified = false;
            FillCharacter = ' ';
            IsLabel = isLabel;
        }

        public NatServiceItemControl(string name, string text, KeyValuePair<string,string>[] reinputWithTextFields, bool readOnly, int naturalLocationX, int naturalLocationY, int locationRow, int locationColumn, int length,
             string style, string editMask, bool forceUpperCase, bool zeroPrinting, bool isModified, bool modified, bool isErased, bool isInError, char fillCharacter, bool isLabel, string numericFormat)
        {
            Name = name;
            if (text == null)
                text = "";
            else if (forceUpperCase)
                text = text.ToUpper();
            Text = text;
            ReinputWithTextFields = reinputWithTextFields;
            ReadOnly = readOnly;
            NaturalLocationX = naturalLocationX;
            NaturalLocationY = naturalLocationY;
            LocationRow = locationRow;
            LocationColumn = locationColumn;
            Length = length;
            Style = style;
            EditMask = editMask;
            ForceUpperCase = forceUpperCase;
            ZeroPrinting = zeroPrinting;
            IsModified = isModified;
            Modified = modified;
            IsErased = isErased;
            IsInError = isInError;
            FillCharacter = fillCharacter;
            IsLabel = isLabel;
            NumericFormat = numericFormat;
        }
        #endregion
    }
}
