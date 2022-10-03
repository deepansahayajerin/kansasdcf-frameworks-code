using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Unity;
using Unity;
using Microsoft.Extensions.Configuration;
using System.Threading;
using MDSY.Framework.Buffer.Services;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Factory object for buffer elements. Use ObjectFactory.Factory for access.
    /// </summary>
    internal sealed class ObjectFactory : IBufferObjectFactory
    {
        private static Dictionary<string, object> recordDefLockColl = new Dictionary<string, object>();
        private static readonly Random Rnd = new Random();

        #region singleton

        /// <summary>
        /// Singleton instance
        /// </summary>
        /// <remarks>
        /// Solution # 6 http://csharpindepth.com/articles/general/singleton.aspx
        /// </remarks>
        private static Lazy<IBufferObjectFactory> _instance = new Lazy<IBufferObjectFactory>(() => new ObjectFactory());

        /// <summary>
        /// Singleton Instance of the Object Factory
        /// </summary>
        /// <remarks>
        /// Using unity to construct this object was a performance issue.
        /// As there are no dependencies injected an alterative method is provided for unit tests.
        /// </remarks>
        public static IBufferObjectFactory Factory => _instance.Value;

        /// <summary>
        /// Used to override the Object Factory for unit tests.
        /// </summary>
        /// <param name="constructor">The constructor for the alternative IBufferObjectFactory</param>
        /// <remarks>
        ///     Consuming classes should be refactored to either accept IBufferObjectFactory as a dependency
        ///     or accept a generic service locator class 
        /// </remarks>
        public static void OverrideDefaultBufferObjectFactory(Func<IBufferObjectFactory> constructor)
        {
            if (_instance.IsValueCreated)
                throw new NotSupportedException("Dependency should have been overrideded before user");
            _instance = new Lazy<IBufferObjectFactory>(constructor);
        }

        #endregion

        #region private methods
        private static bool GetIsZeroBasedArrays()
        {
            // We default to 1-based arrays. We'll only use the 0-based implementation 
            // if the config setting says to. 
            return ConfigSettings.GetAppSettingsBool(Constants.AppSettings.IsZeroBasedArrays);
        }
        /// <summary>
        /// Returns a random interger
        /// </summary>
        /// <returns></returns>
        private static string GetRandom()
        {
            //return Rnd.Next();                              
            return DateTime.Now.ToString("ddHHmmssfffffff");            
        }
        #endregion

        #region utils
        /// <summary>
        /// Generates a Random name
        /// </summary>
        /// <param name="prefix">A prefix to use in the name</param>
        /// <returns>A name with a Random number</returns>
        private static string GetRandomName(string prefix)
        {
            return $"{prefix}_{GetRandom()}";
        }

        internal static int GetFieldBufferLength(FieldType fieldType, int fieldDisplayLength)
        {
            int result = fieldDisplayLength;

            if ((fieldType == FieldType.PackedDecimal) || (fieldType == FieldType.UnsignedPackedDecimal))
            {
                result = (fieldDisplayLength / 2) + 1;
            }
            else if (fieldType == FieldType.CompShort)
            {
                result = Constants.Defaults.CompShortByteCount;
            }
            else if (fieldType == FieldType.CompInt || fieldType == FieldType.ReferencePointer)
            {
                result = Constants.Defaults.CompIntByteCount;
            }
            else if (fieldType == FieldType.CompLong)
            {
                result = Constants.Defaults.CompLongByteCount;
            }

            return result;
        }



        #endregion

        #region NewFieldArrayObject
        /// <summary>
        /// Returns a newly-instatiated FieldArray object created with the given values.
        /// </summary>
        /// <param name="name">Name of the new FieldArray object.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new object.</param>
        /// <param name="buffer">Buffer object for the new object.</param>
        /// <param name="positionInParent">Index position of the new object in its parent.</param>
        /// <param name="numberOfOccurrances">Number of array elements.</param>
        /// <param name="arrayElementLength">Length of each array element.</param>
        /// <param name="isSubArray">Indicates whether the new array object is itself within an array (i.e. nested).</param>
        /// <param name="isFiller">Indicates whether the new object is marked as <c>FILLER</c>.</param>
        /// <returns>A new FieldArray object.</returns>
        public IFieldArrayInitializer NewFieldArrayObject(string name,
            IElementCollection parentCollection,
            IDataBuffer buffer,
            int positionInParent,
            int numberOfOccurrances,
            int arrayElementLength,
            bool isSubArray,
            bool isFiller)
        {
            //Concrete class is used rather than Unity, as there are no dependencies injected.
            //The Object Factory singleton instance can be overridden if needed to return a different type
            var result = new FieldArray
            {
                Name = isFiller ? GetRandomName(name) : name,
                Buffer = buffer,
                Parent = parentCollection,
                PositionInParent = positionInParent,
                ArrayElementCount = numberOfOccurrances,
                ArrayElementLength = arrayElementLength,
                IsInArray = isSubArray,
                IsFiller = isFiller
            };
            return result;
        }

        #endregion

        #region NewDataBufferByteArrayObject
        /// <summary>
        /// Returns a new DataBufferByteArray object built using the given <paramref name="bytes"/>.
        /// </summary>
        /// <param name="bytes">The bytes for the new byte array.</param>
        /// <returns>An IDataBuffer-implementing DataBufferByteArray object.</returns>
        public IDataBuffer NewDataBufferByteArrayObject(IEnumerable<byte> bytes)
        {
            //Concrete class is used rather than Unity, as there are no dependencies injected.
            //The Object Factory singleton instance can be overridden if needed to return a different type
            var result = new DataBufferByteArray();
            result.InitializeBytes(bytes.ToArray());
            return result.AsReadOnly();
        }
        #endregion

        #region NewDataBufferRedefinePipelineObject

        /// <summary>
        /// Returns a new DataBufferRedefinePipeline object built using the given <paramref name="targetElement"/>
        /// </summary>
        /// <param name="targetElement">The target element.</param>
        /// <returns>An IDataBuffer-implementing DataBufferRedefinePipeline object.</returns>
        public IDataBuffer NewDataBufferRedefinePipelineObject(IBufferElement targetElement)
        {
            return new DataBufferRedirectionPipeline() { TargetElement = targetElement };
        }
        #endregion

        #region NewCheckFieldObject
        /// <summary>
        /// Returns a newly instantiated ICheckField object with the given values.
        /// </summary>
        /// <param name="name">Name of the new check field.</param>
        /// <param name="check">Expression used to evaluate the value of the check field.</param>
        /// <returns>A new ICheckField implementer.</returns>
        public ICheckFieldInitializer NewCheckFieldObject(string name, Func<IField, bool> check)
        {
            return NewCheckFieldObject(name, check, null);
        }

        /// <summary>
        /// Returns a newly instantiated ICheckField object with the given values.
        /// </summary>
        /// <param name="name">Name of the new check field.</param>
        /// <param name="check">Expression used to evaluate the value of the check field.</param>
        /// <param name="field">IField object that is processed by <paramref name="check"/>.</param>
        /// <returns>A new ICheckField implementer.</returns>
        public ICheckFieldInitializer NewCheckFieldObject(string name, Func<IField, bool> check, IField field)
        {
            // this should be de-coupled by getting the ICheckField instance from UnitySingleton.Container.
            return new CheckField()
            {
                Name = name,
                Field = field,
                Check = check
            };

        }

        #endregion

        #region NewBufferAddress
        /// <summary>
        /// Returns a new, populated IBufferAddress object.
        /// </summary>
        /// <returns></returns>
        /// <param name="recordKey">The key of the record, in BufferServices.Records, whose Buffer is the new target.</param>
        /// <param name="elementName">The name of the element, in the record, whose PositionInBuffer is the new target.</param>
        public IBufferAddress NewBufferAddress(int recordKey, string elementName)
        {
            return NewBufferAddress(recordKey, elementName, 0);
        }

        /// <summary>
        /// Returns a new, populated IBufferAddress object.
        /// </summary>
        /// <returns>a new, populated IBufferAddress object</returns>
        /// <param name="recordKey">The key of the record, in BufferServices.Records, whose Buffer is the new target.</param>
        /// <param name="elementName">The name of the element, in the record, whose PositionInBuffer is the new target.</param>
        /// <param name="optionalBufferStartIndex">Buffer start position.</param>
        public IBufferAddress NewBufferAddress(int recordKey, string elementName, int optionalBufferStartIndex)
        {
            //Concrete class is used rather than Unity, as there are no dependencies injected.
            //The Object Factory singleton instance can be overridden if needed to return a different type
            IBufferAddress result = new BufferAddress();
            result.ElementName = elementName;
            result.RecordKey = recordKey;
            result.OptionalBufferStartIndex = optionalBufferStartIndex;

            return result;
        }


        #endregion

        #region NewArrayElementAccessorObject
        /// <summary>
        /// Returns a newly instantiated IArrayElementAccessor(of <typeparamref name="T"/>).
        /// </summary>
        /// <typeparam name="T">The type of the array elements.</typeparam>
        /// <param name="name">Name of the new object.</param>
        /// <returns>A new ArrayElementAccessor(of <typeparamref name="T"/>).</returns>
        public IEditableArrayElementAccessor<T> NewArrayElementAccessorObject<T>(string name)
            where T : IArrayElement, IBufferElement
        {
            string resolutionName = IsZeroBasedArrays ?
                                        Constants.TypeMappingRegistrationNames.ZeroBasedIdx :
                                        Constants.TypeMappingRegistrationNames.OneBasedIdx;
            IEditableArrayElementAccessor<T> result;
            //Concrete class is used rather than Unity, as there are no dependencies injected.
            //The Object Factory singleton instance can be overridden if needed to return a different type
            if (resolutionName == Constants.TypeMappingRegistrationNames.ZeroBasedIdx)
            {
                result = new ArrayElementAccessor<T>();
            }
            else
            {
                result = new ArrayElementOneBasedAccessor<T>();

            }
            return result;
        }

        #endregion

        #region NewGroupArrayObject
        /// <summary>
        /// Returns a newly instantiated GroupArray object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new group array object.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new object.</param>
        /// <param name="buffer">Buffer object for the new object.</param>
        /// <param name="positionInParent">Index position of the new object in its parent.</param>
        /// <param name="isSubArray">Indicates whether the new array object is itself within an array (i.e. nested).</param>
        /// <param name="numberOfOccurrances">Number of array elements.</param>
        /// <param name="arrayElementLength">Length of each array element.</param>
        /// <returns>A new IGroupArray object.</returns>
        public IGroupArrayInitializer NewGroupArrayObject(string name,
            IElementCollection parentCollection,
            IDataBuffer buffer,
            int positionInParent,
            bool isSubArray,
            int numberOfOccurrances,
            int arrayElementLength)
        {
            //Concrete class is used rather than Unity, as there are no dependencies injected.
            //The Object Factory singleton instance can be overridden if needed to return a different type
            var result = new GroupArray
            {
                Name = name,
                Buffer = buffer,
                Parent = parentCollection,
                PositionInParent = positionInParent,
                ArrayElementCount = numberOfOccurrances,
                ArrayElementLength = arrayElementLength,
                IsInArray = isSubArray
            };
            return result;
        }

        /// <summary>
        /// Returns a newly instantiated GroupArray object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new group array object.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new object.</param>
        /// <param name="buffer">Buffer object for the new object.</param>
        /// <param name="positionInParent">Index position of the new object in its parent.</param>
        /// <param name="isSubArray">Indicates whether the new array object is itself within an array (i.e. nested).</param>
        /// <param name="numberOfOccurrances">Number of array elements.</param>
        /// <returns>A new GroupArray object.</returns>
        public IGroupArrayInitializer NewGroupArrayObject(string name,
            IElementCollection parentCollection,
            IDataBuffer buffer,
            int positionInParent,
            bool isSubArray,
            int numberOfOccurrances)
        {
            return NewGroupArrayObject(name,
                       parentCollection,
                       buffer,
                       positionInParent,
                       isSubArray,
                       numberOfOccurrances,
                       Constants.Defaults.ArrayElementLength);
        }


        #endregion

        #region NewFieldObject

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="isRedefine">Indicates whether the new field participates in a REDEFINE.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        public IFieldInitializer NewFieldObject(string name, IElementCollection parentCollection, IDataBuffer buffer, bool isRedefine)
        {
            return NewFieldObject(name,
                       parentCollection,
                       buffer,
                       fieldType: FieldType.String,
                       lengthInBuffer: Constants.Defaults.LengthInBuffer,
                       displayLength: Constants.Defaults.LengthInBuffer,
                       positionInParent: Constants.Defaults.PositionInParent,
                       decimalDigits: Constants.Defaults.DecimalDigits,
                       isInArray: Constants.Defaults.IsInArray,
                       arrayElementIndex: Constants.Defaults.ArrayElementIndex,
                       isRedefine: isRedefine,
                       isFiller: Constants.Defaults.IsFiller);
        }

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        public IFieldInitializer NewFieldObject(string name, IElementCollection parentCollection, IDataBuffer buffer)
        {
            return NewFieldObject(name,
                       parentCollection,
                       buffer,
                       fieldType: FieldType.String,
                       lengthInBuffer: Constants.Defaults.LengthInBuffer,
                       displayLength: Constants.Defaults.LengthInBuffer,
                       positionInParent: Constants.Defaults.PositionInParent,
                       decimalDigits: Constants.Defaults.DecimalDigits,
                       isInArray: Constants.Defaults.IsInArray,
                       arrayElementIndex: Constants.Defaults.ArrayElementIndex,
                       isRedefine: Constants.Defaults.IsRedefine,
                       isFiller: Constants.Defaults.IsFiller);
        }

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        public IFieldInitializer NewFieldObject(string name,
            IElementCollection parentCollection,
            IDataBuffer buffer,
            FieldType fieldType)
        {
            return NewFieldObject(name,
                       parentCollection,
                       buffer,
                       fieldType,
                       lengthInBuffer: Constants.Defaults.LengthInBuffer,
                       displayLength: Constants.Defaults.LengthInBuffer,
                       positionInParent: Constants.Defaults.PositionInParent,
                       decimalDigits: Constants.Defaults.DecimalDigits,
                       isInArray: Constants.Defaults.IsInArray,
                       arrayElementIndex: Constants.Defaults.ArrayElementIndex,
                       isRedefine: Constants.Defaults.IsRedefine,
                       isFiller: Constants.Defaults.IsFiller);
        }

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        public IFieldInitializer NewFieldObject(string name,
            IElementCollection parentCollection,
            IDataBuffer buffer,
            FieldType fieldType,
            int lengthInBuffer)
        {
            return NewFieldObject(name,
                       parentCollection,
                       buffer,
                       fieldType,
                       lengthInBuffer,
                       displayLength: lengthInBuffer,
                       positionInParent: Constants.Defaults.PositionInParent,
                       decimalDigits: Constants.Defaults.DecimalDigits,
                       isInArray: Constants.Defaults.IsInArray,
                       arrayElementIndex: Constants.Defaults.ArrayElementIndex,
                       isRedefine: Constants.Defaults.IsRedefine,
                       isFiller: Constants.Defaults.IsFiller);
        }

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        public IFieldInitializer NewFieldObject(string name,
            IElementCollection parentCollection,
            IDataBuffer buffer,
            FieldType fieldType,
            int lengthInBuffer,
            int displayLength)
        {
            return NewFieldObject(name,
                       parentCollection,
                       buffer,
                       fieldType,
                       lengthInBuffer,
                       displayLength,
                       positionInParent: Constants.Defaults.PositionInParent,
                       decimalDigits: Constants.Defaults.DecimalDigits,
                       isInArray: Constants.Defaults.IsInArray,
                       arrayElementIndex: Constants.Defaults.ArrayElementIndex,
                       isRedefine: Constants.Defaults.IsRedefine,
                       isFiller: Constants.Defaults.IsFiller);
        }

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        public IFieldInitializer NewFieldObject(string name,
            IElementCollection parentCollection,
            IDataBuffer buffer,
            FieldType fieldType,
            int lengthInBuffer,
            int displayLength,
            int positionInParent)
        {
            return NewFieldObject(name,
                       parentCollection,
                       buffer,
                       fieldType,
                       lengthInBuffer,
                       displayLength,
                       positionInParent,
                       decimalDigits: Constants.Defaults.DecimalDigits,
                       isInArray: Constants.Defaults.IsInArray,
                       arrayElementIndex: Constants.Defaults.ArrayElementIndex,
                       isRedefine: Constants.Defaults.IsRedefine,
                       isFiller: Constants.Defaults.IsFiller);
        }

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal point.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        public IFieldInitializer NewFieldObject(string name,
            IElementCollection parentCollection,
            IDataBuffer buffer,
            FieldType fieldType,
            int lengthInBuffer,
            int displayLength,
            int positionInParent,
            int decimalDigits)
        {
            return NewFieldObject(name,
                       parentCollection,
                       buffer,
                       fieldType,
                       lengthInBuffer,
                       displayLength,
                       positionInParent,
                       decimalDigits,
                       isInArray: Constants.Defaults.IsInArray,
                       arrayElementIndex: Constants.Defaults.ArrayElementIndex,
                       isRedefine: Constants.Defaults.IsRedefine,
                       isFiller: Constants.Defaults.IsFiller);
        }

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal point.</param>
        /// <param name="isInArray">Indicates whether the new field exists at any level within an array.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        public IFieldInitializer NewFieldObject(string name,
            IElementCollection parentCollection,
            IDataBuffer buffer,
            FieldType fieldType,
            int lengthInBuffer,
            int displayLength,
            int positionInParent,
            int decimalDigits,
            bool isInArray)
        {
            return NewFieldObject(name,
                        parentCollection,
                        buffer,
                        fieldType,
                        lengthInBuffer,
                        displayLength,
                        positionInParent,
                        decimalDigits,
                        isInArray,
                        arrayElementIndex: Constants.Defaults.ArrayElementIndex,
                        isRedefine: Constants.Defaults.IsRedefine,
                        isFiller: Constants.Defaults.IsFiller);
        }

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal point.</param>
        /// <param name="isInArray">Indicates whether the new field exists at any level within an array.</param>
        /// <param name="arrayElementIndex">The index of the new field in its array parent.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        public IFieldInitializer NewFieldObject(string name,
            IElementCollection parentCollection,
            IDataBuffer buffer,
            FieldType fieldType,
            int lengthInBuffer,
            int displayLength,
            int positionInParent,
            int decimalDigits,
            bool isInArray,
            int arrayElementIndex)
        {
            return NewFieldObject(name,
                        parentCollection,
                        buffer,
                        fieldType,
                        lengthInBuffer,
                        displayLength,
                        positionInParent,
                        decimalDigits,
                        isInArray,
                        arrayElementIndex,
                        isRedefine: Constants.Defaults.IsRedefine,
                        isFiller: Constants.Defaults.IsFiller);
        }

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal point.</param>
        /// <param name="isInArray">Indicates whether the new field exists at any level within an array.</param>
        /// <param name="arrayElementIndex">The index of the new field in its array parent.</param>
        /// <param name="isRedefine">Indicates whether the new field participates in a REDEFINE.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        public IFieldInitializer NewFieldObject(string name,
            IElementCollection parentCollection,
            IDataBuffer buffer,
            FieldType fieldType,
            int lengthInBuffer,
            int displayLength,
            int positionInParent,
            int decimalDigits,
            bool isInArray,
            int arrayElementIndex,
            bool isRedefine)
        {
            return NewFieldObject(name,
                        parentCollection,
                        buffer,
                        fieldType,
                        lengthInBuffer,
                        displayLength,
                        positionInParent,
                        decimalDigits,
                        isInArray,
                        arrayElementIndex,
                        isRedefine,
                        isFiller: Constants.Defaults.IsFiller);
        }

        /// <summary>
        /// Returns a newly instantiated IField-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="fieldType">Type of the new field.</param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal point.</param>
        /// <param name="isInArray">Indicates whether the new field exists at any level within an array.</param>
        /// <param name="arrayElementIndex">The index of the new field in its array parent.</param>
        /// <param name="isRedefine">Indicates whether the new field participates in a REDEFINE.</param>
        /// <param name="isFiller">Indicates whether the new field is declared FILLER.</param>
        /// <returns>An appropriate FieldBase descendant.</returns>
        public IFieldInitializer NewFieldObject(string name,
            IElementCollection parentCollection,
            IDataBuffer buffer,
            FieldType fieldType,
            int lengthInBuffer,
            int displayLength,
            int positionInParent,
            int decimalDigits,
            bool isInArray,
            int arrayElementIndex,
            bool isRedefine,
            bool isFiller)
        {

            string resolutionName = isRedefine ?
                Constants.TypeMappingRegistrationNames.Redefine :
                string.Empty;

            IFieldInitializer result;
            //Concrete class is used rather than Unity, as there are no dependencies injected.
            //The Object Factory singleton instance can be overridden if needed to return a different type

            if (resolutionName == Constants.TypeMappingRegistrationNames.Redefine)
                result = new RedefineField();
            else
            {
                result = new Field();
            }
            result.Name = isFiller ? ObjectFactory.GetRandomName(name) : name;
            result.FieldType = fieldType;
            result.LengthInBuffer = lengthInBuffer;
            result.DisplayLength = displayLength;
            result.Parent = parentCollection;
            result.Buffer = buffer;
            result.PositionInParent = positionInParent;
            result.DecimalDigits = decimalDigits;
            result.IsInArray = isInArray;
            result.IsFiller = isFiller;
            result.ArrayElementIndex = arrayElementIndex;
            return result;
        }

        #endregion

        #region NewGroupObject

        /// <summary>
        /// Returns a newly instantiated IGroup-implementing object populated with the given values.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="parentCollection">Parent IElementCollection of the new field.</param>
        /// <param name="buffer">Buffer object for the new field.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <param name="isInArray">Indicates whether the new field exists at any level within an array.</param>
        /// <param name="isInRedefine">Indicates whether the new field participates in a REDEFINE.</param>
        /// <returns>An appropriate GroupBase descendant.</returns>
        public IGroupInitializer NewGroupObject(string name,
            IElementCollection parentCollection,
            IDataBuffer buffer,
            int positionInParent,
            IDictionary<string, IArrayElementAccessorBase> accessors,
            bool isInArray,
            bool isInRedefine)
        {
            string resolutionName = isInRedefine ?
                Constants.TypeMappingRegistrationNames.Redefine :
                string.Empty;
            //Concrete class is used rather than Unity, as there are no dependencies injected.
            //The Object Factory singleton instance can be overridden if needed to return a different type
            IGroupInitializer result;
            if (resolutionName == Constants.TypeMappingRegistrationNames.Redefine)
                result = new RedefineGroup();
            else
            {
                result = new Group();
            }

            result.Name = name;
            result.Buffer = buffer;
            result.Parent = parentCollection;
            result.PositionInParent = positionInParent;
            result.IsInArray = isInArray;
            result.DefineTimeAccessors = accessors;

            return result;
        }
        #endregion

        #region NewRedefineGroupObject

        /// <summary>
        /// Returns a new RedefineField object built from the given info.
        /// </summary>
        /// <param name="name">Name of the new group.</param>
        /// <param name="buffer">Buffer for the new group.</param>
        /// <param name="parentCollection">Parent element collection of the new group.</param>
        /// <param name="elementToRedefine">Group element to be redefined.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <param name="isInArray">Indicates whether the new field exists at any level within an array.</param>
        /// <param name="arrayElementIndex">The index of the new field in its array parent.</param>
        /// <returns>A new RedefineGroup.</returns>
        public IGroupInitializer NewRedefineGroupObject(string name,
            IDataBuffer buffer,
            IElementCollection parentCollection,
            IBufferElement elementToRedefine,
            int positionInParent,
            IDictionary<string, IArrayElementAccessorBase> arrayElementAccessors,
            bool isInArray)
        {
            var result = NewGroupObject(name, parentCollection, buffer, positionInParent, arrayElementAccessors, isInArray, true);
            if (result == null)
                throw new RecordStructureException("While creating a RedefineGroup there was a problem casting the result.");

            if (result is IRedefinition)
            {
                (result as IRedefinition).RedefinedElement = elementToRedefine;
            }
            else
            {
                throw new RecordStructureException("While creating a RedefineGroup there was a problem casting the result.");
            }

            return result;
        }
        #endregion

        #region NewRedefineFieldObject
        /// <summary>
        /// Returns a new RedefineField object built from the given info.
        /// </summary>
        /// <param name="name">Name of the new field.</param>
        /// <param name="buffer">Buffer for the new field.</param>
        /// <param name="parentCollection">Parent element collection of the new field.</param>
        /// <param name="fieldType">Data type of the new field.</param>
        /// <param name="elementToRedefine"></param>
        /// <param name="lengthInBuffer">Number of bytes the field will occupy in the buffer.</param>
        /// <param name="displayLength">Number of bytes required for display.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal point.</param>
        /// <param name="positionInParent">Index position of the new field in its parent.</param>
        /// <param name="isInArray">Indicates whether the new field exists at any level within an array.</param>
        /// <param name="arrayElementIndex">The index of the new field in its array parent.</param>
        /// <param name="isFiller">Indicates whether the new field is declared FILLER.</param>
        /// <returns>A new RedefineField.</returns>
        public IFieldInitializer NewRedefineFieldObject(string name,
            IDataBuffer buffer,
            IElementCollection parentCollection,
            FieldType fieldType,
            IBufferElement elementToRedefine,
            int lengthInBuffer,
            int displayLength,
            int decimalDigits,
            int positionInParent,
            bool isInArray,
            int arrayElementIndex,
            bool isFiller)
        {
            var result = NewFieldObject(name,
                            parentCollection,
                            buffer,
                            fieldType,
                            lengthInBuffer,
                            displayLength,
                            positionInParent,
                            decimalDigits,
                            isInArray,
                            arrayElementIndex,
                            isRedefine: true,
                            isFiller: isFiller);

            if (result is IRedefinition)
            {
                (result as IRedefinition).RedefinedElement = elementToRedefine;
            }
            else
            {
                throw new RecordStructureException("While creating a RedefineField there was a problem casting the result.");
            }

            return result;
        }

        #endregion

        #region NewRecordObject
        /// <summary>
        /// Creates and returns a new IRecord-implementation using the given <paramref name="structureDefinition"/> delegate.
        /// </summary>
        /// <param name="name">Name of the new record.</param>
        /// <param name="structureDefinition">The structure definition logic to be performed on the new record.</param>
        /// <returns>A defined new record object.</returns>
        public IRecord NewRecordObject(string name, Action<IStructureDefinition> structureDefinition)
        {
            IRecord chachedRecord = null;
            if (name == "WsExternals")
            {
                return GetRecord(name, structureDefinition);
            }
            if (BufferServices.RecordDefinitions.ContainsKey(name))
            {
                chachedRecord = BufferServices.RecordDefinitions[name];
            }

            if (chachedRecord == null)
            {
                Object recordDefLock = null;
                if (recordDefLockColl.ContainsKey(name))
                    recordDefLock = recordDefLockColl[name];
                if (recordDefLock == null)
                {
                    lock (recordDefLockColl)
                    {
                        if (!recordDefLockColl.ContainsKey(name) || recordDefLockColl[name] == null)
                        {
                            recordDefLock = new Object();
                            recordDefLockColl.Add(name, recordDefLock);
                        }
                        else
                            recordDefLock = recordDefLockColl[name];
                    }
                }

                lock (recordDefLock)
                {
                    if (!BufferServices.RecordDefinitions.ContainsKey(name))
                    {
                        Record result = GetRecord(name, structureDefinition);
                        BufferServices.RecordDefinitions.Add(name, result);
                    }

                    chachedRecord = BufferServices.RecordDefinitions[name];
                }
            }

            IRecord resultRecord = (IRecord)chachedRecord.Clone();
            // Any records created via factory get added to the system record collection. 
            BufferServices.Records.Add(chachedRecord);

            return resultRecord;
        }

        #endregion

        public Record GetRecord(string name, Action<IStructureDefinition> structureDefinition)
        {
            var result = new Record() { Name = name };
            //We instantiate Buffer with a Concrete type rather than use unity.
            //This factory can be overridden to return a difference type
            result.Buffer = new DataBufferByteList();
            structureDefinition(result);
            result.EndDefinition();
            return result;
        }

        #region public properties

        /// <summary>
        /// Gets whether the array is 0-based or not
        /// </summary>
        public static bool IsZeroBasedArrays
        {
            get
            {
                return GetIsZeroBasedArrays();
            }
        }
        #endregion
    }
}
