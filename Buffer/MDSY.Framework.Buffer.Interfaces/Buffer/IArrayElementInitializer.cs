using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which can receive initialization during the creation of array elements. 
    /// </summary>
    /// <remarks>
    /// When an array is created via IStructureDefinition.CreateNewFieldArray(), or the like, as each 
    /// occurrence of the IArray's element is created, an element initializer of type Action&lt;IArrayElementInit, string, int&gt;
    /// is called. 
    /// </remarks>
    /// <seealso cref="IStructureDefinition.CreateNewFieldArray"/>
    /// <seealso cref="IStructureDefinition.CreateNewGroupArray"/>
    /// <seealso cref="IStructureDefinition.NewFieldArray"/>
    /// <seealso cref="IStructureDefinition.NewGroupArray"/>

    /// <summary>
    /// Gets or sets the index of the element within its array. Do not set ArrayElementIndex; its setter is used 
    /// only for internal initialization. 
    /// </summary>
    public interface IArrayElementInitializer : IAssignable, IBufferElementInitializer
    {
        int ArrayElementIndex { get; set; }
    }

    /// <summary>
    /// Returns the field initializer object as an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IArrayElementInitializer<T> : IArrayElementInitializer
        where T : IArrayElement, IBufferElement
    {
        T AsReadOnly();
        IArrayElementAccessor<T> ArrayElementAccessor { get; set; }
        
    }
}
