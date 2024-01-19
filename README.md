JLogger
=======

Overview
--------

JLogger is a singleton component as a .NET 6 library component that
can be used with any .NET project that supports .NET 6.

JLogger has these characteristics:

-   Write to a file, or write to a database table (SQL Server scripts to create the table and stored procedures is included).

-   Multithreaded use – As a singleton, it is accessible from any thread, and
    uses locking techniques to ensure there are no collisions.

-   High throughput – if the log is being used by many threads concurrently, the
    log writes do not stop the calling thread. JLogger uses a first-in,
    first-out (FIFO) queue where log writes are put in a queue and written to a
    file in a separate thread, concurrently in the background. The WriteDebugLog
    command takes the parameters, creates the log data, puts it in a queue. None
    of those steps are blocking.

-   Send an Email – A debug log write can optionally send an email (SMTP
    configuration data required)

-   Multiple Log Entry Types – there are several log entry types to choose from.
    What they each mean is up to the user writing the code. Some log types are
    reserved for the component, and would be ignored in processing the log
    entry. These are detailed below.

-   New Log File each Day – after midnight, a new log file is created so log
    files are named to show the date and time span the log was active.

-   Log Retention – logs are automatically removed after the specified number of
    days, unless zero is specified, in which case no log files are deleted.
	Retnention also applies to log files stored in Azure.

-   Tab-delimited Log File – the log is written as a tab-delimited file. This
    enables opening up the file in programs like Excel for analysis.
	
-   Azure - can optionally store the log files in Azure.  The log file is created
	locally as a temporary file (better performance for local writes) then when the 
	log closes, it is copied to the specified Azure file storage and deleted locally.

LOG_TYPE Enum
-------------

The enum contains values for the types of logs, and for how the logs are created
and managed.

### LOG_TYPE Values for Logging

Unspecified - Used as a default value until an assignment is made.

Flow – Used to denote a log entry that can be used to trace program flow in the
log.

Error – Used to denote serious exceptions that generally require follow-up and
fixing.

Informational – Denotes the log entry is for information only.

Warning – Means the log entry is warning about a potentially serious condition.

System – Log entry relates to system data.

Performance – Log entry that usually shows elapsed time (as placed in the log
message by the coder) and/or start time.

Test – Used to indicate the log entry was intended for test results.

SendEmail – Used in the WriteDebugLog LOG_TYPE variable to specify that this log
entry should also send an email. The email is only sent if the same flag is used
in Log Management. Use of this flag here only applies to the specific log entry,
not the entire log.

Database - Log entry related to database operations

Service - Log entry related to service operations

Cloud - Log entry related to cloud operations

Management - Log entry related to management concerns or operations

Fatal - Log entry related to some fatal operation or state

Network - Log entry related to network issue or operation

Threat - Log entry related to a threat condition

### LOG_TYPE Values for Log Management

ShowModuleMethodAndLineNumber – Tells JLogger to include any available values
for what module name, method name, and line number the exception or log entry
was made. This is very useful for finding and correcting bugs in development,
quality assurance, and production code.

ShowTimeOnly - Shows time only, not date, in the debug log. Useful since debug
logs are closed and a new one created on the first write the next day after the
log file was opened. Do not use this flag if you want each log entry to show
date and time.

HideThreadID - Hides the thread ID from being printed in the debug log.

IncludeStackTrace – Writes the stack trace to the debug log. Otherwise, leaves
that column blank.

SendEmail – This is used if sending an email from a log entry that also uses
SendMail. The flag, when used in management (DebugLogOptions) enables sending
email if the log entry also calls for it by use of this flag. If this flag is
not set, the use of SendMail in a specific log entry is ignored, and no email is
sent. This allows globally turning email sends on and off by simply changing the
DebugLogOptions of the Logger instance.

IncludeExceptionData – This tells the JLogger instance to examine the Data
collection on all Exceptions, and log any name-value pairs it finds there. The
Exception.Data collection is often used in catch blocks to add real time values
to the exception before executing a “throw”. Use of the Data collection for this
saves much time in troubleshooting.

Example Code
------------

These lines of code are used to illustrate the use of JLogger. There are more
variations than documentation can show, but this shows a fully functioning use
of JLogger.

// Usings

using Jeff.Jones.JLogger;

