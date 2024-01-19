using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Jeff.Jones.JLogger8
{
	/// <summary>
	/// This class represents a single debug log entry.  These are used
	/// as values in the debug log queue (m_dctLogQueue)
	/// </summary>
	[Serializable()]
	public class DebugLogItem
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public DebugLogItem()
		{
			this.LogDateTime = default;

			this.Type = LOG_TYPE.Unspecified;

			this.TypeDescription = this.Type.ToString().Replace(",", "");

			this.DetailMessage = "";

			this.Message = "";

			this.ModuleName = "";

			this.MethodName = "";

			this.LineNumber = 0;

			this.ThreadID = 0;

			this.ExceptionData = "";

			this.StackData = "";

		}


		/// <summary>
		/// Constructor to populate the item
		/// </summary>
		/// <param name="lngType">Type of log entry</param>
		/// <param name="dtmLogDateTime">Date-Time of the log entry</param>
		/// <param name="strLogMessage">Main message</param>
		/// <param name="pDetailMessage">Secondary or detail message</param>
		/// <param name="pExceptionData">Exception.Data information as a single string</param>
		/// <param name="pStackData">Data pulled from the stack trace.</param>
		/// <param name="moduleName">Module name where the log call was made.</param>
		/// <param name="methodName">Method name where the log call was made.</param>
		/// <param name="lineNumber">Line number of the exception or where the call was made.</param>
		/// <param name="threadID">The .NET thread ID where the log call was made.</param>
		public DebugLogItem(LOG_TYPE lngType,
							DateTime dtmLogDateTime,
							String strLogMessage,
							String pDetailMessage,
							String pExceptionData,
							String pStackData,
							String moduleName,
							String methodName,
							Int32 lineNumber,
							Int32 threadID)
		{

			this.LogDateTime = dtmLogDateTime;

			this.Type = lngType;

			this.TypeDescription = lngType.ToString().Replace(",", "");

			this.DetailMessage = pDetailMessage;

			this.Message = strLogMessage;

			this.ModuleName = moduleName;

			this.MethodName = methodName;

			this.LineNumber = lineNumber;

			this.ThreadID = threadID;

			this.ExceptionData = pExceptionData;

			this.StackData = pStackData;

		}  // END public DebugLogItem(...)

		/// <summary>
		/// Date-Time of the log entry
		/// </summary>
		public DateTime LogDateTime
		{
			get;set;
		}

		/// <summary>
		/// Type of log entry
		/// </summary>
		public LOG_TYPE Type
		{
			get; set; 
		}

		/// <summary>
		/// String name of the log type (Type).
		/// </summary>
		public String TypeDescription
		{
			get; set;
		}

		/// <summary>
		/// Main message
		/// </summary>
		public String Message
		{
			get; set;
		}

		/// <summary>
		/// Secondary or detail message
		/// </summary>
		public String DetailMessage
		{
			get; set;
		}

		/// <summary>
		/// Module name where the log call was made.
		/// </summary>
		public String ModuleName
		{
			get; set;
		}

		/// <summary>
		/// Method name where the log call was made.
		/// </summary>
		public String MethodName
		{
			get; set;
		}

		/// <summary>
		/// Line number of the exception or where the call was made.
		/// </summary>
		public Int32 LineNumber
		{
			get; set;
		}

		/// <summary>
		/// The .NET thread ID where the log call was made.
		/// </summary>
		public Int32 ThreadID
		{
			get; set;
		}

		/// <summary>
		/// Exception.Data information as a single string
		/// </summary>
		public String ExceptionData
		{
			get; set;
		}

		/// <summary>
		/// Data pulled from the stack trace.
		/// </summary>
		public String StackData
		{
			get; set;
		}

	}  // END public class DebugLogItem
}
