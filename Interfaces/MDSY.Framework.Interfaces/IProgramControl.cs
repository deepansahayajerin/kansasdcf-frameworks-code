using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Interfaces
{
    public interface IProgramControl
    {
        /// <summary>
        /// Returns a new program name.
        /// </summary>
        /// <param name="programName">the name of the program</param>
        /// <returns></returns>
        string GetNewProgramName(string programName);
        /// <summary>
        /// Returns new program assembly name.
        /// </summary>
        /// <param name="programName">the name of the program</param>
        /// <returns></returns>
        string GetNewProgramAssemblyName(string programName);

        /// <summary>
        /// Returns the Legacy program type: COBOL, DIALOG
        /// </summary>
        /// <param name="programName"></param>
        /// <returns></returns>
        string GetLegacyType(string programName);

    }


}
