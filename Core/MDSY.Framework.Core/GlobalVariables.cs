using System;
using System.Collections.Generic;
using System.Text;

namespace MDSY.Framework.Core
{
    public class GlobalVariables
    {
        [ThreadStatic]
        private static string _userID;

        [ThreadStatic]
        private static string _programName;

        public static string ProgramName
        {
            get 
            {
                if (string.IsNullOrEmpty(_programName))
                    _programName = "PROGRAM?";
                return _programName;
            }
            set 
            { _programName = value; }
        }

        public static string UserID
        {
            get
            {
                if (string.IsNullOrEmpty(_userID))
                    _userID = "USER";
                return _userID;
            }
            set
            { _userID = value; }
        }

    }
}
