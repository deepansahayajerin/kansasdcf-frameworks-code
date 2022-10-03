using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Core;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Interfaces;

namespace MDSY.Framework.Control.CICS
{
    public class BMSFieldControl : IFieldControl
    {
        #region Properties
        public string Name { get; private set; }
        public int RowPosition { get; private set; }
        public int ColumnPosition { get; private set; }
        public int Length { get; private set; }
        public string DefaultValue { get; private set; }
        public string Value { get; set; }
        public IField LengthField { get; private set; }
        public IField AttributeField { get; private set; }
        public IField ValueField { get; private set; }

        public bool isBright { get; set; }
        public bool isBlink { get; set; }
        public bool isDark { get; set; }
        public bool isReadonly { get; set; }
        public bool isAutoskip { get; set; }
        public bool isModified { get; set; }
        public bool isNumeric { get; set; }
        public bool isRightJustify { get; set; }
        public bool isLeftJustify { get; set; }

        private bool isBrightDefault { get; set; }
        private bool isDarkDefault { get; set; }
        private bool isReadonlyDefault { get; set; }
        private bool isAutoskipDefault { get; set; }
        private bool isModifiedDefault { get; set; }
        private bool isNumericDefault { get; set; }

        public bool isDefaultCursor { get; set; }
        public string EditMask { get; private set; }
        public FieldColor BMSFieldColor { get; private set; }
        public FieldHilight BMSFieldHilight { get; private set; } 
        #endregion

        #region Constructors
        public BMSFieldControl(string name, int row, int column, int length,
    string outPic, string defaultValue, IField lengthField, IField attributeField, IField valueField,
    params BMSFieldAttribute[] fieldAttributes)
        {
            Name = name;
            RowPosition = row;
            ColumnPosition = column;
            Length = length;
            DefaultValue = defaultValue;
            LengthField = lengthField;
            AttributeField = attributeField;
            ValueField = valueField;
            isReadonlyDefault = true;
            EditMask = outPic;
            foreach (BMSFieldAttribute attrb in fieldAttributes)
            {
                switch (attrb)
                {
                    case BMSFieldAttribute.Unprotected: isReadonlyDefault = false; break;
                    case BMSFieldAttribute.Bright: isBrightDefault = true; break;
                    case BMSFieldAttribute.Normal: isBrightDefault = false; isDarkDefault = false; break;
                    case BMSFieldAttribute.Dark: isDarkDefault = true; break;
                    case BMSFieldAttribute.Autoskip: isAutoskipDefault = true; break;
                    case BMSFieldAttribute.MDTOn: isModifiedDefault = true; break;
                    case BMSFieldAttribute.Numeric: isNumericDefault = true; break;
                    case BMSFieldAttribute.InitialCursor: isDefaultCursor = true; break;
                    case BMSFieldAttribute.JustifyRight: isRightJustify = true; break;
                    case BMSFieldAttribute.JustifyLeft: isLeftJustify = true; break;
                    default: break;
                }
            }
            UpdateFromDefaultAttributes();
        }

        public BMSFieldControl(string name, int row, int column, int length,
    string outPic, string defaultValue, IField lengthField, IField attributeField, IField valueField, FieldColor fieldColor,
    params BMSFieldAttribute[] fieldAttributes)
        {
            Name = name;
            RowPosition = row;
            ColumnPosition = column;
            Length = length;
            DefaultValue = defaultValue;
            LengthField = lengthField;
            AttributeField = attributeField;
            ValueField = valueField;
            isReadonlyDefault = true;
            EditMask = outPic;
            BMSFieldColor = fieldColor;
            BMSFieldHilight =  FieldHilight.Default;
            foreach (BMSFieldAttribute attrb in fieldAttributes)
            {
                switch (attrb)
                {
                    case BMSFieldAttribute.Unprotected: isReadonlyDefault = false; break;
                    case BMSFieldAttribute.Bright: isBrightDefault = true; break;
                    case BMSFieldAttribute.Normal: isBrightDefault = false; isDarkDefault = false; break;
                    case BMSFieldAttribute.Dark: isDarkDefault = true; break;
                    case BMSFieldAttribute.Autoskip: isAutoskipDefault = true; break;
                    case BMSFieldAttribute.MDTOn: isModifiedDefault = true; break;
                    case BMSFieldAttribute.Numeric: isNumericDefault = true; break;
                    case BMSFieldAttribute.InitialCursor: isDefaultCursor = true; break;
                    case BMSFieldAttribute.JustifyRight: isRightJustify = true; break;
                    case BMSFieldAttribute.JustifyLeft: isLeftJustify = true; break;
                    default: break;
                }
            }
            UpdateFromDefaultAttributes();
        }

