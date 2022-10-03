using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Service.Interfaces;
using MDSY.Framework.Core;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer;
using MDSY.Framework.Interfaces;

namespace MDSY.Framework.Control.CICS
{
    /// <summary>
    /// Implementation class for online map services; Conversion of CICS BMS maps
    /// </summary>
    public class MapServices : IMapServices
    {
        public class MapDefLength
        {
            public int OriginalLength { get; private set; }
            public int ExtraLength { get; private set; }

            private MapDefLength() { }
            public MapDefLength(int originalLength, int extraLength)
            {
                OriginalLength = originalLength;
                ExtraLength = extraLength;
            }
        }

        #region Private variables
        private OnlineControl _control;
        private bool isDataOnly;
        private bool isCursorOption;
        private bool isEraseOption;
        private bool _insideSetErrorMap = false;
        private bool _mapFieldsModified = false;

        [ThreadStatic]
        private static IMapDefinition _activeMapDefinition;
        [ThreadStatic]
        private static IBufferValue _activeMapRecordSource;
        [ThreadStatic]
        private static SendOption[] _activeMapSendOptions;
        [ThreadStatic]
        private static int _errorMapFields;
        [ThreadStatic]
        private static bool _errorMapProcessed;
        [ThreadStatic]
        private static IMapDefinition _lastMapDefinition;
        [ThreadStatic]
        private static Dictionary<string, MapDefLength> mapList;
        #endregion

        #region Constructors
        public MapServices(OnlineControl control)
        {
            _control = control;
        }
        #endregion

        #region Public methods

        public void SendMap(IMapDefinition mapDefinition, IBufferValue mapRecordSource, params SendOption[] sendOptions)
        {
            SendMap(mapDefinition, mapRecordSource, 0, sendOptions);
        }
        /// <summary>
        /// Set up and send Service Controls based on Map buffer fields
        /// </summary>
        /// <param name="mapDefinition"></param>
        /// <param name="mapRecordSource"></param>
        /// <param name="sendOptions"></param>
        public void SendMap(IMapDefinition mapDefinition, IBufferValue mapRecordSource, int cursr, params SendOption[] sendOptions)
        {
            SetUpMapFieldControls(SetErrorMap(mapDefinition, mapRecordSource, cursr, sendOptions), cursr);
            // Send Service Controls
            SetServiceControls();
            SetStatus(0);

        }

        /// <summary>
        /// Set up and send Service Controls based on Map buffer fields
        /// </summary>
        /// <param name="mapDefinition"></param>
        /// <param name="mapRecordSource"></param>
        /// <param name="respCode"></param>
        /// <param name="sendOptions"></param>
        public void SendMap(IMapDefinition mapDefinition, IBufferValue mapRecordSource, IField respCode, params SendOption[] sendOptions)
        {
            SendMap(mapDefinition, mapRecordSource, sendOptions);
            respCode.Assign(_control.RESP.AsInt());
        }

        /// <summary>
        /// Send Service data from Record buffer
        /// </summary>
        /// <param name="mapRecordSource"></param>
        /// <param name="sendLength"></param>
        /// <param name="sendOptions"></param>
        public void SendFrom(IBufferValue mapRecordSource, int sendLength, params SendOption[] sendOptions)
        {
            SendTextToMap(mapRecordSource, sendLength);
        }

        /// <summary>
        /// Send Map with no data
        /// </summary>
        /// <param name="sendOptions"></param>
        public void SendMap(IMapDefinition mapDefinition, params SendOption[] sendOptions)
        {
            SetUpMapControl(mapDefinition.QualifiedMapName, sendOptions);
            IStructureDefinition mapRecordStructure = (IStructureDefinition)((BMSMapDefinitionBase)mapDefinition).MapRecord;
            if (mapRecordStructure.IsDefining)
                mapRecordStructure.EndDefinition();

            SetUpMapFieldControls(mapDefinition);
            SetServiceControls();
        }

