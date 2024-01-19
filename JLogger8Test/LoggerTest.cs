using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jeff.Jones.JLogger8;
using Jeff.Jones.JHelpers8;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using Microsoft.Extensions.Logging;

namespace JLogger8Test
{
	[TestClass]
	public class LoggerTest
	{
		private LOG_TYPE m_DebugLogOptions = Logger.DEFAULT_DEBUG_LOG_OPTIONS;


		[TestInitialize]
		public void SetupLogger()
		{
			Boolean response = false;
			String filePath = CommonHelpers.CurDir + @"\";
			LOG_TYPE logOptions = m_DebugLogOptions | LOG_TYPE.SendEmail;

			response = Logger.Instance.SetLogData(filePath, "JTest", 1, logOptions, "");

			Assert.IsTrue(response, "Failed to set Logger Data");

			List<String> sendToAddresses = new List<String>();

			sendToAddresses.Add("MSBassSinger@comcast.net");
			sendToAddresses.Add("PamJones4@comcast.net");

			response = Logger.Instance.SetEmailData("smtp.host.net",
											"user@host.net",
											"Pa$$w0rd",
											587,
											sendToAddresses,
											"user@host.net",
											"user@host.net",
											true);


			Assert.IsTrue(response, "Failed to set SendMail Data");

			response = Logger.Instance.StartLog();

			Assert.IsTrue(response, "Failed to start the log");

			//DebugLogItem dbgItem = new DebugLogItem();

			//JsonSerializerOptions options = new JsonSerializerOptions();
			//options.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
			//options.PropertyNameCaseInsensitive = true;
			//options.WriteIndented = true;

			//String itemJson = JsonSerializer.Serialize<DebugLogItem>(dbgItem, options);

			//File.WriteAllText($"{filePath}DebugLogItem.json", itemJson);

		}


		[TestMethod]
		public void WriteLogExceptionTest()
		{

			Exception exTest = new Exception("original exception");
			exTest.Data.Add("Data", "ABC");
			exTest.Data.Add("Number", "123");

			Exception exTest2 = new Exception("Next level exception", exTest);
			exTest2.Data.Add("Data", "DeF");
			exTest2.Data.Add("Number", "9876");

			try
			{
				throw exTest2;
			}
			catch (Exception exUnhandled)
			{
				Boolean response = false;

				if ((m_DebugLogOptions & LOG_TYPE.Error) == LOG_TYPE.Error)
				{
					Logger.Instance.WriteDebugLog(LOG_TYPE.Error | LOG_TYPE.SendEmail, exUnhandled, "This is detail message.");

					try
					{
						Parallel.For(1, 1000,
								index =>
								{
									try
									{
										LOG_TYPE msgType = LOG_TYPE.Unspecified;

										if ((index % 2) == 0)
										{
											msgType = LOG_TYPE.Test;
										}
										else if ((index % 3) == 0)
										{
											msgType = LOG_TYPE.Warning;
										}
										else if ((index % 5) == 0)
										{
											msgType = LOG_TYPE.Error;
										}
										else if ((index % 7) == 0)
										{
											msgType = LOG_TYPE.Flow;
										}
										else
										{
											msgType = LOG_TYPE.System;
										}

										response = Logger.Instance.WriteDebugLog(msgType, $"The {msgType.ToString()} is working.", $"This is detail message #{index.ToString()}."); ;

									}
									catch (Exception exThread)
									{
										Assert.Fail(exThread.GetFullExceptionMessage(true, true));
									}
								});

						System.Threading.Thread.Sleep(2000);

						Logger.Instance.Dispose();

						System.Threading.Thread.Sleep(2000);

					}
					catch (Exception exLog)
					{
						Assert.Fail(exLog.GetFullExceptionMessage(true, true));
					}
				}

			}

		}

	}
}
