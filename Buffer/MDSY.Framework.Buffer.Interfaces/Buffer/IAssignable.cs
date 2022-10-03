using MDSY.Framework.Buffer.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{

    /// <summary>
    /// Defines an object whose value can be assigned.
    /// </summary>
    public interface IAssignable
    {
        #region operations
        /// <summary>
        /// Assigns the given value to the object.
        /// </summary>
        /// <param name="value">The new value to assign to the object.</param>
        void Assign(object value);

        void AssignIdRecordName(string value);
        void AssignIdRecordName(IBufferValue value);
        string GetIdRecordName();

        /// <summary>
        /// Assigns the given string value to the object, as appropriate.
        /// </summary>
        /// <param name="value">The string value to be assigned.</param>
        void AssignFrom(string value);

        /// <summary>
        /// Assigns the value of the given <paramref name="element"/> to this object, as appropriate.
        /// </summary>
        /// <param name="element">Specifies the value to be assigned.</param>
        void AssignFrom(IBufferValue element);

        /// <summary>
        /// Assigns the value of the given <paramref name="element"/> to this object, as appropriate.
        /// </summary>
        /// <param name="element">Specifies the value to be assigned.</param>
        /// <param name="sourceFieldType">Specifies the source field type.</param>
        void AssignFrom(IBufferValue element, FieldType sourceFieldType);

        /// <summary>
        ///  Assigns the value of the given <paramref name="group"/> to this object, as appropriate.
        /// </summary>
        /// <param name="group">Specifies the group value to be assigned.</param>
        void AssignFromGroup(IGroup group);

        /// <summary>
        /// Assigns the given <paramref name="bytes"/> to this object, as appropriate.
        /// </summary>
        /// <param name="bytes">Bytes to be assigned.</param>
        void AssignFrom(byte[] bytes);
        #endregion
    }
}