        /// <summary>
        /// Set up and send Service Controls based on Map buffer fields
        /// </summary>
        /// <param name="mapName">Name of the map</param>
        /// <param name="mapRecordSource">The map record source</param>
        /// <param name="sendOptions">Send Options</param>
        public void SendMap(IGroup mapName, IBufferValue mapRecordSource, params SendOption[] sendOptions)
        {
            string map = mapName.AsString();
            if (!mapName.AsString().Contains("_"))
            {
                string mapSet = DBSUtil.GetMapSet(mapName.BytesAsString);
                map = mapSet + "_" + mapName.BytesAsString.Trim() + "_Map";
            }

            DBSUtil.GetBLType(map);
            IMapDefinition mapDefinition = (IMapDefinition)DBSUtil.GetBLType(map).GetProperty("Instance").GetValue(null, null);

            SetUpMapFieldControls(SetErrorMap(mapDefinition, mapRecordSource, sendOptions));
            SetServiceControls();
            SetStatus(0);
        }

        /// <summary>
        /// Send Map with no data
        /// </summary>
        /// <param name="sendOptions"></param>
        public void SendMap(string mapName, params SendOption[] sendOptions)
        {
            string map = mapName;
            if (!mapName.AsString().Contains("_"))
            {
                string mapSet = DBSUtil.GetMapSet(mapName);
                if (mapSet.StartsWith("_"))
                    map = mapName + mapSet + "_Map";
                else
                    map = mapSet + "_" + mapName.Trim() + "_Map";
            }
            DBSUtil.GetBLType(map);
            IMapDefinition mapDefinition = (IMapDefinition)DBSUtil.GetBLType(map).GetProperty("Instance").GetValue(null, null);

            SetUpMapControl(mapDefinition.QualifiedMapName, sendOptions);
            SetUpMapFieldControls(mapDefinition);
            SetServiceControls();
            SetStatus(0);
        }

        /// <summary>
        /// Send Map with no data
        /// </summary>
        /// <param name="sendOptions"></param>
        public void SendMap(IField mapName, params SendOption[] sendOptions)
        {
            SendMap(mapName.ToString(), sendOptions);
        }

        /// <summary>
        /// Send Map with no data
        /// </summary>
        /// <param name="sendOptions"></param>
        public void SendMap(string mapName, IBufferValue mapRecordSource, params SendOption[] sendOptions)
        {
            string map = mapName;
            if (!mapName.Contains("_"))
            {
                string mapSet = DBSUtil.GetMapSet(mapName);
                map = mapSet + "_" + mapName.Trim() + "_Map";
            }
            DBSUtil.GetBLType(map);
            IMapDefinition mapDefinition = (IMapDefinition)DBSUtil.GetBLType(map).GetProperty("Instance").GetValue(null, null);

            SetUpMapFieldControls(SetErrorMap(mapDefinition, mapRecordSource, sendOptions));
            SetServiceControls();
            SetStatus(0);
        }

        /// <summary>
        /// Send Map
        /// </summary>
        /// <param name="sendOptions"></param>
        public void SendMap(IField mapName, IBufferValue mapRecordSource, params SendOption[] sendOptions)
        {
            string map = mapName.AsString().Trim();
            if (!mapName.AsString().Trim().Contains("_"))
            {
                string mapSet = DBSUtil.GetMapSet(mapName.AsString().Trim());
                map = mapSet + "_" + mapName.AsString().Trim() + "_Map";
            }
            DBSUtil.GetBLType(map);
            IMapDefinition mapDefinition = (IMapDefinition)DBSUtil.GetBLType(map).GetProperty("Instance").GetValue(null, null);

            SetUpMapFieldControls(SetErrorMap(mapDefinition, mapRecordSource, sendOptions));
            SetServiceControls();
            SetStatus(0);
        }

        public void SendMap(IField mapSet, IField mapName, IBufferValue mapRecordSource, params SendOption[] sendOptions)
        {
            SendMap(mapSet, mapName, mapRecordSource, 0, sendOptions);
        }

