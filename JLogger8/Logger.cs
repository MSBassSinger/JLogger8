using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;
using System.Timers;
using Jeff.Jones.JHelpers8;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Azure.Storage.Files.Shares.Models;
using System.Xml.Linq;

namespace Jeff.Jones.JLogger8
{

    /// <summary>
    /// Used to define a bitmask that describes how a log entry is handled
    /// </summary>
    [Flags]
    public enum LOG_TYPE
    {
        /// <summary>
        /// Used as a default value until an assignment is made.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Flow messages
        /// </summary>     
        Flow = 1,

        /// <summary>
        /// General errors
        /// </summary>
        Error = 2,

        /// <summary>
        /// Informational messages
        /// </summary>
        Informational = 4,

        /// <summary>
        /// Warning messages
        /// </summary>
        Warning = 8,

        /// <summary>
        /// System-related information
        /// </summary>
        System = 16,

        /// <summary>
        /// App-related performance
        /// </summary>
        Performance = 32,

        /// <summary>
        /// Show module, method, and line number info in the debug log.
        /// </summary>
        ShowModuleMethodAndLineNumber = 64,

        /// <summary>
        /// Shows time only, not date, in the debug log.
        /// Useful if debug logs are closed and a new one created on
        /// the first write the next day after the log file was opened.
        /// </summary>
        ShowTimeOnly = 128,

        /// <summary>
        /// Hides the thread ID from being printed in the debug log.
        /// </summary>
        HideThreadID = 256,

        /// <summary>
        /// Shows test debug log statements in the log.
        /// </summary>
        Test = 512,

        /// <summary>
        /// Adds the stack trace to the debug log when a method exits.
        /// </summary>
        IncludeStackTrace = 1024,

        /// <summary>
        /// Sends an email if the flag is on
        /// </summary>
        SendEmail = 2048,

        /// <summary>
        /// Sends an email if the flag is on
        /// </summary>
        IncludeExceptionData = 4096,

        /// <summary>
        /// Log entry related to database operations
        /// </summary>
        Database = 8192,

        /// <summary>
        /// Log entry related to service operations
        /// </summary>
        Service = 16384,

        /// <summary>
        /// Log entry related to cloud operations
        /// </summary>
        Cloud = 32768,

        /// <summary>
        /// Log entry related to management concerns or operations
        /// </summary>
        Management = 65536,

        /// <summary>
        /// Log entry related to some fatal operation or state
        /// </summary>
        Fatal = 131072,

        /// <summary>
        /// Log entry related to network issue or operation
        /// </summary>
        Network = 262144,

        /// <summary>
        /// Log entry related to a threat condition
        /// </summary>
        Threat = 524288,

        /// <summary>
        /// Only used by the log writer during initial startup
        /// </summary>
        Startup = 1048576

    }  // END public enum LOG_TYPE

    /// <summary>
    /// Where should the logger send log entries?
    /// </summary>
    public enum LOG_DESTINATION_Enum
    {
        /// <summary>
        /// Default value.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Log File intended for disk storage.
        /// </summary>
        DiskFile = 1,

        /// <summary>
        /// log File intended for Azure storage.
        /// </summary>
        AzureStorageFile = 2,

        /// <summary>
        /// Log data intended to go to a SQL Server database.
        /// </summary>
        Database = 3,

        /// <summary>
        /// Log data intended for placing in an Azure Storage Queue.
        /// </summary>
        AzureStorageQueue = 4,

        /// <summary>
        /// Log data intended for an Azure queue.
        /// </summary>
        AzureQueue = 5
    }



    /// <summary>
    /// The Microsoft.Extensions.Logging.LogLevel enum is not a bitset enum ([Flags]).  So this is used to provide
    /// the multi-level capability of logging that is lacking in LogLevel, but remains 100% compatible with it.
    /// This bitset matches the LogLevel enum used by ILogger instances.
    /// 
    /// Using the static conversion methods in this class, the LogLevels in an array (denoting
    /// what should be logged) are converted to a LogLevelBitset value (allowing low-cost comparisons before executing  
    /// a logging method) to log  only those items wanted.  Since the value can be changed in real time, it allows changing  
    /// the what levels of debugging to use at any time.  Fewer for normal operation, more for debugging production code
    /// or during development and testing.
    /// 
    /// Example:
    /// <![CDATA[
    ///	1. 	 if (m_Log != null) && ((m_LogLevels & LogLevelBitset.Error) == LogLevelBitset.Error))
    ///	2. 	 {
    ///	3. 		m_Log.LogError(exUnhandled, exUnhandled.GetFullExceptionMessage(true, false));
    ///	4.	 }
    ///	]]>
    /// 
    /// Line 1 makes sure whatever ILogger was passed in is instantiated before calling a logging method and 
    ///        if instantiated, performs a bit comparison to the desired logging levels, and 
    ///        if it matches, calls the ILogger method.  If the logging level
    ///        does not match, or the logger is not instantiated, then the method is never called, 
    ///        saving the overhead.
    ///        
    /// This approach allows logging to be built in from the start at various levels, but only 
    /// execute the log methods permitted by the current levels permitted.
    /// 
    /// </summary>
    [Flags]
    public enum LogLevelBitset
    {

        /// <summary>
        ///     Not used for writing log messages. Specifies that a logging category should not
        ///     write any messages.
        /// </summary>
        None = 0,


        /// <summary>
        ///     Logs that contain the most detailed messages. These messages may contain sensitive
        ///     application data. These messages are disabled by default and should never be
        ///     enabled in a production environment.
        /// </summary>
        Trace = 1,


        /// <summary>
        ///     Logs that are used for interactive investigation during development. These logs
        ///     should primarily contain information useful for debugging and have no long-term
        ///     value.
        /// </summary>
        Debug = 2,


        /// <summary>
        ///     Logs that track the general flow of the application. These logs should have long-term
        ///     value.
        /// </summary>
        Information = 4,


        /// <summary>
        ///     Logs that highlight an abnormal or unexpected event in the application flow,
        ///     but do not otherwise cause the application execution to stop.
        /// </summary>
        Warning = 8,

        /// <summary>
        ///     Logs that highlight when the current flow of execution is stopped due to a failure.
        ///     These should indicate a failure in the current activity, not an application-wide
        ///     failure.
        /// </summary>
        Error = 16,

        /// <summary>
        ///     Logs that describe an unrecoverable application or system crash, or a catastrophic
        ///     failure that requires immediate attention.
        /// </summary>
        Critical = 32
    }



    /// <summary>
    /// Implementation for Logger used in production.
    /// The class is sealed to prevent derived instances, which would defeat the purpose of a singleton.
    /// </summary>
    public sealed class Logger : IDisposable, ILogger
    {
        /// <summary>
        /// Member variables
        /// </summary>
        private Int32 m_DaysToRetainLogs = DEFAULT_LOG_RETENTION;
        private LOG_TYPE m_DebugLogOptions = DEFAULT_DEBUG_LOG_OPTIONS;
        private LOG_DESTINATION_Enum m_LogDestination = LOG_DESTINATION_Enum.Undefined;
        private LogLevelBitset m_LogLevelBitset = DEFAULT_ILOGGER_LOG_OPTIONS;
        private String m_EmailServer = "";
        private String m_EmailLogonName = "";
        private String m_EmailPassword = "";
        private Int32 m_SMTPPort = 25;
        private List<String> m_SendToAddresses = null;
        private String m_FromAddress = "";
        private String m_ReplyToAddress = "";
        private Boolean m_UseSSL = false;
        private Boolean m_blnDisposeHasBeenCalled = false;
        private Boolean m_LogDataSet = false;
        private String m_LogFileNamePrefix = DEFAULT_LOG_FILE_NAME_PREFIX;
        private String m_EmergencyLogPrefixName = DEFAULT_EMERG_LOG_PREFIX;


        private const String FILE_FULL_DATE_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";
        private const String FULL_DATE_FORMAT = "HH:mm:ss.fff";
        private const String DEFAULT_LOG_FILE_NAME_PREFIX = "JLog_";
        private const String DEFAULT_EMERG_LOG_PREFIX = "JLOGGER_EMERGENCY_LOG_";
        private const String LOG_FIELD_DELIMITER = "\t";
        private const String LOG_FILE_TEMP_SUFFIX = "_through_9999-99-99_99.99.99.999.log";

        private Boolean m_DBEnabled = false;
        private String m_DBServer = "";
        private Boolean m_DBUseAuthentication = false;
        private String m_DBDatabase = "";
        private String m_DBUserName = "";
        private String m_DBPassword = "";

        private String m_AzureStorageResourceID = "";
        private String m_AzureStorageFileShareName = "";
        private String m_AzureStorageDirectory = "";
        private String m_AzureRemoteFileName = "";
        private Boolean m_UseAzureFileStorage = false;
        //private Boolean m_UseAzureStorageQueue = false;
        //private Boolean m_UseAzureQueue = false;
        private String m_AzureStorageQueueName = "";



        /// <summary>
        /// Default log settings
        /// </summary>
        public const LOG_TYPE DEFAULT_DEBUG_LOG_OPTIONS = LOG_TYPE.Error |
                                                            LOG_TYPE.Informational |
                                                            LOG_TYPE.ShowTimeOnly |
                                                            LOG_TYPE.Warning |
                                                            LOG_TYPE.HideThreadID |
                                                            LOG_TYPE.ShowModuleMethodAndLineNumber |
                                                            LOG_TYPE.System;

        /// <summary>
        /// Default value for logging levels when used as ILogger.  Changed by what is passed in at the constructor.
        /// </summary>
        public const LogLevelBitset DEFAULT_ILOGGER_LOG_OPTIONS = LogLevelBitset.Debug |
                                                                 LogLevelBitset.Warning |
                                                                 LogLevelBitset.Critical |
                                                                 LogLevelBitset.Error;


        /// <summary>
        /// By default, how long we retain old debug log files.
        /// </summary>
        public const Int32 DEFAULT_LOG_RETENTION = 14;

        /// <summary>
        /// This is the queue that holds the log entries.
        /// </summary>
        private List<DebugLogItem> m_lstLogQueue;

        /// <summary>
        /// Timer used to manage the log entry items stack in writing to the debug log file.
        /// </summary>
        private System.Timers.Timer m_LogWriteController;

        /// <summary>
        /// The default frequency at which m_LogWriteController writes to the log file.
        /// </summary>
        private Int32 m_LogWritePeriod = LOG_CACHE_FREQUENCY;  // milliseconds

        /// <summary>
        /// The default frequency at which m_LogWriteController writes to the log file.
        /// </summary>
        public const Int32 LOG_CACHE_FREQUENCY = 500;  // milliseconds

        /// <summary>
        /// Variable to hold the date-time String used when the file is created.
        /// </summary>
        private String m_strDateLogFileCreated = "";

        /// <summary>
        /// The path to the debug log file
        /// </summary>
        private String m_LogPath = CommonHelpers.CurDir;

        /// <summary>
        /// The fully qualified file name being used.
        /// </summary>
        private String m_strDebugLogFileName = "";


        /// <summary>
        /// Flag to tell if StartLog was called already, so it can only try to start the log once.
        /// </summary>
        private Boolean m_LogStarted = false;

        private static readonly Lazy<Logger> m_objLogger = new Lazy<Logger>(() => new Logger());

