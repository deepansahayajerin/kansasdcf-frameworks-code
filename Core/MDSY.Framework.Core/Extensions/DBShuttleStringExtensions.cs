using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Extends string objects with the methods that are required for comparison of the DB shuttle strings.
    /// </summary>
    public static class DBShuttleStringExtensions
    {
        /// <summary>
        /// Specialized IsNumeric() extension method allowing ints, decimals as well as positive/negative signs. 
        /// </summary>
        /// <param name="instanceString"></param>
        /// <remarks>Needs to be re-implemented to use built-in .net numeric checking. </remarks>
        /// <returns></returns>
        //public static bool IsNumeric(this string instanceString)
        //{
        //    bool result = true;
        //    if (instanceString.Length <= 32)
        //    {
        //        char[] chars = instanceString.ToCharArray();
        //        bool foundDecimal = false;

        //        int i = 0;
        //        while ((result == true) && (i < chars.Length))
        //        {
        //            switch (chars[i])
        //            {
        //                case '0':
        //                case '1':
        //                case '2':
        //                case '3':
        //                case '4':
        //                case '5':
        //                case '6':
        //                case '7':
        //                case '8':
        //                case '9':
        //                    break;
        //                case '+':
        //                case '-':
        //                    if (i != 0)
        //                    {
        //                        result = false;
        //                    }
        //                    break;
        //                case '.':
        //                    if (foundDecimal)
        //                    {
        //                        result = false;
        //                    }
        //                    else
        //                    {
        //                        foundDecimal = true;
        //                    }
        //                    break;
        //                default:
        //                    result = false;
        //                    break;
        //            }
        //            i++;
        //        }
        //    }
        //    return result;
        //}

        /// <summary>
        /// Returns <c>true</c> if all characters in the string are the same as the given char.
        /// </summary>
        /// <param name="instanceString">The string object, which is extended with the current method.</param>
        /// <param name="character">char against which to match.</param>
        /// <returns></returns>
        public static bool IsAllSameCharacter(this string instanceString, char character) //TFS213 THS
        {
            int i = 0;
            bool result = true;

            while ((i < instanceString.Length) && (result == true))
            {
                if (instanceString[i] != character)
                {
                    result = false;
                }
                i++;
            }

            return result;
        }
        /// <summary>
        /// Compares the instance string to another for equivalency. If the strings match, returns 0, otherwise
        /// returns the 1-based index of the first character mis-match.
        /// </summary>
        /// <param name="instanceString">The string object, which is extended with the current method.</param>
        /// <param name="compareTo">A string for comparison.</param>
        /// <returns>0 if the strings match.</returns>
        public static int MatchesUntil(this string instanceString, string compareTo)
        {
            int result = 0;

            for (int i = 0; i < instanceString.Length; i++)
            {
                if (compareTo.IndexOf(instanceString[i]) == -1)
                {
                    result = i + 1;    //TFS233 THS Use 1-based string positions. See TFS item for details.
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Compares the instance string to another for equivalency. If the strings match, returns 0, otherwise
        /// returns the 1-based index of the first character mis-match. If caseSensitive is <c>false</c>, 
        /// the strings are compared without case sensitivity.
        /// </summary>
        /// <param name="instanceString">The string object, which is extended with the current method.</param>
        /// <param name="compareTo">A string for comparison.</param>
        /// <param name="caseSensitive">Specifies whether the comparison needs to be case sensitive.</param>
        /// <returns>0 if the strings match.</returns>
        public static int MatchesUntil(this string instanceString, string compareTo, bool caseSensitive)
        {
            int result = 0;
            if (!caseSensitive)
            {
                result = MatchesUntil(instanceString.ToUpper(), compareTo.ToUpper());
            }
            else
            {
                result = MatchesUntil(instanceString, compareTo);
            }

            return result;
        }



    }
}