        /// <summary>
        /// Send Map with no data
        /// </summary>
        /// <param name="mapSet">Specified MAPSET</param>
        /// <param name="mapName">Specified Map Name</param>
        /// <param name="mapRecordSource">The record Source</param>
        /// <param name="sendOptions">Send options</param>
        public void SendMap(IField mapSet, IField mapName, IBufferValue mapRecordSource, int cursr, params SendOption[] sendOptions)
        {
            string map = mapSet.AsString().Trim() + "_" + mapName.AsString().Trim() + "_Map";
            DBSUtil.GetBLType(map);
            IMapDefinition mapDefinition = (IMapDefinition)DBSUtil.GetBLType(map).GetProperty("Instance").GetValue(null, null);

            SetUpMapFieldControls(SetErrorMap(mapDefinition, mapRecordSource, cursr, sendOptions));
            SetServiceControls();
            SetStatus(0);
        }

        /// <summary>
        /// Send Map with no data
        /// </summary>
        /// <param name="mapSet">Specified MAPSET</param>
        /// <param name="mapName">Specified Map Name</param>
        /// <param name="sendOptions">Send options</param>
        public void SendMap(IField mapSet, IField mapName, params SendOption[] sendOptions)
        {
            //SendMap(mapSet.ToString(), mapName.ToString(), sendOptions);
            SendMap(mapSet.DisplayValue, mapName.DisplayValue, sendOptions);
        }

        /// <summary>
        /// Send Map with no data
        /// </summary>
        /// <param name="mapSet">Specified MAPSET</param>
        /// <param name="mapName">Specified Map Name</param>
        /// <param name="sendOptions">Send options</param>
        public void SendMap(string mapSet, string mapName, params SendOption[] sendOptions)
        {
            string map = mapSet.Trim() + "_" + mapName.Trim() + "_Map";
            DBSUtil.GetBLType(map);
            IMapDefinition mapDefinition = (IMapDefinition)DBSUtil.GetBLType(map).GetProperty("Instance").GetValue(null, null);

            SetUpMapControl(mapDefinition.QualifiedMapName, sendOptions);
            SetUpMapFieldControls(mapDefinition);
            SetServiceControls();
            SetStatus(0);
        }

        /// <summary>
        /// Receive Service controls and copy into map buffer fields 
        /// </summary>
        /// <param name="mapDefinition"></param>
        /// <param name="mapRecordTarget"></param>
        /// <param name="respCode"></param>
        /// <param name="receiveOptions"></param>
        public void ReceiveMap(IMapDefinition mapDefinition, IBufferValue mapRecordTarget, IField respCode, params ReceiveOption[] receiveOptions)
        {
            if (_control.isMapWaitingSend)
            {
                // Send the Data
                DBSUtil.ExternalThreadHolder.Set();

                // Wait for return
                DBSUtil.InternalThreadHolder.WaitOne();
            }

            GetMapFieldControls(ref mapDefinition);
            int mapExtraLength = 0;
            int mapOriginalLength = 0;
            if (mapList != null && mapList.Keys.Contains(mapDefinition.QualifiedMapName))
            {
                mapOriginalLength = mapList[mapDefinition.QualifiedMapName].OriginalLength;
                mapExtraLength = mapList[mapDefinition.QualifiedMapName].ExtraLength;
            }
            if ((_lastMapDefinition != null && _lastMapDefinition.QualifiedMapName == mapDefinition.QualifiedMapName)
                && (mapRecordTarget.AsBytes.Length < (((BMSMapDefinitionBase)(mapDefinition)).MapRecord).Length - mapExtraLength))
            {
                byte[] sourceBytes = null;
                byte[] mapBytes = mapDefinition.GetBytes();
                if (mapOriginalLength > 0 && mapBytes.Length > mapOriginalLength)
                    sourceBytes = mapDefinition.GetBytes().Skip(mapDefinition.GetBytes().Length - mapExtraLength - mapRecordTarget.AsBytes.Length).ToArray();
                else
                    sourceBytes = mapDefinition.GetBytes().Skip(mapDefinition.GetBytes().Length - mapRecordTarget.AsBytes.Length).ToArray();

                byte[] targetBytes = new byte[mapRecordTarget.AsBytes.Length];
                Array.Copy(sourceBytes, targetBytes, targetBytes.Length);
                mapRecordTarget.AssignFrom(targetBytes);
            }
            else
            {
                mapRecordTarget.AssignFrom(mapDefinition.GetBytes());
            }
            // Set A MApFail condition (code 36) on CLEAR key or PA keys or if no fields were modified and there is no initial cursor
            if ((_control.EIBAID.IsEqualTo('_') || _control.EIBAID.IsEqualTo('%') || _control.EIBAID.IsEqualTo('>') || _control.EIBAID.IsEqualTo(',')) || (!_mapFieldsModified && string.IsNullOrEmpty(mapDefinition.InitialCursorField)))
            {
                SetStatus(36);
            }
            else
                SetStatus(0);

        }

