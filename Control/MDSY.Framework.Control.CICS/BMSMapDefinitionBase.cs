using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Core;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Interfaces;

namespace MDSY.Framework.Control.CICS
{

    public abstract class BMSMapDefinitionBase
    {
        //private char low_value = '\x0000';
        public IRecord MapRecord;
        private List<IFieldControl> _BMSFields;
        private int _extraAttributeBytes = 0;
        public BMSMapDefinitionBase()
        {
            MapRecord = CreateMapRecord();
            _BMSFields = new List<IFieldControl>();
        }

        public string MapName { get; set; }

        public string MapSet { get; set; }

        public string QualifiedMapName { get; set; }

        public string CursorField { get; set; }

        public string InitialCursorField { get; set; }

        public int MapRows { get; set; }

        public int MapColumns { get; set; }

        private static int MapCounter = 0;

        public int ExtraAttributeBytes
        {
            get { return _extraAttributeBytes; }
            set { _extraAttributeBytes = value; }
        }

        public List<IFieldControl> FieldControls
        {
            get { return _BMSFields; }
        }

        private IRecord CreateMapRecord()
        {
            return MDSY.Framework.Buffer.Services.BufferServices.Factory.NewRecord("MapRecord" + ++MapCounter, rec =>
                {
                    rec.NewFillerField(12);

                });
        }

        private void CreateMapFields(string fieldName, int length)
        {

            IStructureDefinition MapRecordStructure = (IStructureDefinition)MapRecord;
            MapRecordStructure.RestartDefinition();

            MapRecordStructure.CreateNewField(string.Format("{0}L", fieldName), Framework.Buffer.Common.FieldType.CompShort, 4);
            MapRecordStructure.CreateNewField(string.Format("{0}A", fieldName), Framework.Buffer.Common.FieldType.String, 1);
            if (ExtraAttributeBytes > 0)
                MapRecordStructure.CreateNewField(string.Format("{0}Fill", fieldName), Framework.Buffer.Common.FieldType.String, ExtraAttributeBytes);
            MapRecordStructure.CreateNewField(string.Format("{0}I", fieldName), Framework.Buffer.Common.FieldType.String, length);
            //if ((length % 2) == 0)
            //{
            //    MapRecordStructure.NewFillerField(1);
            //}

        }

        // Set Initial Control values
        public void DefineMapField(string fieldName, int rowPos, int colPos, int length, string outPIC, string defaultValue, params BMSFieldAttribute[] fieldAttributes)
        {
            CreateMapFields(fieldName, length);
            var fieldLength = MapRecord.StructureElementByName(String.Format("{0}L", fieldName)) as IField;
            var fieldAtt = MapRecord.StructureElementByName(String.Format("{0}A", fieldName)) as IField;
            var fieldValue = MapRecord.StructureElementByName(String.Format("{0}I", fieldName)) as IField;

            _BMSFields.Add(new BMSFieldControl(fieldName, rowPos, colPos, length, outPIC, defaultValue, fieldLength, fieldAtt, fieldValue, fieldAttributes));
        }

        public void DefineMapField(string fieldName, int rowPos, int colPos, int length, string outPIC, string defaultValue, FieldColor bmsFieldColor, params BMSFieldAttribute[] fieldAttributes)
        {
            CreateMapFields(fieldName, length);
            var fieldLength = MapRecord.StructureElementByName(String.Format("{0}L", fieldName)) as IField;
            var fieldAtt = MapRecord.StructureElementByName(String.Format("{0}A", fieldName)) as IField;
            var fieldValue = MapRecord.StructureElementByName(String.Format("{0}I", fieldName)) as IField;

            _BMSFields.Add(new BMSFieldControl(fieldName, rowPos, colPos, length, outPIC, defaultValue, fieldLength, fieldAtt, fieldValue, bmsFieldColor, fieldAttributes));
        }

        public void DefineMapField(string fieldName, int rowPos, int colPos, int length, string outPIC, string defaultValue, FieldColor bmsFieldColor, FieldHilight bmsFieldHilight, params BMSFieldAttribute[] fieldAttributes)
        {
            CreateMapFields(fieldName, length);
            var fieldLength = MapRecord.StructureElementByName(String.Format("{0}L", fieldName)) as IField;
            var fieldAtt = MapRecord.StructureElementByName(String.Format("{0}A", fieldName)) as IField;
            var fieldValue = MapRecord.StructureElementByName(String.Format("{0}I", fieldName)) as IField;

            _BMSFields.Add(new BMSFieldControl(fieldName, rowPos, colPos, length, outPIC, defaultValue, fieldLength, fieldAtt, fieldValue, bmsFieldColor, bmsFieldHilight, fieldAttributes));
        }


