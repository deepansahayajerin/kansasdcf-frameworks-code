using System;
using System.Collections.Generic;
using System.Text;

namespace MDSY.Framework.UI.Angular
{
    public interface IControl
    {
        string Id { get; }
        string Type { get; }
        Dictionary<String, IControl> Controls { get; }
        Dictionary<String, Object> GetControlMap();
    }
}