        /// <summary>
        /// Receive Service controls and copy into map buffer fields 
        /// </summary>
        /// <param name="mapDefinition"></param>
        /// <param name="mapRecordTarget"></param>
        /// <param name="respCode"></param>
        /// <param name="receiveOptions"></param>
        public void ReceiveMap(IMapDefinition mapDefinition, IBufferValue mapRecordTarget, params ReceiveOption[] receiveOptions)
        {
            _activeMapDefinition = mapDefinition;
            _activeMapRecordSource = mapRecordTarget;
            ReceiveMap(mapDefinition, mapRecordTarget, null, receiveOptions);

        }

        /// <summary>
        /// Receive Service controls and copy into map buffer fields 
        /// </summary>
        /// <param name="mapName">Map Name</param>
        /// <param name="mapRecordTarget">Record Target</param>
        /// <param name="receiveOptions">Receive Options</param>
        public void ReceiveMap(IGroup mapName, IBufferValue mapRecordTarget, params ReceiveOption[] receiveOptions)
        {
            string _mapName = mapName.AsString();
            string mapSet = DBSUtil.GetMapSet(_mapName.Trim());
            string map = mapSet + "_" + _mapName.Trim() + "_Map";
            DBSUtil.GetBLType(map);
            IMapDefinition mapDefinition = (IMapDefinition)DBSUtil.GetBLType(map).GetProperty("Instance").GetValue(null, null);
            _activeMapDefinition = mapDefinition;
            _activeMapRecordSource = mapRecordTarget;
            ReceiveMap(mapDefinition, mapRecordTarget, null, receiveOptions);

        }

        /// <summary>
        /// Receive Service controls and copy into map buffer fields 
        /// </summary>
        /// <param name="mapName">Map Name</param>
        /// <param name="mapRecordTarget">Record Target</param>
        /// <param name="receiveOptions">Receive Options</param>
        public void ReceiveMap(string mapName, IBufferValue mapRecordTarget, params ReceiveOption[] receiveOptions)
        {
            string mapSet = DBSUtil.GetMapSet(mapName.Trim());
            string map = mapSet + "_" + mapName.Trim() + "_Map";
            DBSUtil.GetBLType(map);
            IMapDefinition mapDefinition = (IMapDefinition)DBSUtil.GetBLType(map).GetProperty("Instance").GetValue(null, null);
            _activeMapDefinition = mapDefinition;
            _activeMapRecordSource = mapRecordTarget;
            ReceiveMap(mapDefinition, mapRecordTarget, null, receiveOptions);
        }

        /// <summary>
        /// Receive Service controls and copy into map buffer fields 
        /// </summary>
        /// <param name="mapName">Map Name</param>
        /// <param name="mapRecordTarget">Record Target</param>
        /// <param name="receiveOptions">Receive Options</param>
        public void ReceiveMap(IField mapName, IBufferValue mapRecordTarget, params ReceiveOption[] receiveOptions)
        {
            string _mapName = mapName.AsString();
            string mapSet = DBSUtil.GetMapSet(_mapName.Trim());
            string map = mapSet + "_" + _mapName.Trim() + "_Map";
            DBSUtil.GetBLType(map);
            IMapDefinition mapDefinition = (IMapDefinition)DBSUtil.GetBLType(map).GetProperty("Instance").GetValue(null, null);
            _activeMapDefinition = mapDefinition;
            _activeMapRecordSource = mapRecordTarget;
            ReceiveMap(mapDefinition, mapRecordTarget, null, receiveOptions);
        }

