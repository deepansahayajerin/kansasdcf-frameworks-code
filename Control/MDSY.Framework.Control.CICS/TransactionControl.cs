/* ************************************************************
   **   Ateras INC.  COPYRIGHT 2000-2013
   **   DB-SHUTTLE ADS/ONLINE DIALOG PROCESSING REPLACEMENT
   **   Dialog Control Data
   ************************************************************
*/
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using MDSY.Framework.Interfaces;
using System.Xml;
using Microsoft.Extensions.Configuration;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Control.CICS
{
    public class TransactionControl : ITransactionControl
    {
        #region Instance Property
        private static volatile TransactionControl _instance;
        public static TransactionControl Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TransactionControl();
                }
                return _instance;
            }
        }
        #endregion

        #region Constructor
        public TransactionControl()
        {
            ProgramNames = new Dictionary<string, string>();
            LoadProgramNames();
        }
        #endregion


        public IDictionary<string, string> ProgramNames { get; set; }

        public string GetProgramName(string transactionCode)
        {
            if (ProgramNames.ContainsKey(transactionCode))
                return ProgramNames[transactionCode];
            else
                throw new Exception(string.Concat("Trans code ", transactionCode, " not found in ITransactionControl Collection!"));

        }
        #region Private Methods
        private void LoadProgramNames()
        {
            string transactionControlXMLPath = ConfigSettings.GetAppSettingsString("TransactionControlXmlFile");
            XmlDocument xd = new XmlDocument();

            xd.Load(transactionControlXMLPath);

            foreach (XmlElement programNode in xd.SelectNodes("/Trans/Transaction"))
            {
                if (!ProgramNames.ContainsKey(programNode.Attributes["transCode"].Value))
                {
                    ProgramNames.Add(programNode.Attributes["transCode"].Value, programNode.Attributes["transProgram"].Value);
                }
            }

        }
        #endregion
    }
}
