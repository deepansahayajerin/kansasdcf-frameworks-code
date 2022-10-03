
namespace MDSY.Framework.IO.Common
{
    /// <summary>
    /// Set up the different types of file disposition strings.
    /// </summary>
    public class FileDispositions
    {
        #region constants
        #region The status (STAT)
        public const string NEW = "NEW";
        public const string OLD = "OLD";
        public const string SHR = "SHR";
        public const string MOD = "MOD";
        #endregion
        #region The normal dispositions (NDISP) and The abnormal dispositions (ADISP)
        public const string DELETE = "DELETE";
        public const string PASS = "PASS";
        public const string KEEP = "KEEP";
        public const string CATLG = "CATLG";
        public const string UNCATLG = "UNCATLG";
        #endregion
        #endregion

        #region constructors
        public FileDispositions()
        {
        }
        #endregion
    }
}
