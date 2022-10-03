using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Interfaces;

namespace MDSY.Framework.IO.Common
{

    public class SortParm
    {
        #region private fields
        //private readonly IField field;
        #endregion

        #region public properties
        public int Offset { get; set; }
        public int Length { get; set; }
        public SortDirection SortDirection { get; set; }
        public FieldType SortDataType { get; set; }
        #endregion

        #region constructors
        public SortParm()
        {

        }
        //public SortParm(IField sortField, SortDirection sortDir)
        //{
        //    field = sortField;
        //    Length = field.LengthInBuffer;
        //    SortDirection = sortDir;
        //    SortDataType = sortField.FieldType;
        //}
        #endregion


    }
}