        /// <summary>
        /// Receive Service controls and copy into map buffer fields 
        /// </summary>
        /// <param name="mapSet">Map Set</param>
        /// <param name="mapName">Map Name</param>
        /// <param name="mapRecordTarget">Record Target</param>
        /// <param name="receiveOptions">REceive Options</param>
        public void ReceiveMap(IField mapSet, IField mapName, IBufferValue mapRecordTarget, params ReceiveOption[] receiveOptions)
        {
            string map = mapSet.AsString().Trim() + "_" + mapName.AsString().Trim() + "_Map";
            DBSUtil.GetBLType(map);
            IMapDefinition mapDefinition = (IMapDefinition)DBSUtil.GetBLType(map).GetProperty("Instance").GetValue(null, null);
            _activeMapDefinition = mapDefinition;
            _activeMapRecordSource = mapRecordTarget;
            ReceiveMap(mapDefinition, mapRecordTarget, null, receiveOptions);
        }

        /// <summary>
        /// Recieve Data from Client
        /// </summary>
        /// <param name="mapRecordTarget"></param>
        /// <param name="receiveOptions"></param>
        public void ReceiveData(IBufferValue dataRecordTarget, int dataLength, params ReceiveOption[] receiveOptions)
        {

            if (_control.isMapWaitingSend)
            {
                // Send the Data
                DBSUtil.ExternalThreadHolder.Set();

                // Wait for return
                DBSUtil.InternalThreadHolder.WaitOne();
            }
            StringBuilder dataString = new StringBuilder();
            if (DBSUtil.ServiceThreadShareData != null)
            {
                foreach (IAterasServiceItem serviceItem in DBSUtil.ServiceThreadShareData)
                {
                    if (serviceItem is CICSServiceItemKey)
                    {
                        if (serviceItem.Name.Contains("EntryPoint:"))
                        {
                            dataString.Append(serviceItem.Name.Trim().Substring(11));
                        }
                    }
                    if (serviceItem is CICSServiceItemControl)
                    {
                        CICSServiceItemControl serviceControl = (CICSServiceItemControl)serviceItem;
                        dataString.Append(serviceControl.Text);
                    }
                }
            }
            if (dataString.Length > dataLength)
            {
                dataRecordTarget.SetValue(dataString.ToString(0, dataLength));

            }
            else
            {
                dataRecordTarget.SetValue(dataString.ToString());

            }

        }


        public void SendText(IBufferValue mapRecordSource, params SendOption[] sendOptions)
        {
            SendTextToMap(mapRecordSource, 80);
        }

        public void SendText(IBufferValue mapRecordSource, int length, params SendOption[] sendOptions)
        {
            SendTextToMap(mapRecordSource, length);
        }

        /// <summary>
        /// Set Up Service Control Key
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sendOptions"></param>
        public void SetUpMapControl(string name, params SendOption[] sendOptions)
        {
            isDataOnly = false; isCursorOption = false; bool isAlarm = false; isEraseOption = false;
            foreach (SendOption sendOpt in sendOptions)
            {
                if (sendOpt == SendOption.DataOnly)
                    isDataOnly = true;
                else if (sendOpt == SendOption.Alarm)
                    isAlarm = true;
                else if (sendOpt == SendOption.Cursor)
                    isCursorOption = true;
                else if (sendOpt == SendOption.Erase)
                    isEraseOption = true;
            }

            if (DBSUtil.ServiceThreadShareData != null)
            {
                DBSUtil.ServiceThreadShareData.Clear();
                // Add Service control
                DBSUtil.ServiceThreadShareData.Add(new CICSServiceItemKey(name, string.Empty, string.Empty, string.Empty, isAlarm, string.Empty));
            }
        }

