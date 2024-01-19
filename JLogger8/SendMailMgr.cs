using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using Jeff.Jones.JHelpers8;
using System.Threading.Tasks;

namespace Jeff.Jones.JLogger8
{
    /// <summary>
    /// Class for sending email.
    /// </summary>
    internal class SendMailMgr
    {

        private String m_EmailServer = "";
        private String m_EmailLogonName = "";
        private String m_EmailPassword = "";
        private Int32 m_SMTPPort = 25;
        private List<String> m_SendToAddresses = null;
        private String m_FromAddress = "";
        private String m_ReplyToAddress = "";
		private Boolean m_UseSSL = false;

        /// <summary>
        /// Default constructor.  If you use this constructor, populate the required values via their properties.
        /// </summary>
        public SendMailMgr()
        {
            m_SendToAddresses = new List<String>();
        }

		/// <summary>
		/// Constructor to populate the necessary data.
		/// If the list of "Send To" addresses is sent as a null object,
		/// an empty list is created internally.
		/// </summary>
		/// <param name="emailServer">The name or IP address of the email server.</param>
		/// <param name="emailLogonName">Email server logon name.</param>
		/// <param name="emailPassword">Email server password.</param>
		/// <param name="sMTPPort">Port the SMTP server listens on, port 25 by default</param>
		/// <param name="sendToAddresses">A list of 1 to n addresses to send to.  At least one is required.</param>
		/// <param name="fromAddress">The address the email is from.  Required.</param>
		/// <param name="replyToAddress">The address a recipient would reply to.  Optional.</param>
		/// <param name="useSSL">True if he email server requires SSL</param>
		internal SendMailMgr(String emailServer,
                            String emailLogonName,
                            String emailPassword,
                            Int32 sMTPPort,
                            List<String> sendToAddresses,
                            String fromAddress,
                            String replyToAddress, 
							Boolean useSSL)
        {
            String emailAddressFailures = "";

            if (!CommonHelpers.IsEmailFormat(fromAddress))
            {
                emailAddressFailures = $"From address [{fromAddress}]; ";
            }

            replyToAddress = replyToAddress ?? "";

            if (replyToAddress.Length > 0)
            {
                if (!CommonHelpers.IsEmailFormat(replyToAddress))
                {
                    emailAddressFailures += $"Reply address [{replyToAddress}]; ";
                }
            }

            if (sendToAddresses == null)
            {
                sendToAddresses = new List<String>();
            }

            foreach (String toAddress in sendToAddresses)
            {
                if (!CommonHelpers.IsEmailFormat(toAddress))
                {
                    emailAddressFailures = $"To address [{toAddress}]; ";
                }
            }

            if (emailAddressFailures.Length > 0)
            {
                throw new ArgumentOutOfRangeException("These addresses do not appear to be in a valid format. " + emailAddressFailures);
            }

            if (sMTPPort <= 0)
            {
                throw new ArgumentOutOfRangeException($"The SMTP port MUST be greater than zero. [{sMTPPort.ToString()}].");
            }

            m_EmailServer = emailServer;
            m_EmailLogonName = emailLogonName;
            m_EmailPassword = emailPassword;
            m_SMTPPort = sMTPPort;

            if (sendToAddresses == null)
            {
                m_SendToAddresses = new List<String>();
            }
            else
            {
                m_SendToAddresses = sendToAddresses;
            }

            m_FromAddress = fromAddress;
            m_ReplyToAddress = replyToAddress;
			m_UseSSL = useSSL;

        }

