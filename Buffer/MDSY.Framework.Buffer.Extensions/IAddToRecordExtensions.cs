using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Services;

namespace MDSY.Framework.Buffer
{
    /// <summary>
    /// Extension methods for IRecord specific to adding structure elements to record
    /// AFTER it has been closed/made immutable. 
    /// </summary>
    /// <remarks>
    /// Once the record has been defined, it's locked; immutable, just like a string object.
    /// To add elements, we have to clone the record using the standard structure definition 
    /// process, adding the elements in their appropriate location, then replacing 
    /// the original record with the new clone. 
    /// </remarks>
    public static class IAddToRecordExtensions
    {
        #region Entry points
        /// <summary>
        /// Creates a clone of the given <paramref name="instance"/> record and adds 
        /// the elements of the given <paramref name="collection"/> to the end of the 
        /// new record structure. 
        /// </summary>
        /// <remarks>
        /// Since record structures are immutable once they're declared, this is the
        /// only way to add elements to the structure after structure declaration 
        /// is complete. References to <paramref name="instance"/> should be updated
        /// with the result of this method call. 
        /// </remarks>
        /// <param name="instance">Record to be altered.</param>
        /// <param name="collection">New structure elements to be added.</param>
        /// <returns>A cloned version of the original <paramref name="instance"/> containing the new elements.</returns>
        public static IRecord AddToStructure(this IRecord instance, IElementCollection collection)
        {
            return AddToStructure(instance, collection, null);
        }


        /// <summary>
        /// Creates a clone of the given <paramref name="instance"/> record and adds 
        /// the elements of the given <paramref name="newContent"/> to the end of the 
        /// specified <paramref name="newContentParent"/> collection. 
        /// </summary>
        /// <remarks>
        /// Since record structures are immutable once they're declared, this is the
        /// only way to add elements to the structure after structure declaration 
        /// is complete. References to <paramref name="instance"/> should be updated
        /// with the result of this method call. 
        /// </remarks>
        /// <param name="instance">Record to be altered.</param>
        /// <param name="newContent">New structure elements to be added.</param>
        /// <param name="newContentParent">The element collection in the original <paramref name="instance"/>
        /// which will contain the new structure elements.</param>
        /// <returns>A cloned version of the original <paramref name="instance"/> containing the new elements.</returns>
        public static IRecord AddToStructure(this IRecord instance, IElementCollection newContent, IElementCollection newContentParent)
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "instance is null.");
            if (newContent == null)
                throw new ArgumentNullException("newContent", "collection is null.");

