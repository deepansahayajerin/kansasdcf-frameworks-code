using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.IO.Common
{

    public static class ByteListExtension
    {
        /// Replaces one byte for another. 
        /// </summary>
        /// <param name="instanceList"></param>
        /// <param name="oldByte">The byte to be replaced.</param>
        /// <param name="newByte">The byte to replace all occurrences of oldByte.If set to null that position will be removed from the list.</param>
        public static void Replace(this List<byte> instanceList, byte oldByte, byte? newByte)
        {
            for (int count = 0; count < instanceList.Count; count++)
            {
                if (instanceList[count] == oldByte)
                {
                    if (newByte == null)
                    {
                        instanceList.RemoveAt(count);
                        count--;
                    }
                    else
                    {
                        instanceList[count] = (byte)newByte;
                    }
                }
            }
        }
        /// <summary>
        /// Replaces one byte for another. 
        /// </summary>
        /// <param name="instanceList"></param>
        /// <param name="oldByte">The byte to be replaced.</param>
        /// <param name="newByte">The byte to replace all occurrences of oldByte.If set to null that position will be removed from the list.</param>
        public static void Replace(this List<byte> instanceList, List<byte> oldByte, List<byte> newByte)
        {
            int startIndex = -1;
            int oldByteIndex = 0;

            for (int count = 0; count < instanceList.Count; count++)
            {
                if (instanceList[count] == oldByte[oldByteIndex])
                {
                    if (newByte == null)
                    {
                        instanceList.RemoveAt(count);
                        count--;
                    }
                    else
                    {
                        if (startIndex == -1)
                        {
                            startIndex = count;
                        }
                        oldByteIndex++;

                        /// the whole string was found. Time to act on it
                        if (oldByte.Count == oldByteIndex)
                        {
                            /// remove old first
                            for (int countOld = startIndex; countOld < oldByte.Count; countOld++)
                            {
                                instanceList.RemoveAt(countOld);
                            }

                            /// now put new in the place
                            if (newByte != null)
                            {
                                int newByteIndex = 0;
                                for (int countNew = startIndex; countNew < newByte.Count; countNew++)
                                {
                                    instanceList.Insert(countNew, newByte[newByteIndex]);
                                    newByteIndex++;
                                }
                            }

                            oldByteIndex = 0;
                            startIndex = -1;
                        }
                    }
                }
            }
        }
        public static void Trim(this List<byte> instanceList, byte valueToTrim)
        {
            while (instanceList.Count > 0 &&
                ((instanceList[0] == valueToTrim) || (instanceList[instanceList.Count - 1] == valueToTrim)))
            {
                if ((instanceList.Count > 0) && (instanceList[0] == valueToTrim))
                {
                    instanceList.RemoveAt(0);
                }

                if ((instanceList.Count > 0) && (instanceList[instanceList.Count - 1] == valueToTrim))
                {
                    instanceList.RemoveAt(instanceList.Count - 1);
                }
            }
        }
        /// <summary>
        /// Gets the text from the byte list
        /// </summary>
        /// <param name="instanceList"></param>
        /// <param name="stringHex">If set to true will output 2 character hex output. Default is false.</param>
        /// <returns></returns>
        public static string ToHexText(this List<byte> instanceList)
        {
            // TODO need to speed this up.
            StringBuilder value = new StringBuilder();

            foreach (byte instanceItem in instanceList)
            {
                value.Append(instanceItem.ToString("X").PadLeft(2, '0'));
            }

            return value.ToString();
        }

        /// <summary>
        /// Gets the text from the byte list
        /// </summary>
        /// <param name="instanceList"></param>
        /// <returns></returns>
        //public static string ToText(this List<byte> instanceList)
        //{
        //    //----------amended 20121001 145344
        //    //            // TODO need to speed this up.
        //    //            StringBuilder value = new StringBuilder();
        //    //
        //    //            foreach (byte instanceItem in instanceList)
        //    //            {
        //    //                value.Append((char)instanceItem);
        //    //            }
        //    //
        //    //            return value.ToString();
        //    //-----amendment

        //    byte[] bytes = instanceList.ToArray();
        //    char[] result = new char[bytes.Length];

        //    for (int i = 0; i < bytes.Length; i++)
        //    {
        //        result[i] = (char)bytes[i];
        //    }

        //    return new string(result);
        //}

        public static List<byte> PadRight(this List<byte> instanceList, int size, byte value)
        {
            List<byte> result = instanceList.ToList();

            if (result.Count < size)
            {
                result.AddRange(Enumerable.Repeat<byte>(value, size - result.Count).ToList<byte>());
            }

            return result;
        }

        /// <summary>
        /// Pads the right of the Byte List with a list of bytes.
        /// </summary>
        /// <param name="instanceList"></param>
        /// <param name="size"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<byte> PadRight(this List<byte> instanceList, int size, List<byte> value)
        {
            List<byte> result = instanceList.ToList();

            /// add one after the other
            while (result.Count < size)
            {
                result.AddRange(value);
            }

            /// now remove them.
            while (size < result.Count)
            {
                result.RemoveAt(result.Count - 1);
            }

            return result;
        }

        public static List<byte> PadLeft(this List<byte> instanceList, int size, byte value)
        {
            List<byte> result = instanceList.ToList();

            if (result.Count < size)
            {
                result.InsertRange(0, Enumerable.Repeat<byte>(value, size - result.Count).ToList<byte>());
            }

            return result;
        }

        /// <summary>
        /// Parses this text into the ListByte.
        /// </summary>
        /// <param name="instanceList"></param>
        /// <returns></returns>
        public static void ParseText(this List<byte> instanceList, string text)
        {
            foreach (char item in text)
            {
                instanceList.Add((byte)item);
            }
        }

        public static int IndexOf(this List<byte> instanceList, string text)
        {
            int indexOf = -1;

            List<byte> textByteArray = text.ToByteList();

            int textByteArrayCount = 0;

            for (int count = 0; count < instanceList.Count; count++)
            {
                if (instanceList[count] == textByteArray[textByteArrayCount])
                {
                    /// still part of the word
                    if (indexOf == -1)
                    {
                        /// save the current index
                        indexOf = count;
                    }

                    textByteArrayCount++;
                    if (textByteArrayCount == textByteArray.Count)
                    {
                        break;
                    }
                }
                else
                {
                    /// reset all is lost
                    textByteArrayCount = 0;
                    indexOf = -1;
                }
            }

            return indexOf;
        }

        /// <summary>
        /// Returns true if text is found in the byte array
        /// </summary>
        /// <param name="instanceList"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool ContainsString(this List<byte> instanceList, string text)
        {
            return instanceList.IndexOf(text) > 0;
        }

        /// <summary>
        /// Compares each item on the array agaist the compareList.
        /// </summary>
        /// <param name="instanceList"></param>
        /// <param name="compareList"></param>
        /// <returns></returns>
        public static bool CompareListItems(this List<byte> instanceList, List<byte> compareList)
        {
            // after initial JIT compile, SequenceEqual() is faster than iterative comparison. 
            return instanceList.SequenceEqual(compareList);
        }

        public static bool EndsWith(this List<byte> instanceList, string value)
        {
            return instanceList.EndsWith(value.ToByteList());
        }

        public static bool EndsWith(this List<byte> instanceList, List<byte> value)
        {
            bool endWith = true;

            int ratio = instanceList.Count - value.Count;

            if (ratio >= 0)
            {
                for (int count = ratio; count < instanceList.Count; count++)
                {
                    if (value[count - ratio] != instanceList[count])
                    {
                        endWith = false;
                        break;
                    }
                }
            }
            else
            {
                endWith = false;
            }

            return endWith;
        }
        #region compress system
        public static List<byte> FromDecimalToNaturalPacked(this List<byte> as_input, int fieldLength = -1)
        {
            bool negative = false;

            if (fieldLength == -1)
            {
                fieldLength = as_input.Count;
            }

            int ilen;
            int ipos;
            int opos;
            byte sign;
            byte ichar;
            List<byte> as_output = new List<byte>();
            int al_length = (as_input.Count / 2) + 1;

            ilen = as_input.Count;

            for (opos = 0; opos < al_length; opos++)
            {
                as_output.Add(0);
            }

            sign = (byte)(as_input[ilen - 1] - (byte)'0');

            as_output[al_length - 1] = (byte)(sign * 16);
            opos = al_length - 2;

            for (ipos = ilen - 2; ipos >= 0; ipos -= 2)
            {
                if (as_input[ipos] == '-')
                {
                    negative = true;
                }
                else
                {
                    ichar = (byte)(as_input[ipos] - '0');
                    if (ipos > 0)
                    {
                        if (as_input[ipos - 1] == '-')
                        {
                            negative = true;
                        }
                        else
                        {
                            ichar += (byte)((as_input[ipos - 1] - '0') * 16);
                        }
                    }
                    as_output[opos] = ichar;
                    opos--;
                    if (opos < 0)
                    {
                        break;
                    }
                }
            }

            if (negative)
            {
                as_output[al_length - 1] += 13;
            }
            else
            {
                as_output[al_length - 1] += 12;
            }

            if (as_input.Count > fieldLength)
            {
                /// the first byte was the place for the -
                /// sign it. Remove it.
                if ((as_input[0] == '-') && (as_output[0] == 0))
                {
                    as_output.RemoveAt(0);
                }
            }

            return as_output;
        }
        /// <summary>
        /// Gets the buffer output of the Natural Packed Decimal value. 
        /// </summary>
        /// <param name="as_input"></param>
        /// <param name="outputLenght">The lenght of the packed decimal expected.</param>
        /// <returns></returns>
        public static List<byte> FromNaturalPackedToDecimal(this List<byte> as_input, int outputLenght)
        {
            bool negative;

            int al_length = as_input.Count;
            int olen;
            int ipos;
            int opos;
            int ichar;
            int imod;
            List<byte> as_output = new List<byte>();

            olen = (as_input.Count * 2) - 1;

            for (opos = 0; opos < olen; opos++)
            {
                as_output.Add((byte)('0'));
            }

            opos = olen - 1;

            ichar = as_input[al_length - 1];
            if (ichar < 0)
            {
                ichar += 256;
            }

            imod = (ichar % 16);
            if (imod == 13)
            {
                negative = true;
            }
            else
            {
                negative = false;
            }
            ichar -= imod;

            as_output[opos] = (byte)((ichar / 16) + '0');
            opos--;

            for (ipos = (int)(al_length - 2); ipos >= 0; ipos--)
            {
                ichar = as_input[ipos];
                if (ichar < 0)
                {
                    ichar += 256;
                }

                as_output[opos] = (byte)((ichar % 16) + '0');
                opos--;
                if (opos >= 0)
                {
                    as_output[opos] = (byte)((ichar / 16) + '0');
                    opos--;
                }
                else
                {
                    break;
                }

                if (opos < 0)
                {
                    break;
                }
            }

            if (negative)
            {
                if (as_output[0] == 0)
                {
                    as_output[0] = (byte)('-');
                }
                else
                {
                    as_output.Insert(0, (byte)'-');
                }
            }

            if ((as_output[0] != '-') && (as_output.Count > outputLenght))
            {
                as_output.RemoveAt(0);
            }
            else if ((as_output[0] == '-') && (as_output.Count > (outputLenght + 1))) // if there is a negative sign and we are larger still
            {
                as_output.RemoveAt(1);
            }

            return as_output;
        }
        #endregion

        public static string ToText(List<byte> instanceList)
        {
            byte[] bytes = instanceList.ToArray();

            return System.Text.Encoding.Default.GetString(bytes);
        }

        public static string ToText(byte[] instance)
        {
            return System.Text.Encoding.Default.GetString(instance);
        }
    }
}
