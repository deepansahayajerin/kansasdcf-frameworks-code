using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.IO.Common
{
    public interface ICatalogFileInfo
    {
        int CatalogFileID { get; set; }

        string CatalogFilePath { get; set; }

        FileOrganization CatalogFileOrganization { get; set; }

        FileType CatalogFileType { get; set; }

        int CatalogRecordLength { get; set; }

        bool IsCataloged { get; set; }

        string VsamSegmentName { get; set; }

        string VsamKeys { get; set; }

    }
}
