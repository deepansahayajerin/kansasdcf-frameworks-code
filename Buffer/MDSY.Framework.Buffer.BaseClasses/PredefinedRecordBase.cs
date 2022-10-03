using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Services;
using MDSY.Framework.Configuration.Common;
namespace MDSY.Framework.Buffer.BaseClasses
{
    /// <summary>
    /// Provides a base class for record types that would be pre-defined.
    /// </summary>
    /// <remarks>
    /// <para>The actual record structure is contained as an <c>IRecord</c> object in the Record property.</para>
    /// <para>Descendant record classes can use the GetArrayElementAccessor and GetElementByName methods to access elements
    /// within the internal Record object. These two methods allow descendant records to define direct-access record 
    /// element properties (see example).</para>
    /// <para>Descendant classes should override DefineRecordStructure() to provide the actual data structure definition 
    /// for their record. (see example).</para>
    /// </remarks>
    /// <example>
    /// This is an example of a PredefinedRecordBase descendant which describes the structure:
    /// <code>
    /// ExampleRecord
    ///   - ROOTGROUP
    ///     - FIELD01
    ///     - GROUPARRAY[3]
    ///       - FIELD02 
    ///       - FIELD03
    /// public class ExampleRecord : PredefinedRecordBase
    /// {
    ///     /// &lt;summary&gt;
    ///     /// Name constants for record elements; provide one name constant for each element not decorated as &lt;c&gt;FILLER&lt;/c&gt;.
    ///     /// &lt;/summary&gt;
    ///     internal static class Names
    ///     {
    ///         internal const string RecordName = "ExampleRecord";
    ///         internal const string ROOTGROUP = "ROOTGROUP";
    ///         internal const string FIELD01 = "FIELD01";
    ///         internal const string GROUPARRAY = "GROUPARRAY";
    ///         internal const string FIELD02 = "FIELD02";
    ///         internal const string FIELD03 = "FIELD03";
    ///     }
    ///
    ///     public IGroup ROOTGROUP { get { return GetElementByName&lt;IGroup&gt;(Names.ROOTGROUP); } }
    ///     public IField FIELD01 { get { return GetElementByName&lt;IField&gt;(Names.FIELD01); } }
    ///     public IArrayElementAccessor&lt;IGroup&gt; GROUPARRAY { get { return GetArrayElementAccessor&lt;IGroup&gt;(Names.GROUPARRAY); } }
    ///     public IArrayElementAccessor&lt;IField&gt; FIELD02 { get { return GetArrayElementAccessor&lt;IField&gt;(Names.FIELD02); } }
    ///     public IArrayElementAccessor&lt;IField&gt; FIELD03 { get { return GetArrayElementAccessor&lt;IField&gt;(Names.FIELD03); } }
    ///
    ///     /// &lt;summary&gt;
    ///     /// Defines the entirety of the APPLICATION_GLOBAL_RECORD IRecord structure as described by the MDSY.Framework.Buffer API. 
    ///     /// &lt;/summary&gt;
    ///     /// &lt;param name="recordDef"&gt;The IStructureDefinition object to be used in defining the record structure.&lt;/param&gt;
    ///     protected override void DefineRecordStructure(IStructureDefinition recordDef)
    ///     {
    ///         recordDef.NewGroup(Names.ROOTGROUP, (ROOTGROUP) =>
    ///         {
    ///             ROOTGROUP.NewField(Names.FIELD01, FieldType.String, 10, "DefaultVAL");
    ///             ROOTGROUP.NewGroupArray(Names.GROUPARRAY, 3, null,
    ///                 (GROUPARRAY) =>
    ///                 {
    ///                     GROUPARRAY.NewField(Names.FIELD02, BufferFieldType.PackedDecimal, 5, 3.1415, 4);
    ///                     GROUPARRAY.NewField(Names.FIELD03, BufferFieldType.Boolean, 1, true);
    ///                 },
    ///                 null, null);
    ///         });
    ///     }
    ///
    ///     protected override string GetRecordName()
    ///     {
    ///         return Names.RecordName;
    ///     }
    /// }
    /// </code></example>
    [Serializable]
    public abstract class PredefinedRecordBase : IAssignable,
        IComparable<IField>, IComparable<IGroup>, IComparable<IRecord>, IComparable<PredefinedRecordBase>,
        IEquatable<IField>, IEquatable<IGroup>, IEquatable<IRecord>, IEquatable<PredefinedRecordBase>,
        IBufferValue   // <-- just as a heads-up, PredefinedRecordBase should not be implementing IBufferValue, 
    // which defines objects which maintain a value *within* the buffer. e.g. fields, groups, etc. 
    {
        #region protected properties
        private IRecord record = null;

        /// <summary>
        /// Zero-value constant.
        /// </summary>
        protected const int ZERO = 0;

        /// <summary>
        /// Zero-value constant.
        /// </summary>
        protected const int ZEROS = 0;

        /// <summary>
        /// Zero-value constant.
        /// </summary>
        protected const int ZEROES = 0;

        /// <summary>
        /// Single space constant.
        /// </summary>
        protected const string SPACE = " ";

        /// <summary>
        /// Songle space constant.
        /// </summary>
        protected const string SPACES = " ";
        protected const string QUOTE = "'";
        #endregion

        #region protected methods

        /// <summary>
        /// Returns an IArrayElementAccessor(of T) with the given <paramref name="name"/>.
        /// </summary>
        protected IArrayElementAccessor<T> GetArrayElementAccessor<T>(string name) where T : IArrayElement
        {
            return Record.GetArrayElementAccessor<T>(name);
        }

        /// <summary>
        /// Gets an element of type <typeparamref name="T"/>, with the given <paramref name="name"/>, 
        /// from the internal IRecord object.
        /// Can be used by descendant pre-defined record classes to provide direct-access to record structure elements
        /// by name. 
        /// </summary>
        /// <typeparam name="T">The IBufferElement-descendant type of the element to retrieve.</typeparam>
        /// <param name="name">The unique name for which to search.</param>
        /// <returns>An <see cref="IBufferElement"/>-implementing object of type <typeparamref name="T"/> if found, otherwise <c>null</c>.</returns>
        protected T GetElementByName<T>(string name) where T : IBufferElement
        {
            return (T)(Record.StructureElementByName(name));
        }
        /// <summary>
        /// Returns external element from WsExternals
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        protected T GetExternalElementByName<T>(string name) where T : IBufferElement
        {
            return (T)(WsExternals.Instance.Record.StructureElementByName(name));
        }
        /// <summary>
        /// Returns external array element from WsExternal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        protected IArrayElementAccessor<T> GetExternalArrayElementAccessor<T>(string name) where T : IArrayElement
        {
            return WsExternals.Instance.Record.GetArrayElementAccessor<T>(name);
        }

        /// <summary>
        /// Calls to <see cref="MDSY.Framework.Buffer.BufferServices"/>.Factory to create a new IRecord-implementing 
        /// object with a name of RecordName and defined by the DefineRecordStructure method. 
        /// </summary>
        /// <returns>A new IRecord-implementing object.</returns>
        protected virtual IRecord CreateRecordObject()
        {
            return BufferServices.Factory.NewRecord(RecordName, DefineRecordStructure);
        }

        /// <summary>
        /// Defines the entirety of the IRecord structure. Descendant classes should override DefineRecordStructure to
        /// provide record-specific structure logic as described by the MDSY.Framework.Buffer API. 
        /// </summary>
        /// <seealso cref="MDSY.Framework.Buffer.Interfaces.IRecord"/>
        /// <seealso cref="MDSY.Framework.Buffer.Interfaces.IStructureDefinition"/>
        /// <param name="recordDef">The IStructureDefinition object to be used in defining the record structure.</param>
        protected abstract void DefineRecordStructure(IStructureDefinition recordDef);

        /// <summary>
        /// Returns the name of the record object. Descendant classes should override GetRecordName to return a unique name for the record object.
        /// </summary>
        protected abstract string GetRecordName();

        /// <summary>
        /// Resets current record to the initial value.
        /// </summary>
        public virtual void Initialize()
        {
            ResetToInitialValue();
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets the IRecord object that represents the record structure definition. 
        /// The record object is created on demand by calling <see cref="CreateRecordObject"/>.
        /// </summary>
        public IRecord Record
        {
            get
            {
                if (record == null)
                {
                    record = CreateRecordObject();
                }
                return record;
            }
            set
            {
                record = value;
            }
        }

        /// <summary>
        /// Gets the name of this record object and of the IRecord created by <see cref="CreateRecordObject"/>.
        /// </summary>
        public string RecordName
        {
            get { return GetRecordName(); }
        }

        /// <summary>
        /// Returns the contents of the current record as an array of bytes.
        /// </summary>
        public byte[] AsBytes { get { return Record.AsBytes(); } }

        /// <summary>
        /// Sets and returns a reference to the record's buffer.
        /// </summary>
        public IDataBuffer Buffer
        {
            get { return Record.Buffer; }
            set { Record.Buffer = value; }
        }

        /// <summary>
        /// Returns a string representation of the current record content.
        /// </summary>
        public string BytesAsString { get { return Record.AsString(); } }

        /// <summary>
        /// Returns a string representation of the redefined bytes.
        /// </summary>
        public string RedefinedBytesAsString { get { return Record.AsString(); } }

        /// <summary>
        /// Returns a string representation of the current record content.
        /// </summary>
        public string DisplayValue { get { return Record.AsString(); } }

        /// <summary>
        /// Returns the length of the record's buffer.
        /// </summary>
        public int LengthInBuffer { get { return Buffer.Length; } }
        #endregion

        #region public methods

        /// <summary>
        /// Assigns the given value to the object.
        /// </summary>
        /// <param name="value">The new value to assign to the object.</param>
        public void Assign(object value)
        {
            Record.Assign(value);
        }
        /// <summary>
        /// Assigns the given string value to the object, as appropriate.
        /// </summary>
        public void AssignFrom(string value)
        {
            Record.AssignFrom(value);
        }

        /// <summary>
        /// Assigns the given <paramref name="bytes"/> to this object, as appropriate.
        /// </summary>
        public void AssignFrom(byte[] bytes)
        {
            if (bytes == null)
                Record.ResetToInitialValue();
            Record.AssignFrom(bytes);
        }

        /// <summary>
        /// Assigns the value of the given <paramref name="element"/> to this object, as appropriate.
        /// </summary>
        public void AssignFrom(IBufferValue element)
        {
            Record.AssignFrom(element);
        }

        /// <summary>
        /// Assigns provided buffer value to the current record buffer.
        /// </summary>
        /// <param name="element">A reference to the buffer value object.</param>
        /// <param name="sourceFieldType">Not used. Can take any value.</param>
        public void AssignFrom(IBufferValue element, FieldType sourceFieldType)
        {
            Record.AssignFrom(element);
        }

        /// <summary>
        /// Assigns provided group value to the current record buffer.
        /// </summary>
        /// <param name="group">A reference to the group value object.</param>
        public void AssignFromGroup(IGroup group)
        {
            Record.AssignFrom(group.AsBytes);
        }

        /// <summary>
        /// Does nothing. For interface compatibity only.
        /// </summary>
        /// <param name="value">Not used. Can take any value.</param>
        public void AssignIdRecordName(string value)
        {
            // do nothing
        }

        /// <summary>
        /// Copies IdRecordName values from the provided record object to the current record object.
        /// Record objects must have the same names. Does nothing if provided object is not a record 
        /// or if the provided record has different name than the current record.
        /// This method is for passing records as parameters from one program to another program.
        /// </summary>
        /// <param name="value">A reference to a record object.</param>
        public void AssignIdRecordName(IBufferValue value)
        {
            if (value is PredefinedRecordBase && ((PredefinedRecordBase)value).RecordName == RecordName)
            {
                foreach (IBufferElement element in Record.Elements)
                {
                    if (element is IField && (((IField)element).IsNumericType || ((IField)element).FieldType == FieldType.NumericEdited))
                    {
                        IRecord rvalue = ((PredefinedRecordBase)value).Record;
                        IBufferElement be = rvalue[((IField)element).Name];
                        ((IField)element).AssignIdRecordName(((IField)be).GetIdRecordName());
                    }
                    else if (element is IGroup)
                    {
                        foreach (IBufferElement child in ((IGroup)element).ChildCollection.Values)
                        {
                            if (child is IField && (((IField)child).IsNumericType || ((IField)child).FieldType == FieldType.NumericEdited))
                            {
                                IRecord rvalue = ((PredefinedRecordBase)value).Record;
                                IGroup rgroup = (IGroup)rvalue.GetElementByNameNested(((IGroup)element).Name);
                                ((IField)child).AssignIdRecordName(((IField)rgroup.ChildCollection[((IField)child).Name]).GetIdRecordName());
                            }
                        }
                    }
                    else if (element is IGroupArray)
                    {
                        foreach (IBufferElement group in ((IGroupArray)element).Elements)
                        {
                            foreach (IBufferElement child in ((IGroup)group).ChildCollection.Values)
                            {
                                if (child is IField && (((IField)child).IsNumericType || ((IField)child).FieldType == FieldType.NumericEdited))
                                {
                                    IRecord rvalue = ((PredefinedRecordBase)value).Record;
                                    string[] groupName = group.Name.Split(' ');
                                    IGroupArray groupArray = (IGroupArray)rvalue.GetElementByNameNested(groupName[0]);
                                    IGroup rgroup = (IGroup)groupArray.Elements.ToArray()[int.Parse(groupName[1])];
                                    ((IField)child).AssignIdRecordName(((IField)rgroup.ChildCollection[((IField)child).Name]).GetIdRecordName());
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns an empty string. For interface compatibility only.
        /// </summary>
        /// <returns>Returns an empty string.</returns>
        public string GetIdRecordName()
        {
            return "";
        }

        /// <summary>
        /// Assigns provided buffer value to the record's buffer.
        /// </summary>
        /// <param name="value">A reference to the provided buffer value.</param>
        public void SetValue(IBufferValue value)
        {
            AssignFrom(value);
        }

        /// <summary>
        /// Assigns provided record to the current record.
        /// </summary>
        /// <param name="value">A reference to the record object.</param>
        public void SetValue(PredefinedRecordBase value)
        {
            Record.AssignFrom(value.Record);
        }

        /// <summary>
        /// Assigns provided string value to the current record.
        /// </summary>
        /// <param name="value">String value.</param>
        public void SetValue(string value)
        {
            if (value != null)
                Record.AssignFrom(value);
        }

        public void SetValueWithNullCheck(byte[] bytes)
        {
            if (bytes != null && bytes.Length > 0)
                Record.AssignFrom(bytes);
        }

        /// <summary>
        /// Assigns provided array of bytes to the current record.
        /// </summary>
        /// <param name="bytes">Array of bytes value.</param>
        public void SetBytes(byte[] bytes)
        {
            AssignFrom(bytes);
        }

        /// <summary>
        /// Copies contents of one buffer value to another buffer value.
        /// This method is for returning parameters from one program to another.
        /// </summary>
        /// <param name="parm">A reference to the source buffer value.</param>
        /// <param name="arg">A reference to the target buffer value object.</param>
        public void SetReturnParm(IBufferValue parm, object arg)
        {
            if (arg is IBufferValue)
            {
                IBufferValue argvalue = (IBufferValue)arg;
                argvalue.SetValue(parm);
                argvalue.AssignIdRecordName(parm);
            }
            else if (arg is IRecord)
            {
                IRecord argvalue = (IRecord)arg;
                argvalue.AssignFrom(parm);
            }
            else if (arg is StringParm)
            {
                StringParm prm = (StringParm)arg;
                prm.Value = parm.BytesAsString;
            }
            else if (arg is byte[])
            {
                byte[] bytearr = (byte[])arg;
                int baLength = bytearr.Length;
                if (baLength > parm.AsBytes.Length)
                    baLength = parm.AsBytes.Length;
                System.Buffer.BlockCopy(parm.AsBytes, 0, bytearr, 0, baLength);
            }
        }
        /// <summary>
        /// Copies contents of one buffer value to another buffer value.
        /// This method is for returning parameters from one program to another.
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="args"></param>
        /// <param name="parmCtr"></param>
        public void SetReturnParm(IBufferValue parm, object[] args, int parmCtr)
        {
            if (parmCtr < args.Length)
                SetReturnParm(parm, args[parmCtr]);

        }

        /// <summary>
        /// Copies contents of one buffer value to another buffer value.
        /// This method is for passing parameters from one program to another.
        /// </summary>
        /// <param name="parm">A reference to the target buffer value.</param>
        /// <param name="arg">A reference to the source buffer value.</param>
        public void SetPassedParm(IBufferValue parm, object arg)
        {
            if (arg is IBufferValue)
            {
                parm.SetValue((IBufferValue)arg);
                parm.AssignIdRecordName((IBufferValue)arg);
            }
            else if (arg is IRecord)
            {
                parm.SetValue((IRecord)arg);
            }
            else if (arg is string)
            {
                string argString = arg.ToString();
                parm.SetValue(argString);

                if (ConfigSettings.GetAppSettingsBool("IsBatch"))
                {
                    //BEG Does the LINKAGE SECTION have a PARM-LEN PIC S9(4) COMP 
                    List<IBufferElement> elementList = new List<IBufferElement>();
                    elementList = ((IGroup)parm).Elements.ToList();

                    bool needLinkageLen = false;
                    if (((IField)elementList[0]).FieldType == FieldType.CompShort)
                    {
                        needLinkageLen = true;
                    }
                    //END Does the LINKAGE SECTION have a PARM-LEN PIC S9(4) COMP                     

                    //Tests ran on mainframe
                    //  Pass a parm larger than linkage size it reports the linkage size
                    //  Pass a parm smaller than linkage size it reports the actual size

                    if (argString.Length > parm.Buffer.Length)
                    {
                        //The parm being passed is larger than the parm buffer
                        if (needLinkageLen)
                            argString = argString.Substring(0, Buffer.Length - 2);
                        else
                            argString = argString.Substring(0, Buffer.Length);
                    }

                    if (needLinkageLen)
                    {
                        //Get COMP LEN of parms
                        byte[] parmLen = GetParmLenAsComp(argString.Length);

                        //Get Byte representation of parm string
                        byte[] parmVal = Encoding.ASCII.GetBytes(argString);

                        byte[] bytes = new byte[parmLen.Length + parmVal.Length];
                        System.Buffer.BlockCopy(parmLen, 0, bytes, 0, parmLen.Length);
                        System.Buffer.BlockCopy(parmVal, 0, bytes, parmLen.Length, parmVal.Length);
                        parm.SetValue(bytes);
                    }
                    else
                    {
                        parm.SetValue(argString);
                    }
                }
            }
            else if (arg is int)
            {
                parm.SetValue((int)arg);
            }
            else if (arg is byte[])
            {
                parm.SetValue((byte[])arg);
            }
            else if (arg is StringParm)
            {
                StringParm prm = (StringParm)arg;
                parm.SetValue(prm.Value);
            }
        }

        private byte[] GetParmLenAsComp(int ParmLen)
        {
            byte[] result = BitConverter.GetBytes(ParmLen);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            PadTrimNumericBytes(2, ref result);
            return result;
        }

        private void PadTrimNumericBytes(int byteCount, ref byte[] bytes)
        {
            byte[] resultBytes = new byte[byteCount];
            int offset = bytes.Length - byteCount;

            if (offset < 0)
                offset = 0;

            System.Buffer.BlockCopy(bytes, offset, resultBytes, 0, byteCount);
            bytes = resultBytes;
        }


        /// <summary>
        /// Copies contents of one buffer value to another buffer value.
        /// This method is for passing parameters from one program to another.
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="args"></param>
        /// <param name="parmCtr"></param>
        public void SetPassedParm(IBufferValue parm, object[] args, int parmCtr)
        {
            if (parmCtr < args.Length)
                SetPassedParm(parm, args[parmCtr]);
            else
                parm.ResetToInitialValue();
        }
        /// <summary>
        /// Determine DFHRESP error codes showng codes returned from mainframe CICS services
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public int DFHRESP(string errorCode)
        {
            switch (errorCode)
            {
                case "NORMAL": return 0;
                case "ERROR": return 1;
                case "RDATT": return 2;
                case "WRBRK": return 3;
                case "EOF": return 4;
                case "EODS": return 5;
                case "EOC": return 6;
                case "INBFMH": return 7;
                case "ENDINPT": return 8;
                case "NONVAL": return 9;
                case "NOSTART": return 10;
                case "TERMIDERR": return 11;
                case "FILENOTFOUND": return 12;
                case "NOTFND": return 13;
                case "DUPREC": return 14;
                case "DUPKEY": return 15;
                case "INVREQ": return 16;
                case "IOERR": return 17;
                case "NOSPACE": return 18;
                case "NOTOPEN": return 19;
                case "ENDFILE": return 20;
                case "ILLOGIC": return 21;
                case "LENGERR": return 22;
                case "QZERO": return 23;
                case "SIGNAL": return 24;
                case "QBUSY": return 25;
                case "ITEMERR": return 26;
                case "PGMIDERR": return 27;
                case "TRANSIDERR": return 28;
                case "ENDDATA": return 29;
                case "EXPIRED": return 31;
                case "RETPAGE": return 32;
                case "RTEFAIL": return 33;
                case "RTESOME": return 34;
                case "TSIOERR": return 35;
                case "MAPFAIL": return 36;
                case "INVERRTERM": return 37;
                case "INVMPSZ": return 38;
                case "IGREQID": return 39;
                case "OVERFLOW": return 40;
                case "INVLDC": return 41;
                case "NOSTG": return 42;
                case "JIDERR": return 43;
                case "QIDERR": return 44;
                case "NOJBUFSP": return 45;
                case "DSSTAT": return 46;
                case "SELNERR": return 47;
                case "FUNCERR": return 48;
                case "UNEXPIN": return 49;
                case "NOPASSBKRD": return 50;
                case "NOPASSBKWR": return 51;
                case "SYSIDERR": return 53;
                case "ISCINVREQ": return 54;
                case "ENQBUSY": return 55;
                case "IGREQCD": return 57;
                case "SESSIONERR": return 58;
                case "SYSBUSY": return 59;
                case "SESSBUSY": return 60;
                case "NOTALLOC": return 61;
                case "CBIDERR": return 62;
                case "INVEXITREQ": return 63;
                case "INVPARTNSET": return 64;
                case "INVPARTN": return 65;
                case "PARTNFAIL": return 66;
                case "USERIDERR": return 69;
                case "NOTAUTH": return 70;
                case "SUPPRESSED": return 72;
                case "NOSPOOL": return 80;
                case "TERMERR": return 81;
                case "ROLLEDBACK": return 82;
                case "END": return 83;
                case "DISABLED": return 84;
                case "ALLOCERR": return 85;
                case "STRELERR": return 86;
                case "OPENERR": return 87;
                case "SPOLBUSY": return 88;
                case "SPOLERR": return 89;
                case "NODEIDERR": return 90;
                case "TASKIDERR": return 91;
                case "TCIDERR": return 92;
                case "DSNNOTFOUND": return 93;
                case "LOADING": return 94;
                case "MODELIDERR": return 95;
                case "OUTDESCRERR": return 96;
                case "PARTNERIDERR": return 97;
                case "PROFILEIDERR": return 98;
                case "NETNAMERR	": return 99;
                case "LOCKED": return 100;
                case "RECORDBUSY": return 101;
                case "UOWNOTFOUND": return 102;
                case "UOWLNOTFOUND": return 103;
                case "LINKABEND": return 104;
                case "CHANGED": return 105;
                case "PROCESSBUSY": return 106;
                case "ACTIVITYBUSY": return 107;
                case "PROCESSERR": return 108;
                case "ACTIVITYERR": return 109;
                case "CONTAINERERR": return 110;
                case "EVENTERR": return 111;
                case "TOKENERR": return 112;
                case "NOTFINISHED": return 113;
                case "POOLERR": return 114;
                case "TIMERERR": return 115;
                case "SYMBOLERR": return 116;
                case "TEMPLATERR": return 117;
                case "RESUNAVAIL": return 121;
                case "CHANNELERR": return 122;
                case "CCSIDERR": return 123;



                default: return 0;
            }
        }
        #endregion

        #region IRecord pass-through methods

        /// <summary>
        /// Returns a string representation of the record's content.
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            return Record.AsString();
        }

        /// <summary>
        /// Resets record's content to its initial value.
        /// </summary>
        public void ResetToInitialValue()
        {
            if (Record != null)
            {
                Record.ResetToInitialValue();
            }
        }

        /// <summary>
        /// Initializes record's content with low values.
        /// </summary>
        public void InitializeWithLowValues()
        {
            if (Record != null)
            {
                Record.InitializeWithLowValues();
            }
        }

        /// <summary>
        /// Fills record's buffer with the provided filling character.
        /// </summary>
        /// <param name="fillWith">Specifies filling character.</param>
        public void FillAllWith(MDSY.Framework.Buffer.Common.FillWith fillWith)
        {
            if (Record != null)
            {
                Record.FillAllWith(fillWith);
            }
        }

        /// <summary>
        /// Fills record's buffer with the provided byte.
        /// </summary>
        /// <param name="value">Specifies filling byte.</param>
        public void FillWithByte(byte value)
        {
            if (Record != null)
            {
                Record.FillWithByte(value);
            }
        }

        /// <summary>
        /// Fills record's buffer with spaces.
        /// </summary>
        public void SetValueWithSpaces()
        {
            if (Record != null)
            {
                Record.SetValueWithSpaces();
            }
        }

        /// <summary>
        /// Fills record's buffer with zeros.
        /// </summary>
        public void SetValueWithZeroes()
        {
            if (Record != null)
            {
                Record.SetValueWithZeroes();
            }
        }

        /// <summary>
        /// Fills record's buffer with 0x00 value.
        /// </summary>
        public void SetMinValue()
        {
            if (Record != null)
            {
                Record.SetMinValue();
            }
        }

        /// <summary>
        /// Fills record's buffer with 0xFF value.
        /// </summary>
        public void SetMaxValue()
        {
            if (Record != null)
            {
                Record.SetMaxValue();
            }
        }

        /// <summary>
        /// Checks whether record's buffer contains only specified bytes.
        /// </summary>
        /// <param name="byteValue">Specifies which byte to check.</param>
        /// <returns>Returns true if record's buffer contains only specified bytes.</returns>
        public bool ContainsOnly(byte byteValue)
        {
            return Record != null ? Record.ContainsOnly(byteValue) : false;
        }

        /// <summary>
        /// Checks whether record's buffer contains only spaces.
        /// </summary>
        /// <returns>Returns true if record's buffer contains only spaces.</returns>
        public bool IsSpaces()
        {
            return Record != null ? Record.IsSpaces() : false;
        }

        /// <summary>
        /// Checkes whether record's buffer contains only 0xFF values.
        /// </summary>
        /// <returns>Returns true if record's buffer contains only 0xFF values.</returns>
        public bool IsMaxValue()
        {
            return Record != null ? Record.IsMaxValue() : false;
        }

        /// <summary>
        /// Checks whether record's buffer contains only 0x00 values.
        /// </summary>
        /// <returns>Returns true if record's buffer contains only 0x00 values.</returns>
        public bool IsMinValue()
        {
            return Record != null ? Record.IsMinValue() : false;
        }

        /// <summary>
        /// Checks whether record's buffer contains only zero values.
        /// </summary>
        /// <returns>Returns true if record's buffer contains only zero values.</returns>
        public bool IsZeroes()
        {
            return Record != null ? Record.IsZeroes() : false;
        }

        /// <summary>
        /// Checks whether record's buffer does not contain all spaces.
        /// </summary>
        /// <returns>Returns true if record's buffer does not contain all spaces.</returns>
        public bool IsNotSpaces()
        {
            return Record != null ? Record.IsNotSpaces() : false;
        }

        /// <summary>
        /// Checks whether record's buffer does not contain all zeros.
        /// </summary>
        /// <returns>Returns true if record's buffer does not contain all zeros.</returns>
        public bool IsNotZeroes()
        {
            return Record != null ? Record.IsNotZeroes() : false;
        }

        /// <summary>
        /// Checks whether record's buffer does not contain all 0xFF values.
        /// </summary>
        /// <returns>Returns true if record's buffer does not contain all 0xFF values.</returns>
        public bool IsNotMaxValue()
        {
            return Record != null ? Record.IsNotMaxValue() : false;
        }

        /// <summary>
        /// Checks whether record's buffer does not contain all 0x00 values.
        /// </summary>
        /// <returns>Checks whether record's buffer does not contain all 0x00 values.</returns>
        public bool IsNotMinValue()
        {
            return Record != null ? Record.IsNotMinValue() : false;
        }

        /// <summary>
        /// Checks whether current record value is equal to the provided record value.
        /// </summary>
        /// <param name="value">A reference to the record object for comparison.</param>
        /// <returns>Returns true if current record value is equal to the provided record value.</returns>
        public bool IsEqualTo(PredefinedRecordBase value)
        {
            return Record != null ? Record.IsEqualTo(value.Record) : false;
        }
        /// <summary>
        /// Checks whether current record value is equal to the provided I Buffer Value value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is equal to] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEqualTo(IBufferValue value)
        {
            return Record != null ? Record.IsEqualTo(value.BytesAsString) : false;
        }

        /// <summary>
        /// Checks whether current record value is not equal to the provided record value.
        /// </summary>
        /// <param name="value">A reference to the record object for comparison.</param>
        /// <returns>Returns true if current record value is not equal to the provided record value.</returns>
        public bool IsNotEqualTo(PredefinedRecordBase value)
        {
            return Record != null ? Record.IsNotEqualTo(value.Record) : false;
        }
        /// <summary>
        /// Checks whether current record value is not equal to the provided buffer value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is not equal to] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsNotEqualTo(IBufferValue value)
        {
            return Record != null ? Record.IsNotEqualTo(value.BytesAsString) : false;
        }

        /// <summary>
        /// Checks whether current record value is equal to the provided string value.
        /// </summary>
        /// <param name="value">String for comparison.</param>
        /// <returns>Returns true if current record is equal to the provided string value.</returns>
        public bool IsEqualTo(string value)
        {
            return Record != null ? Record.IsEqualTo(value) : false;
        }

        /// <summary>
        /// Checks whether currrent record value is not equal to the provided string.
        /// </summary>
        /// <param name="value">String for comparison.</param>
        /// <returns>Returns true if current record is equal to the provided string value.</returns>
        public bool IsNotEqualTo(string value)
        {
            return Record != null ? Record.IsNotEqualTo(value) : false;
        }

        /// <summary>
        /// Fills record's buffer with 0x00 value.
        /// </summary>
        public void Clear()
        {
            Clear(0x00);
        }

        /// <summary>
        /// Fills record's buffer with the provided byte value.
        /// </summary>
        /// <param name="clearByte">Filling byte falue.</param>
        public void Clear(byte clearByte)
        {
            Record.Buffer.WriteBytes(Enumerable.Repeat<byte>(clearByte, Record.Buffer.Length).ToArray());
        }

        /// <summary>
        /// Adds provided collection of record elements to the current record structure.
        /// </summary>
        /// <param name="collection">A reference to the collection of record elements.</param>
        public void AddToRecordStructure(IElementCollection collection)
        {
            AddToRecordStructure(collection, null);
        }

        /// <summary>
        /// Adds provided record to the current record structure.
        /// </summary>
        /// <param name="record">A reference to the record object that must be added to the current record.</param>
        public void AddToRecordStructure(PredefinedRecordBase record)
        {
            AddToRecordStructure(record, null);
        }

        /// <summary>
        /// Adds provided collection of record elements to the current record structure.
        /// </summary>
        /// <param name="collection">A reference to the collection of record's elements.</param>
        /// <param name="newContentParent">A reference to the collection of elements from the new parent content.</param>
        public void AddToRecordStructure(IElementCollection collection, IElementCollection newContentParent)
        {
            if (Record != null)
            {
                this.record = Record.AddToStructure(collection, newContentParent);
            }
        }

        /// <summary>
        /// Adds provided record to the current record.
        /// </summary>
        /// <param name="record">A reference to the record object that must be added to the current record.</param>
        /// <param name="newContentParent">A reference to the collection of elements from the new parent content.</param>
        public void AddToRecordStructure(PredefinedRecordBase record, IElementCollection newContentParent)
        {
            if ((Record != null) && ((record != null) && (record.Record != null)))
            {
                this.record = Record.AddToStructure(record.Record, newContentParent);
            }
        }

        /// <summary>
        /// Functions as COBOL statement <c>SET ADDRESS OF RECORD_A TO ADDRESS OF RECORD_B</c>.
        /// Causes the record object to point its buffer reference to the 
        /// buffer of the given <paramref name="record"/>.
        /// </summary>
        /// <param name="record">The record object whose DataBuffer this record 
        /// will now point to.</param>
        public void SetAddressToAddressOf(IRecord record)
        {
            Record.SetAddressToAddressOf(record);
        }

        /// <summary>
        /// Functions as COBOL statement <c>SET ADDRESS OF RECORD_A TO ADDRESS OF RECORD_B</c>.
        /// Causes the record object to point its buffer reference to the 
        /// buffer of the given <paramref name="record"/>.
        /// </summary>
        public void SetAddressToAddressOf(PredefinedRecordBase record)
        {
            SetAddressToAddressOf(record.Record);
        }

        /// <summary>
        /// Duplicates method <c>SetAddressToAddressOf()</c>.
        /// </summary>
        public void SetBufferReference(IRecord record)
        {
            SetAddressToAddressOf(record);
        }

        /// <summary>
        /// Duplicates method <c>SetAddressToAddressOf()</c>.
        /// </summary>
        public void SetBufferReference(PredefinedRecordBase record)
        {
            SetAddressToAddressOf(record.Record);
        }

        /// <summary>
        /// Restores the buffer pointer mapping of this record to its original 
        /// IDataBuffer object.
        /// </summary>
        /// <remarks>If SetAddressToAddressOf() has never been called, this method
        /// will have no effect.</remarks>
        public void RestoreInitialDataBuffer()
        {
            Record.RestoreInitialDataBuffer();
        }

        /// <summary>
        /// Sets record's statistics.
        /// </summary>
        public void SetStatistics()
        {
            Record.SetStatistics();
        }

        #endregion

        /// <summary>
        /// Compares current record value with the provided IField value.
        /// </summary>
        /// <param name="other">A reference to the IField for comparison.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.
        /// The return value has the following meanings: Value Meaning Less than zero
        /// This object is less than the other parameter.Zero This object is equal to
        /// other. Greater than zero This object is greater than other.
        /// </returns>
        public int CompareTo(IField other)
        {
            return Record.CompareTo(other);
        }

        /// <summary>
        /// Compares current record with the provided IGroup value.
        /// </summary>
        /// <param name="other">A reference to the group object for comparison.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.
        /// The return value has the following meanings: Value Meaning Less than zero
        /// This object is less than the other parameter.Zero This object is equal to
        /// other. Greater than zero This object is greater than other.
        /// </returns>
        public int CompareTo(IGroup other)
        {
            return Record.CompareTo(other);
        }

        /// <summary>
        /// Compares current record with the provided record object.
        /// </summary>
        /// <param name="other">A reference to the record object for comparison.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.
        /// The return value has the following meanings: Value Meaning Less than zero
        /// This object is less than the other parameter.Zero This object is equal to
        /// other. Greater than zero This object is greater than other.
        /// </returns>
        public int CompareTo(IRecord other)
        {
            return Record.CompareTo(other);
        }

        /// <summary>
        /// Compares current record with the provided record object.
        /// </summary>
        /// <param name="other">A reference to the record object for comparison.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.
        /// The return value has the following meanings: Value Meaning Less than zero
        /// This object is less than the other parameter.Zero This object is equal to
        /// other. Greater than zero This object is greater than other.
        /// </returns>
        public int CompareTo(PredefinedRecordBase other)
        {
            return Record.CompareTo(other.Record);
        }

        /// <summary>
        /// Checks whether current record value is equal to the provided IField value.
        /// </summary>
        /// <param name="other">A reference to the IField object for comparison.</param>
        /// <returns>Returns true if current record value is equal to the IField value.</returns>
        public bool Equals(IField other)
        {
            return Record.Equals(other);
        }

        /// <summary>
        /// Checks whether current record value is equal to the provided IGroup value.
        /// </summary>
        /// <param name="other">A reference to the IGroup object for comparison.</param>
        /// <returns>Returns true if current record value is equal to the IGroup value.</returns>
        public bool Equals(IGroup other)
        {
            return Record.Equals(other);
        }

        /// <summary>
        /// Checks whether current record value is equal to the provided IRecord value.
        /// </summary>
        /// <param name="other">A reference to the IRecord object for comparison.</param>
        /// <returns>Returns true if current record value is equal to the IRecord value.</returns>
        public bool Equals(IRecord other)
        {
            return Record.Equals(other);
        }

        /// <summary>
        /// Checks whether current record value is equal to the provided record value.
        /// </summary>
        /// <param name="other">A reference to the record object for comparison.</param>
        /// <returns>Returns true if current record value is equal to the other record value.</returns>
        public bool Equals(PredefinedRecordBase other)
        {
            return Record.Equals(other.Record);
        }

        /// <summary>
        /// Composes a string value, which contains detais of all record structure elements.
        /// </summary>
        /// <returns>Returns a string, which contains detais of all record structure elements.</returns>
        public string DisplayRecordFieldMap()
        {
            StringBuilder sbDisplay = new StringBuilder();

            foreach (IBufferElement elements in Record.GetStructureElements())
            {
                sbDisplay.AppendLine(String.Concat("FieldName: ", elements.Name.PadRight(32), "Level: ", elements.Level,
                   " Buffer Position: ", elements.PositionInBuffer.AsString().PadLeft(6), "  Buffer Length: ", elements.LengthInBuffer.AsString().PadLeft(5),
                   " InArray?: ", elements.IsInArray));
            }
            return sbDisplay.ToString();
        }

    }
}
