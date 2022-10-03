using System;
using System.Collections.Generic;
using System.Text;

namespace MDSY.Framework.UI.Angular
{
    public class HtmlGenericControl : IControl
    {
        private string _type;
        private string _id = "";
        private string _innerHtml = "";
        private Dictionary<string, IControl> _controls = new Dictionary<string, IControl>();
        private Dictionary<string, string> _attributes = new Dictionary<string, string>();
        private Dictionary<HtmlTextWriterStyle, string> _style = new Dictionary<HtmlTextWriterStyle, string>();

        public HtmlGenericControl(string type)
        {
            _type = type;
        }

        public HtmlGenericControl(Dictionary<string, Object> jsonControls)
        {
            if (!jsonControls.ContainsKey("type") || (string)jsonControls["type"] != "DIV")
                throw new Exception("HtmlGenericControl: attempting to parse json parameters that don't belong to DIV");

            foreach (string key in jsonControls.Keys)
            {
                if (key == "id")
                    _id = (string)jsonControls[key];
                else if (key == "controls")
                {
                    Dictionary<string, Object> controls = (Dictionary<string, Object>)jsonControls[key];
                    foreach (string id in controls.Keys)
                    {
                        Dictionary<string, Object> control = (Dictionary<string, Object>)controls[id];
                        if ((string)control["type"] == "TextBox")
                            _controls.Add(id, new TextBox(control));
                        else if ((string)control["type"] == "DIV")
                            _controls.Add(id, new HtmlGenericControl(control));
                        else if ((string)control["type"] == "hidden")
                            _controls.Add(id, new TextBox(control));
                    }
                }
            }
        }

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Type
        {
            get { return _type; }
        }

        public Dictionary<string, IControl> Controls
        {
            get { return _controls; }
        }

        public Dictionary<HtmlTextWriterStyle, string> Style
        {
            get { return _style; }
        }

        public Dictionary<string, string> Attributes
        {
            get { return _attributes; }
        }

        public string InnerHtml
        {
            get { return _innerHtml; }
            set { _innerHtml = value; }
        }

        public Dictionary<string, Object> GetControlMap()
        {
            Dictionary<string, Object> control = new Dictionary<string, Object>();
            control.Add("type", Type);
            control.Add("id", Id);

            foreach (HtmlTextWriterStyle key in _style.Keys)
                control.Add(key.ToString().ToLower().Replace("_", "-"), _style[key]);

            if (Controls.Count > 0)
            {
                Dictionary<string, Dictionary<string, Object>> controls = new Dictionary<string, Dictionary<string, Object>>();
                foreach (IControl c in Controls.Values)
                    controls.Add(c.Id, c.GetControlMap());
                control.Add("controls", controls);
            }

            return control;
        }
    }
}
