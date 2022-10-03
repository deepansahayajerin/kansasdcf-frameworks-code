using System;
using System.Collections.Generic;
using System.Text;

namespace MDSY.Framework.Service.Interfaces
{
    public class InputAttributes
    {
        private bool _isRequired = false;
        private string _extFormat;
        private string _intFormat;
        private string _messageID;
        private int _mapFieldLength;
        private string _editCodeTable;
        private string _codeTable;
        private string _inputEditProgram;
        private bool _isIncorrectValidation = false;

        public bool IsRequired { get { return _isRequired; } }
        public string ExtFormat { get { return _extFormat; } }
        public string IntFormat { get { return _intFormat; } }
        public string MessageID { get { return _messageID; } }
        public int MapFieldLength { get { return _mapFieldLength; } }
        public string EditCodeTable { get { return _editCodeTable; } }
        public string CodeTable { get { return _codeTable; } }
        public string InputEditProgram { get { return _inputEditProgram; } }
        public bool IsIncorrectValidation { get { return _isIncorrectValidation; } }
        
        public InputAttributes(bool isRequired, string extFormat, string intFormat, string messageID, int mapFieldLength = 0)
        {
            _isRequired = isRequired;
            _extFormat = extFormat;
            _intFormat = intFormat;
            _messageID = messageID;
            _mapFieldLength = mapFieldLength;
        }

        public InputAttributes(bool isRequired, string extFormat, string intFormat, string messageID, string editCodeTable, string inputEditProgram = null)
        {
            _isRequired = isRequired;
            _extFormat = extFormat;
            _intFormat = intFormat;
            _messageID = messageID;
            _editCodeTable = editCodeTable;
            _inputEditProgram = inputEditProgram;
        }

        public InputAttributes(bool isRequired, string extFormat, string intFormat, string messageID, string editCodeTable, string codeTable, string inputEditProgram = null)
        {
            _isRequired = isRequired;
            _extFormat = extFormat;
            _intFormat = intFormat;
            _messageID = messageID;
            _editCodeTable = editCodeTable;
            _codeTable = codeTable;
            _inputEditProgram = inputEditProgram;
        }

        public InputAttributes(bool isRequired, string extFormat, string intFormat, string messageID, string editCodeTable, bool isIncorrectValidation)
        {
            _isRequired = isRequired;
            _extFormat = extFormat;
            _intFormat = intFormat;
            _messageID = messageID;
            _editCodeTable = editCodeTable;
            _isIncorrectValidation = isIncorrectValidation;
        }

        public InputAttributes(bool isRequired, string extFormat, string intFormat, string messageID, string editCodeTable, string codeTable, bool isIncorrectValidation)
        {
            _isRequired = isRequired;
            _extFormat = extFormat;
            _intFormat = intFormat;
            _messageID = messageID;
            _editCodeTable = editCodeTable;
            _codeTable = codeTable;
            _isIncorrectValidation = isIncorrectValidation;
        }
        public InputAttributes (bool isRequired, string extFormat, string intFormat, string messageID, string editCodeTable, string codeTable, bool isIncorrectValidation, int mapFieldLength = 0, string inputEditProgram = null)
        {
            _isRequired = isRequired;
            _extFormat = extFormat;
            _intFormat = intFormat;
            _messageID = messageID;
            _editCodeTable = editCodeTable;
            _codeTable = codeTable;
            _isIncorrectValidation = isIncorrectValidation;
            _inputEditProgram = inputEditProgram;
            _mapFieldLength = mapFieldLength;
        }
    }
}