        public void SetServiceControls()
        {
            // Send Service Controls
            //DBSUtil.ExternalThreadHolder.Set(); //Commented out since ExternalThreaholder.Set is done in ServiceController - Jetro issue 835
            _control.isMapWaitingSend = true;
            // Following Code added for JETRO behavior
            if (ServiceControl.TWARecord != null)
            {
                if (ServiceControl.TWARecord.Record.ContainsElementNested("TWA_COMMON_CODE_AREA"))
                {
                    IField twaElement = ServiceControl.TWARecord.Record.GetFieldByName("TWA_COMMON_CODE_AREA");
                    twaElement.SetMinValue();
                }
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Set up Service Controls based on the map buffer fields
        /// </summary>
        /// <param name="mapDef"></param>
        private void SetUpMapFieldControls(IMapDefinition mapDef, int cursr = 0)
        {
            string cursrPosName = "";
            string initialCursor = "";
            if (!_insideSetErrorMap)
                _lastMapDefinition = mapDef;

            if (!_errorMapProcessed)
            {
                foreach (BMSFieldControl fieldControl in mapDef.FieldControls)
                {
                    string style = string.Empty;
                    if (fieldControl.isBright)
                        style = " BRIGHT";
                    else if (fieldControl.isDark)
                        style = " DARK";

                    style = string.Concat(style, " ", fieldControl.BMSFieldColor.ToString());


                    if (fieldControl != null && fieldControl.BMSFieldHilight != FieldHilight.Default)
                    {
                        style = string.Concat(style, " ", fieldControl.BMSFieldHilight.ToString());
                    }
                    if (DBSUtil.ServiceThreadShareData != null)
                        DBSUtil.ServiceThreadShareData.Add(new CICSServiceItemControl(fieldControl.Name, fieldControl.Value, fieldControl.isReadonly, fieldControl.Length, style, fieldControl.isModified, fieldControl.isAutoskip));

                    if (cursr > 0)
                    {
                        fieldControl.isDefaultCursor = false;
                        int cursrPos = (((fieldControl.RowPosition - 1) * 80) + (fieldControl.ColumnPosition));
                        if (cursrPos == cursr)
                        {
                            fieldControl.isDefaultCursor = true;
                            cursrPosName = fieldControl.Name;
                        }
                    }
                    else
                    {
                        if (fieldControl.LengthField.AsInt() == -1)
                        {
                            mapDef.CursorField = fieldControl.Name;
                            cursrPosName = fieldControl.Name;
                        }

                        if (mapDef.InitialCursorField == fieldControl.Name)
                        {
                            fieldControl.isDefaultCursor = true;
                            initialCursor = fieldControl.Name;
                            
                        }
                        else
                        {
                            fieldControl.isDefaultCursor = false;
                        }
                    }
                }
            }
            if (cursrPosName == "")
                cursrPosName = initialCursor;

            CheckForCursorField(mapDef, cursrPosName);
        }

        /// <summary>
        /// Set Map buffer fields from Service controls
        /// </summary>
        /// <param name="mapDef"></param>
        private void GetMapFieldControls(ref IMapDefinition mapDef)
        {
            int ctr = 0; _mapFieldsModified = false;
            if (_lastMapDefinition != null && _lastMapDefinition.QualifiedMapName != mapDef.QualifiedMapName)
            {
                mapDef = _lastMapDefinition;
            }

            string cursrField = ((CICSServiceItemKey)DBSUtil.ServiceThreadShareData[0]).CurrentControl;
            foreach (IAterasServiceItem serviceItem in DBSUtil.ServiceThreadShareData)
            {
                if (serviceItem is CICSServiceItemControl)
                {
                    CICSServiceItemControl serviceControl = (CICSServiceItemControl)serviceItem;

                    IFieldControl fieldControl = mapDef.FieldControls[ctr - 1];
                    if (ctr <= (mapDef.FieldControls.Count - _errorMapFields))
                    {
                        if (serviceControl.Modified)
                        {
                            // Set Value field
                            fieldControl.Value = serviceControl.Text;

                            // TODO Set MDT
                            fieldControl.isModified = true;

                            _mapFieldsModified = true;

                            //fieldControl.UpdateFieldBufferProperties();
                        }
                        else
                        {
                            if (fieldControl.Value != serviceControl.Text)
                            {
                                fieldControl.Value = serviceControl.Text;
                                //fieldControl.UpdateFieldBufferProperties();
                            }
                        }

                    }
                    else
                    {
                        fieldControl.Value = "";
                        ((BMSFieldControl)fieldControl).ValueField.SetValue("");
                        ((BMSFieldControl)fieldControl).Value = "";
                        ((BMSFieldControl)fieldControl).EmptyDefaultValue();

                    }
                    if (fieldControl.Name == cursrField)
                    {
                        _control.EIBCPOSN.SetValue(((((BMSFieldControl)fieldControl).RowPosition - 1) * 80) + ((BMSFieldControl)fieldControl).ColumnPosition);
                    }
                    if (!string.IsNullOrEmpty(((BMSFieldControl)fieldControl).EditMask))
                    {
                        ((BMSFieldControl)fieldControl).ValueField.EditMask = ((BMSFieldControl)fieldControl).EditMask;
                    }
                    fieldControl.UpdateFieldBufferProperties();
                    //fieldControl.UpdateFromDefaultAttributes();   //Commented OUt this line so that Map.SEND DataOnly will retain attributes
                }
                ctr++;
            }
        }

        private void SendTextToMap(IBufferValue mapMessage, int length)
        {
            StringBuilder errMessage = new StringBuilder();
            errMessage.AppendLine("******************************************************************");
            errMessage.AppendLine(mapMessage.DisplayValue);
            errMessage.AppendLine("******************************************************************");
            DBSUtil.ServiceThreadShareData.Clear();
            DBSUtil.ServiceThreadShareData.Add(new CICSServiceItemKey("Error", "QUIT", null, "0", true, ""));
            DBSUtil.ServiceThreadShareData.Add(new CICSServiceItemControl("Message", errMessage.ToString(), true, length, "Red"));
            SetStatus(0);
        }

        private void SetStatus(int status)
        {
            int mapStatusCode = status;
            DBSUtil.Condition = OnlineControl.GetCondition(mapStatusCode);
            _control.RESP.Assign(mapStatusCode);
            _control.EIBRESP.Assign(mapStatusCode);
            if (mapStatusCode == 0)
                _control.EIBRCODE.SetMinValue();
            else
                _control.EIBRCODE.SetValue("ERROR");

        }

        private bool EraseOption(params SendOption[] sendOptions)
        {
            bool eraseFlag = false;
            foreach (SendOption sendOpt in sendOptions)
            {
                if (sendOpt == SendOption.Erase)
                {
                    eraseFlag = true;
                }
            }
            return eraseFlag;
        }

        private IMapDefinition SetErrorMap(IMapDefinition mapDefinition, IBufferValue mapRecordSource, params SendOption[] sendOptions)
        {
            return SetErrorMap(mapDefinition, mapRecordSource, 0, sendOptions);
        }

        private IMapDefinition SetErrorMap(IMapDefinition mapDefinition, IBufferValue mapRecordSource, int cursr, params SendOption[] sendOptions)
        {
            bool fieldFound = false;
            int cntr = 0;
            _errorMapFields = 0;
            _errorMapProcessed = false;
            int errorMapFieldsLength = 0;
            bool errorFieldsCreated = false;
            string cursrPosName = "";

            if (_lastMapDefinition != null && mapDefinition != _lastMapDefinition && !EraseOption(sendOptions))
            {
                _insideSetErrorMap = true;
                _errorMapFields = mapDefinition.FieldControls.Count;

                //SetUpMapControl(_activeMapDefinition.QualifiedMapName, _activeMapSendOptions);
                //SetUpMapFieldControls(_activeMapDefinition, cursr);
                mapDefinition.SetMapFieldProperties(mapRecordSource, isDataOnly, false, isCursorOption, 0);

                foreach (BMSFieldControl fieldCtrl in mapDefinition.FieldControls)
                {
                    cntr = 0;
                    int fieldCount = 0;
                    foreach (BMSFieldControl ctrl in _lastMapDefinition.FieldControls)
                    {
                        if (ctrl.Name == fieldCtrl.Name)
                        {
                            fieldFound = true;
                            fieldCount = cntr;
                        }
                        if (cursr > 0)
                        {
                            ctrl.isDefaultCursor = false;
                            int cursrPos = (((ctrl.RowPosition - 1) * 80) + (ctrl.ColumnPosition));
                            if (cursrPos == cursr || (cursrPos - 1) == cursr)
                            {
                                ctrl.isDefaultCursor = true;
                                cursrPosName = ctrl.Name;
                            }
                        }
                        cntr++;
                    }
                    if (fieldFound)
                    {
                        ((BMSFieldControl)_lastMapDefinition.FieldControls[fieldCount]).Value = fieldCtrl.Value;
                        ((BMSFieldControl)_lastMapDefinition.FieldControls[fieldCount]).ValueField.SetValue(fieldCtrl.Value);
                        ((CICSServiceItemControl)DBSUtil.ServiceThreadShareData[fieldCount + 1]).Text = fieldCtrl.Value;
                        errorMapFieldsLength = errorMapFieldsLength + fieldCtrl.Length + 3;   //Adding 3 because of 2 bytes for length field and 1 byte for the attribute field
                    }
                    else
                    {
                        if (!errorFieldsCreated)
                        {
                            SetUpMapFieldControls(mapDefinition, cursr);
                            errorFieldsCreated = true;
                        }
                        ((BMSMapDefinitionBase)_lastMapDefinition).DefineMapField(fieldCtrl.Name, fieldCtrl.RowPosition, fieldCtrl.ColumnPosition, fieldCtrl.Length, "", fieldCtrl.Value,
                              BMSFieldAttribute.Protected, BMSFieldAttribute.Normal, BMSFieldAttribute.Autoskip);
                        ((BMSFieldControl)_lastMapDefinition.FieldControls[_lastMapDefinition.FieldControls.Count - 1]).ValueField.SetValue(fieldCtrl.Value);
                        errorMapFieldsLength = errorMapFieldsLength + fieldCtrl.Length + 3;  //Adding 3 because of 2 bytes for length field and 1 byte for the attribute field
                    }
                }
                CheckForCursorField(_lastMapDefinition, cursrPosName);
                if (errorMapFieldsLength > 0)
                {
                    if (mapList != null)
                    {
                        if (!mapList.Keys.Contains(_lastMapDefinition.QualifiedMapName))
                            mapList.Add(_lastMapDefinition.QualifiedMapName, new MapDefLength(_lastMapDefinition.GetBytes().Length, errorMapFieldsLength));
                    }
                    else
                    {
                        mapList = new Dictionary<string, MapDefLength>();
                        mapList.Add(_lastMapDefinition.QualifiedMapName, new MapDefLength(_lastMapDefinition.GetBytes().Length, errorMapFieldsLength));
                    }
                }
                mapDefinition = _lastMapDefinition;
                mapRecordSource = _activeMapRecordSource;
                mapDefinition.SetMapFieldProperties(mapRecordSource, isDataOnly, false, isCursorOption, false, 0);
                _errorMapProcessed = true;
                _insideSetErrorMap = false;
            }
            else
            {
                SetUpMapControl(mapDefinition.QualifiedMapName, sendOptions);
                int errorFields = 0;
                if (mapList != null && mapList.ContainsKey(mapDefinition.QualifiedMapName))
                {
                    errorFields = mapList[mapDefinition.QualifiedMapName].ExtraLength;
                }
                mapDefinition.SetMapFieldProperties(mapRecordSource, isDataOnly, isEraseOption, isCursorOption, errorFields);

                //Clear the error number and error message when loading a map after an error has previously been displayed for that map
                foreach (BMSFieldControl fieldControl in mapDefinition.FieldControls)
                {
                    if (fieldControl.Name == "M0MSGNBR")
                        fieldControl.Value = "";
                    if (fieldControl.Name == "M0MSG")
                        fieldControl.Value = "";
                }

                if (_lastMapDefinition != null && mapDefinition != _lastMapDefinition)
                {
                    if (mapDefinition.InitialCursorField != null && string.IsNullOrEmpty(mapDefinition.CursorField))
                    {
                        mapDefinition.CursorField = mapDefinition.InitialCursorField;
                    }
                }
                _activeMapSendOptions = sendOptions;
            }

            return mapDefinition;
        }

        private void CheckForCursorField(IMapDefinition mapDef, string cursorPos)
        {
            if ((mapDef.CursorField != string.Empty || cursorPos != string.Empty) && DBSUtil.ServiceThreadShareData != null)
            {
                CICSServiceItemKey controlKey = (CICSServiceItemKey)DBSUtil.ServiceThreadShareData[0];
                if (controlKey != null)
                {
                    if (cursorPos == "")
                        controlKey.CurrentControl = mapDef.CursorField;
                    else
                    {
                        ((BMSMapDefinitionBase)mapDef).CursorField = cursorPos;
                        controlKey.CurrentControl = cursorPos;
                    }
                }
            }
        }
        #endregion

    }
}
