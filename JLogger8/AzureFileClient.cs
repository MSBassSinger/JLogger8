using Azure;
using Azure.Storage.Files.Shares;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Files.Shares.Models;

namespace Jeff.Jones.JLogger8
{
	internal class AzureFileClient
	{


		private String m_ResourceID = "";
		private String m_FileShareName = "";
		private String m_DirectoryName = "";
		private String m_LocalFileName = "";
		private String m_RemoteFileName = "";



		public AzureFileClient(String resourceID, 
		                       String fileShareName, 
							   String directoryName, 
							   String remoteFileName, 
							   String localFileName)
		{
			m_ResourceID = resourceID;

			m_FileShareName = fileShareName;

			m_DirectoryName = directoryName;

			m_LocalFileName = localFileName;

			m_RemoteFileName = remoteFileName;
		}

		internal Boolean DoesAzureStorageExist()
		{

			Boolean retVal = false;

			ShareClient share = null;

			ShareDirectoryClient directory = null;

			try
			{
				share = new ShareClient(m_ResourceID, m_FileShareName);

				if (share.Exists())
				{

					directory = share.GetDirectoryClient(m_DirectoryName);

					if (directory.Exists())
					{
						retVal = true;
					}
				}
			}
			catch (Exception exUnhandled)
			{
				exUnhandled.Data.Add("m_FileShareName", m_FileShareName ?? "NULL");
				exUnhandled.Data.Add("m_DirectoryName", m_DirectoryName ?? "NULL");

				throw;
			}
			finally
			{
				directory = null;

				share = null;

			}

			return retVal;

		}

		internal Boolean DoesAzureLogFileExist()
		{

			Boolean retVal = false;

			ShareClient share = null;

			ShareDirectoryClient directory = null;

			ShareFileClient fileClient = null;

			try
			{
				share = new ShareClient(m_ResourceID, m_FileShareName);

				if (share.Exists())
				{

					directory = share.GetDirectoryClient(m_DirectoryName);

					if (directory.Exists())
					{
						fileClient = directory.GetFileClient(m_RemoteFileName);

						if (fileClient.Exists())
						{
							retVal = true;
						}
					}
				}
			}
			catch (Exception exUnhandled)
			{
				exUnhandled.Data.Add("m_FileShareName", m_FileShareName ?? "NULL");
				exUnhandled.Data.Add("m_DirectoryName", m_DirectoryName ?? "NULL");

				throw;
			}
			finally
			{
				directory = null;

				share = null;

				fileClient = null;

			}

			return retVal;

		}

		internal Boolean DeleteAzureLogFile(String fileNameToDelete)
		{

			Boolean retVal = false;

			ShareClient share = null;

			ShareDirectoryClient directory = null;

			ShareFileClient fileClient = null;

			try
			{
				share = new ShareClient(m_ResourceID, m_FileShareName);

				if (share.Exists())
				{

					directory = share.GetDirectoryClient(m_DirectoryName);

					if (directory.Exists())
					{
						fileClient = directory.GetFileClient(fileNameToDelete);

						fileClient.DeleteIfExists();

						retVal = true;
					}
				}
			}
			catch (Exception exUnhandled)
			{
				exUnhandled.Data.Add("m_FileShareName", m_FileShareName ?? "NULL");
				exUnhandled.Data.Add("m_DirectoryName", m_DirectoryName ?? "NULL");

				throw;
			}
			finally
			{
				directory = null;

				share = null;

				fileClient = null;

			}

			return retVal;

		}


		internal Boolean CreateShareAndDirectory()
		{

			Boolean retVal = false;

			ShareClient share = null;

			ShareDirectoryClient directory = null;

			try
			{
				share = new ShareClient(m_ResourceID, m_FileShareName);

				share.CreateIfNotExists();

				directory = share.GetDirectoryClient(m_DirectoryName);

				directory.CreateIfNotExists();

				retVal = true;
			}
			catch (Exception exUnhandled)
			{
				exUnhandled.Data.Add("m_FileShareName", m_FileShareName ?? "NULL");
				exUnhandled.Data.Add("m_DirectoryName", m_DirectoryName ?? "NULL");

				throw;
			}
			finally
			{
				directory = null;

				share = null;

			}

			return retVal;

		}

		internal List<ShareFileItem> GetListOfFiles(String filePattern)
		{

			List<ShareFileItem> retVal = null;

			ShareClient share = null;

			ShareDirectoryClient directory = null;

			try
			{
				share = new ShareClient(m_ResourceID, m_FileShareName);

				directory = share.GetDirectoryClient(m_DirectoryName);

				retVal = directory.GetFilesAndDirectories(filePattern).ToList<ShareFileItem>();

			}
			catch (Exception exUnhandled)
			{
				exUnhandled.Data.Add("m_FileShareName", m_FileShareName ?? "NULL");
				exUnhandled.Data.Add("m_DirectoryName", m_DirectoryName ?? "NULL");

				throw;
			}
			finally
			{
				directory = null;

				share = null;

			}

			return retVal;

		}

		internal Boolean CopyLogFileToLocal()
		{

			Boolean retVal = false;

			ShareClient share = null;

			ShareDirectoryClient directory = null;

			FileStream stream = null;

			ShareFileClient remoteFile = null;

			ShareFileDownloadInfo download = null;

			try
			{
				share = new ShareClient(m_ResourceID, m_FileShareName);

				directory = share.GetDirectoryClient(m_DirectoryName);

				remoteFile = directory.GetFileClient(m_RemoteFileName);

				download = remoteFile.Download();
				stream = File.OpenWrite(m_LocalFileName);
				download.Content.CopyTo(stream);

				retVal = true;
			}
			catch (Exception exUnhandled)
			{
				exUnhandled.Data.Add("m_FileShareName", m_FileShareName ?? "NULL");
				exUnhandled.Data.Add("m_DirectoryName", m_DirectoryName ?? "NULL");

				throw;
			}
			finally
			{
				if (download != null)
				{
					download.Dispose();

					download = null;
				}

				if (stream != null)
				{
					stream.Dispose();

					stream = null;
				}

				remoteFile = null;

				directory = null;

				share = null;
			}

			return retVal;

		}

		internal Boolean CopyLogFileToRemote()
		{

			Boolean retVal = false;

			ShareClient share = null;

			ShareDirectoryClient directory = null;

			FileStream stream = null;

			ShareFileClient remoteFile = null;

			try
			{
				share = new ShareClient(m_ResourceID, m_FileShareName);

				directory = share.GetDirectoryClient(m_DirectoryName);

				remoteFile = directory.GetFileClient(m_RemoteFileName);

				stream = File.OpenRead(m_LocalFileName);

				remoteFile.Create(stream.Length);
				remoteFile.UploadRange(new HttpRange(0, stream.Length), stream);

				retVal = true;
			}
			catch (Exception exUnhandled)
			{
				exUnhandled.Data.Add("m_FileShareName", m_FileShareName ?? "NULL");
				exUnhandled.Data.Add("m_DirectoryName", m_DirectoryName ?? "NULL");

				throw;
			}
			finally
			{

				if (stream != null)
				{
					stream.Dispose();

					stream = null;
				}

				remoteFile = null;

				directory = null;

				share = null;
			}

			return retVal;

		}






	}
}
