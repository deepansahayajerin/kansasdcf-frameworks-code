using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Core
{
    public abstract class StringificationBase
    {
        /// <summary>
        /// Processes delimited strings
        /// </summary>
        #region constructors
        /// <summary>
        /// Initializes a new instance of the DelimitedStringComponentsBase class.
        /// </summary>
        /// <param name="delimiter">Delimiter value.</param>
        public StringificationBase(string delimiter, params string[] texts)
        {
            Texts = texts;
            Delimiter = delimiter;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Returns Delimiter string.
        /// </summary>
        public string Delimiter { get; private set; }

        /// <summary>
        /// Returns a reference to the collection of strings.
        /// </summary>
        public IList<string> Texts { get; private set; }
        #endregion

        #region abstract and virtual
        /// <summary>
        /// Virtual method. If not overridden, returns a substing from the beginning of the string till the first delimiter.
        /// Returns full string if the provided string does not have a delimiter.
        /// </summary>
        /// <param name="text">String for processing.</param>
        /// <returns>Processed string.</returns>
        protected virtual string GetProcessedTextValue(string text)
        {
            string result = text;

            if (text.Contains(Delimiter))
            {
                int delimIndex = text.IndexOf(Delimiter);
                result = text.Substring(0, delimIndex);
            }

            return result;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Returns the concatenated string values for this StringificationBase instance.
        /// Descendants should override GetProcessedTextValue() if they need to 
        /// provide custom behavior for getting the text value from a field.
        /// </summary>
        public string GetStringification()
        {
            StringBuilder result = new StringBuilder();

            foreach (string text in Texts)
            {
                result.Append(GetProcessedTextValue(text));
            }


            return result.ToString();
        }
        #endregion



    }
}

