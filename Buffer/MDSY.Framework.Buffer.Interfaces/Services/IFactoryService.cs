using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Provides object creation services for buffer elements.
    /// </summary>
    [InjectionInterface]
    public interface IFactoryService
    {
        /// <summary>
        /// Returns a new IRecord-implementing object with the given <paramref name="name"/>, whose structure is defined
        /// by the logic of <paramref name="definition"/>.
        /// </summary>
        /// <param name="name">Value of the new record object's Name property.</param>
        /// <param name="definition">A delegate which defines the data structure of the new record via the given
        /// IStructureDefinition object. See example.</param>
        /// <returns>A new IRecord-implementing object with its structure defined by <paramref name="definition"/>.</returns>
        /// <example>
        /// A simple record definition.
        /// <code>
        /// IRecord record = BufferServices.Factory.NewRecord("Linking93011", (recordDef) =>
        /// {
        ///     recordDef.NewGroupArray("MESSAGES", 3, null, (MESSAGES) =>
        ///     {
        ///         MESSAGES.NewGroup("MESSAGE_INFO", (MESSAGE_INFO) =>
        ///         {
        ///             MESSAGE_INFO.NewField("CODE", FieldType.String, 2);
        ///             MESSAGE_INFO.NewField("NUMBER", FieldType.String, 6);
        ///         });
        ///         MESSAGES.NewField("LIB", FieldType.String, 1);
        ///         MESSAGES.NewField("TEXT", FieldType.String, 71);
        ///     },
        ///     null, null);
        /// });
        /// </code>
        /// </example>
        IRecord NewRecord(string name, Action<IStructureDefinition> definition);
    }

}