        /// <summary>
        /// Method to send an emial, using the values provided to connect to the email server, 
        /// and the message body provided here.
        /// 
        /// Throws an InvalidOperationException if thre are no Send To addresses, or one or more addresses 
        /// are in an invalid format.
        /// 
        /// Throws an ApplicationException with an inner SmtpException if there is an exception 
        /// thrown when sending the email.
        /// </summary>
        /// <param name="messageBody">The text that goes in the message body.</param>
        /// <param name="isHTML">True if the messageBody is in HTML format.</param>
        /// <returns>True if the send is successful, false if not.</returns>
        internal Boolean SendEmail(String messageBody, Boolean isHTML)
        {
            Boolean retVal = false;

            SmtpClient smtpClient = null;
            NetworkCredential logonInfo = null;
            MailAddress fromAddress = null;
            MailAddress replyToAddress = null;
            MailMessage msg = null;

            try
            {
                String emailAddressFailures = "";

                if (m_SendToAddresses.Count == 0)
                {
                    throw new InvalidOperationException("There are no addresses given to send to.  You must supply at least one address to send to.");
                }

                foreach (String toAddress in m_SendToAddresses)
                {
                    if (!CommonHelpers.IsEmailFormat(toAddress))
                    {
                        emailAddressFailures = $"To address [{toAddress}]; ";
                    }
                }

                if (emailAddressFailures.Length > 0)
                {
                    throw new InvalidOperationException("These addresses do not appear to be in a valid format. " + emailAddressFailures);
                }

                smtpClient = new SmtpClient(m_EmailServer, m_SMTPPort);
                logonInfo = new NetworkCredential(m_EmailLogonName, m_EmailPassword);
                smtpClient.Credentials = logonInfo;

                fromAddress = new MailAddress(m_FromAddress);
                replyToAddress = new MailAddress(m_ReplyToAddress);
                msg = new MailMessage();

                msg.From = fromAddress;
                msg.ReplyToList.Add(replyToAddress);

                foreach (String toAddress in m_SendToAddresses)
                {
                    msg.To.Add(toAddress);
                }

                msg.IsBodyHtml = isHTML;

                msg.Body = messageBody;

                msg.Subject = "Log Message";

				smtpClient.EnableSsl = m_UseSSL;

				smtpClient.Send(msg);

                retVal = true;

            }  // END try
            catch (SmtpException ex)
            {
                ApplicationException exSMTP = new ApplicationException("There was a problem sending the email message.  The problem is most likely in the SMTP configuration used.", ex);
                exSMTP.Data.Add("FromAddress", m_FromAddress);
                exSMTP.Data.Add("Server", m_EmailServer);
                exSMTP.Data.Add("SMTPPort", m_SMTPPort.ToString());
                exSMTP.Data.Add("ReplyToAddress", m_ReplyToAddress);
                exSMTP.Data.Add("LogonName", m_EmailLogonName);
                exSMTP.Data.Add("Password", m_EmailPassword);
                exSMTP.Data.Add("SendToAddresses", m_SendToAddresses.ToString());

                throw exSMTP;

            }
            catch (Exception exUnhandled)
            {
                exUnhandled.Data.Add("Message Body", messageBody);
                throw;
            }
            finally
            {

                logonInfo = null;

                fromAddress = null;

                replyToAddress = null;

                if (msg != null)
                {
                    msg.Dispose();

                    msg = null;
                }


                if (smtpClient != null)
                {
                    smtpClient.Dispose();

                    smtpClient = null;
                }
            }

            return retVal;

        }

		internal Boolean SendEmail(DebugLogItem bodyItem, Boolean isHTML)
		{
			Boolean retVal = false;

			SmtpClient smtpClient = null;
			NetworkCredential logonInfo = null;
			MailAddress fromAddress = null;
			MailAddress replyToAddress = null;
			MailMessage msg = null;
			StringBuilder sb = null;

			try
			{
				String emailAddressFailures = "";

				if (m_SendToAddresses.Count == 0)
				{
					throw new InvalidOperationException("There are no addresses given to send to.  You must supply at least one address to send to.");
				}

				foreach (String toAddress in m_SendToAddresses)
				{
					if (!CommonHelpers.IsEmailFormat(toAddress))
					{
						emailAddressFailures = $"To address [{toAddress}]; ";
					}
				}

				if (emailAddressFailures.Length > 0)
				{
					throw new InvalidOperationException("These addresses do not appear to be in a valid format. " + emailAddressFailures);
				}

				smtpClient = new SmtpClient(m_EmailServer, m_SMTPPort);
				logonInfo = new NetworkCredential(m_EmailLogonName, m_EmailPassword);
				smtpClient.Credentials = logonInfo;

				fromAddress = new MailAddress(m_FromAddress);
				replyToAddress = new MailAddress(m_ReplyToAddress);
				msg = new MailMessage();

				msg.From = fromAddress;
				msg.ReplyToList.Add(replyToAddress);

				foreach (String toAddress in m_SendToAddresses)
				{
					msg.To.Add(toAddress);
				}

				msg.IsBodyHtml = isHTML;

				String messageBody = "";
				sb = new StringBuilder();

				sb.AppendLine($"Log entry for {bodyItem.LogDateTime.ToShortDateString()} {bodyItem.LogDateTime.ToShortTimeString()}");
				sb.AppendLine($"Message: {bodyItem.Message}");
				sb.AppendLine($"Details (opt): {bodyItem.DetailMessage}");
				sb.AppendLine($"Log Type: {bodyItem.TypeDescription}");
				sb.AppendLine($"Module: {bodyItem.ModuleName}");
				sb.AppendLine($"Method: {bodyItem.MethodName}");
				sb.AppendLine($"Line Number: {bodyItem.LineNumber.ToString()}");
				sb.AppendLine($"Additional Data (opt.): {bodyItem.ExceptionData}");
				sb.AppendLine($".NET Thread ID: {bodyItem.ThreadID.ToString()}");

				String stackData = bodyItem.StackData;

				stackData = bodyItem.StackData.Replace("|", Environment.NewLine + "\t");
				
				sb.AppendLine($"Stack Data: {stackData}");

				messageBody = sb.ToString();

				msg.Body = messageBody;

				msg.Subject = "Log Message";

				smtpClient.EnableSsl = m_UseSSL;

				smtpClient.Send(msg);

				retVal = true;

			}  // END try
			catch (SmtpException ex)
			{
				ApplicationException exSMTP = new ApplicationException("There was a problem sending the email message.  The problem is most likely in the SMTP configuration used.", ex);
				exSMTP.Data.Add("FromAddress", m_FromAddress);
				exSMTP.Data.Add("Server", m_EmailServer);
				exSMTP.Data.Add("SMTPPort", m_SMTPPort.ToString());
				exSMTP.Data.Add("ReplyToAddress", m_ReplyToAddress);
				exSMTP.Data.Add("LogonName", m_EmailLogonName);
				exSMTP.Data.Add("Password", m_EmailPassword);
				exSMTP.Data.Add("SendToAddresses", m_SendToAddresses.ToString());

				throw exSMTP;

			}
			catch (Exception exUnhandled)
			{
				exUnhandled.Data.Add("FromAddress", m_FromAddress);
				exUnhandled.Data.Add("Server", m_EmailServer);
				exUnhandled.Data.Add("SMTPPort", m_SMTPPort.ToString());
				exUnhandled.Data.Add("ReplyToAddress", m_ReplyToAddress);
				exUnhandled.Data.Add("LogonName", m_EmailLogonName);
				exUnhandled.Data.Add("Password", m_EmailPassword);
				exUnhandled.Data.Add("SendToAddresses", m_SendToAddresses.ToString());

				throw;
			}
			finally
			{

				logonInfo = null;

				fromAddress = null;

				replyToAddress = null;

				if (msg != null)
				{
					msg.Dispose();

					msg = null;
				}


				if (smtpClient != null)
				{
					smtpClient.Dispose();

					smtpClient = null;
				}

				if (sb != null)
				{
					sb.Clear();

					sb = null;
				}
			}

			return retVal;

		}


