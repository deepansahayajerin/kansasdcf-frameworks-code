using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using System.ComponentModel;
using Unity;
using MDSY.Framework.Buffer.Common;


namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IGroup.
    /// </summary>
    [InjectionImplementer(typeof(IGroup), "Redefine")]
    [Serializable]
    internal sealed class RedefineGroup : GroupBase, IGroup,
        IGroupInitializer, IBufferValue, IStructureDefinition,
        IBufferElement, IElementCollection, IRedefinition
    {

        #region IRedefinition


        /// <summary>
        /// Returns <c>true</c> if any parent above this element implements IRedefinition.
        /// </summary>
        [Category("IRedefinition")]
        [Description("Indicates whether this object has any parents that are IRedefinitions.")]
        [ReadOnly(true)]
        public bool HasRedefineInParents
        {
            get { return (GetRootLevelRedefinition() != this); }
        }

        /// <summary>
        /// Gets or sets the (optional) buffer element that this redefinition object redefines.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If RedefinedElement is null, this object is participating in a REDEFINE at a level lower 
        /// than the root level, thus it is redefining only a sub-section of the Redefinition root's RedefinedElement.
        /// </para>
        /// <para>To get the root-level RedefinedElement, access RootLevelRedefinition.</para>
        /// </remarks>
        [Category("IRedefinition")]
        [Description("The buffer element that this redefinition object redefines.")]
        [ReadOnly(true)]
        public IBufferElement RedefinedElement { get; set; }

        /// <summary>
        /// Gets the parent element which is the root of the REDEFINE.
        /// </summary>
        [Category("IRedefinition")]
        [Description("The parent element which is the root of the REDEFINE.")]
        [ReadOnly(true)]
        public IRedefinition RootLevelRedefinition
        {
            get { return GetRootLevelRedefinition(); }
        }
        #endregion

        #region overrides
        /// <summary>
        /// Calculates the length of the group object based on internal structure.
        /// </summary>
        /// <returns>Returns length of the current group object.</returns>
        protected override int CalculateLength()
        {
            int result = 0;

            // don't exclude IRedefinitions here
            foreach (IBufferElement element in ChildElements.Elements)
            {
                if (!element.IsARedefine)
                    result += element.LengthInBuffer;
            }

            return result;
        }

        /// <summary>
        /// Creates and returns a new IField object. Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public override IField CreateNewField(string name, FieldType fieldType, int displayLength)
        {
            int position = GetNextElementPosition();
            // anything added below a RedefineGroup must also be a redefine...
            IField result = StructureDef.CreateNewFieldRedefine(name, fieldType, null, displayLength, 0, position, DefineTimeAccessors);
            AddChildElement(result);
            return result;
        }

        /// <summary>
        /// Creates and returns a new IField object whose value is set to <paramref name="initialValue"/>.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <param name="initialValue">Value to be assigned to the new field.</param>
        /// <returns>A new instance of an IField-implementing object.</returns>
        public override IField CreateNewField(string name, FieldType fieldType, int displayLength, object initialValue)
        {
            // this is a redefine, don't use the initialValue
            return CreateNewField(name, fieldType, displayLength);
        }

        /// <summary>
        /// Creates and returns a new IField object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="fieldType">Type of the new field object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="displayLength">The number of bytes required to display the field value.</param>
        /// <returns>An new instance of an IField-implementing object.</returns>
        public override IField CreateNewFieldRedefine(string name, FieldType fieldType, IBufferElement elementToRedefine, int displayLength)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("name is null or empty.", "name");

            IField result = StructureDef.CreateNewFieldRedefine(name, fieldType, null, displayLength, 0, elementToRedefine.PositionInParent, DefineTimeAccessors);
            result.IsARedefine = true;
            AddChildElement(result);
            return result;
        }

        /// <summary>
        /// Creates a new IFiller object and adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="length">Number of bytes the new filler should occupy.</param>
        /// <param name="fillWith">Specifies filling character.</param>
        public override IStructureDefinition NewFillerField(int length, FillWith fillWith)
        {
            // don't 'fillwith' for redefine...

            int position = GetNextElementPosition();

            // anything added below a RedefineGroup must also be a redefine...
            IField filler = StructureDef.CreateNewFieldRedefine("FILLER", FieldType.String, null, length, 0, position, DefineTimeAccessors, true);
            AddChildElement(filler);
            return this;
        }

        /// <summary>
        /// Creates and returns a new IGroup object. Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new group.</param>
        /// <param name="definition">A delegate which contains the logic for defining the structure of the new group.</param>
        /// <returns>A new instance of an IGroup-implementing object.</returns>
        /// <example>
        /// Creates a new record and defines the structure as having one group which contains two fields:
        /// <code>
        /// IRecord myRecord = BufferServices.Factory.NewRecord("myRecord", (rec) =>
        /// {
        ///    IGroup group01 = rec.CreateNewGroup("Group01", (grp) =>
        ///    {
        ///        grp
        ///            .NewField("Field_A", typeof(string), 10, "AAAAAAAAAA")
        ///            .NewField("Field_B", typeof(string), 10, "BBBBBBBBBB");
        ///    });
        /// </code>
        /// </example>
        public override IGroup CreateNewGroup(string name, Action<IStructureDefinition> definition)
        {
            int position = GetNextElementPosition();
            // anything added below a RedefineGroup must also be a redefine...
            IGroup result = StructureDef.CreateNewGroupRedefine(name, null, definition, position, DefineTimeAccessors);
            AddChildElement(result);
            return result;
        }

        /// <summary>
        /// Creates and returns a new IGroup object which redefines a section of the buffer.
        /// Adds the new object to this IStructureDefinition object as a child.
        /// </summary>
        /// <param name="name">Name of the new group object.</param>
        /// <param name="elementToRedefine">The buffer element which this field object will redefine.</param>
        /// <param name="definition">A delegate which contains the logic for defining the structure of the new group.</param>
        /// <returns>A new instance of an IGroup-implementing object.</returns>
        /// <seealso cref="IStructureDefinition.CreateNewGroup"/>
        public override IGroup CreateNewGroupRedefine(string name, IBufferElement elementToRedefine, Action<IStructureDefinition> definition)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("name is null or empty.", "name");
            // In this case, where you're creating a redefine group within a redefine group, 
            // elementToRedefine must be null here, but not null in the upper redefine group.",
            if (definition == null)
                throw new ArgumentNullException("definition", "definition is null.");

            IGroup result = StructureDef.CreateNewGroupRedefine(name, null, definition, elementToRedefine.PositionInParent, DefineTimeAccessors);
            result.IsARedefine = true;
            AddChildElement(result);
            return result;
        }

        /// <summary>
        /// Returns the starting index for the next element to be added. 
        /// </summary>
        /// <returns>Returns the starting index for the next element to be added.</returns>
        protected override int GetNextElementPosition()
        {
            return ChildElements.Elements.Sum(e => !(e.IsARedefine) ? e.LengthInBuffer : 0);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>Returns a string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Format("[Redef]{0}", base.ToString());
        }

        #endregion

        #region private methods
        private IRedefinition GetRootLevelRedefinition()
        {
            IRedefinition result = this;

            if (!(Parent is IRecord))
            {
                var parent = Parent;
                while (parent != null)
                {
                    if (parent is IRedefinition && (parent as IRedefinition).RedefinedElement != null)
                    {
                        result = parent as IRedefinition;
                        break;
                    }
                    else
                    {
                        if (parent is IBufferElement)
                        {
                            parent = (parent as IBufferElement).Parent;
                        }
                        else
                        {
                            break;
                        }
                    }
                }


            }

            return result;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Returns a deep copy of this element object, applying <paramref name="name"/> as the duplicate object's new 
        /// Name, and offsetting the new object's position by the amount given in <paramref name="bufferPositionOffset"/>.
        /// The new object's Parent is the same as this object's Parent.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="bufferPositionOffset">The amount by which to adjust the new object's position.</param>
        /// <param name="arrayIndexes">The indices of this element and possibly its parents if this element is part of 
        /// an array and/or nested array.</param>
        /// <returns>A new IBufferElement instance of the same type as this object.</returns>
        public IBufferElement Duplicate(string name, int bufferPositionOffset, IEnumerable<int> arrayIndexes)
        {
            return Duplicate(name, bufferPositionOffset, this.Parent, arrayIndexes);
        }

        /// <summary>
        /// Returns a deep copy of this element object, applying <paramref name="name"/> as the duplicate object's new 
        /// Name, and offsetting the new object's position by the amount given in <paramref name="bufferPositionOffset"/>.
        /// The new object is re-parented to <paramref name="newParent"/>.
        /// </summary>
        /// <param name="name">Name of the new object.</param>
        /// <param name="bufferPositionOffset">The amount by which to adjust the new object's position.</param>
        /// <param name="newParent">The IElementCollection which will be the new object's Parent.</param>
        /// <param name="arrayIndexes">The indices of this element and possibly its parents if this element is part of 
        /// an array and/or nested array.</param>
        /// <returns>A new IBufferElement instance of the same type as this object.</returns>
        public IBufferElement Duplicate(string name, int bufferPositionOffset, IElementCollection newParent, IEnumerable<int> arrayIndexes)
        {
            string copyName;
            ArrayElementUtils.GetElementIndexes(name, out copyName);

            var result = ObjectFactory.Factory.NewRedefineGroupObject(ArrayElementUtils.MakeElementName(copyName, arrayIndexes),
                Buffer, newParent, RedefinedElement, PositionInParent + bufferPositionOffset,
                DefineTimeAccessors, IsInArray).AsReadOnly();

            foreach (IBufferElement element in Elements)
            {
                // since we're duplicating to a new parent, we DON'T offset the new position.
                var copy = element.Duplicate(element.Name, 0, result, arrayIndexes);
                result.AddChildElement(copy);
            }

            if (IsInArray && ArrayElementAccessor != null)
            {
                var editableAccessor = ArrayElementAccessor as IEditableArrayElementAccessor<IGroup>;
                if (editableAccessor != null)
                {
                    editableAccessor.AddElement(result);
                }
            }

            (result as IStructureDefinition).EndDefinition();
            result.IsARedefine = IsARedefine;
            return result;
        }
        #endregion
        public int BufferAddress
        {
            get
            {
                //TBD not implemented
                return 0;

            }
        }
        /// <summary>
        /// Returns a reference to the current group object.
        /// </summary>
        /// <returns>Returns a reference to the current group object.</returns>
        public IGroup AsReadOnly()
        {
            return this as IGroup;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="element">Not used, can take any value.</param>
        public void SetAddressToAddressOf<T>(T element) where T : IBufferElement, IBufferValue
        {

        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="recordBuffer">Not used, can take any value.</param>
        public void SetAddressToAddressOf(IRecord recordBuffer)
        {

        }

        /// <summary>
        /// Creates a deep copy of the current object.
        /// </summary>
        /// <returns>Returns a deep copy of the current object.</returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
