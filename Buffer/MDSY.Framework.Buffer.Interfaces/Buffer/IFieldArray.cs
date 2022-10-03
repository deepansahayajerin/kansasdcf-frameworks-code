using MDSY.Framework.Buffer.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which implements IArray(of IField) specifically.
    /// </summary>
    public interface IFieldArray : IArray<IField>
    {
        /// <summary>
        /// Creates and returns the field object which is the first element in the array. 
        /// </summary>
        /// <param name="elementName">Name of the new field object.</param>
        /// <param name="fieldType">The field's type</param>
        /// <param name="fieldDisplayLength">The field's display length</param>
        /// <returns></returns>
        /// <param name="decimalDigits">Indicates the number of digits after the decimal separator.</param>
        /// <param name="initialValue">The object initial value </param>
        IField CreateFirstArrayElement(string elementName, FieldType fieldType, int fieldDisplayLength, int decimalDigits, object initialValue);
    }
}
