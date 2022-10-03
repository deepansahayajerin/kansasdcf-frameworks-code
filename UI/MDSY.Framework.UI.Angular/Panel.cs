using System;
using System.Collections.Generic;
using System.Text;

namespace MDSY.Framework.UI.Angular
{
    public class Panel : IControl
    {
        private string _id = "";
        private string _cssClass = "";
        private string _width = "0px";
        private string _height = "0px";
        private string _borderWidth = "0px";
        private bool _visible = true;
        private BorderStyle _borderStyle = BorderStyle.None;
        private Dictionary<string, string> _attributes = new Dictionary<string, string>();
        private Dictionary<HtmlTextWriterStyle, string> _style = new Dictionary<HtmlTextWriterStyle, string>();
        private Dictionary<string, IControl> _controls = new Dictionary<string, IControl>();

        public Panel() { }
        public Panel(string id)
        {
            _id = id;
        }

        public Panel(Dictionary<string, Object> jsonControls)
        {
            if (!jsonControls.ContainsKey("type") || (string)jsonControls["type"] != "Panel")
                throw new Exception("Panel: attempting to parse json parameters that don't belong to Panel");

            foreach (string key in jsonControls.Keys)
            {
                if (key == "id")
                    _id = (string)jsonControls[key];
                else if (key == "width")
                    _width = (string)jsonControls[key];
                else if (key == "height")
                    _height = (string)jsonControls[key];
                else if (key == "cssClass")
                    _cssClass = (string)jsonControls[key];
                else if (key == "borderWidth")
                    _borderWidth = (string)jsonControls[key];
                else if (key == "top")
                    _style.Add(HtmlTextWriterStyle.Top, (string)jsonControls[key]);
                else if (key == "left")
                    _style.Add(HtmlTextWriterStyle.Left, (string)jsonControls[key]);
                else if (key == "position")
                    _style.Add(HtmlTextWriterStyle.Position, (string)jsonControls[key]);
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
                        else if ((string)control["type"] == "Panel")
                            _controls.Add(id, new Panel(control));
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
            get { return this.GetType().Name; }
        }

        public string CssClass
        {
            get { return _cssClass; }
            set { _cssClass = value; }
        }

        public string Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public Dictionary<string, string> Attributes
        {
            get { return _attributes; }
        }

        public string Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public Dictionary<string, IControl> Controls
        {
            get { return _controls; }
        }

        public Dictionary<HtmlTextWriterStyle, string> Style
        {
            get { return _style; }
        }

        public string BorderWidth
        {
            get { return _borderWidth; }
            set { _borderWidth = value; }
        }

        public BorderStyle BorderStyle
        {
            get { return _borderStyle; }
            set { _borderStyle = value; }
        }

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public Dictionary<string, Object> GetControlMap()
        {
            Dictionary<string, Object> panel = new Dictionary<string, Object>();
            panel.Add("type", Type);
            panel.Add("id", Id);
            panel.Add("width", Width);
            panel.Add("height", Height);
            panel.Add("cssClass", CssClass);
            panel.Add("borderWidth", BorderWidth);

            foreach (HtmlTextWriterStyle key in _style.Keys)
                panel.Add(key.ToString().ToLower().Replace("_", "-"), _style[key]);

            if (Controls.Count > 0)
            {
                Dictionary<string, Dictionary<string, Object>> controls = new Dictionary<string, Dictionary<string, Object>>();
                foreach (IControl control in Controls.Values)
                    controls.Add(control.Id, control.GetControlMap());
                panel.Add("controls", controls);
            }

            return panel;
        }
    }
}
