using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Interfaces
{
    public interface IFieldControl
    {
        string Name { get; }
        string Value { get; set; }
        int Length { get; }
        bool isModified { get; set; }
        bool isRightJustify { get; set; }
        void UpdateFieldBufferProperties();
        void UpdateFromDefaultAttributes();
    }
}
