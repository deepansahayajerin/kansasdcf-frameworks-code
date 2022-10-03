using System;
using System.Collections.Generic;
using System.Text;
using MDSY.Framework.Core;

namespace MDSY.Framework.Service.Interfaces
{
    public class OutputAttributes
    {
        private string _extFormat;
        private string _nullAction;
        private string _numericState;
        private bool _zeroSuppress = false;
        private char _padCharacter;
        private FieldAlignment _fieldAlignment;
        private string _codeTable;
        private string _outputProgram;
        private string _fieldColor;
        private bool _underscored = false;

        public string ExtFormat { get { return _extFormat; } }
        public string NullAction { get { return _nullAction; } }
        public string NumericState { get { return _numericState; } }
        public bool ZeroSuppress { get { return _zeroSuppress; } }
        public char PadCharacter { get { return _padCharacter; } }
        public FieldAlignment FieldAlignment { get { return _fieldAlignment; } }
        public string CodeTable { get { return _codeTable; } }
        public string OutputProgram { get { return _outputProgram; } }
        public string FieldColor { get { return _fieldColor; } }
        public bool Underscored { get { return _underscored; } }
        
        public OutputAttributes(string extFormat, string nullAction, string numericState, bool zeroSuppress,
                                        char padCharacter, FieldAlignment fieldAlignment, string codeTable = null, string outputProgram = null, string fieldColor = null)
        {
            _extFormat = extFormat;
            _nullAction = nullAction;
            _numericState = numericState;
            _zeroSuppress = zeroSuppress;
            _padCharacter = padCharacter;
            _fieldAlignment = fieldAlignment;
            _codeTable = codeTable;
            _outputProgram = outputProgram;
            _fieldColor = fieldColor;
        }

        public OutputAttributes(string extFormat, string nullAction, string numericState, bool zeroSuppress,
                                char padCharacter, bool underscored, FieldAlignment fieldAlignment, string codeTable = null, string outputProgram = null, string fieldColor = null)
        {
            _extFormat = extFormat;
            _nullAction = nullAction;
            _numericState = numericState;
            _zeroSuppress = zeroSuppress;
            _padCharacter = padCharacter;
            _fieldAlignment = fieldAlignment;
            _codeTable = codeTable;
            _outputProgram = outputProgram;
            _fieldColor = fieldColor;
            _underscored = underscored;
        }

        public OutputAttributes(string extFormat, string nullAction, string numericState, bool zeroSuppress, char padCharacter, string codeTable = null, string outputProgram = null)
        {
            _extFormat = extFormat;
            _nullAction = nullAction;
            _numericState = numericState;
            _zeroSuppress = zeroSuppress;
            _padCharacter = padCharacter;
            _codeTable = codeTable;
            _outputProgram = outputProgram;
        }
    }
}
