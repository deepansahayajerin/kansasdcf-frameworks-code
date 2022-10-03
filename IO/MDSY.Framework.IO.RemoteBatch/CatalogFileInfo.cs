using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.IO.Common;

namespace MDSY.Framework.IO.RemoteBatch
{
    public class CatalogFileInfo: ICatalogFileInfo
    {

        #region public properties
        public int CatalogFileID { get; set; }

        public string CatalogFilePath { get; set; }

        public FileOrganization CatalogFileOrganization { get; set; }

        public FileType CatalogFileType { get; set; }

        public int CatalogRecordLength { get; set; }

        public bool IsCataloged { get; set; }

        public string VsamSegmentName { get; set; }

        public string VsamKeys { get; set; }

        #endregion

    }
}
