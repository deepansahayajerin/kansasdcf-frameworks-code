using System;
using System.Collections.Generic;
using System.Text;

namespace MDSY.Framework.UI.Angular
{
    public class Button : IControl
    {
        private string _id = "";
        private string _text = "";
        private Dictionary<HtmlTextWriterStyle, string> _style = new Dictionary<HtmlTextWriterStyle, string>();

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Type
        {
            get { return "submit"; }
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public Dictionary<HtmlTextWriterStyle, string> Style
        {
            get { return _style; }
        }

        public Dictionary<string, IControl> Controls
        {
            get { return null; }
        }

        public Dictionary<string, Object> GetControlMap()
        {
            Dictionary<string, Object> button = new Dictionary<string, Object>();
            button.Add("type", Type);
            button.Add("id", Id);
            button.Add("value", Text);

            foreach (HtmlTextWriterStyle key in _style.Keys)
                button.Add(key.ToString().ToLower().Replace("_", "-"), _style[key]);

            return button;
        }
    }
}
