using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Web;

namespace MDSY.Framework.UI.Angular
{
    public class TextBox : IControl
    {
        private string _id = "";
        private string _text = "";
        private string _cssClass = "";
        private string _fontName = "";
        private int _fontSize = 0;
        private bool _isReadOnly = false;
        private int _tabIndex = -1;
        private int _maxLength = 0;
        private int _columns = 0;
        private int _height = 0;
        private string _width = "";
        private bool _isFocused = false;
        private string _toolTip;
        private Color _foreColor = Color.Empty;
        private TextBoxMode _textMode = TextBoxMode.SingleLine;
        private Dictionary<string, string> _attributes = new Dictionary<string, string>();
        private Dictionary<HtmlTextWriterStyle, string> _style = new Dictionary<HtmlTextWriterStyle, string>();

        public TextBox() { }

        public TextBox(Dictionary<string, object> node, string id)
        {
            _id = id;
            _text = (string)node["value"];
            _cssClass = (string)node["cssClass"];
            if (node.ContainsKey("tabIndex"))
                _tabIndex = Convert.ToInt32((long)node["tabIndex"]);
            object readOnly = node["readOnly"];
            if (readOnly is string)
            {
                _isReadOnly = bool.Parse((string)readOnly);
            }
            else if (readOnly is bool)
            {
                _isReadOnly = (bool)readOnly;
            }
        }

        public TextBox(string id)
        {
            _id = id;
        }

        public TextBox(Dictionary<string, object> jsonParams)
        {
            foreach (string key in jsonParams.Keys)
            {
                if (key == "id")
                    _id = (string)jsonParams[key];
                else if (key == "cssClass")
                    _cssClass = (string)jsonParams[key];
                else if (key == "tabIndex")
                    _tabIndex = Convert.ToInt32((long)jsonParams[key]);
                else if (key == "value")
                    _text = (string)jsonParams[key];
                else if (key == "readOnly")
                    _isReadOnly = (bool)jsonParams[key];
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

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public string CssClass
        {
            get { return _cssClass; }
            set { _cssClass = value; }
        }

        public bool ReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }

        public int TabIndex
        {
            get { return _tabIndex; }
            set { _tabIndex = value; }
        }

        public int MaxLength
        {
            get { return _maxLength; }
            set { _maxLength = value; }
        }

        public void Focus()
        {
            _isFocused = true;
        }

        public string ToolTip
        {
            get { return _toolTip; }
            set { _toolTip = value; }
        }

        public Dictionary<string, string> Attributes
        {
            get { return _attributes; }
        }

        public Dictionary<HtmlTextWriterStyle, string> Style
        {
            get { return _style; }
        }

        public int Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }
        public string Width
        {
            get { return _width; }
            set { _width = value; }
        }
        public string FontName
        {
            get { return _fontName; }
            set { _fontName = value; }
        }

        public int FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; }
        }

        public Color ForeColor
        {
            get { return _foreColor; }
            set { _foreColor = value; }
        }

        public TextBoxMode TextMode
        {
            get { return _textMode; }
            set { _textMode = value; }
        }

        public Dictionary<string, object> GetControlMap()
        {
            Dictionary<string, Object> textBox = new Dictionary<string, Object>();
            textBox.Add("type", Type);
            textBox.Add("id", Id);
            textBox.Add("cssClass", _cssClass);
            textBox.Add("value", _text);
            textBox.Add("readOnly", _isReadOnly);

            foreach (HtmlTextWriterStyle key in _style.Keys)
                textBox.Add(key.ToString().ToLower().Replace("_", "-"), _style[key]);

            textBox.Add("columns", _columns);
            textBox.Add("maxLength", _maxLength);
            textBox.Add("height", _height);
            textBox.Add("fontName", _fontName);
            textBox.Add("tabIndex", _tabIndex);

            foreach (string key in _attributes.Keys)
            {
                if (textBox.ContainsKey(key))
                    textBox.Remove(key);
                textBox.Add(key, _attributes[key]);
            }

            return textBox;
        }

        public Dictionary<string, IControl> Controls
        {
            get { return null; }
        }
    }
}
