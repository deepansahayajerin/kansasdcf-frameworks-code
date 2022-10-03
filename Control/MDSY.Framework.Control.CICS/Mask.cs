using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;


namespace MDSY.Framework.Control.CICS
{
    internal enum RangeMode
    {
        NotSet,
        Minimum,
        Maximum,
    }

    public class Mask
    {
        #region private attributes
        private bool _dayCheck;
        private int _day;
        private bool _monthCheck;
        private int _month;
        private bool _yearCheck;
        private int _year;
        private string _minimumString;
        private string _maximumString;
        private long? _minimum;
        private long? _maximum;
        RangeMode _rangeMode = RangeMode.NotSet;
        /// the inputString counter
        int _inputStringCount = 0;
        #endregion


        #region contructors
        private Mask()
        {
            _day = 0;
            _month = 0;
            _year = 0;
            _minimum = null;
            _minimumString = string.Empty;
            _maximum = null;
            _maximumString = string.Empty;
        }
        #endregion


        #region private methods
        /// <summary>
        /// Get the size of characters used by the mask
        /// </summary>
        /// <param name="maskString"></param>
        /// <returns></returns>
        private int MaskSize(string maskString)
        {
            int maskSize = maskString.Length;

            if (maskString.Contains('*') || maskString.Contains('%'))
            {
                throw new InvalidOperationException("Cannot calculate the MaskSize if they are infinity");
            }

            while (maskString.Contains('-') || maskString.Contains(':'))
            {
                int separatorIndex = -1;

                if (maskString.IndexOf('-') > maskString.IndexOf(':'))
                {
                    separatorIndex = maskString.IndexOf('-');
                }
                else if (maskString.IndexOf('-') < maskString.IndexOf(':'))
                {
                    separatorIndex = maskString.IndexOf(':');
                }

                maskString = maskString.Remove(separatorIndex, 1);

                for (int count = separatorIndex; count < maskString.Length; count++)
                {
                    if (char.IsNumber(maskString[separatorIndex]))
                    {
                        /// get the new mask with the remove extra digit of the
                        /// range
                        maskString = maskString.Remove(count, 1);

                        /// we do not want to move it forward
                        count--;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (maskString.Contains('\''))
            {
                maskString = maskString.Replace("\'", "");
            }

            return maskString.Length;
        }
        private bool Compare(string inputString, string maskString, string operand2)
        {
            bool isEqual = true;

            /// if the last character on the mask we need to get the last part of our inputString and the last part of 
            /// our maskString and do this comapre again.
            if ((maskString.Length > 0) && (maskString[maskString.Length - 1] == '/'))
            {
                /// get anything that is after a * or %
                string endMask = maskString;

                int asteriskIndex = maskString.LastIndexOf('*');
                int percentIndex = maskString.LastIndexOf('%');

                /// if we have any asterisk or percent
                if (asteriskIndex > percentIndex)
                {
                    endMask = maskString.Substring(asteriskIndex + 1);
                }
                else if (asteriskIndex < percentIndex)
                {
                    endMask = maskString.Substring(percentIndex + 1);
                }

                /// remove the / from the end
                endMask = endMask.Substring(0, endMask.Length - 1);

                /// remove the other part of the mask
                maskString = maskString.Substring(0, maskString.Length - endMask.Length - 1);

                /// get the size that this mask will consume
                int maskSize = MaskSize(endMask);

                /// get the end of the string
                string endInputString = inputString.Substring(inputString.Length - maskSize, maskSize);

                /// now leave only the part that is not done by the end
                inputString = inputString.Substring(0, inputString.Length - maskSize);

                /// take care of the end
                string endOperand2 = string.Empty;
                if (operand2.Length >= maskSize)
                {
                    /// get the end operand
                    endOperand2 = operand2.Substring(operand2.Length - maskSize, maskSize);
                    operand2 = operand2.Substring(0, operand2.Length - maskSize);
                }

                /// now do this sub mask
                Mask mask = new Mask();
                isEqual = mask.Compare(endInputString, endMask, endOperand2);
            }

            /// if a ' is found than this toggle
            bool stringMode = false;

            for (int count = 0; (count < maskString.Length && isEqual); count++)
            {
                if (_inputStringCount == inputString.Length)
                {
                    bool allCanBeIgnored = true;
                    /// we need to find out if the rest of the mask is just a bunch 
                    /// of ignored characters
                    for (int maskStringCount = count; (maskStringCount < maskString.Length) && allCanBeIgnored; maskStringCount++)
                    {
                        switch (maskString[maskStringCount])
                        {
                            case '.':
                            case '?':
                            case '_':
                            case '*':
                            case '%':
                            case '\'':
                                /// that is fine to be these
                                break;
                            default:
                                allCanBeIgnored = false;
                                break;
                        }
                    }

                    if (!allCanBeIgnored)
                    {
                        isEqual = false;
                    }
                    break;
                }

                if (stringMode)
                {
                    if (maskString[count] == '\'')
                    {
                        stringMode = !stringMode;
                    }
                    else if (maskString[count] != inputString[_inputStringCount])
                    {
                        /// now it should be the same, if not
                        /// we need to get out.
                        isEqual = false;
                        count = maskString.Length;
                    }
                    else
                    {
                        _inputStringCount++;
                    }
                }
                else if ((char.IsNumber(maskString[count])) || (maskString[count] == ':') || (maskString[count] == '-'))
                {
                    switch (maskString[count])
                    {
                        case ':':
                        case '-':
                            #region going to max
                            switch (_rangeMode)
                            {
                                case RangeMode.Minimum:
                                    _rangeMode = RangeMode.Maximum;
                                    break;
                                case RangeMode.NotSet:
                                case RangeMode.Maximum:
                                    throw new InvalidOperationException("Invalid range state machine jump, cannot jump from Max to another");
                            }
                            break;
                            #endregion
                        default: /// this is a number
                            #region range storing information
                            switch (_rangeMode)
                            {
                                case RangeMode.NotSet:
                                case RangeMode.Minimum:
                                    #region minimum
                                    _rangeMode = RangeMode.Minimum;
                                    _minimumString += maskString[count];
                                    if (_minimum == null)
                                    {
                                        _minimum = 0;
                                    }
                                    else if (_minimum > 0)
                                    {
                                        _minimum *= 10;
                                    }
                                    _minimum += Convert.ToInt64(maskString[count].ToString());
                                    break;
                                    #endregion
                                case RangeMode.Maximum:
                                    #region maximum
                                    _maximumString += maskString[count];
                                    if (_maximum == null)
                                    {
                                        _maximum = 0;
                                    }
                                    else if (_maximum > 0)
                                    {
                                        _maximum *= 10;
                                    }
                                    _maximum += Convert.ToInt64(maskString[count].ToString());
                                    #endregion
                                    break;
                            }
                            break;
                            #endregion
                    }
                }
                else
                {
                    switch (_rangeMode)
                    {
                        case RangeMode.Minimum:
                        case RangeMode.Maximum:
                            #region minimum and maximum ranges check
                            isEqual = RangeCheck(inputString);
                            #endregion
                            break;
                    }
                    switch (maskString[count])
                    {
                        case 'C':
                            #region alpha, numeric and blank
                            /// The position is to be checked for an alphabetical character (upper or lower case), a numeric character, or a blank. 
                            if (!char.IsLetter(inputString[_inputStringCount]) &&
                                !char.IsNumber(inputString[_inputStringCount]) &&
                                !char.IsWhiteSpace(inputString[_inputStringCount]))
                            {
                                isEqual = false;
                                count = maskString.Length;
                            }
                            _inputStringCount++;
                            break;
                            #endregion
                        case 'M':
                            #region month
                            /// The positions are to be checked for a valid month (01 - 12); see also Checking Dates.
                            int monthTemp;
                            if (Int32.TryParse(inputString[_inputStringCount].ToString(), out monthTemp))
                            {
                                if (_month > 0)
                                {
                                    _month *= 10;
                                }
                                _month += monthTemp;
                                _monthCheck = true;
                            }
                            else
                            {
                                count = maskString.Length;
                            }
                            _inputStringCount++;
                            break;
                            #endregion
                        case 'D':
                            #region day
                            /// The two positions are to be checked for a valid day notation (01 - 31; dependent on the values of MM and YY/YYYY, if specified; see also Checking Dates). 
                            int dayTemp;
                            if (Int32.TryParse(inputString[_inputStringCount].ToString(), out dayTemp))
                            {
                                if (_day > 0)
                                {
                                    _day *= 10;
                                }
                                _day += dayTemp;
                                _dayCheck = true;
                            }
                            else
                            {
                                count = maskString.Length;
                            }
                            _inputStringCount++;
                            break;
                            #endregion
                        case 'Y':
                            #region year
                            /// The two positions are to be checked for a valid year (00 - 99). See also Checking Dates. 
                            /// The four positions are checked for a valid year (0000 - 2699). Use the COMPOPT
                            ///option MASKCME=ON to restrict the range of valid years to 1582 - 2699; see also 
                            ///Checking Dates. If the profile parameter MAXYEAR is set to 9999, the upper year limit
                            ///is 9999. 
                            int yearTemp;
                            if (Int32.TryParse(inputString[_inputStringCount].ToString(), out yearTemp))
                            {
                                /// move the year to the left
                                if (_year > 0)
                                {
                                    _year *= 10;
                                }

                                _year += yearTemp;
                                _yearCheck = true;
                            }
                            else
                            {
                                count = maskString.Length;
                            }
                            _inputStringCount++;
                            break;
                            #endregion
                        case 'A':
                            #region alpha
                            /// The position is to be checked for an alphabetical character (upper or lower case).
                            if (!char.IsLetter(inputString[_inputStringCount]))
                            {
                                isEqual = false;
                                count = maskString.Length;
                            }
                            _inputStringCount++;
                            break;
                            #endregion
                        case 'N':
                            #region numeric
                            /// The position is to be checked for a numeric digit.
                            if (!char.IsNumber(inputString[_inputStringCount]))
                            {
                                isEqual = false;
                                count = maskString.Length;
                            }
                            _inputStringCount++;
                            break;
                            #endregion
                        case 'S':
                            #region special character
                            /// The position is to be checked for special characters. 
                            if (!char.IsPunctuation(inputString[_inputStringCount]))
                            {
                                isEqual = false;
                                count = maskString.Length;
                            }
                            _inputStringCount++;
                            break;
                            #endregion
                        case 'U':
                            #region upper-case alphabetical
                            /// The position is to be checked for an upper-case alphabetical character (A - Z). 
                            if (!char.IsUpper(inputString[_inputStringCount]))
                            {
                                isEqual = false;
                                count = maskString.Length;
                            }
                            _inputStringCount++;
                            break;
                            #endregion
                        case 'X':
                            #region comparison of the second operand2
                            /// The position is to be checked against the equivalent position in the value (operand2)
                            /// following the mask-definition. 
                            /// "X" is not allowed in a variable mask definition, as it makes no sense.
                            if (inputString[_inputStringCount] != operand2[_inputStringCount])
                            {
                                isEqual = false;
                                count = maskString.Length;
                            }
                            _inputStringCount++;
                            break;
                            #endregion
                        case '\'':
                            #region text mode toggle
                            /// now we are getting into or getting out of stringMode
                            stringMode = !stringMode;
                            break;
                            #endregion
                        case '.':
                        case '?':
                        case '_':
                            #region ignore
                            /// A period, question mark or underscore indicates a single position that is not to be checked.
                            /// do nothing let the mask move forward
                            _inputStringCount++;
                            break;
                            #endregion
                        case '*':
                        case '%':
                            /// get everything without whatever character we had now
                            string newMask = maskString.Substring(count + 1);

                            for (; _inputStringCount < inputString.Length; _inputStringCount++)
                            {
                                Mask mask = new Mask();
                                /// we are sending empty for operant2 because when using * or % it is invalid 
                                /// to use X
                                isEqual = mask.Compare(inputString.Substring(_inputStringCount), newMask, string.Empty);

                                /// if we found a good one let it go. This way we will check
                                /// this again and this one should work, if we get to the end
                                /// and there is no good one, then there is no good one.
                                if (isEqual)
                                {
                                    break;
                                }
                            }
                            break;
                        case 'H':
                        case 'J':
                        case 'L':
                        case 'P':
                        case 'Z':
                        default:
                            throw new InvalidCastException("Mask option not implemented");
                    }
                }
            }

            /// do we have any range do be done?
            /// if no do the range check
            switch (_rangeMode)
            {
                case RangeMode.Maximum:
                case RangeMode.Minimum:
                    isEqual = RangeCheck(inputString);
                    break;
            }

            if (isEqual && (_yearCheck || _monthCheck || _dayCheck))
            {
                /// checks if we date is ok.
                isEqual = DateCheck();
            }

            return isEqual;
        }
        /// <summary>
        /// Checks the current range
        /// </summary>
        /// <param name="inputStringCount"></param>
        /// <param name="inputString"></param>
        /// <returns></returns>
        private bool RangeCheck(string inputString)
        {
            bool isEqual = true;

            /// get the string we are going to be looking at
            string intputParsedString = string.Empty;
            long intputParsedLong;
            for (int minimumStringCount = _inputStringCount; minimumStringCount < (_inputStringCount + _minimumString.Length); minimumStringCount++)
            {
                if (minimumStringCount >= inputString.Length)
                {
                    isEqual = false;
                    break;
                }

                intputParsedString += inputString[minimumStringCount];
            }

            // since we moved so many entries we need to move our pointer
            _inputStringCount = _inputStringCount + _minimumString.Length;

            if (long.TryParse(intputParsedString, out intputParsedLong))
            {
                /// only check the max if it is not null
                if (_maximum != null)
                {
                    if ((intputParsedLong < (long)_minimum) || (intputParsedLong > (long)_maximum))
                    {
                        isEqual = false;
                    }
                }
                else
                {
                    /// if we have no max the only option is the minimum option
                    for (int count = 0; count < _minimumString.Length; count++)
                    {
                        if (Convert.ToInt32(inputString[count]) > Convert.ToInt32(_minimumString[count]))
                        {
                            isEqual = false;
                        }
                    }

                    //if (intputParsedLong != (long)_minimum)
                    //{
                    //    isEqual = false;
                    //}
                }
            }
            else
            {
                isEqual = false;
            }

            /// now check against the max and min
            /// we have done our work. Reset the entries
            _minimum = null;
            _minimumString = string.Empty;
            _maximum = null;
            _maximumString = string.Empty;
            _rangeMode = RangeMode.NotSet;

            return isEqual;
        }
        /// <summary>
        /// With the data that is already loaded do we have a good one?
        /// </summary>
        /// <returns></returns>
        private bool DateCheck()
        {
            #region year check
            if (_yearCheck)
            {
                /// do we need to add the century?
                if (_year < 100)
                {
                    /// remove the year from the century than add as
                    /// a century to our current year
                    _year += (DateTime.Now.Year / 100) * 100;
                }
            }
            else
            {
                _year = DateTime.Now.Year;
            }
            #endregion

            #region month check
            if (!_monthCheck)
            {
                _month = DateTime.Now.Month;
            }
            #endregion

            #region day check
            if (!_dayCheck)
            {
                /// we set to 1 because all months have 
                /// day 1
                _day = 1;
            }
            #endregion

            // now try to parse and if it is valid
            DateTime tempDateTime;

            /// we must set the correct date. Since we are formating in english US we 
            /// need to set the culture to that.
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

            /// now get the date to parse from string to what we need
            return DateTime.TryParse(String.Format("{0}/{1}/{2}", _month, _day, _year), culture, DateTimeStyles.None, out tempDateTime);
        }
        #endregion


        #region public static methods
        /// <summary>
        /// Applies the mask on the string sent in and returns
        /// the applied string.
        /// </summary>
        /// <param name="stringIn"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static string ApplyMask(string text, string mask)
        {
            // we need to remove the "(EM=" and the ")" at the end
            mask = mask.Trim();

            /// remove the "( EM =" and ")"
            if (mask.StartsWith("(") && mask.EndsWith(")"))
            {
                mask = mask.Substring(1, mask.Length - 2);
                mask = mask.Trim();
            }

            if (mask.StartsWith("EM"))
            {
                mask = mask.Substring(2, mask.Length - 2);
                mask = mask.TrimStart();

                if (mask.StartsWith("="))
                {
                    mask = mask.Substring(1, mask.Length - 1);
                    mask = mask.TrimStart();
                }
            }

            string returnString = string.Empty;

            /// do we need to expand our mask? Expanding means
            /// the duplicate itms when paratesis are set
            while (mask.Contains("("))
            {
                /// find the first counter
                string stringCounter = mask.Substring(mask.IndexOf("(") + 1);
                stringCounter = stringCounter.Substring(0, stringCounter.IndexOf(")"));
                int numberOfRepeats = Convert.ToInt32(stringCounter);

                /// get the first and second part of the string removing the 
                /// part between the paratesis "()"
                string firstPart = mask.Substring(0, mask.IndexOf("("));
                string secondPart = mask.Substring(mask.IndexOf(")") + 1);

                for (int count = 1; count < numberOfRepeats; count++)
                {
                    firstPart += firstPart[firstPart.Length - 1];
                }

                /// now set the new mask opened up
                mask = firstPart + secondPart;
            }

            /// if we have an X that means we are going to be text driven
            if (mask.Contains("X"))
            {
                #region string mask
                int textCount = 0;
                for (int maskCount = 0; maskCount < mask.Length; maskCount++)
                {
                    if (mask[maskCount] == 'X')
                    {
                        returnString += text[textCount];
                        textCount++;
                    }
                    else if (mask[maskCount] == '^')
                    {
                        returnString += " ";
                    }
                    else
                    {
                        returnString += mask[maskCount];
                    }
                }
                #endregion
            }
            else if (mask.Contains("Z") || mask.Contains("9") || mask.Contains(NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator))
            {
                #region numeric mask
                // this is a numeric mask

                // Check for Negative sign
                bool valueIsNegative = false;
                if (text.Contains("-"))
                {
                    valueIsNegative = true;
                    text = text.Replace("-", "");
                }
                // Check for leading or trailing signs on mask
                string leadingSign = string.Empty;
                string trailingSign = string.Empty;
                string leadingChar = string.Empty;
                if (mask[0] == NumberFormatInfo.CurrentInfo.NegativeSign[0] || mask[0] == NumberFormatInfo.CurrentInfo.PositiveSign[0])
                {
                    leadingSign = mask[0].ToString();
                    mask = " " + mask.Remove(0, 1);
                }
                if (mask[mask.Length - 1] == NumberFormatInfo.CurrentInfo.NegativeSign[0] || mask[mask.Length - 1] == NumberFormatInfo.CurrentInfo.PositiveSign[0])
                {
                    trailingSign = mask[mask.Length - 1].ToString();
                    mask = mask.Remove(mask.Length - 1) + " ";
                }

                if (mask[0] != '9' && mask[0] != '-' && mask[0] != 'Z' && mask[0] != 'H' && mask[0] != 'X' && leadingSign == string.Empty)
                {
                    leadingChar = mask[0].ToString();
                    mask = " " + mask.Remove(0, 1);
                }

                /// get the value right
                string decimalNible = string.Empty;
                string numericNible = string.Empty;

                // do we have a decimal point?
                if (text.Contains(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator))
                {
                    decimalNible = text.Substring(text.IndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator) + 1);
                    numericNible = text.Substring(0, text.IndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator));
                }
                else
                {
                    decimalNible = string.Empty;
                    numericNible = text;
                }

                /// get the mask right
                string decimalMask = string.Empty;
                string numericMask = string.Empty;

                /// does the mask has the decimal point?
                if (mask.Contains(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator))
                {
                    decimalMask = mask.Substring(mask.IndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator) + 1);
                    numericMask = mask.Substring(0, mask.IndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator));
                }
                else
                {
                    decimalMask = string.Empty;
                    numericMask = mask;
                }

                int countDecimalNible = 0;

                /// do he decimal first
                for (int countDecimalMask = 0; countDecimalMask < decimalMask.Length; countDecimalMask++)
                {
                    switch (decimalMask[countDecimalMask])
                    {
                        case '9':// zero is not suppressed
                            if (decimalNible.Length <= countDecimalNible)
                            {
                                returnString += '0';
                            }
                            goto case 'Z';
                        case 'Z': // zero supressed
                            if (decimalNible.Length > countDecimalNible)
                            {
                                returnString += decimalNible[countDecimalNible];
                                countDecimalNible++;
                            }
                            break;
                        case '-': // zero supressed with -
                            if (decimalNible.Length > countDecimalNible)
                            {
                                returnString += decimalNible[countDecimalNible];
                                countDecimalNible++;
                            }
                            break;
                        default:
                            /// this is some sort of charater we need to insert
                            returnString += decimalMask[countDecimalMask];
                            break;
                    }
                }

                /// if we have a decimal mask we need a decimal point
                if (decimalMask.Length > 0)
                {
                    returnString = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator + returnString;
                }

                int countNumericNible = numericNible.Length - 1;
                /// on the numeric area we go backwards
                /// so we can get all the numbers
                for (int countNumericMask = (numericMask.Length - 1); countNumericMask >= 0; countNumericMask--)
                {
                    switch (numericMask[countNumericMask])
                    {
                        case '9':
                            if (countNumericNible < 0)
                            {
                                returnString = '0' + returnString;
                            }
                            goto case 'Z';
                        case 'Z':
                            if (numericNible.Length > countNumericNible)
                            {
                                if (countNumericNible >= 0)
                                {
                                    if ((numericNible.Substring(0, countNumericNible + 1).TrimStart(' ', '0').Length > 0) ||
                                        (numericMask[countNumericMask] == '9'))
                                    {
                                        returnString = numericNible[countNumericNible] + returnString;
                                    }
                                    else
                                    {
                                        returnString = " " + returnString;
                                    }
                                    countNumericNible--;
                                }
                                else if ((numericMask[countNumericMask] >= 0) && (numericMask[countNumericMask] != '9'))
                                {
                                    returnString = " " + returnString;
                                }
                            }
                            break;
                        case ',':
                            if (numericNible.Length > countNumericNible)
                            {
                                if (returnString[0] == ' ' && NumberFormatInfo.CurrentInfo.NumberGroupSeparator == ",")
                                {
                                    returnString = " " + returnString;
                                }
                                else
                                {
                                    returnString = "," + returnString;
                                }
                            }
                            break;
                        case '-':
                            if (numericNible.Length > countNumericNible)
                            {
                                if (countNumericNible >= 0)
                                {
                                    if ((numericNible.Substring(0, countNumericNible + 1).TrimStart(' ', '0').Length > 0) &&
                                        (numericMask[countNumericMask] == '-'))
                                    {
                                        returnString = numericNible[countNumericNible] + returnString;
                                    }
                                    else
                                    {
                                        returnString = " " + returnString;
                                    }
                                    countNumericNible--;
                                }
                                else if ((numericMask[countNumericMask] >= 0) && (numericMask[countNumericMask] != '-'))
                                {
                                    returnString = " " + returnString;
                                }
                            }
                            break;
                        case '.':
                            if (numericNible.Length > countNumericNible)
                            {
                                if (returnString[0] == ' ' && NumberFormatInfo.CurrentInfo.NumberGroupSeparator == ".")
                                {
                                    returnString = " " + returnString;
                                }
                                else
                                {
                                    returnString = "." + returnString;
                                }
                            }
                            break;
                        default:
                            if ((decimalMask.Length <= 0) || (countNumericNible >= 0))
                            {
                                /// this is some sort of charater we need to insert
                                returnString = numericMask[countNumericMask] + returnString;
                            }
                            break;
                    }
                }
                // Handle Leading or traing signs
                returnString = returnString.Trim();
                if (returnString.Length > 0)
                {
                    if (returnString[0].ToString().Length > 0)
                    {
                        if (returnString[0] == NumberFormatInfo.CurrentInfo.NumberGroupSeparator[0])
                        {
                            returnString = returnString.Remove(0, 1);
                        }
                    }
                }
                if (valueIsNegative)
                {
                    if (trailingSign != string.Empty)
                        returnString = returnString + NumberFormatInfo.CurrentInfo.NegativeSign;
                    if (leadingSign != string.Empty)
                        returnString = NumberFormatInfo.CurrentInfo.NegativeSign + returnString;
                }
                else
                {
                    if (trailingSign == NumberFormatInfo.CurrentInfo.PositiveSign)
                    {
                        returnString = returnString + trailingSign;
                    }
                    else
                    {
                        returnString = returnString + " ";
                    }

                    if (leadingSign == NumberFormatInfo.CurrentInfo.PositiveSign)
                        returnString = leadingSign + returnString;
                }
                //Handle other leading char
                if (leadingChar != string.Empty)
                {
                    returnString = returnString.PadLeft(mask.Length - 1);
                    returnString = leadingChar + returnString;
                }
                else
                {
                    returnString = returnString.PadLeft(mask.Length);
                }
                #endregion
            }
            else if (mask.Contains("D") || mask.Contains("M") || mask.Contains("Y") || mask.Contains("H") || mask.Contains("I") || mask.Contains("S") || mask.Contains("T"))
            {
                #region date time mask
                DateTime dateTime;

                if (DateTime.TryParse(text, out dateTime))
                {
                    mask = mask.Replace("I", "m").Replace("Y", "y").Replace("D", "d");

                    returnString = dateTime.ToString(mask);
                }
                else
                {
                    throw new ArgumentException(String.Format("The mask {0} implies this is a date time mask, but could not parse {1} to a date time format", mask, text));
                }
                #endregion
            }
            else
            {
                throw new ArgumentException(String.Format("The mask {0} is not implemented or incorrect.", mask));
            }

            return returnString;
        }

        /// <summary>
        /// Matches the Mask for the Natural system
        /// </summary>
        /// <param name="inputString">the string to be checked against</param>
        /// <param name="maskString">the natural mask string</param>
        /// <returns></returns>
        public static bool Match(string inputString, string maskString)
        {
            return Match(inputString, maskString, string.Empty);
        }
        /// <summary>
        /// Matches the Mask
        /// </summary>
        /// <param name="inputString">the string to be checked against</param>
        /// <param name="maskString">the natural mask string</param>
        /// <param name="operand2">The second operand in case we are using X on the maskString</param>
        /// <returns></returns>
        public static bool Match(string inputString, string maskString, string operand2)
        {
            bool matched = false;

            /// create the object to run.
            Mask mask = new Mask();
            matched = mask.Compare(inputString, maskString, operand2);

            return matched;
        }
        #endregion
    }
}















































/// Praise the LORD, O my soul. 
/// O LORD my God, you are very great; 
/// you are clothed with splendor and majesty.
///                             - Psalm 104:1