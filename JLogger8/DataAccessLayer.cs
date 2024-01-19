using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Jeff.Jones.JHelpers8;

namespace Jeff.Jones.JLogger8
{
	/// <summary>
	/// 
	/// </summary>
	internal class DataAccessLayer
	{

		private String m_Server = "";
		private String m_DefaultDB = "";
		private Boolean m_UseAuthentication = false;
		private String m_UserName = "";
		private String m_Password = "";
		private Int32 m_ConnectionTimeout = 10;
		private Int32 m_CommandTimeout = 20;
		private Int32 m_PortNumber = 1433;
		private Int32 m_ConnectRetryCount = 3;
		private Int32 m_ConnectRetryInterval = 10;
		private String m_ApplicationName = "";
		private String m_WorkstationID = "";
		private Boolean m_ConnectionPooling = true;

		/// <summary>
		/// Constructor to populate the instance.
		/// </summary>
		/// <param name="server"></param>
		/// <param name="defaultDB"></param>
		/// <param name="useAuthentication"></param>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <param name="connectionTimeout"></param>
		/// <param name="commandTimeout"></param>
		/// <param name="connectRetryCount"></param>
		/// <param name="connectRetryInterval"></param>
		/// <param name="applicationName"></param>
		/// <param name="workstationID"></param>
		/// <param name="portNumber"></param>
		/// <param name="connectionPooling"></param>
		public DataAccessLayer(String server,
							   String defaultDB,
							   Boolean useAuthentication,
							   String username,
							   String password,
							   Int32 connectionTimeout,
							   Int32 commandTimeout,
							   Int32 connectRetryCount = 3,
							   Int32 connectRetryInterval = 10,
							   String applicationName = "",
							   String workstationID = "",
							   Int32 portNumber = 1433,
							   Boolean connectionPooling = true)
		{
			m_Server = server ?? "";
			m_DefaultDB = defaultDB ?? "";
			m_UseAuthentication = useAuthentication;
			m_UserName = username ?? "";
			m_Password = password ?? "";
			m_ConnectionTimeout = connectionTimeout;
			m_CommandTimeout = commandTimeout;
			m_ConnectRetryCount = connectRetryCount;
			m_ConnectRetryInterval = connectRetryInterval;

			if (portNumber <= 0)
			{
				m_PortNumber = 1433;
			}
			else
			{
				m_PortNumber = portNumber;
			}

			m_WorkstationID = workstationID ?? "";
			m_ApplicationName = applicationName ?? "";
			m_ConnectionPooling = connectionPooling;

		}

		/// <summary>
		/// Method to build a SQL Server connection string.
		/// </summary>
		/// <returns>Fully formed connection string</returns>
		private String BuildConnectionString()
		{

			String retVal = "";

			String server = "";

			SqlConnectionStringBuilder sqlSB = null;


			try
			{
				if (m_PortNumber <= 0)
				{
					m_PortNumber = 1433;
				}

				if (m_PortNumber != 1433)
				{
					server = m_Server + ":" + m_PortNumber.ToString();
				}
				else
				{
					server = m_Server;
				}

				sqlSB = new SqlConnectionStringBuilder
				{
					ConnectRetryCount = m_ConnectRetryCount,
					ConnectRetryInterval = m_ConnectRetryInterval,
					ApplicationName = m_ApplicationName,
					ConnectTimeout = m_ConnectionTimeout,
					DataSource = server,
					InitialCatalog = m_DefaultDB,
					IntegratedSecurity = m_UseAuthentication,
					Password = m_Password,
					Pooling = m_ConnectionPooling,
					UserID = m_UserName,
					WorkstationID = m_WorkstationID
				};

				retVal = sqlSB.ConnectionString;

				//retVal = $"Server={server};Database={m_DefaultDB};User ID = {m_UserName}; Password={m_Password}";

			} // END try

			catch (Exception exUnhandled)
			{
				exUnhandled.Data.Add("Server", m_Server);
				exUnhandled.Data.Add("DefaultDB", m_DefaultDB);
				exUnhandled.Data.Add("UserName", m_UserName);
				exUnhandled.Data.Add("PortNumber", m_PortNumber.ToString());
				exUnhandled.Data.Add("UseAuthentication", m_UseAuthentication.ToString());
				exUnhandled.Data.Add("ConnectRetryCount", m_ConnectRetryCount.ToString());
				exUnhandled.Data.Add("ConnectRetryInterval", m_ConnectRetryInterval.ToString());
				exUnhandled.Data.Add("ApplicationName", m_ApplicationName);
				exUnhandled.Data.Add("ConnectionTimeout", m_ConnectionTimeout.ToString());
				exUnhandled.Data.Add("WorkstationID", m_WorkstationID);

				throw;

			}  // END catch (Exception eUnhandled)

			finally
			{

				if (sqlSB != null)
				{
					sqlSB.Clear();

					sqlSB = null;
				}
			}  // END finally

			return retVal;

		}  // END BuildConnectionString()

