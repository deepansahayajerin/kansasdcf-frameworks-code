using System;
using System.Text;

namespace MDSY.Framework.Interfaces
{
    /// <summary>
    /// Returns message.
    /// </summary>
    public interface IMessages
    {
        string GetMessage(string messageID);
    }
}