        /// <summary>
        /// Used for thread locking 
        /// </summary>
        private Object m_objPadLock = new Object();

        /// <summary>
        /// Used for thread locking 
        /// </summary>
        private Object m_objPadLockWrite = new Object();

        private DataAccessLayer m_DAC = null;



        /// <summary>
        /// Parameterless constructor required for a singleton
        /// </summary>
        private Logger()
        {
            m_SendToAddresses = new List<String>();

            m_strDateLogFileCreated = DateTime.Now.ToShortDateString();

            m_lstLogQueue = new List<DebugLogItem>();

            String strCurDir = CommonHelpers.CurDir;

            // Remove an ending "\"
            if (strCurDir.EndsWith(@"\"))
            {
                m_LogPath = strCurDir;
            }
            else
            {
                m_LogPath = strCurDir + @"\";
            }


        }



        /// <summary>
        /// Once the Logger instance is configured, this is used to start logging.
        /// </summary>
        /// <returns></returns>
        public Boolean StartLog()
        {
            Boolean retVal = false;

            try
            {

                if (m_lstLogQueue == null)
                {
                    m_lstLogQueue = new List<DebugLogItem>();
                }


                if (m_DBEnabled)
                {
                    if (!m_LogStarted)
                    {
                        m_DAC = new DataAccessLayer(m_DBServer, m_DBDatabase, m_DBUseAuthentication, m_DBUserName, m_DBPassword, 10, 10);

                        ProcessLogRetention();

                        CreateDebugLogFile();

                        StartTimer();

                        WriteDebugLog(LOG_TYPE.Informational, "Initial Log entry", "");

                        m_LogStarted = true;

                        retVal = true;
                    }
                }
                else
                {
                    if (m_LogDataSet && !m_LogStarted)
                    {
                        ProcessLogRetention();

                        CreateDebugLogFile();

                        StartTimer();

                        retVal = WriteToLog(LOG_TYPE.Informational, "Initial Log entry", "");

                        m_LogStarted = retVal;
                    }
                    else
                    {
                        retVal = false;
                    }

                }
            }
            catch (Exception exUnhandled)
            {
                exUnhandled.Data.Add("User", Environment.UserName);

                throw;
            }

            return retVal;
        }

        /// <summary>
        /// When the Logger instance is running, this is used to stop logging.
        /// </summary>
        /// <returns></returns>
        public Boolean StopLog()
        {
            Boolean retVal = false;

            try
            {

                if (m_LogWriteController != null)
                {
                    m_LogWriteController.Stop();

                    m_LogWriteController.Dispose();

                    m_LogWriteController = null;

                }

                if (m_LogStarted)
                {
                    WriteDebugLog(LOG_TYPE.Management, Properties.Resources.LAST_LINE_IN_LOG, "");

                    ProcessLogQueue();
                }

                if (m_lstLogQueue != null)
                {
                    if (m_lstLogQueue.Count > 1)
                    {
                        m_lstLogQueue.Clear();
                    }

                    m_lstLogQueue = null;
                }

                if (m_DBEnabled)
                {
                    m_DAC = null;
                }
                else
                {
                    if (m_LogStarted)
                    {
                        CloseLogFile();
                    }
                }

                m_LogStarted = false;

                retVal = true;
            }
            catch (Exception exUnhandled)
            {
                exUnhandled.Data.Add("User", Environment.UserName);
                throw;
            }

            return retVal;

        }

        /// <summary>
        /// The property that consumer code uses to access the singleton instance.
        /// </summary>
        public static Logger Instance
        {
            get
            {
                return m_objLogger.Value;
            }
        }

        /// <summary>
        /// Fully qualified file name for the log file.
        /// </summary>
        public String LogFileName
        {
            get
            {
                return m_strDebugLogFileName;
            }
        }

        /// <summary>
        /// Fully qualified path for the log file.
        /// </summary>
        public String LogPath
        {
            get
            {
                return m_LogPath;
            }
            set
            {
                if (value == null)
                {
                    m_LogPath = CommonHelpers.CurDir;
                }
                else if (value.Trim().Length == 0)
                {
                    m_LogPath = CommonHelpers.CurDir;
                }
                else if (!Directory.Exists(value.Trim()))
                {
                    m_LogPath = CommonHelpers.CurDir;

                    throw new FileNotFoundException($"The folder [{value.Trim()}] does not exist.");
                }
                else
                {
                    m_LogPath = value.Trim();
                }
            }
        }

        /// <summary>
        /// How many days that the Logger instance retains previous log files.
        /// </summary>
        public Int32 DaysToRetainLogs
        {
            get
            {
                return m_DaysToRetainLogs;
            }
        }

        /// <summary>
        /// The debug flags that are active during the lifetime of the Logger instance
        /// </summary>
        public LOG_TYPE DebugLogOptions
        {
            get
            {
                return m_DebugLogOptions;
            }
            set
            {
                m_DebugLogOptions = value;
                m_LogLevelBitset = ConvertLogLevelsToILoggerBitset(value);
            }
        }

        /// <summary>
        /// The debug flags that are active during the lifetime of the Logger instance
        /// </summary>
        public LogLevelBitset ILoggerOptions
        {
            get
            {
                return m_LogLevelBitset;
            }
            set
            {
                m_LogLevelBitset = value;
                m_DebugLogOptions = ConvertILoggerBitsetToLogLevel(value);
            }
        }

        /// <summary>
        /// The IP address or DNS name of the outgoing mail server
        /// </summary>
        public String EmailServer
        {
            get
            {
                return m_EmailServer;
            }
        }

        /// <summary>
        /// The logon name expected by the SMTP email server.
        /// </summary>
        public String EmailLogonName
        {
            get
            {
                return m_EmailLogonName;
            }
        }

        /// <summary>
        /// The logon password expected by the SMTP email server.
        /// </summary>
        public String EmailPassword
        {
            get
            {
                return m_EmailPassword;
            }
        }

        /// <summary>
        /// The port that the SMTP email server listens on.
        /// </summary>
        public Int32 SMTPPort
        {
            get
            {
                return m_SMTPPort;
            }
        }

        /// <summary>
        /// A list of email addresses that the Logger instance sends emails to if 
        /// email is enabled.
        /// </summary>
        public List<String> SendToAddresses
        {
            get
            {
                return m_SendToAddresses;
            }
            set
            {
                if (value == null)
                {
                    m_SendToAddresses = new List<String>();
                }
                else
                {
                    m_SendToAddresses = value;
                }
            }
        }

        /// <summary>
        /// The email address to use with sending emails to indicate who the email is from.
        /// </summary>
        public String FromAddress
        {
            get
            {
                return m_FromAddress;
            }
            set
            {
                if (value == null)
                {
                    m_FromAddress = "";
                }
                else
                {
                    m_FromAddress = value;
                }
            }
        }

        /// <summary>
        /// The email address used to tell the recipient what address to reply to.
        /// </summary>
        public String ReplyToAddress
        {
            get
            {
                return m_ReplyToAddress;
            }
            set
            {
                if (value == null)
                {
                    m_ReplyToAddress = "";
                }
                else
                {
                    m_FromAddress = value;
                }
            }
        }

        /// <summary>
        /// True if sending email is enabled globally, false to turn it off globally.
        /// </summary>
        public Boolean EmailEnabled
        {
            get
            {
                return m_DebugLogOptions.HasFlag(LOG_TYPE.SendEmail);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean DBEnabled
        {
            get
            {
                if (m_LogDestination == LOG_DESTINATION_Enum.Database)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    m_LogDestination = LOG_DESTINATION_Enum.Database;
                }
                else
                {
                    m_LogDestination = LOG_DESTINATION_Enum.Undefined;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public String DBServer
        {
            get
            {
                return m_DBServer;
            }
            set
            {
                m_DBServer = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean DBUseAuthentication
        {
            get
            {
                return m_DBUseAuthentication;
            }
            set
            {
                m_DBUseAuthentication = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public String DBDatabase
        {
            get
            {
                return m_DBDatabase;
            }
            set
            {
                m_DBDatabase = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public String DBUserName
        {
            get
            {
                return m_DBUserName;
            }
            set
            {
                m_DBUserName = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public String DBPassword
        {
            get
            {
                return m_DBPassword;
            }
            set
            {
                m_DBPassword = value;
            }
        }



        /// <summary>
        /// True if the email server requires using SSL, false if not.
        /// </summary>
        public Boolean UseSSL
        {
            get
            {
                return m_UseSSL;
            }
            set
            {
                m_UseSSL = value;
            }
        }

        /// <summary>
        /// True if using Azure Storage for the log file.
        /// False if not.
        /// A local file is used while the log is open.
        /// When the log is closed, the log file is palced into Azure Storage.
        /// </summary>
        public Boolean UseAzureFileStorage
        {
            get
            {
                if (m_LogDestination == LOG_DESTINATION_Enum.AzureStorageFile)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    m_LogDestination = LOG_DESTINATION_Enum.AzureStorageFile;
                }
                else
                {
                    m_LogDestination = LOG_DESTINATION_Enum.Undefined;
                }
            }
        }

        /// <summary>
        /// The resource ID provided in the Azure portal
        /// </summary>
        public String AzureStorageResourceID
        {
            get
            {
                return m_AzureStorageResourceID;
            }
            set
            {
                m_AzureStorageResourceID = value;
            }
        }

        /// <summary>
        /// The file share name which houses remote directories and remote files.
        /// </summary>
        public String AzureStorageFileShareName
        {
            get
            {
                return m_AzureStorageFileShareName;
            }
            set
            {
                m_AzureStorageFileShareName = value;
            }
        }

        /// <summary>
        /// The Azure Storage Queue name to which LogItem instances are sent.
        /// </summary>
        public String AzureStorageQueueName
        {
            get
            {
                return m_AzureStorageQueueName;
            }
            set
            {
                m_AzureStorageQueueName = value;
            }
        }

        /// <summary>
        /// The directory name where the remote file(s) are kept.
        /// </summary>
        public String AzureStorageDirectory
        {
            get
            {
                return m_AzureStorageDirectory;
            }
            set
            {
                m_AzureStorageDirectory = value;
            }
        }

        /// <summary>
        /// Name of the remote log file to save.
        /// </summary>
        public String AzureRemoteFileName
        {
            get
            {
                return m_AzureRemoteFileName;
            }
            set
            {
                m_AzureRemoteFileName = value;
            }
        }

        /// <summary>
        /// Method used to set the email sending configuration.
        /// </summary>
        /// <param name="emailServer">IP address or DNS name of the SMTP server.</param>
        /// <param name="emailLogonName">Username expected by the SMTP server.</param>
        /// <param name="emailPassword">Password expected by the SMTP server.</param>
        /// <param name="smtpPort">Port number the SMTP port listens on.</param>
        /// <param name="sendToAddresses">A List(String) of 1 to n addresses to send to.</param>
        /// <param name="fromAddress">The address to show that this email is from.</param>
        /// <param name="replyToAddress">The address to show as the address to reply to.</param>
        /// <param name="useSSL">True if the SMTP server expects SSL, false if not.</param>
        /// <returns>True if sent, false if not.</returns>
        public Boolean SetEmailData(String emailServer,
                                    String emailLogonName,
                                    String emailPassword,
                                    Int32 smtpPort,
                                    List<String> sendToAddresses,
                                    String fromAddress,
                                    String replyToAddress,
                                    Boolean useSSL)
        {

            Boolean retVal = false;

            try
            {
                m_EmailServer = emailServer ?? "";
                m_EmailLogonName = emailLogonName ?? "";
                m_EmailPassword = emailPassword ?? "";
                m_SMTPPort = smtpPort;
                this.SendToAddresses = sendToAddresses;
                m_FromAddress = fromAddress;
                m_ReplyToAddress = replyToAddress;
                m_UseSSL = useSSL;

                retVal = true;
            }
            catch (Exception exUnhandled)
            {
                exUnhandled.Data.Add("m_EmailServer", m_EmailServer);
                throw;
            }

            return retVal;


        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbServer"></param>
        /// <param name="dbUserName"></param>
        /// <param name="dbPassword"></param>
        /// <param name="useWindowsAuthentication"></param>
        /// <param name="dbLogEnabled"></param>
        /// <param name="dbName"></param>
        /// <param name="daysToRetainLogs"></param>
        /// <param name="debugLogOptions"></param>
        /// <returns></returns>
        public Boolean SetDBConfiguration(String dbServer,
                                    String dbUserName,
                                    String dbPassword,
                                    Boolean useWindowsAuthentication,
                                    Boolean dbLogEnabled,
                                    String dbName,
                                    Int32 daysToRetainLogs,
                                    LOG_TYPE debugLogOptions)
        {

            Boolean retVal = false;

            try
            {
                m_LogDestination = LOG_DESTINATION_Enum.Database;
                this.DBServer = dbServer;
                this.DBUseAuthentication = useWindowsAuthentication;
                this.DBDatabase = dbName;
                this.DBUserName = dbUserName;
                this.DBPassword = dbPassword;
                m_DaysToRetainLogs = daysToRetainLogs;
                this.DebugLogOptions = debugLogOptions;
                this.ILoggerOptions = ConvertLogLevelsToILoggerBitset(debugLogOptions);
                m_DBEnabled = dbLogEnabled;

                retVal = true;
            }
            catch (Exception exUnhandled)
            {
                exUnhandled.Data.Add("dbServer", dbServer);
                exUnhandled.Data.Add("dbName", dbName);
                exUnhandled.Data.Add("dbUserName", dbUserName);
                exUnhandled.Data.Add("useWindowsAuthentication", useWindowsAuthentication.ToString());
                exUnhandled.Data.Add("dbLogEnabled", dbLogEnabled.ToString());
                throw;
            }

            return retVal;

        }

        /// <summary>
        /// This sets the configuration if using Azure File Storage for the log file.
        /// Note that for performance reasons, the active log file is kept locally
        /// (see SetLogData method) and when closed, copied to the Azure file storage share.
        /// The file name construction is based on the SetLogData method parameters.
        /// </summary>
        /// <param name="azureStorageResourceID">The connection string from the storage directory Access Keys</param>
        /// <param name="azureStorageFileShareName">The URL provided on the fileshare properties page</param>
        /// <param name="azureStorageDirectory">The storage directory name</param>
        /// <param name="useAzureFileStorage">True to turn on the use of the Azure file storage matching the configuration.</param>
        /// <returns></returns>
        public Boolean SetAzureConfiguration(String azureStorageResourceID,
                                             String azureStorageFileShareName,
                                             String azureStorageDirectory,
                                             Boolean useAzureFileStorage)
        {

            Boolean retVal = false;

            try
            {
                this.AzureStorageResourceID = azureStorageResourceID;
                this.AzureStorageFileShareName = azureStorageFileShareName;
                this.AzureStorageDirectory = azureStorageDirectory;

                if (useAzureFileStorage)
                {
                    m_LogDestination = LOG_DESTINATION_Enum.AzureStorageFile;
                }

                retVal = true;
            }
            catch (Exception exUnhandled)
            {
                exUnhandled.Data.Add("azureStorageResourceID", azureStorageResourceID ?? "NULL");
                exUnhandled.Data.Add("azureStorageFileShareName", azureStorageFileShareName ?? "NULL");
                exUnhandled.Data.Add("azureStorageDirectory", azureStorageDirectory ?? "NULL");
                exUnhandled.Data.Add("useAzureFileStorage", useAzureFileStorage.ToString());
                throw;
            }

            return retVal;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="azureStorageResourceID"></param>
        /// <param name="azureStorageQueueName"></param>
        /// <param name="azureStorageDirectory"></param>
        /// <param name="useAzureStorageQueueConfiguration"></param>
        /// <returns></returns>
        public Boolean SetAzureStorageQueueConfiguration(String azureStorageResourceID,
                                                         String azureStorageQueueName,
                                                         String azureStorageDirectory,
                                                         Boolean useAzureStorageQueueConfiguration)
        {

            Boolean retVal = false;

            try
            {
                this.AzureStorageResourceID = azureStorageResourceID;
                this.AzureStorageQueueName = azureStorageQueueName;
                this.AzureStorageDirectory = azureStorageDirectory;

                if (useAzureStorageQueueConfiguration)
                {
                    m_LogDestination = LOG_DESTINATION_Enum.AzureStorageQueue;
                }

                retVal = true;
            }
            catch (Exception exUnhandled)
            {
                exUnhandled.Data.Add("azureStorageResourceID", azureStorageResourceID ?? "NULL");
                exUnhandled.Data.Add("azureStorageQueueName", azureStorageQueueName ?? "NULL");
                exUnhandled.Data.Add("azureStorageDirectory", azureStorageDirectory ?? "NULL");
                exUnhandled.Data.Add("useAzureStorageQueueConfiguration", useAzureStorageQueueConfiguration.ToString());
                throw;
            }

            return retVal;

        }

        /// <summary>
        /// Method used to configure the Logger instance before starting the log..
        /// </summary>
        /// <param name="logPath">Path to where the log file should be written.  Do NOT provide a file name.  The filename is dynamcially built.</param>
        /// <param name="logFileNamePrefix">A prefix to use for the log file name.</param>
        /// <param name="daysToRetainLogs">How many days to retain prior day's logs.  Zero (0) indicates all logs are retained.</param>
        /// <param name="debugLogOptions">The bitmask value to indicate what bits will be logged and what management bits are on.</param>
        /// <param name="emergencyLogPrefixName">A prefix for the file name of a file that is used when the log file cannot be used, and data should be written.</param>
        /// <returns>True if the log data is set, false if not.</returns>
        public Boolean SetLogData(String logPath,
                                  String logFileNamePrefix,
                                  Int32 daysToRetainLogs,
                                  LOG_TYPE debugLogOptions,
                                  String emergencyLogPrefixName = DEFAULT_EMERG_LOG_PREFIX)
        {

            Boolean retVal = false;

            try
            {
                m_LogFileNamePrefix = logFileNamePrefix;

                if (daysToRetainLogs <= 0)
                {
                    m_DaysToRetainLogs = 0;
                }
                else
                {
                    m_DaysToRetainLogs = daysToRetainLogs;
                }

                m_DebugLogOptions = debugLogOptions;
                this.ILoggerOptions = ConvertLogLevelsToILoggerBitset(debugLogOptions);

                m_LogDataSet = true;


                if (emergencyLogPrefixName.Length == 0)
                {
                    emergencyLogPrefixName = DEFAULT_EMERG_LOG_PREFIX;
                }

                m_EmergencyLogPrefixName = emergencyLogPrefixName;

                if (logPath.EndsWith(@"\"))
                {
                    m_LogPath = logPath;
                }
                else
                {
                    m_LogPath = logPath + @"\";
                }

                retVal = true;
            }
            catch (Exception exUnhandled)
            {
                exUnhandled.Data.Add("m_LogFileName", m_strDebugLogFileName);
                throw;
            }

            return retVal;

        }


        /// <summary>
        /// Starts the timer that controls writing to the log
        /// </summary>
        private void StartTimer()
        {
            try
            {
                m_LogWriteController = new System.Timers.Timer(m_LogWritePeriod); // Set up the timer for 0.5 seconds
                m_LogWriteController.SynchronizingObject = null;
                m_LogWriteController.Elapsed += new ElapsedEventHandler(ProcessLogQueue);
                m_LogWriteController.Enabled = true; // Enable it

            }  // END try

            catch (Exception ex)
            {
                // We couldn't open a file, so this goes in the Event Viewer.
                // Log the error that precipitated this     
                String emerglogFileName = m_LogPath + m_EmergencyLogPrefixName + CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now) + ".log";

                CommonHelpers.WriteToLog(emerglogFileName,
                                         Properties.Resources.START_TIMER_ERROR,
                                         ex.GetFullExceptionMessage(true, true));

            }  // END catch

        }

        /// <summary>
        /// This method is called to write any debug log items waiting in the queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProcessLogQueue(object sender, ElapsedEventArgs e)
        {

            try
            {
                m_LogWriteController.Enabled = false;

                ProcessLogQueue();

                if (m_LogWriteController != null)
                {
                    m_LogWriteController.Enabled = true;
                }

            }  // END try
            catch (Exception ex)
            {
                // We couldn't open a file, so this goes in the Event Viewer.
                // Log the error that precipitated this     
                String emerglogFileName = m_LogPath + m_EmergencyLogPrefixName + CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now) + ".log";

                CommonHelpers.WriteToLog(emerglogFileName,
                                         Properties.Resources.UNABLE_TO_PROCESS_QUEUE,
                                         ex.GetFullExceptionMessage(true, true));

            }  // END catch

        }

        /// <summary>
        /// This method is called to write any debug log items waiting in the queue.
        /// If the debug log file write fails, the log item is written to an alternate file.
        /// </summary>
        private void ProcessLogQueue()
        {


            StreamWriter DebugLogFileStream = null;

            try
            {
                if (m_DBEnabled)
                {

                    if (m_lstLogQueue != null)
                    {

                        if (m_lstLogQueue.Count > 0)
                        {
                            lock (m_objPadLock)
                            {
                                DebugLogItem objItem;

                                // Loop through the log item queue to process any waiting log entries.
                                do
                                {
                                    if (m_lstLogQueue.Count > 0)
                                    {
                                        objItem = m_lstLogQueue[0];

                                        if (objItem != null)
                                        {
                                            m_DAC.WriteDBLog(objItem);

                                            SendMailMgr mgr = null;

                                            try
                                            {
                                                if (objItem.Type.HasFlag(LOG_TYPE.SendEmail) &&
                                                    m_DebugLogOptions.HasFlag(LOG_TYPE.SendEmail))
                                                {
                                                    mgr = new SendMailMgr(m_EmailServer,
                                                                        m_EmailLogonName,
                                                                        m_EmailPassword,
                                                                        m_SMTPPort,
                                                                        m_SendToAddresses,
                                                                        m_FromAddress,
                                                                        m_ReplyToAddress,
                                                                        m_UseSSL);

                                                    Boolean result = mgr.SendEmail(objItem, false);

                                                }
                                            }
                                            catch (Exception exMail)
                                            {
                                                String emerglogFileName = m_LogPath + m_EmergencyLogPrefixName + CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now) + ".log";

                                                CommonHelpers.WriteToLog(emerglogFileName,
                                                                         Properties.Resources.UNABLE_TO_PROCESS_DEBUG_LOG_ITEM_TITLE,
                                                                         $"Email send failed with message [{exMail.GetFullExceptionMessage(true, true)}");

                                            }
                                            finally
                                            {
                                                mgr = null;
                                            }

                                        }

                                        if (m_lstLogQueue != null)
                                        {
                                            if (m_lstLogQueue.Count > 0)
                                            {
                                                if (m_lstLogQueue.Contains(objItem))
                                                {
                                                    m_lstLogQueue.Remove(objItem);
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Queue does not contain item");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Queue has zero items");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Queue null");
                                        }
                                    }
                                }
                                while (m_lstLogQueue.Count > 0);
                            }
                        }
                    }
                }
                else
                {
                    String TodaysDate = DateTime.Now.ToShortDateString();

                    if (m_strDateLogFileCreated != TodaysDate)
                    {

                        CloseLogFile();
                        CreateDebugLogFile();

                        m_strDateLogFileCreated = DateTime.Now.ToShortDateString();

                        ProcessLogRetention();

                    }  // END if (m_strDateLogFileCreated != TodaysDate)

                    if (m_lstLogQueue != null)
                    {

                        if (m_lstLogQueue.Count > 0)
                        {
                            lock (m_objPadLock)
                            {
                                DebugLogItem objItem;

                                // Create an object to open the log file for appending.
                                DebugLogFileStream = File.AppendText(m_strDebugLogFileName);

                                try
                                {
                                    String strDateTimeStamp = "";

                                    // Loop through the log item queue to process any waiting log entries.
                                    do
                                    {
                                        if (m_lstLogQueue.Count > 0)
                                        {
                                            objItem = m_lstLogQueue[0];

                                            if (objItem != null)
                                            {
                                                // Get the appropriate timestamp for whether showing the date or not.
                                                if (m_DebugLogOptions.HasFlag(LOG_TYPE.ShowTimeOnly))
                                                {
                                                    strDateTimeStamp = objItem.LogDateTime.ToString(FULL_DATE_FORMAT);
                                                }
                                                else
                                                {
                                                    strDateTimeStamp = objItem.LogDateTime.ToString(FILE_FULL_DATE_FORMAT);
                                                }

                                                String Item2Write = "";
                                                String threadID = objItem.ThreadID.ToString();
                                                String exceptionData = "N/A";
                                                String stackTrace = "N/A";
                                                String module = "N/A";
                                                String methodName = "N/A";
                                                String lineNumber = "N/A";

                                                if (m_DebugLogOptions.HasFlag(LOG_TYPE.HideThreadID))
                                                {
                                                    threadID = "N/A";
                                                }

                                                if (m_DebugLogOptions.HasFlag(LOG_TYPE.IncludeExceptionData))
                                                {
                                                    if (objItem.ExceptionData.Length > 0)
                                                    {
                                                        exceptionData = objItem.ExceptionData;
                                                    }
                                                }

                                                if (m_DebugLogOptions.HasFlag(LOG_TYPE.IncludeStackTrace))
                                                {
                                                    if (objItem.StackData.Length > 0)
                                                    {
                                                        stackTrace = objItem.StackData;
                                                    }
                                                }

                                                if (m_DebugLogOptions.HasFlag(LOG_TYPE.ShowModuleMethodAndLineNumber))
                                                {
                                                    module = objItem.ModuleName;
                                                    methodName = objItem.MethodName;
                                                    lineNumber = objItem.LineNumber.ToString();
                                                }

                                                Item2Write += $"{strDateTimeStamp}{LOG_FIELD_DELIMITER}{objItem.TypeDescription}{LOG_FIELD_DELIMITER}{objItem.Message}{LOG_FIELD_DELIMITER}{objItem.DetailMessage}{LOG_FIELD_DELIMITER}{exceptionData}{LOG_FIELD_DELIMITER}{stackTrace}{LOG_FIELD_DELIMITER}{module}{LOG_FIELD_DELIMITER}{methodName}{LOG_FIELD_DELIMITER}{lineNumber}{LOG_FIELD_DELIMITER}{threadID}";

                                                SendMailMgr mgr = null;

                                                try
                                                {
                                                    if (objItem.Type.HasFlag(LOG_TYPE.SendEmail) &&
                                                        m_DebugLogOptions.HasFlag(LOG_TYPE.SendEmail))
                                                    {
                                                        mgr = new SendMailMgr(m_EmailServer,
                                                                            m_EmailLogonName,
                                                                            m_EmailPassword,
                                                                            m_SMTPPort,
                                                                            m_SendToAddresses,
                                                                            m_FromAddress,
                                                                            m_ReplyToAddress,
                                                                            m_UseSSL);

                                                        Boolean result = mgr.SendEmail(objItem, false);

                                                    }
                                                }
                                                catch (Exception exMail)
                                                {
                                                    String emerglogFileName = m_LogPath + m_EmergencyLogPrefixName + CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now) + ".log";

                                                    CommonHelpers.WriteToLog(emerglogFileName,
                                                                             Properties.Resources.UNABLE_TO_PROCESS_DEBUG_LOG_ITEM_TITLE,
                                                                             $"Email send failed with message [{exMail.GetFullExceptionMessage(true, true)}");

                                                }
                                                finally
                                                {
                                                    mgr = null;
                                                }

                                                try
                                                {
                                                    DebugLogFileStream.WriteLine(Item2Write);

                                                    DebugLogFileStream.Flush();
                                                }
                                                catch (Exception exFile)
                                                {
                                                    // We couldn't open a file, so this goes in the emergency log.
                                                    // Log the error that precipitated this                    
                                                    String emerglogFileName = m_LogPath + m_EmergencyLogPrefixName + CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now) + ".log";

                                                    CommonHelpers.WriteToLog(emerglogFileName,
                                                                             Properties.Resources.UNABLE_TO_PROCESS_DEBUG_LOG_ITEM_TITLE,
                                                                             exFile.GetFullExceptionMessage(true, true));

                                                    CommonHelpers.WriteToLog(emerglogFileName,
                                                                             Properties.Resources.UNABLE_TO_PROCESS_DEBUG_LOG_ITEM_TITLE,
                                                                             Item2Write);

                                                }
                                            }

                                            if (m_lstLogQueue != null)
                                            {
                                                if (m_lstLogQueue.Count > 0)
                                                {
                                                    if (m_lstLogQueue.Contains(objItem))
                                                    {
                                                        m_lstLogQueue.Remove(objItem);
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Queue does not contain item");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Queue has zero items");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Queue null");
                                            }
                                        }
                                    }
                                    while (m_lstLogQueue.Count > 0);

                                    DebugLogFileStream.Flush();

                                    DebugLogFileStream.Close();

                                }  // END try

                                catch (Exception ex)
                                {
                                    // We couldn't open a file, so this goes in the emergency log.
                                    // Log the error that precipitated this                    
                                    String emerglogFileName = m_LogPath + m_EmergencyLogPrefixName + CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now) + ".log";

                                    CommonHelpers.WriteToLog(emerglogFileName,
                                                             Properties.Resources.UNABLE_TO_PROCESS_DEBUG_LOG_ITEM_TITLE,
                                                             ex.GetFullExceptionMessage(true, true));

                                }  // END catch (Exception ex)
                                finally
                                {
                                    if (DebugLogFileStream != null)
                                    {
                                        DebugLogFileStream.Close();
                                        DebugLogFileStream.Dispose();
                                        DebugLogFileStream = null;
                                    }
                                }

                            }

                        }  // END if (m_lstLogQueue.Count > 0)

                    }  // END if (m_lstLogQueue != null)

                }  // END else of [if (m_DBEnabled)]

            }  // END try

            catch (Exception ex)
            {
                // We couldn't open a file, so this goes in the emergency log.
                // Log the error that precipitated this                    
                String emerglogFileName = m_LogPath + m_EmergencyLogPrefixName + CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now) + ".log";

                CommonHelpers.WriteToLog(emerglogFileName,
                                         Properties.Resources.UNABLE_TO_PROCESS_DEBUG_LOG_ITEM_TITLE,
                                         ex.GetFullExceptionMessage(true, true));
            }  // END catch
        }

        /// <summary>
        /// Closes the log file.
        /// </summary>
        private void CloseLogFile()
        {
            StreamWriter DebugLogFileStream = null;

            AzureFileClient azClient = null;

            try
            {

                // Rename file to show when debug ended
                String strEndDate = CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now);

                // Get the temporary file name that needs to be deleted
                String oldAzureFileName = Path.GetFileName(m_strDebugLogFileName);

                // change the debug file name for closing
                String strDebugFileName = m_strDebugLogFileName.Replace("9999-99-99_99.99.99.999", strEndDate);

                // This copies over the current local file from the temporary to the permanent name.
                File.Move(m_strDebugLogFileName, strDebugFileName);

                // If using Azure storage, the file needs to be copied there, and deleted locally.
                if (m_UseAzureFileStorage)
                {
                    // Get the temporary file name to use in Azure
                    m_AzureRemoteFileName = Path.GetFileName(strDebugFileName);

                    // Create the Azure client instance
                    azClient = new AzureFileClient(m_AzureStorageResourceID,
                                                    m_AzureStorageFileShareName,
                                                    m_AzureStorageDirectory,
                                                    m_AzureRemoteFileName,
                                                    strDebugFileName);

                    // Make sure the Azure storage exists
                    if (!azClient.DoesAzureStorageExist())
                    {
                        azClient.CreateShareAndDirectory();
                    }

                    // Copy the local log file to Azure
                    azClient.CopyLogFileToRemote();

                    // Delete the temporary log file in Azure if it exists.
                    azClient.DeleteAzureLogFile(oldAzureFileName);

                    // Last step - delete the local log file now that it is in Azure storage.
                    File.Delete(strDebugFileName);

                }

            }  // END try

            catch (Exception ex)
            {
                // Log the error that precipitated this                    
                String emerglogFileName = m_LogPath + m_EmergencyLogPrefixName + CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now) + ".log";

                CommonHelpers.WriteToLog(emerglogFileName,
                                         Properties.Resources.CLOSE_LOGFILE_UNHANDLED_TITLE,
                                         ex.GetFullExceptionMessage(true, true));

            }  // END catch
            finally
            {
                if (DebugLogFileStream != null)
                {
                    DebugLogFileStream.Dispose();

                    DebugLogFileStream = null;
                }

                azClient = null;
            }


        }  // END private void CloseLogFile()


        /// <summary>
        /// Internal method that performs the log write.
        /// </summary>
        /// <param name="logType">The log option describing this log entry.</param>
        /// <param name="mainMessage">A primary message describing the log entry.</param>
        /// <param name="secondMessage">An optional second, or detail, message.</param>
        /// <returns>True if written to the file or DB, false if not.</returns>
        private Boolean WriteToLog(LOG_TYPE logType,
                                   String mainMessage,
                                   String secondMessage)
        {
            Boolean retVal = false;

            StackTrace st = new StackTrace(true);

            StackFrame frame = null;

            try
            {
                if (st.FrameCount >= 3)
                {
                    frame = st.GetFrame(2);
                }
                else if (st.FrameCount == 2)
                {
                    frame = st.GetFrame(1);
                }
                else
                {
                    frame = st.GetFrame(0);
                }

                MethodBase callingMethod = frame.GetMethod();
                String callingFilePath = frame.GetFileName();
                String callingFileName = Path.GetFileName(callingFilePath);
                String callingMethodName = callingMethod.Name;
                Int32 callingFileLineNumber = frame.GetFileLineNumber();

                Int32 threadID = Thread.CurrentThread.ManagedThreadId;

                if (m_DBEnabled)
                {

                    DebugLogItem item = new DebugLogItem(logType,
                                                         DateTime.Now,
                                                         mainMessage,
                                                         secondMessage,
                                                         "",
                                                         "N/A",
                                                         callingFileName,
                                                         callingMethodName,
                                                         callingFileLineNumber,
                                                         threadID);

                    if (m_DAC != null)
                    {
                        m_DAC.WriteDBLog(item);
                    }

                    retVal = true;

                }
                else
                {
                    using (StreamWriter logfile = File.AppendText(m_strDebugLogFileName))
                    {
                        String logDateTime = "";

                        if ((m_DebugLogOptions & LOG_TYPE.ShowTimeOnly) == LOG_TYPE.ShowTimeOnly)
                        {
                            logDateTime = DateTime.Now.ToString(FULL_DATE_FORMAT);
                        }
                        else
                        {
                            logDateTime = DateTime.Now.ToString(FILE_FULL_DATE_FORMAT);
                        }

                        logfile.WriteLine($"{logDateTime}{LOG_FIELD_DELIMITER}{logType.ToString()}{LOG_FIELD_DELIMITER}{mainMessage}{LOG_FIELD_DELIMITER}{secondMessage}{LOG_FIELD_DELIMITER}N/A{LOG_FIELD_DELIMITER}N/A{LOG_FIELD_DELIMITER}{callingFileName}{LOG_FIELD_DELIMITER}{callingMethod.Name}{LOG_FIELD_DELIMITER}{callingFileLineNumber.ToString()}{LOG_FIELD_DELIMITER}{threadID}");
                        logfile.Flush();
                        logfile.Close();

                        retVal = true;
                    }
                }
            }
            catch (Exception exUnhandled)
            {
                // Log the error that precipitated this                    
                String emerglogFileName = m_LogPath + m_EmergencyLogPrefixName + CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now) + ".log";

                CommonHelpers.WriteToLog(emerglogFileName,
                                         Properties.Resources.CLOSE_LOGFILE_UNHANDLED_TITLE,
                                         exUnhandled.GetFullExceptionMessage(true, true));

                retVal = false;

            }

            return retVal;

        }

        /// <summary>
        /// Creates the header line, the first line in the log.
        /// </summary>
        /// <returns>True if written, false if not.</returns>
        private Boolean WriteHeaderToLog()
        {
            Boolean retVal = false;

            try
            {
                if (!m_DBEnabled)
                {
                    using (StreamWriter logfile = File.AppendText(m_strDebugLogFileName))
                    {
                        String hdr = GetHeaderString();

                        logfile.WriteLine(hdr);
                        logfile.Flush();
                        logfile.Close();

                        retVal = true;
                    }
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception exUnhandled)
            {
                // Log the error that precipitated this                    
                String emerglogFileName = m_LogPath + m_EmergencyLogPrefixName + CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now) + ".log";

                CommonHelpers.WriteToLog(emerglogFileName,
                                         Properties.Resources.CLOSE_LOGFILE_UNHANDLED_TITLE,
                                         exUnhandled.GetFullExceptionMessage(true, true));

                retVal = false;

            }

            return retVal;

        }

        internal String GetHeaderString()
        {
            String retVal = "";

            String logDateTime = "";

            if ((m_DebugLogOptions & LOG_TYPE.ShowTimeOnly) == LOG_TYPE.ShowTimeOnly)
            {
                logDateTime = "Time";
            }
            else
            {
                logDateTime = "Date Time";
            }

            retVal = $"{logDateTime}{LOG_FIELD_DELIMITER}Log Type{LOG_FIELD_DELIMITER}Message{LOG_FIELD_DELIMITER}Addtl Info{LOG_FIELD_DELIMITER}Exception Data{LOG_FIELD_DELIMITER}Stack Info{LOG_FIELD_DELIMITER}Module{LOG_FIELD_DELIMITER}Method{LOG_FIELD_DELIMITER}Line No.{LOG_FIELD_DELIMITER}ThreadID";

            return retVal;

        }

        /// <summary>
        /// Method used to write exception information to the log.
        /// This method writes a DebugLogItem instance to a queue, which is then emptied FIFO
        /// on a separate thread so calling this method does not block 
        /// main thread activity.  
        /// </summary>
        /// <param name="pExceptionType">Type of log entry to use.</param>
        /// <param name="pExceptionToUse">Exception instance to analyze for log data.</param>
        /// <param name="pOptionalLogMessage">Optional addition message besides what is in the Exception</param>
        /// <returns></returns>
        public Boolean WriteDebugLog(LOG_TYPE pExceptionType,
                                     Exception pExceptionToUse,
                                     String pOptionalLogMessage)
        {
            Boolean retVal = false;

            LOG_TYPE opFlag = LOG_TYPE.Unspecified;

            try
            {

                opFlag = GetOperationalFlag(pExceptionType);

                if (m_DebugLogOptions.HasFlag(opFlag))
                {

                    Int32 lngThreadID = Thread.CurrentThread.ManagedThreadId;

                    StackTrace CallingStackTrace = null;

                    StackFrame[] CallingStackFrames;

                    String StackTraceDescrs = "";

                    String StackMethodName = "";
                    String StackModuleName = "";
                    String LineNumberToDisplay = "";

                    Int32 PreviousLineNumber = 0;

                    String ExceptionMessages = "";

                    if (pExceptionToUse == null)
                    {
                        ExceptionMessages = "Exception was null.  No information can be obtained.";
                    }
                    else
                    {
                        ExceptionMessages = pExceptionToUse.GetFullExceptionMessage(false, false);
                    }

                    String ExceptionData = "";

                    StackTrace PreviousStackTrace = new StackTrace(true);

                    Int32 FrameNumber2Use = 1;

                    StackFrame[] PreviousStackFrames = PreviousStackTrace.GetFrames();

                    if (PreviousStackFrames.GetLength(0) == 1)
                    {
                        FrameNumber2Use = 0;
                    }

                    String PreviousModule = PreviousStackTrace.GetFrame(FrameNumber2Use).GetMethod().ReflectedType.Name;

                    // Get the name of the method in that module from where this exception was raised.
                    String PreviousMethod = PreviousStackTrace.GetFrame(FrameNumber2Use).GetMethod().ToString();

                    // If the method name is ".ctor", that is shorthand for a Constructor method.
                    if (PreviousMethod == ".ctor")
                    {
                        PreviousMethod = "Default Constructor";
                    }
                    else if (PreviousMethod.Contains("Void .ctor"))
                    {
                        PreviousMethod = PreviousMethod.Replace("Void .ctor", "");
                    }
                    else
                    {
                        // No change needed
                    }

                    PreviousLineNumber = PreviousStackTrace.GetFrame(FrameNumber2Use).GetFileLineNumber();

                    ExceptionData = "";

                    if (pExceptionToUse == null)
                    {
                        CallingStackTrace = new StackTrace(true);
                    }
                    else
                    {
                        Exception CurrentException = pExceptionToUse;

                        CallingStackTrace = new StackTrace(pExceptionToUse, true);

                        while (CurrentException != null)
                        {

                            if (CurrentException.Data != null)
                            {
                                if (CurrentException.Data.Count > 0)
                                {
                                    ExceptionData += "[Exception Data:] ";

                                    foreach (System.Collections.DictionaryEntry DataDetail in CurrentException.Data)
                                    {
                                        String DetailKey = ((DataDetail.Key == null) ? "Unknown" : DataDetail.Key.ToString());

                                        if (DetailKey.Length == 0)
                                        {
                                            DetailKey = "Unknown";
                                        }

                                        String DetailValue = ((DataDetail.Value == null) ? "NULL" : DataDetail.Value.ToString());

                                        ExceptionData += String.Format("[{0}]=[{1}]; ", DetailKey, DetailValue);

                                    }  // END foreach (System.Collections.DictionaryEntry DataDetail in CurrentException.Data)

                                }  // END if (CurrentException.Data.Count > 0)

                            }  // END if (CurrentException.Data != null)

                            CurrentException = CurrentException.InnerException;

                        }  // END while (CurrentException != null)

                    }  // END else of [if (pExceptionToUse == null)]

                    if (ExceptionData.Length == 0)
                    {
                        ExceptionData = "Data collection was not used.";
                    }

                    CallingStackFrames = CallingStackTrace.GetFrames();

                    if (CallingStackFrames.Length > 0)
                    {
                        PreviousLineNumber = CallingStackFrames[CallingStackFrames.Length - 1].GetFileLineNumber();

                        StackTraceDescrs = "";

                        Int32 StackCounter = 0;

                        if (pExceptionToUse == null)
                        {
                            StackCounter = 1;
                        }

                        for (int i = StackCounter; i < CallingStackFrames.Length; i++)
                        {
                            StackFrame CallingStackFrame = CallingStackFrames[i];

                            String CallingFileName = CallingStackFrame.GetFileName();

                            if (CallingFileName != null)
                            {
                                if (CallingFileName.Length > 0)
                                {
                                    StackModuleName = CallingStackFrame.GetMethod().ReflectedType.Name;

                                    StackMethodName = CallingStackFrame.GetMethod().ToString();

                                    if (StackMethodName == ".ctor")
                                    {
                                        StackMethodName = "()";
                                    }

                                    if (PreviousLineNumber == 0)
                                    {
                                        PreviousLineNumber = CallingStackFrame.GetFileLineNumber();
                                    }

                                    StackTraceDescrs += String.Format("  {0}::{1}(Line {2}, Col {3})",
                                                                        StackModuleName,
                                                                        StackMethodName,
                                                                        CallingStackFrame.GetFileLineNumber().ToString(),
                                                                        CallingStackFrame.GetFileColumnNumber().ToString()) + " | ";
                                }  // END if (CallingFileName.Length > 0)

                            }  // END if (CallingFileName != null)

                        }  // END for (int i = 0; i < CallingStackFrames.Length; i++)

                    } // END if (CallingStackFrames.Length > 0)

                    if (PreviousLineNumber == 0)
                    {
                        LineNumberToDisplay = "Not available";
                    }
                    else
                    {
                        LineNumberToDisplay = PreviousLineNumber.ToString();

                    }

                    String CurrentThreadID = lngThreadID.ToString();

                    String LogMessage = "Type: [" + pExceptionType.ToString() + "] - " + ExceptionMessages;

                    LogMessage = LogMessage.Replace(Environment.NewLine, " | ");

                    if (pOptionalLogMessage == null)
                    {
                        pOptionalLogMessage = "";
                    }
                    else
                    {
                        pOptionalLogMessage = pOptionalLogMessage.Replace(Environment.NewLine, " | ");
                    }

                    DateTime dtmNow = DateTime.Now;

                    if (m_lstLogQueue != null)
                    {
                        m_lstLogQueue.Add(new DebugLogItem(pExceptionType, dtmNow, LogMessage, pOptionalLogMessage, ExceptionData, StackTraceDescrs, PreviousModule, PreviousMethod, PreviousLineNumber, lngThreadID));
                    }

                    retVal = true;
                }
                else
                {
                    retVal = false;
                }

            }  // END try
            catch (Exception exUnhandled)
            {
                exUnhandled.Data.Add("debugLogOptions", pExceptionType.ToString());
                throw;
            }

            return retVal;

        }

        /// <summary>
        /// Method used to write message information to the log.
        /// This method writes a DebugLogItem instance to a queue, which is then emptied FIFO
        /// on a separate thread so calling this method does not block 
        /// main thread activity.  
        /// </summary>
        /// <param name="pExceptionType">Type of log entry to use.</param>
        /// <param name="message">Message to write to the log</param>
        /// <param name="secondaryMessage">Optional second message</param>
        /// <returns>True if written to log queue, false if not.</returns>
        public Boolean WriteDebugLog(LOG_TYPE pExceptionType,
                                     String message,
                                     String secondaryMessage = "")
        {
            Boolean retVal = false;

            LOG_TYPE opFlag = LOG_TYPE.Unspecified;

            try
            {

                opFlag = GetOperationalFlag(pExceptionType);

                if (m_DebugLogOptions.HasFlag(opFlag))
                {

                    Int32 lngThreadID = Thread.CurrentThread.ManagedThreadId;

                    StackTrace CallingStackTrace = null;

                    StackFrame[] CallingStackFrames;

                    String StackTraceDescrs = "";

                    String StackMethodName = "";
                    String StackModuleName = "";
                    String LineNumberToDisplay = "";

                    Int32 PreviousLineNumber = 0;

                    StackTrace PreviousStackTrace = new StackTrace(true);

                    Int32 FrameNumber2Use = 1;

                    StackFrame[] PreviousStackFrames = PreviousStackTrace.GetFrames();

                    if (PreviousStackFrames.GetLength(0) == 1)
                    {
                        FrameNumber2Use = 0;
                    }

                    String PreviousModule = PreviousStackTrace.GetFrame(FrameNumber2Use).GetMethod().ReflectedType.Name;

                    // Get the name of the method in that module from where this exception was raised.
                    String PreviousMethod = PreviousStackTrace.GetFrame(FrameNumber2Use).GetMethod().ToString();

                    // If the method name is ".ctor", that is shorthand for a Constructor method.
                    if (PreviousMethod == ".ctor")
                    {
                        PreviousMethod = "Default Constructor";
                    }
                    else if (PreviousMethod.Contains("Void .ctor"))
                    {
                        PreviousMethod = PreviousMethod.Replace("Void .ctor", "");
                    }
                    else
                    {
                        // No change needed
                    }

                    PreviousLineNumber = PreviousStackTrace.GetFrame(FrameNumber2Use).GetFileLineNumber();

                    CallingStackTrace = new StackTrace(true);

                    CallingStackFrames = CallingStackTrace.GetFrames();

                    if (CallingStackFrames.Length > 0)
                    {
                        PreviousLineNumber = CallingStackFrames[CallingStackFrames.Length - 1].GetFileLineNumber();

                        StackTraceDescrs = "";

                        Int32 StackCounter = 0;

                        for (int i = StackCounter; i < CallingStackFrames.Length; i++)
                        {
                            StackFrame CallingStackFrame = CallingStackFrames[i];

                            String CallingFileName = CallingStackFrame.GetFileName();

                            if (CallingFileName != null)
                            {
                                if (CallingFileName.Length > 0)
                                {
                                    StackModuleName = CallingStackFrame.GetMethod().ReflectedType.Name;

                                    StackMethodName = CallingStackFrame.GetMethod().ToString();

                                    if (StackMethodName == ".ctor")
                                    {
                                        StackMethodName = "()";
                                    }

                                    if (PreviousLineNumber == 0)
                                    {
                                        PreviousLineNumber = CallingStackFrame.GetFileLineNumber();
                                    }

                                    StackTraceDescrs += String.Format("  {0}::{1}(Line {2}, Col {3})",
                                                                        StackModuleName,
                                                                        StackMethodName,
                                                                        CallingStackFrame.GetFileLineNumber().ToString(),
                                                                        CallingStackFrame.GetFileColumnNumber().ToString()) + " | ";
                                }  // END if (CallingFileName.Length > 0)

                            }  // END if (CallingFileName != null)

                        }  // END for (int i = 0; i < CallingStackFrames.Length; i++)

                    } // END if (CallingStackFrames.Length > 0)

                    if (PreviousLineNumber == 0)
                    {
                        LineNumberToDisplay = "Not available";
                    }
                    else
                    {
                        LineNumberToDisplay = PreviousLineNumber.ToString();

                    }

                    String CurrentThreadID = lngThreadID.ToString();

                    message = message.Replace(Environment.NewLine, " | ");

                    secondaryMessage = secondaryMessage.Replace(Environment.NewLine, " | ");

                    DateTime dtmNow = DateTime.Now;

                    if (m_lstLogQueue != null)
                    {
                        m_lstLogQueue.Add(new DebugLogItem(pExceptionType, dtmNow, message, secondaryMessage, "", StackTraceDescrs, PreviousModule, PreviousMethod, PreviousLineNumber, lngThreadID));
                    }

                    retVal = true;
                }
                else
                {
                    retVal = false;
                }

            }  // END try
            catch (Exception exUnhandled)
            {
                exUnhandled.Data.Add("debugLogOptions", pExceptionType.ToString());
                throw;
            }


            return retVal;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExceptionType"></param>
        /// <returns></returns>
        private LOG_TYPE GetOperationalFlag(LOG_TYPE pExceptionType)
        {

            LOG_TYPE retVal = LOG_TYPE.Unspecified;

            if (pExceptionType.HasFlag(LOG_TYPE.Informational))
            {
                retVal = LOG_TYPE.Informational;
            }
            else if (pExceptionType.HasFlag(LOG_TYPE.Cloud))
            {
                retVal = LOG_TYPE.Cloud;
            }
            else if (pExceptionType.HasFlag(LOG_TYPE.Database))
            {
                retVal = LOG_TYPE.Database;
            }
            else if (pExceptionType.HasFlag(LOG_TYPE.Error))
            {
                retVal = LOG_TYPE.Error;
            }
            else if (pExceptionType.HasFlag(LOG_TYPE.Fatal))
            {
                retVal = LOG_TYPE.Fatal;
            }
            else if (pExceptionType.HasFlag(LOG_TYPE.Flow))
            {
                retVal = LOG_TYPE.Flow;
            }
            else if (pExceptionType.HasFlag(LOG_TYPE.Management))
            {
                retVal = LOG_TYPE.Management;
            }
            else if (pExceptionType.HasFlag(LOG_TYPE.Network))
            {
                retVal = LOG_TYPE.Network;
            }
            else if (pExceptionType.HasFlag(LOG_TYPE.Performance))
            {
                retVal = LOG_TYPE.Performance;
            }
            else if (pExceptionType.HasFlag(LOG_TYPE.Service))
            {
                retVal = LOG_TYPE.Service;
            }
            else if (pExceptionType.HasFlag(LOG_TYPE.Startup))
            {
                retVal = LOG_TYPE.Startup;
            }
            else if (pExceptionType.HasFlag(LOG_TYPE.System))
            {
                retVal = LOG_TYPE.System;
            }
            else if (pExceptionType.HasFlag(LOG_TYPE.Test))
            {
                retVal = LOG_TYPE.Test;
            }
            else if (pExceptionType.HasFlag(LOG_TYPE.Threat))
            {
                retVal = LOG_TYPE.Threat;
            }
            else if (pExceptionType.HasFlag(LOG_TYPE.Warning))
            {
                retVal = LOG_TYPE.Warning;
            }
            else
            {
                retVal = pExceptionType;
            }

            return retVal;

        }

        /// <summary>
        /// Tells the caller if this instance is already being disposed.
        /// 
        /// </summary>
        /// <returns>Returns true if the Object is being disposed, false if not.</returns>
        public Boolean IsDisposing
        {
            get
            {
                return m_blnDisposeHasBeenCalled;
            }
        }

        /// <summary>
        /// Method to look at the log files, and delete those older 
        /// than "DaysToRetainLogs".  
        /// 
        /// If "DaysToRetainLogs" is set as zero or less, then no 
        /// logs are deleted.
        /// </summary>
        private void ProcessLogRetention()
        {
            DateTime dtmFileLastWrite;

            Int32 lngDaysDiff = 0;

            String[] aryLogFiles = null;

            AzureFileClient azureFileClient = null;

            try
            {
                if (m_DaysToRetainLogs > 0)
                {

                    if (m_DBEnabled)
                    {

                        if (m_DAC != null)
                        {
                            m_DAC.ProcessLogRetention(m_DaysToRetainLogs);
                        }

                    }
                    else
                    {
                        if (m_UseAzureFileStorage)
                        {
                            azureFileClient = new AzureFileClient(m_AzureStorageResourceID,
                                                                  m_AzureStorageFileShareName,
                                                                  m_AzureStorageDirectory,
                                                                  m_AzureRemoteFileName,
                                                                  m_strDebugLogFileName);

                            List<ShareFileItem> fileList = azureFileClient.GetListOfFiles(m_LogFileNamePrefix);

                            if (m_DaysToRetainLogs > 0)
                            {
                                if (fileList != null)
                                {
                                    if (fileList.Count > 0)
                                    {

                                        foreach (ShareFileItem fileShare in fileList)
                                        {
                                            dtmFileLastWrite = (fileShare.Properties.LastModified ?? new DateTimeOffset(DateTime.Now)).DateTime;

                                            lngDaysDiff = (DateTime.Now.Date - dtmFileLastWrite.Date).Days;

                                            if (lngDaysDiff >= m_DaysToRetainLogs)
                                            {
                                                try
                                                {
                                                    azureFileClient.DeleteAzureLogFile(fileShare.Name);
                                                }
                                                catch (Exception ex)
                                                {

                                                    String strErrorMessage = ex.GetFullExceptionMessage(true, true);

                                                    String strLogMessage = String.Format(Properties.Resources.PROCESSLOGRETENTION_FILEDELETE_MSG, fileShare.Name, strErrorMessage);

                                                    String emerglogFileName = m_LogPath + m_EmergencyLogPrefixName + CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now) + ".log";

                                                    CommonHelpers.WriteToLog(emerglogFileName, strLogMessage, "");

                                                }  // END catch (Exception ex)

                                            }  // END if (CommonItems.DateDiff(CommonItems.RMDateIntervalEnum.DIDay, File.GetLastWriteTime(m_strLogPathFile + @"\" + strFileName), DateTime.Now) >= 14)

                                        }  // END foreach (ShareFileItem fileShare in fileList)

                                    }  // END if (fileList.Count > 0)

                                } // END if (fileList != null)

                            }  // END if (m_DaysToRetainLogs > 0)
                        }
                        else
                        {

                            aryLogFiles = Directory.GetFiles(m_LogPath, m_LogFileNamePrefix + "*.log", SearchOption.TopDirectoryOnly);

                            if (m_DaysToRetainLogs > 0)
                            {
                                if (aryLogFiles != null)
                                {
                                    if (aryLogFiles.Length > 0)
                                    {

                                        FileInfo objFileInfo = null;

                                        foreach (String strFileName in aryLogFiles)
                                        {
                                            objFileInfo = new FileInfo(strFileName);

                                            dtmFileLastWrite = objFileInfo.LastWriteTime;

                                            objFileInfo = null;

                                            lngDaysDiff = (DateTime.Now.Date - dtmFileLastWrite.Date).Days;

                                            if (lngDaysDiff >= m_DaysToRetainLogs)
                                            {
                                                try
                                                {
                                                    File.Delete(strFileName);
                                                }
                                                catch (Exception ex)
                                                {

                                                    String strErrorMessage = ex.GetFullExceptionMessage(true, true);

                                                    String strLogMessage = String.Format(Properties.Resources.PROCESSLOGRETENTION_FILEDELETE_MSG, strFileName, strErrorMessage);

                                                    String emerglogFileName = m_LogPath + m_EmergencyLogPrefixName + CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now) + ".log";

                                                    CommonHelpers.WriteToLog(emerglogFileName, strLogMessage, "");

                                                }  // END catch (Exception ex)

                                            }  // END if (CommonItems.DateDiff(CommonItems.RMDateIntervalEnum.DIDay, File.GetLastWriteTime(m_strLogPathFile + @"\" + strFileName), DateTime.Now) >= 14)

                                        }  // END foreach (String strFileName in aryLogFiles)

                                    }  // END if (aryLogFiles.Length > 0)

                                }  // END if (aryLogFiles != null)

                            }  // END if (m_lngLogRetention > 0)

                        }

                    }  // END else of [if (m_DBEnabled)]

                } // END if (m_DaysToRetainLogs > 0)

            }  // END try

            catch (Exception ex)
            {
                String strLogRetentionMessage = "";

                strLogRetentionMessage = String.Format(Properties.Resources.PROCESSLOGRETENTION_UNHANDLED_MSG,
                                                        m_DaysToRetainLogs.ToString(),
                                                        ex.GetFullExceptionMessage(true, true));

                // Log the error that precipitated this                    
                CommonHelpers.WriteToLog("", strLogRetentionMessage, "");

            }  // END catch
            finally
            {
                azureFileClient = null;
            }

        }


        /// <summary>
        /// Creates the debug log file.  The file has a specific naming convention.
        /// Some system information is recorded in the first entries of the debug log.
        /// </summary>
        private void CreateDebugLogFile()
        {

            String strFileStartDate = "";

            String strDebugFileName = "";

            AzureFileClient azClient = null;

            try
            {

                ProcessLogRetention();

                if (m_DBEnabled)
                {
                    // Add header to file.  No need to add to DB.
                    WriteHeaderToLog();
                    InitialWriteToDebugLogEntries();
                }
                else
                {
                    m_strDateLogFileCreated = DateTime.Now.ToShortDateString();

                    strFileStartDate = CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now);

                    strDebugFileName = m_LogFileNamePrefix + "_" + strFileStartDate + LOG_FILE_TEMP_SUFFIX;

                    if (m_LogPath.EndsWith(@"\"))
                    {
                        m_strDebugLogFileName = m_LogPath + strDebugFileName;
                    }
                    else
                    {
                        m_strDebugLogFileName = m_LogPath + @"\" + strDebugFileName;
                    }

                    if (!File.Exists(m_strDebugLogFileName))
                    {
                        // Add header to file.  No need to add to DB.
                        WriteHeaderToLog();

                        InitialWriteToDebugLogEntries();

                    }  // END if (!File.Exists(m_strDebugLogFileName))

                    if (m_UseAzureFileStorage)
                    {
                        m_AzureRemoteFileName = strDebugFileName;

                        azClient = new AzureFileClient(m_AzureStorageResourceID,
                                                        m_AzureStorageFileShareName,
                                                        m_AzureStorageDirectory,
                                                        m_AzureRemoteFileName,
                                                        m_strDebugLogFileName);

                        if (!azClient.DoesAzureStorageExist())
                        {
                            azClient.CreateShareAndDirectory();
                        }

                        // Copy the new file to Azure
                        // It will be deleted when closed.
                        azClient.CopyLogFileToRemote();
                    }


                }
            }  // END try
            catch (Exception ex)
            {
                // We couldn't open a file, so this goes in the Event Viewer.
                // Log the error that precipitated this
                String emerglogFileName = m_LogPath + m_EmergencyLogPrefixName + CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now) + ".log";

                CommonHelpers.WriteToLog(emerglogFileName,
                                         Properties.Resources.UNABLE_TO_OPEN_DEBUG_LOG_TITLE,
                                         ex.GetFullExceptionMessage(true, true));


            }  // END catch
            finally
            {
                azClient = null;
            }

        }  // END private void CreateDebugLogFile()


        /// <summary>
        /// 
        /// </summary>
        internal void InitialWriteToDebugLogEntries()
        {

            try
            {

                WriteToLog(LOG_TYPE.Startup, "Default Debug Options Bitset Value: [" + m_DebugLogOptions.ToString("X") + "].", "");

                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_COMPUTERNAME, CommonHelpers.GetDNSName()), "");

                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_FULLCOMPUTERNAME, CommonHelpers.FullComputerName), "");

                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_COMPUTERDOMAINNAME, CommonHelpers.GetComputerDomainName()), "");

                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_IS_IN_DOMAIN, CommonHelpers.IsInDomain().ToString()), "");

                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_DST, CommonHelpers.IsDaylightSavingsTime().ToString()), "");

                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_NUM_PROCS, Environment.ProcessorCount.ToString()), "");

                String AvailRAM = CommonHelpers.AvailableRAMinMB().ToString("N0") + " MB.";

                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_AVAIL_RAM, AvailRAM), "");

                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_CURR_TZ, CommonHelpers.CurrentTimeZoneName), "");

                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_CURR_TZ_DS, CommonHelpers.CurrentTimeZoneDaylightName), "");

                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_RUNNING_IN_IDE, CommonHelpers.AmIRunningInTheIDE.ToString()), "");

                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_COMMAND_LINE, Environment.CommandLine), "");

                if (Environment.Is64BitOperatingSystem)
                {
                    WriteToLog(LOG_TYPE.Startup, "Running on 64 bit OS", "");
                }
                else
                {
                    WriteToLog(LOG_TYPE.Startup, "Running on 32 bit OS", "");

                }

                if (Environment.Is64BitProcess)
                {
                    WriteToLog(LOG_TYPE.Startup, "Running in 64 bit process", "");
                }
                else
                {
                    WriteToLog(LOG_TYPE.Startup, "Running in 32 bit process", "");

                }

                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_OS_VERSION, Environment.OSVersion.VersionString), "");
                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_OS_PLATFORM, Environment.OSVersion.Platform.ToString()), "");
                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_OS_SERVICE_PACK, Environment.OSVersion.ServicePack.ToString()), "");
                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_SYS_DIR, Environment.SystemDirectory), "");
                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_USR_INTERACTIVE, Environment.UserInteractive.ToString()), "");
                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_CLR_VERSION, Environment.Version.ToString()), "");

                String strHostName = Dns.GetHostName();

                WriteToLog(LOG_TYPE.Startup, String.Format(Properties.Resources.LOG_DNS_HOST_NAME, strHostName), "");

                IPHostEntry objIPEntry = Dns.GetHostEntry(strHostName);

                if (objIPEntry != null)
                {
                    WriteToLog(LOG_TYPE.Startup, "IP Address Information for [" + strHostName + "] (reported by DNS)", "");

                    IPAddress[] objIPAddress = objIPEntry.AddressList;

                    for (int i = 0; i < objIPAddress.Length; i++)
                    {
                        if (!IPAddress.IsLoopback(objIPAddress[i]))
                        {
                            if (objIPAddress[i].ToString().Contains("::"))
                            {
                                WriteToLog(LOG_TYPE.Startup, String.Format("IPv6 Address {0}: {1} ", i, objIPAddress[i].ToString()), "");
                            }
                            else
                            {
                                WriteToLog(LOG_TYPE.Startup, String.Format("IPv4 Address {0}: {1} ", i, objIPAddress[i].ToString()), "");
                            }

                        }
                    }

                }
                else
                {
                    WriteToLog(LOG_TYPE.Startup, "Unable to resolve [" + strHostName + "] with DNS.", "");
                }


                NetworkInterface[] objNICs = NetworkInterface.GetAllNetworkInterfaces();

                if (objNICs.Length > 0)
                {
                    WriteToLog(LOG_TYPE.Startup, "Network Interface Card Information for [" + strHostName + "] (operational NICs only)", "");

                    foreach (NetworkInterface objNIC in objNICs)
                    {
                        if (objNIC.OperationalStatus == OperationalStatus.Up)
                        {
                            WriteToLog(LOG_TYPE.Startup, "NIC Name = [" + objNIC.Name + "]", "");

                            WriteToLog(LOG_TYPE.Startup, "    Description = [" + objNIC.Description + "]", "");

                            WriteToLog(LOG_TYPE.Startup, "    MAC Address = [" + objNIC.GetPhysicalAddress().ToString() + "]", "");

                            WriteToLog(LOG_TYPE.Startup, "    Interface Type = [" + objNIC.NetworkInterfaceType.ToString() + "]", "");

                            WriteToLog(LOG_TYPE.Startup, "    Operational Status = [" + objNIC.OperationalStatus.ToString() + "]", "");

                            WriteToLog(LOG_TYPE.Startup, "    Speed = [" + (objNIC.Speed / 1000000).ToString() + " mbps]", "");

                            WriteToLog(LOG_TYPE.Startup, "    DNS Suffix = [" + objNIC.GetIPProperties().DnsSuffix + "]", "");

                            IPAddressCollection objIPs = objNIC.GetIPProperties().DnsAddresses;

                            if (objIPs.Count > 0)
                            {
                                foreach (IPAddress objIP in objIPs)
                                {
                                    if (objIP.ToString().Contains("::"))
                                    {
                                        WriteToLog(LOG_TYPE.Startup, "    DNS IPv6 Address = [" + objIP.ToString() + "]", "");
                                    }
                                    else
                                    {
                                        WriteToLog(LOG_TYPE.Startup, "    DNS IPv4 Address = [" + objIP.ToString() + "]", "");
                                    }

                                }
                            }
                            else
                            {
                                WriteToLog(LOG_TYPE.Startup, "    No DNS IP addresses.", "");
                            }

                            IPAddressCollection objWINSIPs = null;

                            if (OperatingSystem.IsMacOS())
                            {
                                WriteToLog(LOG_TYPE.Startup, "    No WINS IP addresses on MacOS.", "");
                            }
                            else
                            {
                                objWINSIPs = objNIC.GetIPProperties().WinsServersAddresses;

                                if (objWINSIPs.Count > 0)
                                {
                                    foreach (IPAddress objWINSIP in objWINSIPs)
                                    {
                                        if (objWINSIP.ToString().Contains("::"))
                                        {
                                            WriteToLog(LOG_TYPE.Startup, "    WINS IPv6 Address = [" + objWINSIP.ToString() + "]", "");
                                        }
                                        else
                                        {
                                            WriteToLog(LOG_TYPE.Startup, "    WINS IPv4 Address = [" + objWINSIP.ToString() + "]", "");
                                        }

                                    }
                                }
                                else
                                {
                                    WriteToLog(LOG_TYPE.Startup, "    No WINS IP addresses.", "");
                                }

                            }

                            GatewayIPAddressInformationCollection objGatewayIPs = objNIC.GetIPProperties().GatewayAddresses;

                            if (objGatewayIPs.Count > 0)
                            {
                                foreach (GatewayIPAddressInformation objGatewayIP in objGatewayIPs)
                                {
                                    if (objGatewayIP.Address.ToString().Contains("::"))
                                    {
                                        WriteToLog(LOG_TYPE.Startup, "    Gateway IPv6 Address = [" + objGatewayIP.Address.ToString() + "]", "");
                                    }
                                    else
                                    {
                                        WriteToLog(LOG_TYPE.Startup, "    Gateway IPv4 Address = [" + objGatewayIP.Address.ToString() + "]", "");
                                    }

                                }
                            }
                            else
                            {
                                WriteToLog(LOG_TYPE.Startup, "    No Gateway IP addresses.", "");
                            }

                            IPAddressCollection objDHCPIPs = null;

                            if (OperatingSystem.IsMacOS())
                            {
                                WriteToLog(LOG_TYPE.Startup, "    No DHCP IP addresses accessible on MacOS.", "");
                            }
                            else
                            {
                                objDHCPIPs = objNIC.GetIPProperties().DhcpServerAddresses;

                                if (objDHCPIPs.Count > 0)
                                {

                                    foreach (IPAddress objDHCPIP in objDHCPIPs)
                                    {
                                        if (objDHCPIP.ToString().Contains("::"))
                                        {
                                            WriteToLog(LOG_TYPE.Startup, "    DHCP IPv6 Address = [" + objDHCPIP.ToString() + "]", "");
                                        }
                                        else
                                        {
                                            WriteToLog(LOG_TYPE.Startup, "    DHCP IPv4 Address = [" + objDHCPIP.ToString() + "]", "");
                                        }

                                    }
                                }
                                else
                                {
                                    WriteToLog(LOG_TYPE.Startup, "    No DHCP IP addresses.", "");
                                }

                            }

                        }   //END if (objNIC.OperationalStatus == OperationalStatus.Up)

                    }  // END foreach (NetworkInterface objNIC in objNICs)

                }
                else
                {
                    WriteToLog(LOG_TYPE.Startup, "Unable to find network interface cards for [" + strHostName + "].", "");
                }


            }  // END try

            catch (Exception ex)
            {
                // We couldn't open a file, so this goes in the Event Viewer.
                // Log the error that precipitated this
                String emerglogFileName = m_LogPath + m_EmergencyLogPrefixName + CommonHelpers.GetFullDateTimeStampForFileName(DateTime.Now) + ".log";

                CommonHelpers.WriteToLog(emerglogFileName,
                                         Properties.Resources.UNABLE_TO_OPEN_DEBUG_LOG_TITLE,
                                         ex.GetFullExceptionMessage(true, true));


            }  // END catch



        }

        #region ILogger Interface
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {

            LOG_TYPE logType = LOG_TYPE.Unspecified;

            switch (logLevel)
            {
                case LogLevel.Critical:
                    logType = logType & LOG_TYPE.Fatal;
                    break;

                case LogLevel.Debug:
                    logType = logType & LOG_TYPE.System;
                    break;

                case LogLevel.Error:
                    logType = logType & LOG_TYPE.Error;
                    break;

                case LogLevel.Information:
                    logType = logType & LOG_TYPE.Informational;
                    break;

                case LogLevel.Warning:
                    logType = logType & LOG_TYPE.Warning;
                    break;

                case LogLevel.Trace:
                    logType = logType & LOG_TYPE.Flow;
                    break;

                default:
                    break;
            }

            this.WriteDebugLog(logType, exception, $"EventID = {eventId.ToString()}; State = {state.ToString()}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            Boolean retVal = false;

            LOG_TYPE logType = LOG_TYPE.Unspecified;

            switch (logLevel)
            {
                case LogLevel.Critical:
                    logType = LOG_TYPE.Fatal;
                    break;

                case LogLevel.Debug:
                    logType = LOG_TYPE.System;
                    break;

                case LogLevel.Error:
                    logType = LOG_TYPE.Error;
                    break;

                case LogLevel.Information:
                    logType = LOG_TYPE.Informational;
                    break;

                case LogLevel.Warning:
                    logType = LOG_TYPE.Warning;
                    break;

                case LogLevel.Trace:
                    logType = LOG_TYPE.Flow;
                    break;

                default:
                    logType = LOG_TYPE.System;
                    break;
            }

            retVal = ((m_DebugLogOptions & logType) == logType);

            return retVal;

        }

        /// <summary>
        /// Takes a LogLevelBitset value and converts it back to an array 
        /// of LogLevel values.
        /// </summary>
        /// <param name="logLevelBitset">The ILogger bitset you want converted.</param>
        /// <returns>LogLevel[] array.</returns>
        public static LogLevel[] ConvertILoggerBitsetToLogLevels(LogLevelBitset logLevelBitset)
        {

            LogLevel[] retVal;
            List<LogLevel> tempList = new List<LogLevel>();

            try
            {

                if ((logLevelBitset & LogLevelBitset.Critical) == LogLevelBitset.Critical)
                {
                    tempList.Add(LogLevel.Critical);
                }

                if ((logLevelBitset & LogLevelBitset.Debug) == LogLevelBitset.Debug)
                {
                    tempList.Add(LogLevel.Debug);
                }

                if ((logLevelBitset & LogLevelBitset.Information) == LogLevelBitset.Information)
                {
                    tempList.Add(LogLevel.Information);
                }

                if ((logLevelBitset & LogLevelBitset.Warning) == LogLevelBitset.Warning)
                {
                    tempList.Add(LogLevel.Warning);
                }

                if ((logLevelBitset & LogLevelBitset.Error) == LogLevelBitset.Error)
                {
                    tempList.Add(LogLevel.Error);
                }

                if ((logLevelBitset & LogLevelBitset.Trace) == LogLevelBitset.Trace)
                {
                    tempList.Add(LogLevel.Trace);
                }

                retVal = tempList.ToArray();

            }
            catch
            {
                throw;

            }

            return retVal;
        }

        /// <summary>
        /// Takes a LogLevelBitset value and converts it to a LOG_TYPE bitset.
        /// </summary>
        /// <param name="logLevelBitset">The ILogger bitset you want converted.</param>
        /// <returns>LogLevel bitset.</returns>
        public static LOG_TYPE ConvertILoggerBitsetToLogLevel(LogLevelBitset logLevelBitset)
        {

            LOG_TYPE retVal = LOG_TYPE.Unspecified;

            try
            {

                if ((logLevelBitset & LogLevelBitset.Critical) == LogLevelBitset.Critical)
                {
                    retVal |= LOG_TYPE.Fatal;
                }

                if ((logLevelBitset & LogLevelBitset.Debug) == LogLevelBitset.Debug)
                {
                    retVal |= LOG_TYPE.Test;
                }

                if ((logLevelBitset & LogLevelBitset.Information) == LogLevelBitset.Information)
                {
                    retVal |= LOG_TYPE.Informational;
                }

                if ((logLevelBitset & LogLevelBitset.Warning) == LogLevelBitset.Warning)
                {
                    retVal |= LOG_TYPE.Warning;
                }

                if ((logLevelBitset & LogLevelBitset.Error) == LogLevelBitset.Error)
                {
                    retVal |= LOG_TYPE.Error;
                }

                if ((logLevelBitset & LogLevelBitset.Trace) == LogLevelBitset.Trace)
                {
                    retVal |= LOG_TYPE.Flow;
                }

            }
            catch
            {
                throw;
            }

            return retVal;
        }

        /// <summary>
        /// This static method takes an array of the LogLevel enum values the app should support 
        /// and converts them to a bitset that determines if a log entry is called (thus reducing the 
        /// overhead of a function call that is unwanted).
        /// 
        /// The use of a bitset makes the decision to call a log method very low cost, as a bitset comparison
        /// uses much less processing, with better speed, than any other way to compare a non-flags enum in an array of values.
        /// </summary>
        /// <param name="logLevel">LOG_TYPE bitset</param>
        /// <returns>A LogLevelBitset bitset matching the array values</returns>
        public static LogLevelBitset ConvertLogLevelsToILoggerBitset(LOG_TYPE logLevel)
        {

            LogLevelBitset retVal = LogLevelBitset.None;

            try
            {

                if ((logLevel & LOG_TYPE.Fatal) == LOG_TYPE.Fatal)
                {
                    retVal |= LogLevelBitset.Critical;
                }

                if ((logLevel & LOG_TYPE.Test) == LOG_TYPE.Test)
                {
                    retVal |= LogLevelBitset.Debug;
                }

                if ((logLevel & LOG_TYPE.Error) == LOG_TYPE.Error)
                {
                    retVal |= LogLevelBitset.Error;
                }

                if ((logLevel & LOG_TYPE.Informational) == LOG_TYPE.Informational)
                {
                    retVal |= LogLevelBitset.Information;
                }

                if ((logLevel & LOG_TYPE.Flow) == LOG_TYPE.Flow)
                {
                    retVal |= LogLevelBitset.Trace;
                }

                if ((logLevel & LOG_TYPE.Warning) == LOG_TYPE.Warning)
                {
                    retVal |= LogLevelBitset.Warning;
                }

            }
            catch
            {
                throw;

            }

            return retVal;
        }





        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            this.StartLog();

            return (IDisposable)this;
        }

        #endregion ILogger Interface


        #region IDisposable Implementation

        /// <summary>
        /// Implement the IDisposable.Dispose() method
        /// Developers are supposed to call this method when done with this Object.
        /// There is no guarantee when or if the GC will call it, so 
        /// the developer is responsible to.  GC does NOT clean up unmanaged 
        /// resources, such as COM objects, so we have to clean those up, too.
        /// 
        /// </summary>
        public void Dispose()
        {
            try
            {
                // Check if Dispose has already been called 
                // Only allow the consumer to call it once with effect.
                if (!m_blnDisposeHasBeenCalled)
                {
                    // Call the overridden Dispose method that contains common cleanup code
                    // Pass true to indicate that it is called from Dispose
                    Dispose(true);

                    // Prevent subsequent finalization of this Object. This is not needed 
                    // because managed and unmanaged resources have been explicitly released
                    GC.SuppressFinalize(this);
                }
            }

            catch (Exception exUnhandled)
            {
                exUnhandled.Data.Add("m_blnDisposeHasBeenCalled", m_blnDisposeHasBeenCalled.ToString());

                throw;

            }
        }

        /// <summary>
        /// Explicit Finalize method.  The GC calls Finalize, if it is called.
        /// There are times when the GC will fail to call Finalize, which is why it is up to 
        /// the developer to call Dispose() from the consumer Object.
        /// </summary>
        ~Logger()
        {
            // Call Dispose indicating that this is not coming from the public
            // dispose method.
            Dispose(false);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        public void Dispose(Boolean disposing)
        {

            try
            {

                // Here we dispose and clean up the unmanaged objects and managed Object we created in code
                // that are not in the IContainer child Object of this object.
                // Unmanaged objects do not have a Dispose() method, so we just set them to null
                // to release the reference.  For managed objects, we call their respective Dispose()
                // methods and then release the reference.
                // DEVELOPER NOTE:
                //if (m_obj != null)
                //    {
                //    m_obj = null;
                //    }


                // Set the flag that Dispose has been called and executed.
                m_blnDisposeHasBeenCalled = true;

                StopLog();

            }

            catch (Exception exUnhandled)
            {

                exUnhandled.Data.Add("m_blnDisposeHasBeenCalled", m_blnDisposeHasBeenCalled.ToString());

                throw;


            }
        }
        #endregion IDisposable Implementation

    }
}
