using System;
using System.Collections.Generic;
using System.Text;
using MDSY.Framework.Core;

namespace MDSY.Framework.UI.Angular
{
    public class FormatData
    {
        /// <summary>
        /// Sets and returns the external format.
        /// </summary>
        public string extFormat { get; set; }
        /// <summary>
        /// Sets and returns null action string.
        /// </summary>
        public string nullAction { get; set; }
        /// <summary>
        /// Sets and returns the numeric state string.
        /// </summary>
        public string numericState { get; set; }
        /// <summary>
        /// Sets and returns if data should be zero suppressed or not.
        /// </summary>
        public bool zeroSuppress { get; set; }
        /// <summary>
        /// Sets and returns id data should be set to zero when the value is null.
        /// </summary>
        public bool zeroWhenNull { get; set; }
        /// <summary>
        /// Sets and returns the padding character to be used.
        /// </summary>
        public char padCharacter { get; set; }
        /// <summary>
        /// Sets and returns the code table.
        /// </summary>
        public string codeTable { get; set; }
        /// <summary>
        /// Sets and returns the output program.
        /// </summary>
        public string outputProgram { get; set; }
        /// <summary>
        /// Sets and returns the style to be used.
        /// </summary>
        public string style { get; set; }
        /// <summary>
        /// Sets and returns if the field is a read only field or not.
        /// </summary>
        public bool isReadOnly { get; set; }
        /// <summary>
        /// Sets and returns if the field has been modified or not.
        /// </summary>
        public bool isModified { get; set; }
        /// <summary>
        /// Sets and returns whether to skip or not.
        /// </summary>
        public bool isSkip { get; set; }
        /// <summary>
        /// Sets and returns whether to change characters to Upper case or not.
        /// </summary>
        public bool isUpper { get; set; }
        /// <summary>
        /// Sets and returns whether there's an error or not.
        /// </summary>
        public bool isError { get; set; }
        /// <summary>
        /// Sets and returns field alignment value
        /// </summary>
        public FieldAlignment fieldAlignment { get; set; }
        /// <summary>
        /// Sets and returns whether it's default cursor position or not. 
        /// </summary>
        public bool isDefaultCursor { get; set; }
        /// <summary>
        /// Sets and returns whether it's the initial value.
        /// </summary>
        public string initialValue { get; set; }
        /// <summary>
        /// Sets and returns the entered value.
        /// </summary>
        public string enteredValue { get; set; }
        /// <summary>
        /// Sets and returns whether output data is set to yes or no.
        /// </summary>
        public FieldAttributeOutputData outputSetting { get; set; }
        /// <summary>
        /// Sets values for data formatting.
        /// </summary>
        public string fieldColor { get; set; }
        /// <summary>
        /// Sets and returns the external format.
        /// </summary>
        public FormatData()
        {
            extFormat = "";
            outputSetting = FieldAttributeOutputData.Yes;
            //nullAction = "";
            //numericState = "";
            //codeTable = "";
            //outputProgram = "";
        }
    }
}