        public void SetMapFieldProperties(IBufferValue programFields, bool isDataOnly, bool isEraseOption, bool isCursorOption, int extraFields)
        {
            SetMapFieldProperties(programFields, isDataOnly, isEraseOption, isCursorOption, true, extraFields);
        }

        // Update UI Controls from the Internal Buffer definitions
        //public void SetMapFieldProperties(FieldBase_Old programFields, bool isDataOnly)
        public void SetMapFieldProperties(IBufferValue programFields, bool isDataOnly, bool isEraseOption, bool isCursorOption, bool isDefining, int extraFields)
        {
            if (isDefining)
            {
                IStructureDefinition MapRecordStructure = (IStructureDefinition)MapRecord;

                if (MapRecordStructure.IsDefining)
                {
                    MapRecordStructure.EndDefinition();
                }
                if (MapRecord.Length > programFields.Buffer.Length + 1)
                {
                    if (MapRecord.Length - extraFields > programFields.Buffer.Length + 1)
                        throw new Exception(string.Format("Buffers do not match for {0}! MapRecLength = {1}, ProgramRecLength = {2}",
                            MapName, MapRecord.Length, programFields.Buffer.Length));
                }

                // Copy programBuffer to Internal map buffer
                MapRecord.AssignFrom(programFields.AsBytes);
                //MapRecord.DBSBuffer.AssignSubRangeValue(programFields.Buffer, 0);
            }

            string defaultCursor = string.Empty; CursorField = null;
            //CursorField = string.Empty;
            foreach (BMSFieldControl fcontrol in _BMSFields)
            {
                string controlAttribute = fcontrol.AttributeField.AsString();
                //Update value from Buffer text or default text


                if (fcontrol.ValueField.AsBytes[0] == AsciiChar.MinValue)
                {
                    if (!isDataOnly || (isEraseOption && controlAttribute[0] != AsciiChar.MinValue))
                    {
                        //if (string.IsNullOrEmpty(fcontrol.Value))
                        fcontrol.Value = fcontrol.DefaultValue;   //???? Why is this here?
                    }
                    //else
                    //{
                    //    // Do not update Cntrol value
                    //}
                }
                else
                {
                    fcontrol.Value = fcontrol.ValueField.AsString();
                }


                // Update Attributes

                if (controlAttribute[0] != AsciiChar.MinValue)
                {
                    //Set Attributes from record buffer attribute
                    fcontrol.isReadonly = true;

                    fcontrol.isBright = (controlAttribute.LastIndexOfAny(new char[8] { 'H', 'I', 'Q', 'R', '8', '9', 'Y', 'Z' }) == 0);
                    fcontrol.isDark = (controlAttribute.LastIndexOfAny(new char[8] { '<', '(', '%', '*', ')', '_', '@', '\'' }) == 0);
                    fcontrol.isReadonly = !(controlAttribute.LastIndexOfAny(new char[15] { ' ', 'A', 'D', 'E', 'H', 'I', '(', 'J', 'M', 'N', 'Q', 'R', '*', ')', '&' }) == 0);
                    fcontrol.isModified = (controlAttribute.LastIndexOfAny(new char[16] { 'A', 'E', 'I', '(', 'J', 'N', 'R', ')', '/', 'V', 'Z', '_', '1', '5', '9', '\'' }) == 0);
                }
                else if (!isDataOnly)
                {
                    //Set Attributes from deafults
                    fcontrol.UpdateFromDefaultAttributes();
                }


                if (fcontrol.isDefaultCursor)
                {
                    defaultCursor = fcontrol.Name;
                    CursorField = fcontrol.Name;
                }

                if (isCursorOption && fcontrol.LengthField.AsInt() == -1)
                {
                    CursorField = fcontrol.Name;
                }
            }

            if (CursorField == null)
            {
                CursorField = defaultCursor;
            }

            if (InitialCursorField == null)
            {
                InitialCursorField = defaultCursor;
            }
        }

    }

}

