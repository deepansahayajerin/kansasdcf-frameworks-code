using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Services;
using MDSY.Framework.Buffer.Unity;
using Microsoft.Extensions.Configuration;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Buffer
{

    /// <summary>
    /// Extension methods for objects which implement IRecord.
    /// </summary>
    public static class IRecordExtensions
    {
        #region private

        private static string BytesToString(byte[] bytes)
        {
            return bytes.Select(b => AsciiChar.From(b)).NewString();
        }

        ///// <summary>
        ///// Adds the given <paramref name="field"/> and any associated CheckFields 
        ///// to the <paramref name="structDef"/> currently being defined.
        ///// </summary>
        //private static void AddFieldToStructure(IField field, IStructureDefinition structDef)
        //{
        //    //if (field.IsInRedefine)

        //    var newField = structDef.CreateNewField(field.Name, field.FieldType, field.DisplayLength);

        //    foreach (var chk in field.CheckFields)
        //    {
        //        newField.CreateNewCheckField(chk.Name, chk.Check);
        //    }
        //}


        ///// <summary>
        ///// Adds the given <paramref name="group"/> and its child elements to the 
        ///// <paramref name="structDef"/> currently being defined.
        ///// </summary>
        //private static void AddGroupToStructure(IGroup group, IStructureDefinition structDef)
        //{

        //}

        ///// <summary>
        ///// Adds the given <paramref name="array"/> and its child items to the <paramref name="structDef"/>
        ///// currently being defined.
        ///// </summary>
        ///// <typeparam name="TItem"></typeparam>
        ///// <param name="array"></param>
        ///// <param name="structDef"></param>
        //private static void AddArrayToStructure<TItem>(IArray<TItem> array, IStructureDefinition structDef)
        //{
        //    throw new NotImplementedException();
        //}



        //private static IRecord AddToStructureUnderParentCollection(IRecord record, IElementCollection newContent, IElementCollection newContentParent)
        //{
        //    IRecord result = BufferServices.Factory.NewRecord(record.Name,
        //        def =>
        //        {
        //            foreach (IBufferElement element in record.Elements)
        //            {
        //                if (element is IField)
        //                {
        //                    AddFieldToStructure(element as IField, def);
        //                }
        //                else if (element is IGroup)
        //                {
        //                    AddGroupToStructure(element as IGroup, def);
        //                }
        //                else if (element is IArrayBase)
        //                {
        //                    AddArrayToStructure<IGroup>(element as IArray<IGroup>, def);
        //                }
        //                else
        //                {
        //                    throw new MDSY.Framework.Buffer.Common.ElementCollectionException("Unrecognized IBufferElement type encountered during record cloning.");
        //                }


        //                //// if we find the specified newContentParent while traversing elements, 
        //                //// we need to add the newContent to the end of it.
        //                //if ((record != newContentParent) && (element == newContentParent)) //||
        //                ////((element is IElementCollection) &&
        //                //// ((element as IElementCollection).ContainsElementNested((newContentParent as IBufferElement).Name))))
        //                //{

        //                //}
        //                //else
        //                //{
        //                //    AddElementToStructure(element, def);
        //                //}
        //            }

        //            if (record == newContentParent)
        //            {
        //                AddCollectionToStructure();
        //            }
        //        });

        //}
        #endregion

        #region public methods

        /// <summary>
        /// Returns contents of the current record object.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <returns>Returns contents of the current record object.</returns>
        public static byte[] AsBytes(this IRecord instance)
        {
            if (instance == null)
                return null;
            if (instance.Buffer.Length == 0)
                return new byte[] { };
            else
                return instance.Buffer.ReadBytes();
        }

        /// <summary>
        /// Assigns contents of the provided record object to the current record object.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <param name="other">A reference to the record object, which contents should be assigned to the current record object.</param>
        public static void AssignFrom(this IRecord instance, IRecord other)
        {
            instance.AssignFrom(other.Buffer.ReadBytes());
        }

        /// <summary>
        /// Returns a string representation of the subset of the value of the Record's buffer object, as specified by 
        /// <paramref name="start"/> and <paramref name="count"/>.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <param name="start">Start index.</param>
        /// <param name="count">The number of the bytes to be read from the buffer.</param>
        /// <returns>Returns a string representation of the subset of the value of the Record's buffer object.</returns>
        public static string AsString(this IRecord instance, int start, int count)
        {
            var bytes = instance.Buffer.ReadBytes(start, count);
            return BytesToString(bytes);
        }

        /// <summary>
        /// Returns a string representation of the value of the Record's buffer object.
        /// </summary>
        /// <param name="instance">A refernce to the current record object.</param>
        /// <returns>Returns a string representation</returns>
        public static string AsString(this IRecord instance)
        {
            if (instance.IsMinValue())
            {
                instance.ResetToInitialValue();
            }
            var bytes = instance.Buffer.ReadBytes();
            return BytesToString(bytes);
        }
        /// <summary>
        /// Returns an element of type <typeparamref name="T"/> with the given <paramref name="name"/> 
        /// from the IRecord's StructuralElements, if found.
        /// </summary>
        /// <typeparam name="T">The type of element for which to search.</typeparam>
        /// <param name="instance">The record in which to search.</param>
        /// <param name="name">The name of the buffer element for which to search.</param>
        /// <returns>If found, returns the element with the given <paramref name="name"/>, otherwise, returns 
        /// default for type <typeparamref name="T"/> (likely <c>null</c>).</returns>
        public static T RecordElement<T>(this IRecord instance, string name)
            where T : class, IBufferElement
        {
            return instance.ContainsStructuralElement(name) ?
                       instance.StructureElementByName(name) as T :
                       default(T);
        }

        /// <summary>
        /// Returns an array element of type <typeparamref name="T"/> with the given <paramref name="name"/> and 
        /// <paramref name="arrayIndexes"/> from the IRecord's StructuralElements, if found.
        /// </summary>
        /// <typeparam name="T">The type of element for which to search.</typeparam>
        /// <param name="instance">The record in which to search.</param>
        /// <param name="name">The name of the buffer element for which to search.</param>
        /// <param name="arrayIndexes">The array index value(s) for which to search if <paramref name="name"/> does not 
        /// contain index information.</param>
        /// <returns>If found, returns the element with the given <paramref name="name"/>, otherwise, returns 
        /// default for type <typeparamref name="T"/> (likely <c>null</c>).</returns>
        public static T RecordElement<T>(this IRecord instance, string name, params int[] arrayIndexes)
            where T : class, IBufferElement
        {
            var svc = UnitySingleton.Container.ResolveType<IArrayUtilitiesService>();
            string idxName = svc.MakeElementName(name, arrayIndexes);
            return RecordElement<T>(instance, idxName);
        }

        /// <summary>
        /// Causes the record to restore all its children's values to their original data.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        public static void ResetToInitialValue(this IRecord instance)
        {
            instance.ResetInitialValue();
        }

        /// <summary>
        /// Initializes the current record object with low values.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        public static void InitializeWithLowValues(this IRecord instance)
        {
            foreach (IBufferValue item in instance.Elements)
            {
                item.InitializeWithLowValues();
            }
        }

        /// <summary>
        /// Fills the buffer of the current record with the specified values.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <param name="fillWith">Specifies the filling value.</param>
        public static void FillAllWith(this IRecord instance, FillWith fillWith)
        {
            AsciiChar asciiChar = AsciiChar.From(0x00);
            switch (fillWith)
            {
                case Common.FillWith.DontFill:
                case Common.FillWith.Nulls:
                    break;
                case Common.FillWith.Spaces:
                    asciiChar = AsciiChar.From(' ');
                    break;
                case Common.FillWith.Zeroes:
                    asciiChar = AsciiChar.From('0');
                    break;
                case Common.FillWith.Hashes:
                    asciiChar = AsciiChar.From(' ');
                    break;
                case FillWith.Equals:
                    asciiChar = AsciiChar.From('=');
                    break;
                case FillWith.Underscores:
                    asciiChar = AsciiChar.From('_');
                    break;
                case Common.FillWith.Dashes:
                    asciiChar = AsciiChar.From('-');
                    break;
                case FillWith.HighValues:
                    asciiChar = AsciiChar.MaxValue;
                    break;
            }

            FillWithByte(instance, asciiChar.AsByte);
        }

        /// <summary>
        /// Fills the current record with the specified byte value.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <param name="value">Specifies the filling byte value.</param>
        public static void FillWithByte(this IRecord instance, byte value)
        {
            var bytes = Enumerable.Repeat<byte>(value, instance.Buffer.Length).ToArray();
            instance.Buffer.WriteBytes(bytes);
        }

        /// <summary>
        /// Emulates COBOL statement <c>SET ADDRESS OF RECORD_A TO ADDRESS OF RECORD_B</c>.
        /// Causes the record object to point its buffer reference to the 
        /// buffer of the given <paramref name="other"/>.
        /// </summary>
        /// <param name="instance">The record whose buffer pointer is being changed.</param>
        /// <param name="other">The record object whose DataBuffer this record 
        /// will now point to.</param>
        /// <remarks>To restore the record's original Buffer Pointer mapping, 
        /// call IRecord.RestoreInitialDataBuffer().</remarks>
        public static void SetBufferReference(this IRecord instance, IRecord other)
        {
            instance.SetAddressToAddressOf(other);
        }

        /// <summary>
        /// Fills the buffer of the current record object with the spaces.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        public static void SetValueWithSpaces(this IRecord instance)
        {
            FillAllWith(instance, Common.FillWith.Spaces);
        }

        /// <summary>
        /// Fills the buffer of the current record object with the zeroes.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        public static void SetValueWithZeroes(this IRecord instance)
        {
            FillAllWith(instance, Common.FillWith.Zeroes);
        }

        /// <summary>
        /// Fills the buffer of the current record object with the byte minimum value.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        public static void SetMinValue(this IRecord instance)
        {
            FillWithByte(instance, byte.MinValue);
        }

        /// <summary>
        /// Fills the buffer of the current record object with the byte maximum value.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        public static void SetMaxValue(this IRecord instance)
        {
            FillWithByte(instance, byte.MaxValue);
        }

        /// <summary>
        /// Checks whether the current record contains only specified byte value.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <param name="byteValue">Specifies the byte value for the comparison.</param>
        /// <returns>Returns true if the current record contains only specified byte value.</returns>
        public static bool ContainsOnly(this IRecord instance, byte byteValue)
        {
            return instance.Buffer.ReadBytes().All(b => b.Equals(byteValue));
        }

        /// <summary>
        /// Checks whether the current record contains only spaces.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <returns>Returns true if the current record contains only spaces.</returns>
        public static bool IsSpaces(this IRecord instance)
        {
            return ContainsOnly(instance, AsciiChar.From(' ').AsByte);
        }

        /// <summary>
        /// Checks whether the current record contains only byte maximum values.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <returns>Returns true if the current record contains only byte maximum values.</returns>
        public static bool IsMaxValue(this IRecord instance)
        {
            return ContainsOnly(instance, byte.MaxValue);
        }

        /// <summary>
        /// Checks whether the current record contains only byte minimum values.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <returns>Returns true if the current record contains only byte minimum values.</returns>
        public static bool IsMinValue(this IRecord instance)
        {
            return ContainsOnly(instance, byte.MinValue);
        }

        /// <summary>
        /// Checks whether the current record contains only zeroes.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <returns>Returns true if the current record contains only zeroes.</returns>
        public static bool IsZeroes(this IRecord instance)
        {
            return ContainsOnly(instance, AsciiChar.From('0').AsByte);
        }

        /// <summary>
        /// Checks whether the current record does not contain spaces.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <returns>Returns true if the current record does not contain spaces.</returns>
        public static bool IsNotSpaces(this IRecord instance)
        {
            return !(IsSpaces(instance));
        }

        /// <summary>
        /// Checks whether the current record does not contain zeroes.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <returns>Returns true if the current record does not contain zeroes.</returns>
        public static bool IsNotZeroes(this IRecord instance)
        {
            return !(IsZeroes(instance));
        }

        /// <summary>
        /// Checks whether the current record contains only zeroes.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <returns>Returns true if the current record contains only zeroes.</returns>
        public static bool IsNotMaxValue(this IRecord instance)
        {
            return !(IsMaxValue(instance));
        }

        /// <summary>
        /// Checks whether the current record contains only zeroes.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <returns>Returns true if the current record contains only zeroes.</returns>
        public static bool IsNotMinValue(this IRecord instance)
        {
            return !(IsMinValue(instance));
        }

        /// <summary>
        /// Checks whether the current record value is equal to the specified record value.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <param name="other">A reference to the other record object for the comparison.</param>
        /// <returns>Returns true if the current record value is equal to the provided record value.</returns>
        public static bool IsEqualTo(this IRecord instance, IRecord other)
        {
            return instance.Equals(other);
            //   return (instance.AsString().CompareTo(other.AsString()) == 0);
        }

        /// <summary>
        /// Checks whether the current record value is  not equal to the specified record value.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <param name="other">A reference to the other record object for the comparison.</param>
        /// <returns>Returns true if the current record value is not equal to the provided record value.</returns>
        public static bool IsNotEqualTo(this IRecord instance, IRecord other)
        {
            return !instance.Equals(other);
            //return !(instance.AsString().CompareTo(other.AsString()) == 0);
        }

        /// <summary>
        /// Checks whether the current record value is equal to the specified string value.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <param name="other">A reference to the string for the comparison.</param>
        /// <returns>Returns true if the current record value is equal to the specified string value.</returns>
        public static bool IsEqualTo(this IRecord instance, string other)
        {
            return instance.Equals(other);
        }

        public static bool IsEqualTo(this IRecord instance, IField other)
        {
            return instance.Equals(other);
        }

        /// <summary>
        /// Checks whether the current record value is not equal to the specified string value.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <param name="other">A reference to the string for the comparison.</param>
        /// <returns>Returns true if the current record value is not equal to the specified string value.</returns>
        public static bool IsNotEqualTo(this IRecord instance, string other)
        {
            return !instance.Equals(other);
            //return !(instance.AsString().CompareTo(other) == 0);
        }

        /// <summary>
        /// Sets current date and time values to the DATE_TODAY adn TIME_TODAY fields of the current record.
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        public static void SetStatistics(this IRecord instance)
        {
             IField FLD = (IField)instance.GetElementByNameNested("DATE_TODAY");
             if (FLD != null)
             {
                 String dateFormat = "MM/DD/YY";
                 if (!(String.IsNullOrEmpty(ConfigSettings.GetAppSettingsString("DateFormat"))))
                 {
                     dateFormat = ConfigSettings.GetAppSettingsString("DateFormat");
                 }
                 if (dateFormat == "MM/DD/YY")
                     FLD.SetValue(DateTime.Today.ToString("MM/dd/yy"));
                 else if (dateFormat == "DD/MM/YY")
                     FLD.SetValue(DateTime.Today.ToString("dd/MM/yy"));
                 else if (dateFormat == "YYMMDD")
                     FLD.SetValue(DateTime.Today.ToString("yyMMdd"));
                 else if (dateFormat == "YYYYMMDD")
                     FLD.SetValue(DateTime.Today.ToString("yyyyMMdd"));

             }
             IField FLD2 = (IField)instance.GetElementByNameNested("TIME_TODAY");
             if (FLD2 != null)
             {
                 FLD2.SetValue(DateTime.Now.TimeOfDay.ToString("hhmmssff"));
             }
     
        }

        /// <summary>
        /// Returns a substring of IRecord value (position based)
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <param name="length">Specifies the number of bytes in the substring.</param>
        /// <returns>Returns a substring obtained from the current record value.</returns>
        public static string GetSubstring(this IRecord instance, int startPosition, int length)
        {
            if (instance.AsString().Length < length)
                length = instance.AsString().Length;
            return instance.AsString().Substring(startPosition - 1, length);
        }

        /// <summary>
        /// Returns a substring of IRecord value (position based)
        /// </summary>
        /// <param name="instance">A reference to the current record object.</param>
        /// <param name="startPosition">Specifies start index.</param>
        /// <returns>Returns a substring obtained from the current record value.</returns>
        public static string GetSubstring(this IRecord instance, int startPosition)
        {
            return instance.AsString().Substring(startPosition - 1);
        }
        #endregion
    }
}
