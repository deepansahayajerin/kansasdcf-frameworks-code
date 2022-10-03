
using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Interfaces
{
    public interface IMapDefinition
    {
        void SetMapFieldProperties(IBufferValue bufferDefinition, bool isDataOnly, bool isEraseOption, bool isCursorOption, int extraFields);

        void SetMapFieldProperties(IBufferValue bufferDefinition, bool isDataOnly, bool isEraseOption, bool isCursorOption, bool isDefining, int extraFields);

        string QualifiedMapName { get; }

        string CursorField { get; set; }

        string InitialCursorField { get; }

        List<IFieldControl> FieldControls { get; }

        //IDbsByteBasedBuffer_Old RecordBuffer { get; }
        byte[] GetBytes();

    }
}