        public BMSFieldControl(string name, int row, int column, int length,
string outPic, string defaultValue, IField lengthField, IField attributeField, IField valueField, FieldColor fieldColor, FieldHilight fieldHilight,
params BMSFieldAttribute[] fieldAttributes)
        {
            Name = name;
            RowPosition = row;
            ColumnPosition = column;
            Length = length;
            DefaultValue = defaultValue;
            LengthField = lengthField;
            AttributeField = attributeField;
            ValueField = valueField;
            isReadonlyDefault = true;
            EditMask = outPic;
            BMSFieldColor = fieldColor;
            BMSFieldHilight = fieldHilight;
            foreach (BMSFieldAttribute attrb in fieldAttributes)
            {
                switch (attrb)
                {
                    case BMSFieldAttribute.Unprotected: isReadonlyDefault = false; break;
                    case BMSFieldAttribute.Bright: isBrightDefault = true; break;
                    case BMSFieldAttribute.Normal: isBrightDefault = false; isDarkDefault = false; break;
                    case BMSFieldAttribute.Dark: isDarkDefault = true; break;
                    case BMSFieldAttribute.Autoskip: isAutoskipDefault = true; break;
                    case BMSFieldAttribute.MDTOn: isModifiedDefault = true; break;
                    case BMSFieldAttribute.Numeric: isNumericDefault = true; break;
                    case BMSFieldAttribute.InitialCursor: isDefaultCursor = true; break;
                    case BMSFieldAttribute.JustifyRight: isRightJustify = true; break;
                    case BMSFieldAttribute.JustifyLeft: isLeftJustify = true; break;
                    default: break;
                }
            }
            UpdateFromDefaultAttributes();
        }
        
        
        #endregion

        #region Public Methods
        public void UpdateFromDefaultAttributes()
        {
            isBright = isBrightDefault;
            isDark = isDarkDefault;
            isReadonly = isReadonlyDefault;
            isAutoskip = isAutoskipDefault;
            isModified = isModifiedDefault;
            isNumeric = isNumericDefault;
        }

        public void UpdateFieldBufferProperties()
        {
            int tmpLength = 0;
            if (isRightJustify && Value != null && Value.Length > 0)
            {
                Value = Value.Trim().Replace(",", "");
                if (Length > Value.Length)
                {
                    tmpLength = Value.Length;
                    Value = Value.PadLeft(Length, '0');
                }
            }

            if (isNumericDefault && !isLeftJustify)
            {
                string value = string.IsNullOrEmpty(Value) ? null : Value.TrimEnd().PadLeft(Length, ' ');
                ValueField.Assign(value);
            }
            else if (isLeftJustify)
            {
                string value = string.IsNullOrEmpty(Value) ? null : Value.TrimEnd().PadRight(Length, ' ');
                ValueField.Assign(value);
            }
            else
                ValueField.Assign(string.IsNullOrEmpty(Value) ? null : Value);

            if (Value == null)
                LengthField.Assign(0);
            else
            {
                if (!isModified)
                    LengthField.Assign(0);
                else
                {
                    if (tmpLength > 0)
                        LengthField.Assign(tmpLength);
                    else
                        LengthField.Assign(Value.Length);
                }
            }
        }

        public void EmptyDefaultValue()
        {
            DefaultValue = "";
        }

        #endregion
    }
}