		/// <summary>
		/// DNS name or IP address of the SMTP email server.
		/// Throws an ArgumentOutOfRangeException if the name is blank or null.
		/// </summary>
		internal String EmailServer
        {
            get
            {
                return m_EmailServer;
            }
            set
            {
                value = value ?? "";

                if (value.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("The email server name cannot be blank.");
                }
                else
                {
                    m_EmailServer = value;
                }
            }
        }

        /// <summary>
        /// Logon name required by the email server.
        /// </summary>
        internal String EmailLogonName
        {
            get
            {
                return m_EmailLogonName;
            }
            set
            {
                m_EmailLogonName = value ?? "";
            }
        }

        /// <summary>
        /// Password required by the email server.
        /// </summary>
        internal String EmailPassword
        {
            get
            {
                return m_EmailPassword;
            }
            set
            {
                m_EmailPassword = value ?? "";
            }
        }

        /// <summary>
        /// The port the SMTP email server is listening on.  Default is 25.
        /// Throws an ArgumentOutOfRangeException if less than or equal to zero.
        /// </summary>
        internal Int32 SMTPPort
        {
            get
            {
                return m_SMTPPort;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException($"The SMTP port MUST be greater than zero. [{value.ToString()}].");
                }
                else
                {
                    m_SMTPPort = value;
                }
            }
        }

        /// <summary>
        /// List of addresses to send to.  
        /// </summary>
        internal List<String> SendToAddresses
        {
            get
            {
                return m_SendToAddresses;
            }
        }

        /// <summary>
        /// The address that shows as "From" in the email.
        /// Throws an ArgumentOutOfRangeException if the value is null or empty, or in an invalid format.
        /// </summary>
        internal String FromAddress
        {
            get
            {
                return m_FromAddress;
            }
            set
            {
                value = value ?? "";

                if (value.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("The from address cannot be blank.");
                }
                else
                {
                    if (!CommonHelpers.IsEmailFormat(value))
                    {
                        throw new ArgumentOutOfRangeException($"The from address appears to not be in the correct format [{value}].");
                    }
                    else
                    {
                        m_FromAddress = value;
                    }
                }
            }
        }

        /// <summary>
        /// The address that "reply to" uses for the email recipient.
        /// Throws an ArgumentOutOfRangeException if the value is in an invalid format.
        /// </summary>
        internal String ReplyToAddress
        {
            get
            {
                return m_ReplyToAddress;
            }
            set
            {
                value = value ?? "";

                if (value.Length > 0)
                {
                    if (!CommonHelpers.IsEmailFormat(value))
                    {
                        throw new ArgumentOutOfRangeException($"The reply to address appears to not be in the correct format [{value}].");
                    }
                    else
                    {
                        m_ReplyToAddress = value;
                    }

                }
            }



        }
    }
}
