using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Data.IDMS
{
    public class LogicalRecordPathInfo
    {

        #region Public Properties

        public string Action
        {
            get;
            set;
        }

        public string PathGroupName
        {
            get;
            set;
        }

        public Dictionary<string, string> LogicalRecordParmList
        {
            get;
            set;
        } 

        #endregion

        #region Constructors

        public LogicalRecordPathInfo()
        {
            LogicalRecordParmList = new Dictionary<string, string>();
        } 

        #endregion

    }
}