using Jeff.Jones.JHelpers;

// Setting a class-wide variable. What you set may

// be different for development, QA, production, and troubleshooting production.

// This global value for the program is usually stored in some

// configuration data location.

LOG_TYPE m_DebugLogOptions = LOG_TYPE.Error \| LOG_TYPE.Informational \|

LOG_TYPE.ShowTimeOnly \|

LOG_TYPE.Warning \|

LOG_TYPE.HideThreadID \|

LOG_TYPE.ShowModuleMethodAndLineNumber \|

LOG_TYPE.System \|

LOG_TYPE.SendEmail;

// Setting variables used to configure the Logger

// Typically in the programs startup code, as early as possible.

Boolean response = false;

String filePath = CommonHelpers.CurDir + \@"\\";

String fileNamePrefix = "MyLog";
// This value applies to both debug files and to DB log entries.
Int32 daysToRetainLogs = 30;

// Setting the Logger data so it knows how to build a log file, and

// how long to keep them. The initial debug log options is set here,

// and can be changed programmatically at anytime in the

// Logger.Instance.DebugLogOptions property.

response = Logger.Instance.SetLogData(filePath, fileNamePrefix,
daysToRetainLogs, logOptions, "");

// These lines show how to setup the DB-based logging.  The T-SQL script 

// for the DBLog table and the two stored procedures must be executed

// on the database where you want the log entries.

// If using Windows Authentication for access to your DB, make sure 

// the windows account has the necessary permissions on SQL Server, and 

// you can leave the DBUserName and DBPassword as "".

response = Logger.Instance.SetDBConfiguration("server_instance_name",
											  "DBUserName",
											  "DBPassword",
											  UseWindowsAuthentication (true/false),
											  EnableDBLogging (true/false),
											  "DBName");

// These next lines may be omitted if not sending email from your log.

// Email setup.

Int32 smtpPort = 587;

Boolean useSSL = true;

List\<String\> sendToAddresses = new List\<String\>();

sendToAddresses.Add("MyBuddy\@somewhere.net");

sendToAddresses.Add("John.Smith\@anywhere.net");

response = Logger.Instance.SetEmailData("smtp.mymailserver.net",

"logonEmailAddress\@work.net",

"logonEmailPassword",

smtpPort,

sendToAddresses,

"emailFromAddress\@work.net",

"<emailReplyToAddress@work.net>",

>   useSSL);

// End of email setup.

// Optional configuration for Azure file storage
String resourceID = "<AZURE_CONNECTION_STRING>";
String fileShareName = "<AZURE_FILE_SHARE_NAME>";
String directoryName = "<AZURE_DIRECTORY_NAME>";
response = Logger.Instance.SetAzureConfiguration(resourceID, fileShareName, directoryName, true);



// This starts the log operation AFTER you have set the initial parameters.

response = Logger.Instance.StartLog();

// This ends the configuration example

// Example of use in a method

void SomeMethod()

