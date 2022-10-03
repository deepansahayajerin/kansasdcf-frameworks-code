using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Interfaces
{
    /// <summary>
    /// Defines an object which implements IArray(of IGroup) specifically.
    /// </summary>
    public interface IGroupArray : IArray<IGroup>
    {
        /// <summary>
        /// Creates and returns the group object which is the first element in the array. 
        /// </summary>
        /// <param name="elementName">Name of the new group object.</param>
        /// <param name="groupInit">A delegate containing logic for setting up the new group object.</param>
        /// <param name="groupDefinition">A delegate containing the logic for defining the structure of the new group.</param>
        /// <param name="arrayElementInit">A delegate containing logic for initializing the new array element. 
        /// Note: this is here to allow you to use the same delegate code here, for the first element, as you do 
        /// for all the subsequent duplicates of the new group.</param>
        IGroup CreateFirstArrayElement(string elementName,
            Action<IGroupInitializer> groupInit,
            Action<IStructureDefinition> groupDefinition,
            Action<IArrayElementInitializer, string, int> arrayElementInit,
            IDictionary<string, IArrayElementAccessorBase> arrayElementAccessors);
    }
}
