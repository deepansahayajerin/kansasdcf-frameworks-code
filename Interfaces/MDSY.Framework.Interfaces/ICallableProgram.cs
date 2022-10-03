using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Interfaces
{

    /// <summary>
    /// Defines a type which represents a routine of executable code 
    /// which can be called from an external source. 
    /// </summary>
    /// <remarks>
    /// Objects which implement ICallableProgram allow for the emulation of, for instance,
    /// calling a bit of COBOL code from a Natural program.
    /// </remarks>
    public interface ICallableProgram: ILegacyProgram
    {
        /// <summary>
        /// Causes the implementing object's code to execute.
        /// </summary>
        /// <param name="inValue">The input value (such as a buffer string).</param>
        /// <param name="outValue">The resulting output value (such as the altered buffer string).</param>
        /// <returns>An exit value.</returns>
        Int32 Execute(string inValue, out string outValue);
    }
}