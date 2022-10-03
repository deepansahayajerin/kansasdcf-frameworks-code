using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MDSY.Framework.Core;

namespace MDSY.Framework.Data.SQL
{
    public class TimeStampOverride
    {
        #region private static attributes
        [ThreadStatic]
        private static volatile TimeStampOverride _instance;
        #endregion

        #region private attributes
        private List<string> _timeStampList;
        #endregion

        #region public static properties
        public static TimeStampOverride Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TimeStampOverride();
                }
                return _instance;
            }
        }
        #endregion

        #region public properties
        public List<string> TimeStampList
        {
            get { return _timeStampList; }
        }
        #endregion

        #region constructors
        public TimeStampOverride()
        {
            const string keyDefinitionTypeName = "TimeStampColumns";
            const string keyDefinitionMethodName = "GetColumnList";

            _timeStampList = new List<string>();  

            try
            {
                Type type = ProgramUtilities.GetBLType(keyDefinitionTypeName);
                if (type != null)
                {
                    MethodInfo method = type.GetMethod(keyDefinitionMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    if (method != null)
                    {
                        object invokeResult = method.Invoke(null, null);

                        if (invokeResult != null)
                        {
                            _timeStampList = (List<string>)invokeResult;
                        }
                    }
                }
            }
            catch (ApplicationException ex)
            {
                if (ex.Message.Contains("InvokeHasMainMethod:"))
                {
                    // it is ok if we do not find any entries
                }
                else
                {
                    throw ex;
                }
            }
        }
        #endregion
    }
}
