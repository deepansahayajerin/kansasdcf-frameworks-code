using System;
using System.Collections.Generic;
using System.Text;

namespace MDSY.Framework.UI.Angular
{
    public class Label : IControl
    {
        private string _id = "";
        private string _text = "";
        private string _cssClass = "LABEL";
        private bool _fontBold = false;
        private string _width = "";
        private string _height = "";
        private int _fontSize = 0;
        private Dictionary<HtmlTextWriterStyle, string> _style = new Dictionary<HtmlTextWriterStyle, string>();

        public Label() { }

        public Label(Dictionary<string, Object> node, string id)
        {
            _id = id;
            _text = (string)node["value"];
            _cssClass = (string)node["cssClass"];
        }

        public Label(string id) { _id = id; }

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Type
        {
            get { return this.GetType().Name; }
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public string Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public int FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; }
        }

        public string CssClass
        {
            get { return _cssClass; }
            set
            {
                if (value.Trim().Length > 0)
                    _cssClass = value;
            }
        }

        public Dictionary<string, IControl> Controls { get { return null; } }

        public Dictionary<string, Object> GetControlMap()
        {
            Dictionary<string, Object> label = new Dictionary<string, object>();
            label.Add("type", Type);
            label.Add("id", Id);
            if (!string.IsNullOrEmpty(_cssClass))
                label.Add("cssClass", _cssClass);

            foreach (HtmlTextWriterStyle key in _style.Keys)
                label.Add(key.ToString().ToLower().Replace("_", "-"), _style[key]);

            label.Add("value", _text);

            return label;
        }

        public bool FontBold
        {
            get { return _fontBold; }
            set { _fontBold = value; }
        }
        public string Width { set { _width = value; } }
        public Dictionary<HtmlTextWriterStyle, string> Style
        {
            get { return _style; }
        }
    }
}
