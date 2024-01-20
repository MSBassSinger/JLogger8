using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jeff.Jones.JLogger8;
using Jeff.Jones.JHelpers8;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JLogger8Test
{

	[TestClass]
	public class LoggerDBTest
	{

		private LOG_TYPE m_DebugLogOptions = Logger.DEFAULT_DEBUG_LOG_OPTIONS;

		[TestInitialize]
		public void SetupLogger()
		{
			Boolean response = false;

			LOG_TYPE logOptions = m_DebugLogOptions | LOG_TYPE.SendEmail;

			response = Logger.Instance.SetDBConfiguration("<COMPUTER_NAME>",
														  "Logger",
														  "Logger4Me",
														  false,
														  true,
														  "Test", 
														  1,
														  logOptions);

			Assert.IsTrue(response, "Failed to set Logger Data");

			Logger.Instance.DebugLogOptions = logOptions;

			List<String> sendToAddresses = new List<String>();

			sendToAddresses.Add("YourEmailAddress");

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
					Logger.Instance.WriteDebugLog(LOG_TYPE.Error | LOG_TYPE.SendEmail, exUnhandled, "This is detail message with email.");

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

						Logger.Instance.WriteDebugLog(LOG_TYPE.Error, exUnhandled, "This is detail message without sending email.");

					}
					catch (Exception exLog)
					{
						Assert.Fail(exLog.GetFullExceptionMessage(true, true));
					}
				}

			}

		}  // END public void WriteLogExceptionTest()


		[TestCleanup]
		public void TestShutdown()
		{
			System.Threading.Thread.Sleep(2000);

			Logger.Instance.Dispose();

			System.Threading.Thread.Sleep(2000);

			Logger.Instance.StopLog();
		}

	}  // END public class LoggerDBTest

}  // END namespace JLogger8Test