            // New elements will be added at the end of the "newParent". If 
            // newContentParent is null, make the record the new Parent.
            return AddToStructureUnderParentCollection(instance, newContent, newContentParent ?? instance);
        }

        #endregion

        /// <summary>
        /// Entry point of cloning process. Calls <c>BufferServices.Factory.NewRecord()</c>
        /// to begin the cloning. 
        /// </summary>
        /// <remarks>
        /// The IStructureDefinition object from the 
        /// Action&lt;IStructureDefinition&gt; is passed recursively through child 
        /// methods which recreate the structure of the given <paramref name="record"/>.
        /// </remarks>
        /// <param name="record">The IRecord object whose structure will be cloned. Required.</param>
        /// <param name="newElements">The new structure elements being added. Required.</param>
        /// <param name="newElementParent">The element collection to which the <paramref name="newElements"/> will be
        /// added as children. Required.</param>
        /// <returns>Returns a new instance of the record object.</returns>
        private static IRecord AddToStructureUnderParentCollection(IRecord record, IElementCollection newElements, IElementCollection newElementParent)
        {
            if (record == null)
                throw new ArgumentNullException("record", "record is null.");
            if (newElements == null)
                throw new ArgumentNullException("newElements", "newElements is null.");
            if (newElementParent == null)
                throw new ArgumentNullException("newElementParent", "newElementParent is null.");

            IRecord result = BufferServices.Factory.NewRecord(record.Name, def =>
            {
                if (newElementParent == record)
                {
                    // this will clone the record structure as is, then add the 
                    // new elements at the end
                    CloneStructureTree(record, def);
                    CloneCollectionElement(newElements, def);
                }
                else
                {
                    // this will clone the record structure, and when we find 
                    // the specified new parent collection, we'll add the
                    // newElements add the end of its element list.
                    CloneStructureTreeWithNewContent(record, def, newElements, newElementParent);
                }
            });

            return result;

        }

        /// <summary>
        /// Clones the given <paramref name="record"/>'s structure exactly into the 
        /// given <paramref name="structDef"/>.
        /// </summary>
        /// <remarks>
        /// <para>This is called if the new elements are to be added to the end of the 
        /// current <paramref name="record"/>'s elements collection.</para>
        /// <note>Non-recursive: this is the top level the record structure.</note>
        /// </remarks>
        /// <param name="record">A reference to the source record.</param>
        /// <param name="structDef">A reference to the target structure definition object.</param>
        private static void CloneStructureTree(IRecord record, IStructureDefinition structDef)
        {
            foreach (IBufferElement element in record.Elements)
            {
                AddElementToStructureDefinition(element, structDef);
            }
        }

        /// <summary>
        /// Copies data structure elements of the provided record to the specified structure definition object.
        /// </summary>
        /// <param name="record">A reference to the source record.</param>
        /// <param name="structDef">A reference to the target structure definition object.</param>
        /// <param name="newElements">A reference to the collection of the new elements.</param>
        /// <param name="newElementParent">A reference to the collection of the parent objects to the new elements.</param>
        private static void CloneStructureTreeWithNewContent(IRecord record,
            IStructureDefinition structDef, IElementCollection newElements, IElementCollection newElementParent)
        {
            foreach (IBufferElement element in record.Elements)
            {
                AddElementToStructureDefinition(element, structDef, newElements, newElementParent);
            }
        }

        /// <summary>
        /// Creates a new IGroup object and adds it to the specified IStructureDefinition object as a child.
        /// Clones the provided <paramref name="record"/>'s structure into the newly created IGroup object.
        /// </summary>
        /// <param name="newRecord">A reference to the source record.</param>
        /// <param name="structDef">A reference to the target structure definition.</param>
        private static void AddRecordToStructureDefinition(IRecord newRecord, IStructureDefinition structDef)
        {
            structDef.NewGroup(newRecord.Name, grpDef =>
                {
                    CloneStructureTree(newRecord, grpDef);
                });
        }


        /// <summary>
        /// Creates a new IField object of the appropriate type in the given <paramref name="structDef"/>;
        /// adds the field and any check fields. 
        /// </summary>
        /// <param name="field">The field object to clone.</param>
        /// <param name="structDef">The IStructureDefinition of the new record object.</param>
        private static void CloneFieldElement(IField field, IStructureDefinition structDef)
        {
            IField newField = null;

            // if the field is a redefine, and RedefinedElement != null this is the right path
            // but if it's a redefine and RedefinedElement == null, it's a child of a redefine
            // so take the other path - the parent redefine will create the right kind of field.
            if (field.IsInRedefine && ((field as IRedefinition).RedefinedElement != null))
            {
                newField = structDef.CreateNewFieldRedefine(field.Name, field.FieldType, (field as IRedefinition).RedefinedElement, field.DisplayLength);
            }
            else
            {
                newField = structDef.CreateNewField(field.Name, field.FieldType, field.DisplayLength, null, field.DecimalDigits);
                newField.AssignFrom(field);
            }

            if (field.CheckFields.Count() > 0)
            {
                foreach (ICheckField checkField in field.CheckFields)
                {
                    newField.CreateNewCheckField(checkField.Name, checkField.Check);
                }
            }
        }



        /// <summary>
        /// Creates a new IArray&lt;T&gt; in the given <paramref name="structDef"/>;
        /// adds the array element and all children.
        /// </summary>
        private static void CloneArrayElement(IArrayBase arrayElement, IStructureDefinition structDef)
        {
            if (arrayElement is IArray<IField>)
            {
                var fieldArray = arrayElement as IFieldArray;
                IField firstField = fieldArray[0];
                structDef.CreateNewFieldArray(fieldArray.Name, fieldArray.ArrayElementCount,
                    firstField.FieldType, firstField.DisplayLength, firstField.GetValue<object>(), firstField.DecimalDigits);
            }
            else if (arrayElement is IArray<IGroup>)
            {
                var groupArray = arrayElement as IGroupArray;
                IGroup firstGroup = groupArray[0];
                structDef.CreateNewGroupArray(groupArray.Name, groupArray.ArrayElementCount,
                    def => { AddElementToStructureDefinition(firstGroup, def); });
            }
        }

        /// <summary>
        /// Clones provided array object into the specified structure definition object.
        /// </summary>
        /// <param name="arrayElement">A reference to the source array object.</param>
        /// <param name="structDef">A reference to the target structure definition object.</param>
        /// <param name="newElements">A reference to the collection of the new elements.</param>
        /// <param name="newElementParent">A reference to the collection of the parent objects to the new elements.</param>
        private static void CloneArrayElement(IArrayBase arrayElement, IStructureDefinition structDef,
            IElementCollection newElements, IElementCollection newElementParent)
        {
            if (arrayElement is IArray<IField>)
            {
                var fieldArray = arrayElement as IFieldArray;
                IField firstField = fieldArray[0];
                structDef.CreateNewFieldArray(fieldArray.Name, fieldArray.ArrayElementCount,
                    firstField.FieldType, firstField.DisplayLength, firstField.GetValue<object>(), firstField.DecimalDigits);
            }
            else if (arrayElement is IArray<IGroup>)
            {
                var groupArray = arrayElement as IGroupArray;
                IGroup firstGroup = groupArray[0];
                structDef.CreateNewGroupArray(groupArray.Name, groupArray.ArrayElementCount,
                    def => { AddElementToStructureDefinition(firstGroup, def, newElements, newElementParent); });
            }
        }

        /// <summary>
        /// Clones provided buffer element object to the specified structure definition object.
        /// </summary>
        /// <param name="element">A reference to the source buffer element.</param>
        /// <param name="structDef">A reference to hte target structure definition object.</param>
        /// <param name="newElements">A reference to the collection of the new elements.</param>
        /// <param name="newElementParent">A reference to the collection of the parent objects to the new elements.</param>
        private static void AddElementToStructureDefinition(IBufferElement element,
            IStructureDefinition structDef, IElementCollection newElements, IElementCollection newElementParent)
        {
            if (element is IField)
            {
                CloneFieldElement(element as IField, structDef);
            }

            // leave this case ahead of IElementCollection case for optimization
            else if (element is IArrayBase)
            {
                CloneArrayElement(element as IArrayBase, structDef, newElements, newElementParent);
            }

            else if (element is IGroup)
            {
                CloneCollectionElement(element as IElementCollection, structDef, newElements, newElementParent);
            }

            else if (element is ICheckField)
            {
                // should never get here; check field's parents are fields, not collections.
            }

            else
            {
                throw new RecordStructureException("Attempted to clone element of unrecognized type.");
            }
        }

        /// <summary>
        /// Adds the specified <paramref name="element"/> to the given <paramref name="structDef"/> 
        /// as is appropriate for the <paramref name="element"/>'s type.
        /// </summary>
        /// <param name="element">A reference to the source buffer element object.</param>
        /// <param name="structDef">A reference to the structure definition object.</param>
        private static void AddElementToStructureDefinition(IBufferElement element, IStructureDefinition structDef)
        {
            if (element is IField)
            {
                CloneFieldElement(element as IField, structDef);
            }

            // leave this case ahead of IElementCollection case for optimization
            else if (element is IArrayBase)
            {
                CloneArrayElement(element as IArrayBase, structDef);
            }

            else if (element is IGroup)
            {
                CloneCollectionElement(element as IElementCollection, structDef);
            }

            else if (element is ICheckField)
            {
                // should never get here; check field's parents are fields, not collections.
            }

            else
            {
                throw new RecordStructureException("Attempted to clone element of unrecognized type.");
            }
        }

        /// <summary>
        /// Adds provided buffer elements to the specified structure definition.
        /// </summary>
        /// <param name="structDef">A reference to the target structure definition.</param>
        /// <param name="elements">A referenece to the collection of the source elements.</param>
        private static void AddElementsToStructureDefinition(IStructureDefinition structDef, IEnumerable<IBufferElement> elements)
        {
            foreach (var element in elements)
            {
                AddElementToStructureDefinition(element, structDef);
            }
        }

        /// <summary>
        /// Adds provided collection of the data structure elements to the specified structure definition.
        /// </summary>
        /// <param name="collection">A reference to the collection of the source elements.</param>
        /// <param name="structDef">A reference to the target structure definition object.</param>
        private static void CloneCollectionElement(IElementCollection collection, IStructureDefinition structDef)
        {
            if (collection is IGroup)
            {
                var group = collection as IGroup;

                // if the group is a redefine, and RedefinedElement != null this is the right path
                // but if it's a redefine and RedefinedElement == null, it's a child of a redefine
                // so take the other path - the parent redefine will create the right kind of child group.
                if (group.IsInRedefine && ((group as IRedefinition).RedefinedElement != null))
                {
                    structDef.NewGroupRedefine(group.Name, (collection as IRedefinition).RedefinedElement, groupDef =>
                            AddElementsToStructureDefinition(groupDef, group.Elements));
                }
                else
                {
                    structDef.NewGroup(group.Name, groupDef =>
                            AddElementsToStructureDefinition(groupDef, group.Elements));
                }
            }
            else if (collection is IRecord)
            {
                var record = collection as IRecord;
                structDef.NewGroup(record.Name, groupDef =>
                        AddElementsToStructureDefinition(groupDef, record.Elements));
            }
            else
            {
                // we shouldn't have gotten here. It's a array, which should have already been handled,
                // or some new type we don't know about. 
                throw new RecordStructureException("Attempted to clone IElementCollection of unrecognized type.");
            }
        }

        /// <summary>
        /// Clones provided collection of the data structure elements to the specified structure definition.
        /// </summary>
        /// <param name="collection">A reference to the source collection of the data structure elements.</param>
        /// <param name="structDef">A reference to the target structure definition object.</param>
        /// <param name="newElements">A reference to the collection of the new elements.</param>
        /// <param name="newElementParent">A reference to the collection of the parent objects to the new elements.</param>
        private static void CloneCollectionElement(IElementCollection collection,
            IStructureDefinition structDef, IElementCollection newElements, IElementCollection newElementParent)
        {
            if (collection is IGroup)
            {
                var group = collection as IGroup;

                // if the group is a redefine, and RedefinedElement != null this is the right path
                // but if it's a redefine and RedefinedElement == null, it's a child of a redefine
                // so take the other path - the parent redefine will create the right kind of child group.
                if (group.IsInRedefine && ((group as IRedefinition).RedefinedElement != null))
                {
                    structDef.NewGroupRedefine(group.Name, (collection as IRedefinition).RedefinedElement, groupDef =>
                            AddElementsToStructureDefinition(groupDef, group.Elements));
                }
                else
                {
                    if (group == newElementParent)
                    {
                        structDef.NewGroup(group.Name, groupDef =>
                            {
                                AddElementsToStructureDefinition(groupDef, group.Elements);
                                string newGroupName;
                                IEnumerable<IBufferElement> groupSubElements;

                                if (newElements is IRecord)
                                {
                                    newGroupName = (newElements as IRecord).Name;
                                    groupSubElements = (newElements as IRecord).Elements;
                                }
                                else
                                {
                                    newGroupName = (newElements as IBufferElement).Name;
                                    groupSubElements = newElements.Elements;
                                }

                                groupDef.NewGroup(newGroupName, def =>
                                    {
                                        AddElementsToStructureDefinition(def, groupSubElements);
                                    });

                            });
                    }
                    else
                    {
                        structDef.NewGroup(group.Name, groupDef =>
                            AddElementsToStructureDefinition(groupDef, group.Elements));
                    }
                }
            }
            else if (collection is IRecord)
            {
                var record = collection as IRecord;
                structDef.NewGroup(record.Name, groupDef =>
                        AddElementsToStructureDefinition(groupDef, record.Elements));
            }
            else
            {
                // we shouldn't have gotten here. It's a array, which should have already been handled,
                // or some new type we don't know about. 
                throw new RecordStructureException("Attempted to clone IElementCollection of unrecognized type.");
            }
        }


    }
}
