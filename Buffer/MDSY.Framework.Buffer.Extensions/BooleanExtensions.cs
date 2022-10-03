using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Common;
using System.Diagnostics;


namespace MDSY.Framework.Buffer
{
    /// <summary>
    /// Extension methods for String
    /// </summary>
    public static class BooleanExtensions
    {

        /// <summary>
        /// Returns the current string as it is.
        /// </summary>
        /// <param name="instance">A reference to the current string.</param>
        /// <returns>Returns the current string as it is.</returns>
        public static string AsString(this bool instance)
        {
            return instance.AsString();
        }

        #region IsEqualTo

        /// <summary>
        /// Returns true if the value of this string is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this bool instance, bool other)
        {
            return (instance == other);
        }

        /// <summary>
        /// Returns true if the value of this string is equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c><paramref name="instance"/>.Equals(<paramref name="other"/>);</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally equivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsEqualTo(this bool instance, int other)
        {
            return (instance && other == 1);
        }


        #endregion

        #region IsNotEqualTo

        /// <summary>
        /// Returns <c>true</c> if the value of this string is not equal to the value of <paramref name="other"/>.
        /// This method is semantically equivalent to <c>!(<paramref name="instance"/>.Equals(<paramref name="other"/>));</c>.
        /// </summary>
        /// <param name="instance">The field object that will perform the comparison.</param>
        /// <param name="other">The value to be compared against.</param>
        /// <returns><c>true</c> if the value of <paramref name="instance"/> is functionally inequivalent to 
        /// <paramref name="other"/>. Otherwise, returns <c>false</c>.</returns>
        public static bool IsNotEqualTo(this bool instance, bool other)
        {
            return !(instance == other);
        }

        #endregion

        public static void SetValue(this bool instance, bool newvalue)
        {
            instance = newvalue;
        }


    }
}
