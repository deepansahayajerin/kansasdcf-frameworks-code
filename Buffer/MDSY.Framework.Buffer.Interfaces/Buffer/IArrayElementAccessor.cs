using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which allows access to individual array elements in a manner consistent 
    /// with legacy arrays. 
    /// </summary>
    /// <remarks>
    /// <para>Array element accessing in, say COBOL, is different from C# (see example). 
    /// IArrayElementAccessor provides a way to emulate legacy array management. </para>
    /// <para>For each array element in a record structure an IArrayElementAccessor will
    /// be added to the Record's ArrayElementAccessor collection, each with the 
    /// original declared name of the array element. This should take the place 
    /// of individual direct-access element properties declared in a predefined
    /// record class. e.g. for a non-array element, a declaration like this is 
    /// generated:</para>
    /// <code>public IField G9301_TEXT_MESSAGE { get { return GetElementByName&lt;IField&gt;(Names.G9301_TEXT_MESSAGE); } }</code>
    /// <para>But to generate one of these for every element in a multidimensional array 
    /// would be excessive and would not lend itself to emulating legacy code.</para>
    /// <para>Thus for each array, a direct-access declaration will be added that 
    /// returns an IArrayElementAccessor.</para>
    /// </remarks>
    /// <example>
    /// Assume the following record structure:
    /// <code>
    /// myRecord
    ///  - MYGROUPARRAY [10]
    ///    - MYFIELD
    ///    - MYFIELDARRAY [10]
    /// </code>
    /// <para>There are now 10 occurrences of <c>MYGROUPARRAY</c> each with 10 occurrences 
    /// of <c>MYFIELDARRAY</c> for 100 individual field instances.</para>
    /// <para>Say we wish to access the 4th field of the 2nd group. In C#, we 
    /// might see something like:</para>
    /// <code>myRecord.myGroupArray[2].myFieldArray[4]</code>
    /// <para>But in COBOL, we could see:</para>
    /// <code>MYFIELDARRAY[2,4]</code>
    /// <para>Instead of 100 direct-access IField properties added to the pre-defined
    /// record, we'll generate the something like the following:</para>
    /// <code>
    /// public IArrayElementAccessor&lt;IGroup&gt; MYGROUPARRAY { get { return GetElementAccessor&lt;IGroup&gt;(Names.MYGROUPARRAY); } }
    /// public IArrayElementAccessor&lt;IField&gt; MYFIELDARRAY { get { return GetElementAccessor&lt;IField&gt;(Names.MYFIELDARRAY); } }
    /// </code>
    /// </example>
    /// <typeparam name="TItem">The type of the array element.</typeparam>
    public interface IArrayElementAccessor<TItem> : IArrayElementAccessorBase
        where TItem : IArrayElement
    {
        /// <summary>
        /// Multidimensional indexer property for array elements.
        /// </summary>
        TItem this[params int[] index] { get; }

        /// <summary>
        /// Multidimensional indexer property for array elements using numeric fields for indices.
        /// </summary>
        /// <param name="index">Array index</param>
        /// <returns></returns>
        TItem this[params IField[] index] { get; }

        #region mixed types
        /// <summary>
        /// Multidimensional indexer property for array elements.
        /// </summary>
        TItem this[int index1, IField index2] { get; }

        /// <summary>
        /// Multidimensional indexer property for array elements.
        /// </summary>
        TItem this[IField index1, int index2] { get; }
        #endregion

        /// <summary>
        /// Gets an array of all array elements represented by this accessor.
        /// </summary>
        TItem[] All { get; }

        /// <summary>
        /// Gets or sets whether this field should be blanked if it's value is zero.
        /// </summary>
        void SetIsBlankWhenZero(bool isBlankAsZero);
    }


}

