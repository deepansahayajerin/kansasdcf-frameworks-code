using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer;
using MDSY.Framework.Interfaces;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Provides common, <c>static</c> string utilities.
    /// </summary>
    public static class StringUtils
    {
        #region private methods

        private static Tuple<string, bool> FindFirstDelimiter(string workString, IList<Tuple<string, bool>> containedDelimiters, out int delimiterStartIndex)
        {
            int foundDelimIndex;
            // find first char of first delimiter in workString

            delimiterStartIndex = workString.FirstIndexOfAny(containedDelimiters.Select(tup => tup.Item1), out foundDelimIndex);

            return foundDelimIndex >= 0 ?
                containedDelimiters[foundDelimIndex] :
                null;
        }

        ///// <summary>
        ///// Returns the concatenated text of each field in the given list, from 
        ///// the first char of the value to the end of the substring as determined
        ///// by the specified delimiter.
        ///// </summary>
        ///// <param name="fields">List of BaseFields on which to operate.</param>
        ///// <param name="currentOutput">Existing string on which to append the processed texts.</param>
        ///// <param name="delimText">A string value to act as an End-Of-String indicator.</param>
        ///// <returns><c>currentOutput</c> + the field string values.</returns>
        //private static string ProcessDelimitedStringComponents(IList<BaseField> fields,
        //    string currentOutput,
        //    string delimText)
        //{
        //    StringBuilder result = new StringBuilder(currentOutput);

        //    foreach (BaseField field in fields)
        //    {
        //        string text = field.Text;
        //        if (text.Contains(delimText))
        //        {
        //            int delimIndex = text.IndexOf(delimText);
        //            result.Append(text.Substring(0, delimIndex + 1));
        //        }
        //        else
        //        {
        //            result.Append(text);
        //        }
        //    }

        //    return result.ToString();
        //}

        ///// <summary>
        ///// Returns the concatenated text of each field in the given list, from 
        ///// the first char of the value to the given length of the string.
        ///// </summary>
        ///// <param name="fields">List of BaseFields on which to operate.</param>
        ///// <param name="currentOutput">Existing string on which to append the processed texts.</param>
        ///// <param name="delimSize">Length, in chars, of the string to retrieve from each field.</param>
        ///// <returns><c>currentOutput</c> + the field string values.</returns>
        //private static string ProcessDelimitedStringComponents(IList<BaseField> fields, string currentOutput,
        //    Nullable<int> delimSize)
        //{
        //    StringBuilder result = new StringBuilder(currentOutput);

        //    if (delimSize.HasValue)
        //    {
        //        int size = (int)delimSize;
        //        foreach (BaseField field in fields)
        //        {
        //            int length = Math.Min(size, field.Text.Length);
        //            result.Append(field.Text.Take(length));
        //        }
        //    }

        //    return result.ToString();
        //}



        /// <summary>
        /// Applies the appropriate substring to the given field's text, 
        /// updates the workstring by removing the specified substring,
        /// and returns a new Quadruple (which defines this update instance),
        /// ready to be added to the Unstringify return collection.
        /// </summary>
        /// <param name="workString"></param>
        /// <param name="sourceSubString"></param>
        /// <param name="field"></param>
        /// <param name="foundDelimiter"></param>
        /// <param name="removeCharCount"></param>
        /// <returns></returns>
        private static Tuple<IField, int, string, string> ProcessFieldText(ref string workString,
            string sourceSubString,
            IField field,
            string foundDelimiter, int removeCharCount)
        {
            field.Assign(sourceSubString.PadRight(field.DisplayLength));

            // remove the substring
            workString = workString.Remove(0, removeCharCount);
            return Tuple.Create(field, sourceSubString.Length, sourceSubString, foundDelimiter);
        }
        #endregion

        #region public methods
        /// <summary>
        /// A helper method; accepts a set of DelimitedStringComponentsBase descendants
        /// and returns an IList&lt;&gt; containing the given objects, ready to pass to 
        /// Stringify().
        /// </summary>
        public static IList<StringificationBase> JoinDelimeterLists(params StringificationBase[] args)
        {
            return args.ToList();
        }

        /// <summary>
        /// Factory method which returns an IStringification 
        /// specific to the given BaseField delimiter type.
        /// </summary>
        /// <returns>IStringification</returns>
        public static IStringificationDef DelimitedByField(IField delimiter, params string[] texts)
        {
            return new StringificationByField(delimiter, texts);
        }

        /// <summary>
        /// Factory method which returns an IStringification 
        /// specific to the given string delimiter type.
        /// </summary>
        /// <returns>IStringification</returns>
        public static IStringificationDef DelimitedByString(string delimiter, params string[] texts)
        {
            return new StringificationByString(delimiter, texts);
        }

        /// <summary>
        /// Factory method which returns an IStringification 
        /// specific to the given int (size) delimiter type.
        /// </summary>
        /// <returns>IStringification</returns>
        public static IStringificationDef DelimitedBySize(params string[] texts)
        {
            return new StringificationBySize(texts);
        }

        /// <summary>
        /// Factory method which returns an IStringification which delimits 
        /// its texts by the space char.
        /// </summary>
        /// <returns>IStringification</returns>
        public static IStringificationDef DelimitedBySpace(params string[] fields)
        {
            return new StringificationByString(" ", fields);
        }




        //public static IList<BaseField> GetFieldList(params BaseField[] args)
        //{
        //    return args.ToList();
        //}


        /// <summary>
        /// Emulates the COBOL STRING statement.
        /// Accepts one or more sets of fields and delimiter information and 
        /// concatenates a new string from the field text values.
        /// </summary>
        /// <remarks>
        /// The COBOL STRING statement "strings together the partial or complete 
        /// contents of two or more data items...into one single data item. 
        /// One STRING statement can be written instead of a a series of MOVE statements."
        /// - COBOL docs.
        /// Stripify() accepts a list of IStringificationDef implementations, each of 
        /// which contains one or more fields, from which the new string will be 
        /// built, and delimiter information (a string, a field, etc.)
        /// See example. 
        /// </remarks>
        /// <example>
        /// Given the following fields:
        /// <code>
        /// Field field01 = new Field("01") { FieldType = FieldType.String, Length = 6, Text = "123*45" };
        /// Field field02 = new Field("02") { FieldType = FieldType.String, Length = 4, Text = "A*BC" };
        /// Field field03 = new Field("03") { FieldType = FieldType.String, Length = 5, Text = "DE*FG" };
        /// Field field04 = new Field("04") { FieldType = FieldType.String, Length = 1, Text = "*" };
        /// Field field05 = new Field("05") { FieldType = FieldType.String, Length = 6, Text = "6789*0" };
        /// Field field06 = new Field("06") { FieldType = FieldType.String, Length = 6, Text = "HIJ*KL" };
        /// Field field07 = new Field("07") { FieldType = FieldType.String, Length = 3, Text = "M*N" };
        /// </code>
        /// The following code will return <c>"123AStringDE6789*0LiteralHIJ*KLM*N"</c>:
        /// <code>
        /// private static string Example01()
        /// {
        ///     return StringUtils.Stringify(
        ///         StringUtils.DelimitedByField(field04, field01.Text, "String*literal", field02.Text, field03.Text),
        ///         StringUtils.DelimitedBySize(field05.Text, "Literal", field06.Text, field07.Text));
        /// }
        /// </code>
        /// 
        /// Another example:
        /// Given the following fields:
        /// <code>
        /// Field payeeName = new Field();
        /// Field firstName = new Field() { FieldType = FieldType.String, Length = 10, Text = "Joseph" };
        /// Field middleName = new Field() { FieldType = FieldType.String, Length = 10, Text = "Quincy" };
        /// Field lastName = new Field() { FieldType = FieldType.String, Length = 10, Text = "Public" };
        /// Field suffix = new Field() { FieldType = FieldType.String, Length = 10, Text = "Ph.D." };
        /// </code>
        /// The following code will assign to payeeName.Text, "Joseph Q Public Ph.D."
        /// <code>
        /// // If Example02 is a method of a <c>BatchBase</c>, extension methods of BatchBase allow us 
        /// // to call <c>this.</c> instead of <c>StringUtils</c>.
        /// private static void Example02()
        /// {
        ///     payeeName.Text = this.Stringify(
        ///         this.DelimitedByString(" ", firstName.Text),
        ///         this.DelimitedBySize(" "),
        ///         this.DelimitedByString(" ", middleName.Text.CobolSubString(1, 1)),
        ///         this.DelimitedBySize(" "),
        ///         this.DelimitedByString(" ", middleName.Text),
        ///         this.DelimitedBySize(" "),
        ///         this.DelimitedByString(" ", suffix.Text));
        /// }
        /// </code>
        /// </example>
        public static string Stringify(params IStringificationDef[] args)
        {
            StringBuilder result = new StringBuilder();

            //The resulting string was trimmed
            //Array.ForEach(args, stringDef => result.Append(stringDef.GetStringification()));

            foreach(IStringificationDef isDef in args)
            {
                if (isDef.GetType() == typeof(StringificationBySize))
                {
                    foreach(string currString in isDef.Texts)
                    {
                        result.Append(currString);
                    }
                }
                else
                {
                    //Array.ForEach(args, stringDef => result.Append(stringDef.GetStringification()));
                    result.Append(isDef.GetStringification());
                }
            }
            return result.ToString();
        }


        /// <summary>
        /// Emulates the COBOL UNSTRING statement.
        /// </summary>
        /// <remarks>
        /// Splits up the given source string, separating it at the given delimiters, 
        /// and assigns the substrings to the specified fields.
        /// 
        /// The return value is a list of Quadruples (or Tuple&lt;T1,T2,T3,T4&gt;)
        /// which contains information about the various fields affected by the 
        /// Unstringify() process. Each Quadruple in the list is defined thus:
        /// Tuple&lt;IBaseField, int, string, string&gt;
        ///   - IBaseField - the field which contains a substring of <c>source</c>
        ///     as defined by the occurrance of one of the specified delimiters.
        ///   - int - the length of the substring applied to the field value.
        ///   - string - the substring which has been applied to the field value.
        ///   - string - the delimiter that was encountered which defined the current substring.
        /// </remarks>
        /// <param name="source">The delimited string which is to be broken up.</param>
        /// <param name="delimiters">A list of delimiter strings, each qualified by a
        /// bool indicator corresponding to the COBOL "ALL" flag. </param>
        /// <param name="fields">A list of IBaseField objects, each to receive a portion 
        /// of the broken-up <c>source</c> string.</param>
        /// <returns>A list of Quadruple, of BaseField, int (count of chars), 
        /// string (the substring applied to the field), and string (the delimiter 
        /// encountered to create this substring).</returns>
        /// <example>
        /// Given the following source string:
        /// <code>string sourceString = "123**45678??90ABC";</code>
        /// ...and the following delimiters:
        /// <code>
        /// IList&lt;Tuple&lt;string, bool&gt;&gt; delims = new List&lt;Tuple&lt;string, bool&gt;&gt;();
        /// delims.Add(Tuple.Create("*", true)); // equivalent to COBOL: 'ALL "*" '
        /// delims.Add(Tuple.Create("?", false));
        /// </code>
        /// ...and the following fields:
        /// <code>
        /// Field field1 = new Field() { FieldType = FieldType.String, Length = 6 };
        /// Field field2 = new Field() { FieldType = FieldType.String, Length = 6 };
        /// Field field3 = new Field() { FieldType = FieldType.String, Length = 3 };
        /// Field field4 = new Field() { FieldType = FieldType.String, Length = 5 };
        /// </code>
        /// ...calling Unstringify() thus:
        /// <code>
        /// IList&lt;Tuple&lt;IBaseField, int, string, string&gt;&gt; result = 
        ///   StringUtils.Unstringify(sourceString, delims, field1, field2, field3, field4);
        /// </code>
        /// ...the result list would contain 4 Quadruples with the following values:
        /// <pre>
        /// Index  Field   Field.Text  SubString.Length  Substring  Delimiter
        /// -----  ------  ----------  ----------------  ---------  --------------------
        /// 0      Field1  "123   "    3                 "123"      "*"
        /// 1      Field2  "45678 "    5                 "45678"    "?"
        /// 2      Field3  "   "       0                 empty      "?"
        /// 3      Field4  "90ABC"     5                 "90ABC"    n/a - end of source
        /// </pre>
        /// </example>
        public static IList<Tuple<IField, int, string, string>> Unstringify(string source,
            IList<Tuple<string, bool>> delimiters,
            params IField[] fields)
        {
            #region init
            string workString = source;
            IList<Tuple<IField, int, string, string>> result = new List<Tuple<IField, int, string, string>>();

            // reduce the list of delimiters to only those that actually appear in the source string.
            IList<Tuple<string, bool>> containedDelimiters = delimiters.Where(tup => source.Contains(tup.Item1)).ToList();

            int currentFieldIndex = 0;
            IList<IField> fieldList = fields.ToList();
            #endregion

            // while the workString's not empty and we've still got fields to work with...
            while ((String.Compare(workString, string.Empty) != 0) && (currentFieldIndex < fields.Count()))
            {
                int delimiterStartIndex;
                Tuple<string, bool> foundDelim = FindFirstDelimiter(workString, containedDelimiters, out delimiterStartIndex);
                string foundDelimText = foundDelim != null ? foundDelim.Item1 : string.Empty;

                IField field = fieldList[currentFieldIndex];
                Tuple<IField, int, string, string> resultQuadruple = null;

                // are there any delims in the workString?
                if (delimiterStartIndex > -1)
                {
                    string sourceSubString = workString.Substring(0, delimiterStartIndex);
                    resultQuadruple = ProcessFieldText(ref workString, sourceSubString, field, foundDelimText, sourceSubString.Length);
                    // remove the delimiter
                    workString = workString.Remove(0, foundDelimText.Length);

                    if ((workString.IndexOf(foundDelimText) == 0))
                    {
                        // Item2 of the delimiter Tuple contains a bool indicating whether
                        // the COBOL "ALL" flag was present for the delimiter.
                        // if ALL is present (true) then any consecutive delims will be 
                        // removed. If not (false) then only one delim is removed and we 
                        // continue with a delim at position 0. 
                        if (foundDelim != null && foundDelim.Item2)
                        {
                            // if "ALL" flag is set and multiple consecutive delimiters 
                            // are encountered, delete them all without any other processing.
                            while ((workString.IndexOf(foundDelimText) == 0) && (workString.Length > 0))
                            {
                                workString = workString.Remove(0, foundDelimText.Length);
                            }
                        }
                        else
                        {
                            // if "ALL" flag is not set for consecutive delimiters, 
                            // process each one, filling a field with blanks for each one found.
                            while ((workString.IndexOf(foundDelimText) == 0) && (workString.Length > 0) && currentFieldIndex < fieldList.Count - 1)
                            {
                                // inc to next field index
                                currentFieldIndex++;
                                field = fieldList[currentFieldIndex];
                                result.Add(ProcessFieldText(ref workString, string.Empty, field, foundDelimText, foundDelimText.Length));
                            }
                        }
                    }
                }
                else
                {
                    int fieldLength = field.DisplayLength;
                    if (workString.Length <= fieldLength)
                    {
                        //Not used string fieldText = workString;

                        //// this'll be our last field.
                        //if (workString.Length < fieldLength)
                        //{
                        //    // no delimiters found AND the remaining workString is too short...
                        //    fieldText = workString.PadRight(fieldLength, ' ');
                        //}


                        // TODO: verify behavior. Local var 'fieldText' is never actually used. Should it be plugged into 
                        // this next call to ProcessFieldText()? 
                        resultQuadruple = ProcessFieldText(ref workString, workString, field, string.Empty, workString.Length);

                        // This commented-out line seems right, but does not pass unit test StringUtils_Unstringify.SimpleCase_TwoDelimiters_5Fields_LastValueOverflows_Success().
                        //resultQuadruple = ProcessFieldText(ref workString, fieldText, field, string.Empty, workString.Length);
                    }
                    else
                    {
                        // no delimiters found, but we've got enough text remaining for 
                        // more than one more field. Give the next field what it'll hold then keep looping...
                        resultQuadruple = ProcessFieldText(ref workString, workString.Substring(0, fieldLength), field, string.Empty, fieldLength);
                    }
                }

                if (resultQuadruple != null)
                    result.Add(resultQuadruple);

                currentFieldIndex++;
            }
            return result;
        }

        public static IList<Tuple<IField, int, string, string>> Unstringify(string source,
            IList<Tuple<string, bool>> delimiters,
            params IBufferValue[] fields)
        {
            #region init
            string workString = source;
            IList<Tuple<IField, int, string, string>> result = new List<Tuple<IField, int, string, string>>();

            // reduce the list of delimiters to only those that actually appear in the source string.
            IList<Tuple<string, bool>> containedDelimiters = delimiters.Where(tup => source.Contains(tup.Item1)).ToList();

            int currentFieldIndex = 0;
            IList<IBufferValue> fieldList = fields.ToList();
            #endregion

            // rjg begin - there was no implementation here; the method did nothing so here is a quick implementation because of Jetro time contraint; probably needs redone to match IField version.
            int i = 0;
            string[] delims = new string[delimiters.Count];
            for (; i < delimiters.Count; i++)
                delims[i] = delimiters[i].Item1;
            String[] tokens = source.Split(delims, StringSplitOptions.None);
            i = 0;
            while (i < tokens.Length && i < fields.Length)
            {
                fields[i].Assign(tokens[i]);
                i++;
            }
            // end 

            return result;
        }

        public static bool UnString(string source, IField startPointer, IList<UnStringDelimiter> delimiters, IList<UnStringIntoParm> intoParms, IField tallyField)
        {
            bool returnBool = false;
            returnBool = UnString(source, startPointer.AsInt(), delimiters, intoParms, tallyField);
            startPointer.SetValue(_currentPosition);
            return returnBool;
        }
        public static bool UnString(string source, IList<UnStringDelimiter> delimiters, IList<UnStringIntoParm> intoParms, IField tallyField)
        {
            return UnString(source, 1, delimiters, intoParms, tallyField);
        }
        private static bool UnString(string source, int startPos, IList<UnStringDelimiter> delimiters, IList<UnStringIntoParm> intoParms, IField tallyField)
        {
            bool overflow = false;
            int intoFieldCount = 0;
            int delimiterStartIndex = 0;
            List<Tuple<string, bool>> containedDelimiters = new List<Tuple<string, bool>>();
            foreach (UnStringDelimiter usd in delimiters)
            {
                containedDelimiters.Add(usd.Delimiter, usd.IsDelimitAll);
            }

            while (intoFieldCount < intoParms.Count)
            {
                if (startPos >= source.Length)
                {
                    intoFieldCount = intoParms.Count;
                    overflow = true;
                    break;
                }
                string workString = source.Substring(startPos - 1);
                Tuple<string, bool> foundDelim = FindFirstDelimiter(workString, containedDelimiters, out delimiterStartIndex);
                string foundDelimText = foundDelim != null ? foundDelim.Item1 : string.Empty;
                if (string.IsNullOrEmpty(foundDelimText))
                {
                    delimiterStartIndex = workString.Length - 1;
                }
                string sourceSubString = workString.Substring(0, delimiterStartIndex);
                try
                {
                    IField field = (IField)intoParms[intoFieldCount].UnStringField;
                    field.Assign(sourceSubString.PadRight(field.DisplayLength));
                }
                catch
                {
                    intoParms[intoFieldCount].UnStringField.SetValue(sourceSubString.PadRight(intoParms[intoFieldCount].UnStringField.DisplayValue.Length));
                }
                if (intoParms[intoFieldCount].UnStringCount != null)
                    intoParms[intoFieldCount].UnStringCount.SetValue(sourceSubString.Length);
                if (intoParms[intoFieldCount].UnStringDelimiter != null)
                    intoParms[intoFieldCount].UnStringDelimiter.SetValue(foundDelimText);
                startPos = startPos + delimiterStartIndex + foundDelimText.Length;
                intoFieldCount++;

                if (string.IsNullOrEmpty(foundDelimText))
                {
                    intoFieldCount = intoParms.Count;
                }

            }
            _currentPosition = startPos;

            return overflow;
        }

        #endregion

        #region private variables
        [ThreadStatic]
        private static int _currentPosition;
        #endregion
    }

    public class UnStringDelimiter
    {
        public UnStringDelimiter(string delimiter, bool isDelimitAll)
        {
            Delimiter = delimiter;
            IsDelimitAll = isDelimitAll;
        }
        public string Delimiter;
        public bool IsDelimitAll;
    }

    public class UnStringIntoParm
    {
        public UnStringIntoParm(IBufferValue unstringField, IField unstringCount, IField unStringDelimiter)
        {
            UnStringField = unstringField;
            UnStringCount = unstringCount;
            UnStringDelimiter = unStringDelimiter;
        }
        public UnStringIntoParm(IBufferValue unstringField, IField unstringCount)
        {
            UnStringField = unstringField;
            UnStringCount = unstringCount;
            UnStringDelimiter = null;
        }
        public IBufferValue UnStringField;
        public IField UnStringCount;
        public IField UnStringDelimiter;
    }
}
