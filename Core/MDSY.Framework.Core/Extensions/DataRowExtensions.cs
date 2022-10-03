using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MDSY.Framework.Core
{
    public static class DataRowExtensions
    {
        /// <summary>
        /// Extensions methods for DataRow type.
        /// </summary>
        /// <typeparam name="T">The type to convert</typeparam>
        /// <param name="instance">The DataRow instance</param>
        /// <param name="columnName">The column name</param>
        /// <returns></returns>
        public static T SafeConvert<T>(this DataRow instance, string columnName)
        {
            T result = default(T);

            // first make sure our DataRow instance is good
            if (instance != null)
            {
                Type toType = typeof(T);

                // generic constraints can't help us here, so first check to see that 
                // a valid convert-to type was passed in:
                if ((toType == typeof(string)) || (toType == typeof(int)) ||
                    (toType == typeof(decimal)) || (toType == typeof(bool)))
                {
                    // now see if we have a valid field value
                    object fieldValue = instance[columnName];

                    if (fieldValue != null)
                    {
                        if (fieldValue is DateTime)
                        {
                            // Add logic for String vs int, decimal etc...
                            try
                            {
                                result = (T)Convert.ChangeType(fieldValue, toType);
                            }
                            catch (InvalidCastException)
                            {
                                result = default(T);
                            }
                        }
                        else
                        {
                            try
                            {
                                result = (T)Convert.ChangeType(fieldValue, toType);
                            }
                            catch (InvalidCastException)
                            {
                                result = default(T);
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Invalid convert-to type.");
                }
            }

            return result;
        }
    }
}
