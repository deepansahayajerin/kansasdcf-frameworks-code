using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Buffer.Common;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Internal helper methods for managing Array elements. 
    /// </summary>
    internal static class ArrayElementUtils
    {
        #region constants
        private const char c_Delim = ' ';
        #endregion

        #region public methods

        /// <summary>
        /// Returns the element base name from the given <paramref name="elementName"/>.
        /// </summary>
        /// <param name="elementName">The element's name.</param>
        /// <returns>The element base name</returns>
        public static string GetElementBaseName(string elementName)
        {
            string result = elementName;

            int pos = elementName.IndexOf(c_Delim);
            if (pos >= 0)
            {
                result = elementName.Substring(0, pos);
            }

            return result;

            //return new string(elementName.ToCharArray().TakeWhile(c => c != c_Delim).ToArray());
        }

        /// <summary>
        /// Returns the list of indexes built into the given <paramref name="elementName"/> via index suffixes.
        /// For nested indexes, parent indexes are first. 
        /// </summary>
        /// <example>
        /// Where <c>elementName == "myElement 3 0 12"</c>, GetElementIndexes() would return an IEnumerable(of int)
        /// thus:
        /// <code>
        /// result[0] = 3
        /// result[1] = 0
        /// result[2] = 12
        /// </code>
        /// </example>
        /// <param name="elementName">the element's name.</param>
        /// <returns>The element's indexes.</returns>
        public static IEnumerable<int> GetElementIndexes(string elementName, out string baseName)
        {
            List<int> result = new List<int>();
            baseName = elementName;

            if (elementName.Contains(c_Delim))
            {
                //var work = elementName.AsEnumerable();
                baseName = GetElementBaseName(elementName);
                var work = elementName.AsEnumerable().SkipWhile(c => c != c_Delim);

                while (work.Contains(c_Delim) && (work.Count() > 0))
                {
                    var num = work.SkipWhile(c => c == c_Delim).TakeWhile(c => c != c_Delim);
                    string numStr = new string(num.ToArray());
                    int idx;

                    if (!int.TryParse(numStr, out idx))
                        throw new ArrayElementException(string.Format("Invalid array element index suffix; index was {0}", numStr));

                    result.Insert(0, idx);
                    // find the next delim/number pair...
                    work = work.SkipWhile(c => c == c_Delim).SkipWhile(c => c != c_Delim);
                }
            }

            // we've processed the indexes into reverse order; swap them back:
            result.Reverse();
            return result;
        }

        /// <summary>
        /// Constructs an appropriate array element name from the given <paramref name="baseName"/> and 
        /// <paramref name="singleLevelIndex"/>. Returns a string in the form "ArrayName 0"
        /// </summary>
        public static string MakeElementName(string baseName, int singleLevelIndex)
        {
            var list = new List<int>();
            list.Add(singleLevelIndex);
            return MakeElementName(baseName, list);
        }

        /// <summary>
        /// Constructs an appropriate multi-index array element name from the given <paramref name="baseName"/> and 
        /// <paramref name="indexes"/>. Returns a string in the form "ArrayName 0 1 2..."
        /// </summary>
        public static string MakeElementName(string baseName, IEnumerable<int> indexes)
        {
            StringBuilder builder = new StringBuilder(baseName);

            foreach (int idx in indexes)
            {
                builder.AppendFormat("{1}{0}", idx, c_Delim);
            }

            return builder.ToString();
        }
        #endregion

    }
}