		public String ProcessLogRetention(Int32 daysToRetainLogs)
		{

			List<SqlParameter> sqlParams = null;

			DBReturnValue result = null;

			String retVal = "";

			try
			{
				String spName = "spDebugLogDelete";

				DateTime dateToDelete = DateTime.Now.AddDays(-1 * daysToRetainLogs);


				sqlParams = new List<SqlParameter>();

				//@DeletionDate datetime,
				SqlParameter paramLogDateTime = new SqlParameter("DeletionDate", SqlDbType.DateTime)
				{
					Value = dateToDelete
				};
				sqlParams.Add(paramLogDateTime);

				//@RowsAffected INT = 0 OUTPUT,
				SqlParameter paramRowsAffected = new SqlParameter("RowsAffected", SqlDbType.Int)
				{
					Direction = ParameterDirection.Output
				};
				sqlParams.Add(paramRowsAffected);

				//@ErrMessage NVARCHAR(255) = '' OUTPUT   )
				SqlParameter paramErrMessage = new SqlParameter("ErrMessage", SqlDbType.NVarChar, 255)
				{
					Direction = ParameterDirection.Output
				};
				sqlParams.Add(paramErrMessage);

				//@ErrMessage NVARCHAR(255) = '' OUTPUT   )
				SqlParameter paramReturnValue = new SqlParameter("RetVal", SqlDbType.Int)
				{
					Direction = ParameterDirection.ReturnValue
				};
				sqlParams.Add(paramReturnValue);

				result = ExecuteStatement(spName, true, sqlParams);

				String errMessage = "";
				Int32 rowsAffected = 0;
				Int32 errorNum = 0;

				if (result != null)
				{
					paramErrMessage = result.SQLParams.Find(p => p.ParameterName == "ErrMessage");

					if (paramErrMessage != null)
					{
						errMessage = paramErrMessage.Value.ToString();
					}

					paramRowsAffected = result.SQLParams.Find(p => p.ParameterName == "RowsAffected");

					if (paramRowsAffected != null)
					{
						rowsAffected = (Int32)paramRowsAffected.Value;
					}

					paramReturnValue = result.SQLParams.Find(p => p.ParameterName == "RetVal");

					if (paramReturnValue != null)
					{
						errorNum = (Int32)paramReturnValue.Value;
					}

					if (errMessage.Length > 0)
					{
						retVal = $"{errMessage}; Error Number: {errorNum.ToString()}; Rows Affected: {rowsAffected.ToString()}.";
					}

				}
				else
				{
					retVal = "Nothing returned from attempted cleanup of old DBLog entries.";
				}

			}
			catch (Exception exUnhandled)
			{
				exUnhandled.Data.Add("daysToRetainLogs", daysToRetainLogs.ToString());

				retVal += exUnhandled.GetFullExceptionMessage(false, false);

			}
			finally
			{
				if (sqlParams != null)
				{
					sqlParams.Clear();

					sqlParams = null;
				}

				if (result != null)
				{
					result.Dispose();

					result = null;
				}
			}

			return retVal;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logEntry"></param>
		/// <returns></returns>
		public String WriteDBLog(DebugLogItem logEntry)
		{
			String retVal = "";

			String spName = "spDebugLogInsert";

			List<SqlParameter> sqlParams = null;

			DBReturnValue result = null;

			try
			{

				sqlParams = new List<SqlParameter>();

				//@ID bigint OUTPUT, 
				SqlParameter paramID = new SqlParameter("ID", SqlDbType.BigInt)
				{
					Direction = ParameterDirection.Output
				};
				sqlParams.Add(paramID);

				//@LogType varchar(50), 
				SqlParameter paramLogType = new SqlParameter("LogType", SqlDbType.VarChar, 50)
				{
					Value = logEntry.TypeDescription
				};
				sqlParams.Add(paramLogType);

				//@LogDateTime datetime,
				SqlParameter paramLogDateTime = new SqlParameter("LogDateTime", SqlDbType.DateTime)
				{
					Value = logEntry.LogDateTime
				};
				sqlParams.Add(paramLogDateTime);

				//@LogMessage         nvarchar(MAX), 
				SqlParameter paramLogMessage = new SqlParameter("LogMessage", SqlDbType.NVarChar)
				{
					Value = logEntry.Message
				};
				sqlParams.Add(paramLogMessage);

				//@DetailMessage nvarchar(MAX), 
				SqlParameter paramDetailMessage = new SqlParameter("DetailMessage", SqlDbType.NVarChar)
				{
					Value = logEntry.DetailMessage
				};
				sqlParams.Add(paramDetailMessage);

				//@ModuleName nvarchar(MAX), 
				SqlParameter paramModuleName = new SqlParameter("ModuleName", SqlDbType.NVarChar)
				{
					Value = logEntry.ModuleName
				};
				sqlParams.Add(paramModuleName);

				//@MethodName nvarchar(MAX), 
				SqlParameter paramMethodName = new SqlParameter("MethodName", SqlDbType.NVarChar)
				{
					Value = logEntry.MethodName
				};
				sqlParams.Add(paramMethodName);

				//@LineNumber         int,
				SqlParameter paramLineNumber = new SqlParameter("LineNumber", SqlDbType.Int)
				{
					Value = logEntry.LineNumber
				};
				sqlParams.Add(paramLineNumber);

				//@ThreadID           int,
				SqlParameter paramThreadID = new SqlParameter("ThreadID", SqlDbType.Int)
				{
					Value = logEntry.ThreadID
				};
				sqlParams.Add(paramThreadID);

				//@ExceptionData      nvarchar(MAX), 
				SqlParameter paramExceptionData = new SqlParameter("ExceptionData", SqlDbType.NVarChar)
				{
					Value = logEntry.ExceptionData
				};
				sqlParams.Add(paramExceptionData);

				//@StackData nvarchar(MAX), 
				SqlParameter paramStackData = new SqlParameter("StackData", SqlDbType.NVarChar)
				{
					Value = logEntry.StackData
				};
				sqlParams.Add(paramStackData);

				//@RowsAffected INT = 0 OUTPUT,
				SqlParameter paramRowsAffected = new SqlParameter("RowsAffected", SqlDbType.Int)
				{
					Direction = ParameterDirection.Output
				};
				sqlParams.Add(paramRowsAffected);

				//@ErrMessage NVARCHAR(255) = '' OUTPUT   )
				SqlParameter paramErrMessage = new SqlParameter("ErrMessage", SqlDbType.NVarChar, 255)
				{
					Direction = ParameterDirection.Output
				};
				sqlParams.Add(paramErrMessage);

				//@RetVal int
				SqlParameter paramReturnValue = new SqlParameter("RetVal", SqlDbType.Int)
				{
					Direction = ParameterDirection.ReturnValue
				};
				sqlParams.Add(paramReturnValue);

				result = ExecuteStatement(spName, true, sqlParams);

				String errMessage = "";
				Int32 rowsAffected = 0;
				Int32 errorNum = 0;

				if (result != null)
				{
					paramErrMessage = result.SQLParams.Find(p => p.ParameterName == "ErrMessage");

					if (paramErrMessage != null)
					{
						errMessage = paramErrMessage.Value.ToString();
					}

					paramRowsAffected = result.SQLParams.Find(p => p.ParameterName == "RowsAffected");

					if (paramRowsAffected != null)
					{
						rowsAffected = (Int32)paramRowsAffected.Value;
					}

					paramReturnValue = result.SQLParams.Find(p => p.ParameterName == "RetVal");

					if (paramReturnValue != null)
					{
						errorNum = (Int32)paramReturnValue.Value;
					}

					if (errMessage.Length > 0)
					{
						retVal = $"{errMessage}; Error Number: {errorNum.ToString()}; Rows Affected: {rowsAffected.ToString()}.";
					}

				}
				else
				{
					retVal = $"Nothing returned from attempted insert into log table. Date: {logEntry.LogDateTime.ToShortDateString()} {logEntry.LogDateTime.ToShortTimeString()} Message: {logEntry.Message}";
				}

			}
			catch (Exception exUnhandled)
			{
				exUnhandled.Data.Add("User", Environment.UserName);

				retVal += exUnhandled.GetFullExceptionMessage(false, false);

			}
			finally
			{
				if (sqlParams != null)
				{
					sqlParams.Clear();

					sqlParams = null;
				}


				if (result != null)
				{
					result.Dispose();

					result = null;
				}
			}

			return retVal;

		}

		/// <summary>
		/// A true/false check to see if the connection can be made.
		/// </summary>
		/// <returns></returns>
		public Boolean CheckConnection()
		{

			Boolean retVal = false;

			SqlConnection sqlConn = null;

			String connString = "";

			try
			{
				connString = BuildConnectionString();

				sqlConn = new SqlConnection(connString);

				try
				{
					sqlConn.Open();

					retVal = true;
				}
				catch (InvalidOperationException exConnOp)
				{
					exConnOp.Data.Add("Check connection", exConnOp.GetFullExceptionMessage(false, false));
					throw;
				}
				catch (SqlException exConnSql)
				{
					exConnSql.Data.Add("ConnectionString", connString);
					throw;
				}

			}  // END try
			catch (Exception exUnhandled)
			{
				exUnhandled.Data.Add("Server", m_Server);
				exUnhandled.Data.Add("DefaultDB", m_DefaultDB);
				exUnhandled.Data.Add("UserName", m_UserName);
				exUnhandled.Data.Add("PortNumber", m_PortNumber.ToString());
				exUnhandled.Data.Add("UseAuthentication", m_UseAuthentication.ToString());
				exUnhandled.Data.Add("ConnectRetryCount", m_ConnectRetryCount.ToString());
				exUnhandled.Data.Add("ConnectRetryInterval", m_ConnectRetryInterval.ToString());
				exUnhandled.Data.Add("ApplicationName", m_ApplicationName);
				exUnhandled.Data.Add("ConnectionTimeout", m_ConnectionTimeout.ToString());
				exUnhandled.Data.Add("WorkstationID", m_WorkstationID);

				throw;
			}
			finally
			{

				if (sqlConn != null)
				{
					if (sqlConn.State != ConnectionState.Closed)
					{
						sqlConn.Close();
					}

					sqlConn.Dispose();

					sqlConn = null;
				}
			}

			return retVal;

		}  // END public Boolean CheckConnection()

		/// <summary>
		/// Method to execute a query and return data.
		/// For example, query DB log table, return data for date span,
		/// abd write it to a log file.
		/// </summary>
		/// <param name="cmd">SQL Command</param>
		/// <param name="isSP">True if a stored procedure, false if not.</param>
		/// <param name="sqlParams">List of parameter objects, or null if no parameters used.</param>
		/// <returns>DBReturnValue instance with results and parameters that have post-execution values.</returns>
		public DBReturnValue ExecuteQuery(String cmd, Boolean isSP, List<SqlParameter> sqlParams)
		{
			DataSet retDS = new DataSet();

			SqlConnection sqlConn = null;

			SqlCommand sqlCmd = null;

			String connString = "";

			SqlDataAdapter sqlAdapter = null;

			DBReturnValue retVal = new DBReturnValue();

			try
			{

				connString = BuildConnectionString();

				sqlConn = new SqlConnection(connString);
				sqlCmd = sqlConn.CreateCommand();
				sqlCmd.CommandText = cmd;
				sqlCmd.CommandTimeout = m_CommandTimeout;

				if (isSP)
				{
					sqlCmd.CommandType = CommandType.StoredProcedure;
				}
				else
				{
					sqlCmd.CommandType = CommandType.Text;
				}

				if (sqlParams == null)
				{
					sqlCmd.Parameters.Clear();
				}
				else
				{
					if (sqlParams.Count > 0)
					{
						foreach (SqlParameter sqlParam in sqlParams)
						{
							sqlCmd.Parameters.Add(sqlParam);

						}  // END foreach (SqlParameter sqlParam in sqlParams)

					}  // END if (sqlParams.Count > 0)

				}

				try
				{
					sqlConn.Open();
				}
				catch (InvalidOperationException exConnOp)
				{
					exConnOp.Data.Add("SQL Command", cmd);
					throw;
				}
				catch (SqlException exConnSql)
				{
					exConnSql.Data.Add("ConnectionString", connString);
					throw;
				}

				try
				{
					retDS = new DataSet();
					sqlAdapter = new SqlDataAdapter(sqlCmd);
					retVal.RetCode = sqlAdapter.Fill(retDS);
					retVal.Data = retDS;
				}
				catch (Exception exFill)
				{
					exFill.Data.Add("Failure during fill", exFill.Message);
					throw;
				}

				if (sqlCmd.Parameters != null)
				{
					if (sqlCmd.Parameters.Count > 0)
					{
						if (sqlParams == null)
						{
							sqlParams = new List<SqlParameter>();
						}
						else
						{
							sqlParams.Clear();
						}

						foreach (SqlParameter sqlParam in sqlCmd.Parameters)
						{
							retVal.SQLParams.Add(new SqlParameter
							{
								CompareInfo = sqlParam.CompareInfo,
								DbType = sqlParam.DbType,
								Direction = sqlParam.Direction,
								IsNullable = sqlParam.IsNullable,
								LocaleId = sqlParam.LocaleId,
								Offset = sqlParam.Offset,
								ParameterName = sqlParam.ParameterName,
								Precision = sqlParam.Precision,
								Scale = sqlParam.Scale,
								Size = sqlParam.Size,
								SourceColumn = sqlParam.SourceColumn,
								SourceColumnNullMapping = sqlParam.SourceColumnNullMapping,
								SourceVersion = sqlParam.SourceVersion,
								SqlDbType = sqlParam.SqlDbType,
								SqlValue = sqlParam.SqlValue,
								TypeName = sqlParam.TypeName,
								UdtTypeName = sqlParam.UdtTypeName,
								Value = sqlParam.Value
							});
						}  // END foreach (SqlParameter sqlParam in sqlCmd.Parameters)

					}  // END if (sqlParams.Count > 0)

				}  // ENDF if (sqlParams != null)

			}  // END try
			catch (Exception exUnhandled)
			{
				exUnhandled.Data.Add("Server", m_Server);
				exUnhandled.Data.Add("DefaultDB", m_DefaultDB);
				exUnhandled.Data.Add("UserName", m_UserName);
				exUnhandled.Data.Add("PortNumber", m_PortNumber.ToString());
				exUnhandled.Data.Add("UseAuthentication", m_UseAuthentication.ToString());
				exUnhandled.Data.Add("ConnectRetryCount", m_ConnectRetryCount.ToString());
				exUnhandled.Data.Add("ConnectRetryInterval", m_ConnectRetryInterval.ToString());
				exUnhandled.Data.Add("ApplicationName", m_ApplicationName);
				exUnhandled.Data.Add("ConnectionTimeout", m_ConnectionTimeout.ToString());
				exUnhandled.Data.Add("WorkstationID", m_WorkstationID);

				retVal.ErrorMessage = exUnhandled.GetFullExceptionMessage(true, true);

				throw;
			}
			finally
			{

				if (sqlAdapter != null)
				{
					sqlAdapter.Dispose();

					sqlAdapter = null;
				}

				if (sqlCmd != null)
				{
					sqlCmd.Dispose();

					sqlCmd = null;
				}

				if (sqlConn != null)
				{
					if (sqlConn.State != ConnectionState.Closed)
					{
						sqlConn.Close();
					}

					sqlConn.Dispose();

					sqlConn = null;
				}
			}

			return retVal;
		}  // END public DBReturnValue ExecuteQuery(String cmd, Boolean isSP, List<SqlParameter> sqlParams)



		/// <summary>
		/// Asynchronous method to execute SQL that does not return a dataset.
		/// </summary>
		/// <param name="cmd">SQL Command</param>
		/// <param name="isSP">True if a stored procedure, false if not.</param>
		/// <param name="sqlParams">List of parameter objects, or null if no parameters used.</param>
		/// <returns>DBReturnValue instance with results and parameters that have post-execution values.</returns>
		public DBReturnValue ExecuteStatement(String cmd, Boolean isSP, List<SqlParameter> sqlParams)
		{

			SqlConnection sqlConn = null;

			SqlCommand sqlCmd = null;

			String connString = "";

			DBReturnValue retVal = new DBReturnValue();

			try
			{

				connString = BuildConnectionString();

				sqlConn = new SqlConnection(connString);
				sqlCmd = sqlConn.CreateCommand();
				sqlCmd.CommandText = cmd;
				sqlCmd.CommandTimeout = m_CommandTimeout;

				if (isSP)
				{
					sqlCmd.CommandType = CommandType.StoredProcedure;
				}
				else
				{
					sqlCmd.CommandType = CommandType.Text;
				}

				if (sqlParams == null)
				{
					sqlCmd.Parameters.Clear();
				}
				else
				{
					if (sqlParams.Count > 0)
					{
						foreach (SqlParameter sqlParam in sqlParams)
						{
							sqlCmd.Parameters.Add(sqlParam);

						}  // END foreach (SqlParameter sqlParam in sqlParams)

					}  // END if (sqlParams.Count > 0)

				}

				try
				{
					sqlConn.Open();
				}
				catch (InvalidOperationException exConnOp)
				{
					exConnOp.Data.Add("SQL Command", cmd);
					throw;
				}
				catch (SqlException exConnSql)
				{
					exConnSql.Data.Add("ConnectionString", connString);
					throw;
				}

				try
				{
					sqlCmd.ExecuteNonQuery();

					retVal.RetCode = 0;
				}
				catch (Exception exFill)
				{
					exFill.Data.Add("Failure during execution.", exFill.Message);
					throw;
				}

				if (sqlCmd.Parameters != null)
				{
					if (sqlCmd.Parameters.Count > 0)
					{
						if (sqlParams == null)
						{
							sqlParams = new List<SqlParameter>();
						}
						else
						{
							sqlParams.Clear();
						}

						foreach (SqlParameter sqlParam in sqlCmd.Parameters)
						{
							retVal.SQLParams.Add(new SqlParameter
							{
								CompareInfo = sqlParam.CompareInfo,
								DbType = sqlParam.DbType,
								Direction = sqlParam.Direction,
								IsNullable = sqlParam.IsNullable,
								LocaleId = sqlParam.LocaleId,
								Offset = sqlParam.Offset,
								ParameterName = sqlParam.ParameterName,
								Precision = sqlParam.Precision,
								Scale = sqlParam.Scale,
								Size = sqlParam.Size,
								SourceColumn = sqlParam.SourceColumn,
								SourceColumnNullMapping = sqlParam.SourceColumnNullMapping,
								SourceVersion = sqlParam.SourceVersion,
								SqlDbType = sqlParam.SqlDbType,
								SqlValue = sqlParam.SqlValue,
								TypeName = sqlParam.TypeName,
								UdtTypeName = sqlParam.UdtTypeName,
								Value = sqlParam.Value
							});
						}  // END foreach (SqlParameter sqlParam in sqlCmd.Parameters)

					}  // END if (sqlParams.Count > 0)

				}  // END if (sqlParams != null)

			}  // END try
			catch (Exception exUnhandled)
			{
				exUnhandled.Data.Add("Server", m_Server);
				exUnhandled.Data.Add("DefaultDB", m_DefaultDB);
				exUnhandled.Data.Add("UserName", m_UserName);
				exUnhandled.Data.Add("PortNumber", m_PortNumber.ToString());
				exUnhandled.Data.Add("UseAuthentication", m_UseAuthentication.ToString());
				exUnhandled.Data.Add("ConnectRetryCount", m_ConnectRetryCount.ToString());
				exUnhandled.Data.Add("ConnectRetryInterval", m_ConnectRetryInterval.ToString());
				exUnhandled.Data.Add("ApplicationName", m_ApplicationName);
				exUnhandled.Data.Add("ConnectionTimeout", m_ConnectionTimeout.ToString());
				exUnhandled.Data.Add("WorkstationID", m_WorkstationID);

				retVal.ErrorMessage = exUnhandled.GetFullExceptionMessage(true, true);

				throw;

			}
			finally
			{

				if (sqlCmd != null)
				{
					sqlCmd.Dispose();

					sqlCmd = null;
				}

				if (sqlConn != null)
				{
					if (sqlConn.State != ConnectionState.Closed)
					{
						sqlConn.Close();
					}

					sqlConn.Dispose();

					sqlConn = null;
				}
			}

			return retVal;

		}  // END public DBReturnValue ExecuteStatement(String cmd, Boolean isSP, List<SqlParameter> sqlParams)













	}   // END private class DataAccessLayer

}  // namespace Jeff.Jones.JLogger8
