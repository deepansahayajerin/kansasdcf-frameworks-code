using System;
using System.Collections.Generic;
using System.Linq;
using Unity;
using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Unity;
using System.ComponentModel;
using System.Diagnostics;
using MDSY.Framework.Buffer.Services;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Logging service conditional encapsulator. 
    /// </summary>
    internal static class Log
    {
        #region private fields
        private static ILoggingService loggingService;
        #endregion

        #region private methods
        private static StackFrame GetCallingStackFrame()
        {
            // GetFrame(0) --> GetCallingStackFrame()
            // GetFrame(1) --> Calling method (like EnterMethod())
            // GetFrame(2) --> Method that called calling method
            return new StackTrace().GetFrame(2);
        }

        // Fields...

        private static ILoggingService LoggingService
        {
            get
            {
                // Create on demand...
                if (loggingService == null)
                    loggingService = BufferServices.Logging;
                return loggingService;
            }
        }
        

        #endregion

        #region stack methods
        [Conditional("LOGGING")]
        internal static void EnterMethod()
        {
            StackFrame frame = GetCallingStackFrame();
            System.Reflection.MethodBase method = frame.GetMethod();
            LoggingService.IndentStack(String.Format("{0}.{1}", method.DeclaringType.Name, method.Name));
        }

        [Conditional("LOGGING")]
        internal static void IndentStack(string frameName)
        {
            LoggingService.IndentStack(frameName);
        }

        [Conditional("LOGGING")]
        internal static void ExitMethod()
        {
            StackFrame frame = GetCallingStackFrame();
            System.Reflection.MethodBase method = frame.GetMethod();
            LoggingService.OutdentStack(String.Format("{0}.{1}", method.DeclaringType.Name, method.Name));
        }

        [Conditional("LOGGING")]
        internal static void OutdentStack(string frameName)
        {
            LoggingService.OutdentStack(frameName);
        }


        #endregion

        #region send methods
        [Conditional("LOGGING")]
        internal static void Send(string message, string value)
        {
            LoggingService.Send(message, value);
        }

        [Conditional("LOGGING")]
        internal static void Send(string message, object value)
        {
            LoggingService.Send(message, value);
        }

        #endregion

        #region SendMsg
        [Conditional("LOGGING")]
        internal static void SendMsg(string message)
        {
            LoggingService.SendMsg(message);
        }

        [Conditional("LOGGING")]
        internal static void SendMsg(string fmtString, params string[] args)
        {
            LoggingService.SendMsg(string.Format(fmtString, args));
        }

        [Conditional("LOGGING")]
        internal static void SendMsg(string fmtString, MessagePriority color, params string[] args)
        {
            SendMsg(string.Format(fmtString, args), color);
        }

        [Conditional("LOGGING")]
        internal static void SendMsg(string message, MessagePriority priority)
        {
            LoggingService.SendMsg(priority, message);
        }
        #endregion

        [Conditional("LOGGING")]
        internal static void SendIf(bool expression, string message, MessagePriority priority = MessagePriority.Medium)
        {
            if (expression)
            {
                SendMsg(message, priority);
            }
        }


    }

    ///// <summary>
    ///// CodeSiteLogging conditional encapsulator. 
    ///// </summary>
    //internal static class CS
    //{
    //    internal enum MessageColor
    //    {
    //        Red = 1,
    //        Orange = 2,
    //        Yellow = 3,
    //        Green = 4,
    //        Blue = 5,
    //        Indigo = 6,
    //        Violet = 6
    //    }

    //    #region private methods
    //    private static StackFrame GetCallingStackFrame()
    //    {
    //        // GetFrame(0) --> GetCallingStackFrame()
    //        // GetFrame(1) --> Calling method (like EnterMethod())
    //        // GetFrame(2) --> Method that called calling method
    //        return new StackTrace().GetFrame(2);
    //    }

    //    // Fields...
    //    private static ILoggingService loggingService;

    //    public static ILoggingService LoggingService
    //    {
    //        get
    //        {
    //            // Create on demand...
    //            if (loggingService == null)
    //                loggingService = BufferServices.Logging;
    //            return loggingService;
    //        }
    //    }


    //    #endregion

    //    #region stack methods
    //    [Conditional("CODESITE")]
    //    internal static void EnterMethod()
    //    {
    //        StackFrame frame = GetCallingStackFrame();
    //        System.Reflection.MethodBase method = frame.GetMethod();
    //        CodeSite.EnterMethod(String.Format("{0}.{1}", method.DeclaringType.Name, method.Name));
    //    }

    //    [Conditional("CODESITE")]
    //    internal static void ExitMethod()
    //    {
    //        StackFrame frame = GetCallingStackFrame();
    //        System.Reflection.MethodBase method = frame.GetMethod();
    //        CodeSite.ExitMethod(String.Format("{0}.{1}", method.DeclaringType.Name, method.Name));
    //    }

    //    #endregion

    //    #region send methods
    //    [Conditional("CODESITE")]
    //    internal static void Send(string msg, string value)
    //    {
    //        CodeSite.Send(msg, value);
    //    }

    //    [Conditional("CODESITE")]
    //    internal static void Send(string msg, object value)
    //    {
    //        CodeSite.Send(msg, value);
    //    }

    //    #endregion

    //    #region SendMsg
    //    [Conditional("CODESITE")]
    //    internal static void SendMsg(string msg)
    //    {
    //        CodeSite.SendMsg(msg);
    //    }

    //    [Conditional("CODESITE")]
    //    internal static void SendMsg(string fmtString, params string[] args)
    //    {
    //        CodeSite.SendMsg(string.Format(fmtString, args));
    //    }

    //    [Conditional("CODESITE")]
    //    internal static void SendMsg(string fmtString, MessageColor color, params string[] args)
    //    {
    //        SendMsg(string.Format(fmtString, args), color);
    //    }

    //    [Conditional("CODESITE")]
    //    internal static void SendMsg(string msg, MessageColor color)
    //    {
    //        CodeSite.SendMsg((int)color, msg);
    //    }
    //    #endregion


    //}
}
