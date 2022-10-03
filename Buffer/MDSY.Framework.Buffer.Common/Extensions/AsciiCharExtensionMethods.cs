using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Extension methods for AsciiChar 
    /// </summary>
    public static class AsciiCharExtensionMethods
    {
        /// <summary>
        /// Indicates whether the character is categorized as a control character.
        /// </summary>
        public static bool IsControl(this AsciiChar c)
        {
            return char.IsControl(c.AsChar);
        }

        /// <summary>
        /// Indicates whether the character is categorized as a decimal digit.
        /// </summary>
        public static bool IsDigit(this AsciiChar c)
        {
            return char.IsDigit(c.AsChar);
        }

        /// <summary>
        /// Indicates whether the character is categorized as a letter.
        /// </summary>
        public static bool IsLetter(this AsciiChar c)
        {
            return char.IsLetter(c.AsChar);
        }

        /// <summary>
        /// Indicates whether the character is categorized as a letter or a decimal digit.
        /// </summary>
        public static bool IsLetterOrDigit(this AsciiChar c)
        {
            return char.IsLetterOrDigit(c.AsChar);
        }

        /// <summary>
        /// Indicates whether the character is categorized as a lowercase letter.
        /// </summary>
        public static bool IsLower(this AsciiChar c)
        {
            return char.IsLower(c.AsChar);
        }

        /// <summary>
        /// Indicates whether the character is categorized as a number.
        /// </summary>
        public static bool IsNumber(this AsciiChar c)
        {
            return char.IsNumber(c.AsChar);
        }

        /// <summary>
        /// Indicates whether the character is categorized as a punctuation mark.
        /// </summary>
        public static bool IsPunctuation(this AsciiChar c)
        {
            return char.IsPunctuation(c.AsChar);
        }

        /// <summary>
        /// Indicates whether the character is categorized as a separator character.
        /// </summary>
        public static bool IsSeparator(this AsciiChar c)
        {
            return char.IsSeparator(c.AsChar);
        }

        /// <summary>
        /// Indicates whether the character is categorized as a symbol character.
        /// </summary>
        public static bool IsSymbol(this AsciiChar c)
        {
            return char.IsSymbol(c.AsChar);
        }

        /// <summary>
        /// Indicates whether the character is categorized as an uppercase letter.
        /// </summary>
        public static bool IsUpper(this AsciiChar c)
        {
            return char.IsUpper(c.AsChar);
        }

        /// <summary>
        /// Indicates whether the character is categorized as white space.
        /// </summary>
        public static bool IsWhiteSpace(this AsciiChar c)
        {
            return char.IsWhiteSpace(c.AsChar);
        }

        /// <summary>
        /// Converts the character to its lowercase equivalant. 
        /// </summary>
        public static AsciiChar ToLower(this AsciiChar c)
        {
            return new AsciiChar(char.ToLower(c.AsChar));
        }

        /// <summary>
        /// Converts the character to its uppercase equivalant. 
        /// </summary>
        public static AsciiChar ToUpper(this AsciiChar c)
        {
            return new AsciiChar(char.ToUpper(c.AsChar));
        }

    }
}

