using MDSY.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Core
{
    public sealed class StringificationBySize : StringificationBase, IStringificationDef
    {
        /// <summary>
        /// Initializes a new instance of the DelimitedStringComponentsBase class.
        /// </summary>
        /// <param name="texts">An array of strings.</param>
        public StringificationBySize(params string[] texts)
            : base(string.Empty, texts)
        {

        }

        /// <summary>
        /// Returns the whole text value.
        /// </summary>
        /// <param name="text">Text for processing.</param>
        /// <returns>Processed string<returns>
        protected override string GetProcessedTextValue(string text)
        {
            //Delim by size takes the whole text value.
            string textValue = text.Replace('\0'.ToString(), "");
            if (textValue.Trim() == string.Empty)
                return textValue;
            else
                return textValue.TrimEnd();
        }
    }
}

