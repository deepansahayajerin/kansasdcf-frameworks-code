using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using System.ComponentModel;
using System.Linq.Expressions;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which evaluates a given IField for a specific value.
    /// </summary>
    [InjectionInterface]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public interface ICheckField : IBufferElement, IArrayElement
    {
        /// <summary>
        /// Gets the list of values against which this check field's IField value will be evaluated.
        /// </summary>
        IList<object> CheckValues { get; }

        /// <summary>
        /// Gets or sets the evaluation delegate for this check field.
        /// </summary>
        /// <example>
        /// To check the associated field object's value for the string value "Y" do the following:
        /// <code>
        /// myCheckField.Check = (fld) => fld.Value&lt;string&gt;() == "Y";
        /// </code>
        /// </example>
        Func<IField, bool> Check { get; set; }

        /// <summary>
        /// Returns a string representation of the Check expression evaluation performed by this checkfield.
        /// Note: the string returned is likely to be only psuedocode. 
        /// </summary>
        /// <returns></returns>
        string DisplayString { get; }

        /// <summary>
        /// Gets or sets the field associated with this check field.
        /// </summary>
        IField Field { get; set; }

        /// <summary>
        /// Returns the result of the evaluation logic in <c>Check</c>.
        /// </summary>
        /// <returns></returns>
        bool Value { get; }

        /// <summary>
        /// If <paramref name="isSetValue"/> is <c>true</c>, sets the value of 
        /// the checkfield's Field property to the checkfield's first CheckValue. 
        /// </summary>
        /// <param name="isSetValue">Indicates whether or not to execute the 
        /// SetValue() logic.</param>
        void SetValue(bool isSetValue);
    }
}