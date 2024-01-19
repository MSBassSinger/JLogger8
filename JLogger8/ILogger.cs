using System;
using System.Collections.Generic;

namespace Jeff.Jones.JLogger
{
    /// <summary>
    /// Used to define a bitmask that describes how a log entry is handled
    /// </summary>
    [Flags]
    public enum LOG_TYPE
    {
        Unspecified = 0,

        // Flow messages
        Flow = 1,

        // General errors
        Error = 2,

        // Informational messages
        Informational = 4,

        // Warning messages
        Warning = 8,

        // System-related information
        System = 16,

        // App-related performance
        Performance = 32,

        // Show module, class, and line number info in the debug log.
        ShowModuleClassAndLineNumber = 64,

        // Shows time only, not date, in the debug log.
        // Useful if debug logs are closed and a new one created on
        // the first write the next day after the log file was opened.
        ShowTimeOnly = 128,

        // Hides the thread ID from being printed in the debug log.
        HideThreadID = 256,

        // Shows test debug log statements in the log.
        Test = 512,

        // Adds the stack trace to the debug log when a method exits.
        IncludeStackTrace = 1024,

        // Sends an email if the flag is on
        SendEmail = 2048

    }  // END public enum CONSOLE_EXCEPTION_TYPE
    public interface ILogger : IDisposable
    {

        Boolean SetLogData(String logFileName, 
                           Int32 daysToRetainLogs, 
                           LOG_TYPE debugLogOptions);

        String LogFileName { get; }

        Int32 DaysToRetainLogs { get; }

        LOG_TYPE DebugLogOptions { get; set; }

        Boolean SetEmailData(String emailServer, 
                             String emailLogonName, 
                             String emailPassword, 
                             Int32 portSMTP, 
                             List<String> sendToAddresses, 
                             String fromAddress, 
                             String replyToAddress,
                             Boolean emailEnabled);

        String EmailServer { get; }

        String EmailLogonName { get; }

        String EmailPassword { get; }

        Int32 SMTPPort { get; }

        Boolean EmailEnabled { get; }

        List<String> SendToAddresses { get; set; }

        String FromAddress { get; set; }

        String ReplyToAddress { get; set; }

        Boolean WriteDebugLog(LOG_TYPE debugLogOptions, 
                              Exception ex, 
                              String detailMessage);

        Boolean WriteDebugLog(LOG_TYPE debugLogOptions, 
                              String message);

        Boolean StartLog();

        Boolean StopLog();


    }
}
