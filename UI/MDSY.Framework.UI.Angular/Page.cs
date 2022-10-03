using System;
using System.Collections.Generic;

namespace MDSY.Framework.UI.Angular
{
    [Serializable]
    public class Page : IControl
    {
        protected const string PAGE_CLASS = "pageClass";
        protected MDSYSession _session;

        protected string _name = "";
        protected string _currentForm = "";
        protected string _keyPressed = " ";
        protected string _rowColumnPosition = "   ";
        protected string _eofFlags = "";
        protected string _mdtFlags = "";

        protected string _lastFocus = "";
        protected string _eventTarget = "";
        protected string _eventArgument = "";

        protected string _message = null;
        protected string _connectionId = null;
        protected Dictionary<string, object> _mapData = null;

        protected string _focusedFieldID = "";
        protected string _errorField = "";
        protected string _activeDivID = "";
        protected string _homeKeyCode = "36";
        protected string _helpField = "";
        protected bool? _activateSpellcheck = null;

        protected Page(Dictionary<string, object> parameters)
        {
            Dictionary<string, object> mapBaseData = null;
            if (parameters.ContainsKey("mapBaseData"))
                mapBaseData = (Dictionary<string, object>)parameters["mapBaseData"];

            if (mapBaseData != null)
            {
                _name = ((string)mapBaseData["name"]);
                if (mapBaseData.ContainsKey("currentForm") && mapBaseData["currentForm"] != null)
                    _currentForm = (String)mapBaseData["currentForm"];
                if (mapBaseData.ContainsKey("eventKeyPress") && mapBaseData["eventKeyPress"] != null)
                    _keyPressed = (string)mapBaseData["eventKeyPress"];
                if (mapBaseData.ContainsKey("eventCursorPos") && mapBaseData["eventCursorPos"] != null) 
                    _rowColumnPosition = (string)mapBaseData["eventCursorPos"];
                if (mapBaseData.ContainsKey("eeofFlags") && mapBaseData["eeofFlags"] != null) 
                    _eofFlags = (string)mapBaseData["eeofFlags"];
                if (mapBaseData.ContainsKey("mdtFlags") && mapBaseData["mdtFlags"] != null)
                    _mdtFlags = (string)mapBaseData["mdtFlags"];

                if (mapBaseData.ContainsKey("lastFocus") && mapBaseData["lastFocus"] != null)
                    _lastFocus = (string)mapBaseData["lastFocus"];
                if (mapBaseData.ContainsKey("eventTarget") && mapBaseData["eventTarget"] != null)
                    _eventTarget = (string)mapBaseData["eventTarget"];
                if (mapBaseData.ContainsKey("eventArgument") && mapBaseData["eventArgument"] != null)
                    _eventArgument = (string)mapBaseData["eventArgument"];

                if (mapBaseData.ContainsKey("focusedFieldID") && mapBaseData["focusedFieldID"] != null) 
                    _focusedFieldID = (string)mapBaseData["focusedFieldID"];
                if (mapBaseData.ContainsKey("activeDivID") && mapBaseData["activeDivID"] != null) 
                    _activeDivID = (string)mapBaseData["activeDivID"];
                if (mapBaseData.ContainsKey("homeKeyCode") && mapBaseData["homeKeyCode"] != null)
                    _homeKeyCode = (string)mapBaseData["homeKeyCode"];
                if (mapBaseData.ContainsKey("helpField") && mapBaseData["helpField"] != null)
                    _helpField = (string)mapBaseData["helpField"];

                if (mapBaseData.ContainsKey("activateSpellcheck") && mapBaseData["activateSpellcheck"] != null)
                    _activateSpellcheck = bool.Parse((string)mapBaseData["activateSpellcheck"]);

                if (mapBaseData.ContainsKey("connectionId") && mapBaseData["connectionId"] != null)
                    _session = MDSYSession.GetSession((string)mapBaseData["connectionId"]);
            }
            else
            {
                _activeDivID = "";
                _name = ((string)parameters["name"]);
                if (parameters.ContainsKey("connectionId") && parameters["connectionId"] != null)
                    _session = MDSYSession.GetSession((string)parameters["connectionId"]);
            }

            if (parameters.ContainsKey("mapData"))
            {
                _mapData = (Dictionary<string, object>)parameters["mapData"];
                if (_name == "DynamicMap" && _mapData.Count == 1 && _mapData.ContainsKey("controlDiv"))
                {
                    _mapData = (Dictionary<string, object>)_mapData["controlDiv"];
                }
            }
        }

        protected Page(string name, Page page)
        {
            _name = name;
            _currentForm = name;
            _connectionId = page._connectionId;
            _session = page._session;
            _keyPressed = page._keyPressed;
        }

        public string Name { get { return _name; } }

        public string Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Dictionary<string, IControl> Controls
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public String getName() { return _name; }
        public MDSYSession GetSession() { return _session; }

        /// <summary>
        /// This method is only declared for interface purposes
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, object> GetControlMap()
        {
            throw new NotImplementedException();
        }
    }
}