{

// Use of the Flow LOG_TYPE shows in the log when a method was entered,

// and exited. Useful for debugging, QA, and development. The Flow bit

// mask is usually turned off in production to reduce log size.

if ((m_DebugLogOptions & LOG_TYPE.Flow) == LOG_EXCEPTION_TYPE.Flow)

{

Logger.Instance.WriteToDebugLog(LOG_TYPE.Flow,

"1st line in method",

“”);

}

// This variable notes when the method started.

DateTime methodStart = DateTime.Now;

try

{

// Do some work here

// This is an example of logging used during

// process flow. The bitmask used here does not

// have to be “Informational”, and may be turned

// off in production.

Logger.Instance.WriteToDebugLog(LOG_TYPE.Informational,

"Primary message",

"Optional detail message");

// Do some more work

}

catch (Exception exUnhandled)

{

// Capture some runtime data that may be useful in debugging.

exUnhandled.Data.Add(“SomeName”, “SomeValue”);

if ((m_DebugLogOptions & LOG_TYPE.Error) == LOG_TYPE.Error)

{

Logger.Instance.WriteToDebugLog(LOG_TYPE.Error,

exUnhandled,

"Optional detail message");

}

}

finally

{

if ((m_DebugLogOptions & LOG_TYPE.Performance) == LOG_TYPE.Performance)

{

TimeSpan elapsedTime = DateTime.Now - methodStart;

Logger.Instance.WriteToDebugLog(LOG_TYPE.Performance,

String.Format("END; elapsed time = [{0:mm} mins,

{0:ss} secs, {0:fff} msecs].", objElapsedTime));

}

// Capture the flow for exiting the method.

if ((m_DebugLogOptions & LOG_TYPE.Flow) == LOG_EXCEPTION_TYPE.Flow)

{

Logger.Instance.WriteToDebugLog(LOG_TYPE.Flow, "Exiting method", “”);

}

} // END of method

Logger Methods and Properties
-----------------------------

### Static 

DEFAULT_DEBUG_LOG_OPTIONS – constant with manufacturer recommend initial
settings. You do NOT have to use this and can build your own that resides in
your program.

DEFAULT_LOG_RETENTION – constant that is the default for how many days log files
are retained. You can use your own value and do NOT have to use this one.

LOG_CACHE_FREQUENCY – constant for how long between attempts to write the log
queue to the log file, in milliseconds.

### Instance

DaysToRetainLogs – (Get/Set) - How many days that the Logger instance retains
previous log files.

DebugLogOptions – (Get/Set) - The debug flags that are active during the
lifetime of the Logger instance

Dispose() – Implement the IDisposable.Dispose() method. Developers are supposed
to call this method when done with this Object. There is no guarantee when or if
the GC will call it, so the developer is responsible to. GC does NOT clean up
unmanaged resources, such as COM objects, so we have to clean those up, too.
There are no COM objects used in JLogger.

EmailEnabled – (Get ONLY) - True if sending email is enabled globally, false if
off globally. Email sending is set by the LOG_TYPE.SendMail bit being on or off
in DebugLogOptions,

EmailLogonName - (Get/Set) - The logon name expected by the SMTP email server.

EmailPassword - (Get/Set) - The logon password expected by the SMTP email
server.

EmailServer - (Get/Set) - The IP address or DNS name of the outgoing mail server

FromAddress - (Get/Set) - The email address to use with sending emails to
indicate who the email is from.

IsDisposing – (Get ONLY) - Tells the caller if this instance is already being
disposed. Returns true if the JLogger instance is being disposed, false if not.

LogFileName – (Get ONLY) - Fully qualified file name for the log file.

LogPath - (Get/Set) - Fully qualified path for the log file.

ReplyToAddress - (Get/Set) - The email address used to tell the recipient what
address to reply to.

SendToAddresses – (Get ONLY List\<String\>, but List\<String\> still supports
Add and other functionality.  
List\<String\> cannot be “set” as a List\<String\> object – internal creation
only.) -  
These are the email addresses for log emails to be sent to.

Boolean SetEmailData(String emailServer,  
String emailLogonName,  
String emailPassword,  
Int32 smtpPort,  
List\<String\> sendToAddresses,  
String fromAddress,  
String replyToAddress,

Boolean useSSL) – Configure the email send functionality.

Boolean SetLogData(String logPath,  
String logFileNamePrefix,  
Int32 daysToRetainLogs,  
LOG_TYPE debugLogOptions,  
String emergencyLogPrefixName = DEFAULT_EMERG_LOG_PREFIX) –  
Configures the Logger for logging before starting the log.

Boolean SetAzureConfiguration(String azureStorageResourceID,
String azureStorageFileShareName,
String azureStorageDirectory,
Boolean useAzureFileStorage)

SMTPPort – (Get/Set) - The port that the SMTP email server listens on.

Boolean StartLog() – Once the Logger instance is configured, this is used to
start logging.

Boolean StopLog() – When the Logger instance is running, this is used to stop
logging.

UseSSL – (Get/Set) - True if the email server requires using SSL, false if not.

Boolean WriteDebugLog(LOG_TYPE pExceptionType,  
Exception pExceptionToUse,  
String pOptionalLogMessage) - Method used to write exception information to the
log. This method writes a DebugLogItem instance to a queue, which is then
emptied FIFO on a separate thread so calling this method does not block main
thread activity.

Boolean WriteDebugLog(LOG_TYPE pExceptionType,  
String message,  
String secondaryMessage = "") - Method used to write message information to the
log. This method writes a DebugLogItem instance to a queue, which is then
emptied FIFO on a separate thread so calling this method does not block main
thread activity.
