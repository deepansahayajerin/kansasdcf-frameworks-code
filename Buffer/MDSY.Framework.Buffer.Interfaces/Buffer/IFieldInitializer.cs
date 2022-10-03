using MDSY.Framework.Buffer.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{

    /// <summary>
    /// Defines an object which can receive initialization during the creation of a field. 
    /// </summary>
    public interface IFieldInitializer : IBufferElementInitializer, IAssignable,
        IArrayElementInitializer<IField>
    {
        int DisplayLength { get; set; }

        /// <summary>
        /// Gets or sets the Type of the new field.
        /// </summary>
        FieldType FieldType { get; set; }

        /// <summary>
        /// Gets or sets the number of digits to the right of the decimal for numeric fields.
        /// </summary>
        int DecimalDigits { get; set; }

        /// <summary>
        /// Gets or sets the object's initial value.
        /// </summary>
        object InitialValue { get; set; }

        /// <summary>
        /// Since Checkfields are created by their owner fields and not by StructureDefinitionCompositor
        /// (which normally handles the define-time array element accessors), field initializer
        /// needs to have a reference to the root record's define-time accessor list so the field can 
        /// add accessors for any check fields.
        /// </summary>
        IDictionary<string, IArrayElementAccessorBase> DefineTimeAccessors { get; set; }
    }
}
