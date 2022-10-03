using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;
using System.Runtime.Serialization;
//CHADusing Unity.Attributes;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface ICheckField.
    /// </summary>
    [InjectionImplementer(typeof(ICheckField))]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    internal sealed class CheckField : BufferElementBase, ICheckField, ICheckFieldInitializer,
        IBufferElement, ISerializable
    {
        #region private
        [NonSerialized]
        private Func<IField, bool> check;

        private string checkDisplayString;
        private readonly List<object> checkValues = new List<object>();
        private readonly Nullable<bool> serializedValue;

        private string GetDisplayString(Func<IField, bool> checkExp)
        {
            string result = string.Empty;

            if (CheckValues.Count > 0)
            {
                if (CheckValues.Count == 1)
                {
                    string valueStr = GetValueString(CheckValues.First());
                    result = string.Format("(fld) => fld.Value == {0}", valueStr);
                }
                else
                {
                    StringBuilder values = new StringBuilder();
                    for (int i = 0; i < CheckValues.Count; i++)
                    {
                        string fmtStr = i == 0 ? "{0}" : ", {0}";
                        values.AppendFormat(fmtStr, GetValueString(CheckValues[i]));
                    }

                    result = string.Format("(fld) => fld.Value in ({0})", values);
                }
            }
            else
            {
                result = checkExp.ToString();
            }

            return result;
        }
        private static string GetValueString(object value)
        {
            string result = string.Empty;

            if (value != null)
            {
                result = value is string ? string.Format("'{0}'", value) : value.ToString();
            }

            return result;
        }
        #endregion

        /// <summary>
        /// Creates and initializes a new instance of the CheckField class.
        /// </summary>
        /// <param name="info">A reference to the serialization object.</param>
        /// <param name="context">A reference to the streaming context object.</param>
        private CheckField(SerializationInfo info, StreamingContext context)
        {
            checkDisplayString = info.GetString("checkDisplayString");
            checkValues = (List<object>)info.GetValue("checkValues", typeof(List<object>));
            serializedValue = (Nullable<bool>)info.GetValue("serializedValue", typeof(Nullable<bool>));
            Field = (IField)info.GetValue("Field", typeof(IField));

            IsFiller = info.GetBoolean("IsFiller");
            IsInArray = info.GetBoolean("IsInArray");
            ArrayElementIndex = info.GetInt32("ArrayElementIndex");
            LengthInBuffer = info.GetInt32("LengthInBuffer");
            PositionInParent = info.GetInt32("PositionInParent");
            Name = info.GetString("Name");
            Parent = (IElementCollection)info.GetValue("Parent", typeof(IElementCollection));
            Record = (IRecord)info.GetValue("Record", typeof(IRecord));
            Buffer = (IDataBuffer)info.GetValue("Buffer", typeof(IDataBuffer));


        }

        /// <summary>
        /// Initializes a new instance of the CheckField class.
        /// </summary>
        public CheckField()
        {
            check = null;
            checkDisplayString = String.Empty;
            Field = null;
            serializedValue = null;
        }

        #region public properties
        /// <summary>
        /// Gets or sets the evaluation delegate for this check field.
        /// </summary>
        /// <example>
        /// To check the associated field object's value for the string value "Y" do the following:
        /// <code>
        /// myCheckField.Check = (fld) => fld.Value&lt;string&gt;() == "Y";
        /// </code>
        /// </example>
        [Category("ICheckField")]
        [Description("This is the evaluation delegate for this check field object.")]
        public Func<IField, bool> Check
        {
            get
            {
                return check;
            }
            set
            {
                check = value;
                checkDisplayString = GetDisplayString(check);
            }
        }


        /// <summary>
        /// Returns a string representation of the Check expression evaluation performed by this checkfield.
        /// Note: the string returned is likely to be only psuedocode. 
        /// </summary>
        public string DisplayString
        {
            get
            {
                return checkDisplayString;
            }
        }

        /// <summary>
        /// Gets the field associated with this check field.
        /// </summary>
        [Category("ICheckField")]
        [Description("The field object associated with this check field.")]
        [ReadOnly(true)]
        public IField Field { get; set; }

        /// <summary>
        /// Returns the result of the evaluation logic in <c>Check</c>.
        /// </summary>
        [Category("ICheckField")]
        [Description("Returns the result of the evaluation logic in the Check property")]
        [ReadOnly(true)]
        public bool Value
        {
            get
            {
                bool result = false;

                if (serializedValue.HasValue)
                {
                    result = serializedValue.Value;
                }
                else
                {
                    result = Field != null ? Check(Field) : false;
                }
                return result;
            }
        }

        public MDSY.Framework.Buffer.Common.FieldFormat FieldJustification { get; set; }
        #endregion

        /// <summary>
        /// Gets the list of values against which this check field's IField value will be evaluated.
        /// Note: this list is largely just for debug-time reference, to provide a list of values.
        /// </summary>
        public IList<object> CheckValues
        {
            get { return checkValues; }
        }


        #region public methods

        /// <summary>
        /// Initializes provided serialization object with the CheckField data.
        /// </summary>
        /// <param name="info">A reference to the serialization object.</param>
        /// <param name="context">A reference to the streaming context object.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info", "info is null.");

            info.AddValue("serializedValue", Value);
            info.AddValue("Field", Field);
            info.AddValue("checkValues", checkValues);
            info.AddValue("checkDisplayString", checkDisplayString);
            info.AddValue("IsFiller", IsFiller);
            info.AddValue("IsInArray", IsInArray);
            info.AddValue("LengthInBuffer", LengthInBuffer);
            info.AddValue("PositionInParent", PositionInParent);
            info.AddValue("Name", Name);
            info.AddValue("Parent", Parent);
            info.AddValue("Record", Record);
            info.AddValue("Buffer", Buffer);
            info.AddValue("ArrayElementIndex", ArrayElementIndex);
        }

        /// <summary>
        /// If <paramref name="isSetValue"/> is <c>true</c>, sets the value of 
        /// the checkfield's Field property to the checkfield's first CheckValue. 
        /// </summary>
        /// <param name="isSetValue">Indicates whether or not to execute the 
        /// SetValue() logic.</param>
        public void SetValue(bool isSetValue)
        {
            if (isSetValue)
            {
                Field.Assign(CheckValues[0]);
            }
        }

        /// <summary>
        /// Creates a deep copy of teh current check field object.
        /// </summary>
        /// <returns>Retruns a copy of the current object.</returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        #endregion


        #region overrides


        /// <summary>
        /// Creates a duplicate CheckField object with the new buffer position offset.
        /// </summary>
        /// <param name="name">The name of the check field to be duplicated.</param>
        /// <param name="bufferPositionOffset">Position offset in the buffer.</param>
        /// <param name="arrayIndexes">A collection of the array indexes.</param>
        /// <returns>Returns the duplicated check field object.</returns>
        public IBufferElement Duplicate(string name, int bufferPositionOffset, IEnumerable<int> arrayIndexes)
        {
            return Duplicate(name, bufferPositionOffset, null, arrayIndexes);
        }

        /// <summary>
        /// Creates a duplicate check field object for the new parent.
        /// </summary>
        /// <param name="name">The name of the check field to be duplicated.</param>
        /// <param name="bufferPositionOffset">Position offset in the buffer.</param>
        /// <param name="newParent">A reference to the new parent object.</param>
        /// <param name="arrayIndexes">A collection of the array indexes.</param>
        /// <returns>Returns the duplicated check field object.</returns>
        public IBufferElement Duplicate(string name, int bufferPositionOffset, IElementCollection newParent, IEnumerable<int> arrayIndexes)
        {
            // this is a checkfield; do nothing with newParent and ignore bufferPositionOffset.
            string newName;
            ArrayElementUtils.GetElementIndexes(name, out newName);
            newName = ArrayElementUtils.MakeElementName(newName, arrayIndexes);

            ICheckFieldInitializer result = ObjectFactory.Factory.NewCheckFieldObject(newName, Check);
            result.ArrayElementAccessor = ArrayElementAccessor;

            if (IsInArray && ArrayElementAccessor != null)
            {
                var editableAccessor = ArrayElementAccessor as IEditableArrayElementAccessor<ICheckField>;
                if (editableAccessor != null)
                {
                    editableAccessor.AddElement(result.AsReadOnly());
                }
            }

            // need to set result.Field upon return...

            return result.AsReadOnly();
        }

        /// <summary>
        /// Returns the length (in bytes) of this buffer element in the buffer.
        /// </summary>
        /// <returns>Returns the length (in bytes) of this buffer element in the buffer.</returns>
        protected override int GetLength()
        {
            return 0;
        }

        /// <summary>
        /// Retrieves the level of the curent check field.
        /// </summary>
        /// <returns>Returns the level of the current check field.</returns>
        protected override int GetLevel()
        {
            return Field.Level + 1;
        }

        #endregion

        /// <summary>
        /// Sets and returns a reference to the check field array element accessor object.
        /// </summary>
        public IArrayElementAccessor<ICheckField> ArrayElementAccessor { get; set; }

        /// <summary>
        /// Returns a reference to the current object.
        /// </summary>
        /// <returns>Returns a reference to the current object.</returns>
        public ICheckField AsReadOnly()
        {
            return this;
        }

        /// <summary>
        /// Not implemented, throws a NotImplementedException exception.
        /// </summary>
        /// <param name="value">Not used. Can take any value.</param>
        public void Assign(object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Does nothing. For interface compatibility only.
        /// </summary>
        /// <param name="value">Not used. Can take any value.</param>
        public void AssignIdRecordName(string value)
        {
            // do nothing
        }

        /// <summary>
        /// Does nothing. For interface compatibility only.
        /// </summary>
        /// <param name="value">Not used. Can take any value.</param>
        public void AssignIdRecordName(IBufferValue value)
        {
            // do nothing
        }

        /// <summary>
        /// Does nothing. For interface compatibility only.
        /// </summary>
        /// <returns>Returns an empty string.</returns>
        public string GetIdRecordName()
        {
            return "";
        }

        /// <summary>
        /// Not implemented, throws a NotImplementedException exception.
        /// </summary>
        /// <param name="bytes">Not used. Can take any value.</param>
        public void AssignFrom(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, throws a NotImplementedException exception.
        /// </summary>
        /// <param name="element">Not used. Can take any value.</param>
        /// <param name="sourceFieldType">Not used. Can take any value.</param>
        public void AssignFrom(IBufferValue element, Common.FieldType sourceFieldType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, throws a NotImplementedException exception.
        /// </summary>
        /// <param name="element">Not used. Can take any value.</param>
        public void AssignFrom(IBufferValue element)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, throws a NotImplementedException exception.
        /// </summary>
        /// <param name="value">Not used. Can take any value.</param>
        public void AssignFrom(string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, throws a NotImplementedException exception.
        /// </summary>
        /// <param name="value">Not used. Can take any value.</param>
        public void AssignFromGroup(IGroup value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        public void SetReferenceTo(IRecord record)
        {
            this.Record = record;
            this.Buffer = record.Buffer;
        }
    }
}
