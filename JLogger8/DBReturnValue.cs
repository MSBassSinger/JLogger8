using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Jeff.Jones.JLogger8
{
	/// <summary>
	/// Class is used for returning multiple values after executing a SQL statement.
	/// </summary>
	internal class DBReturnValue : IDisposable
	{
		private Boolean m_blnDisposeHasBeenCalled = false;
		private List<SqlParameter> m_SQLParams = null;
		private Int32 m_RetCode = 0;
		private String m_ErrorMessage = "";
		private DataSet m_Dataset = null;

		/// <summary>
		/// Empty constructor.  This will initialize the backing variables for the properties.  
		/// The SQLParameter list will be instantiated, and empty.
		/// No dataset is passed in at construction.  It is set by the procedure executing the 
		/// query in JDataAccess.
		/// </summary>
		public DBReturnValue()
		{
			m_SQLParams = new List<SqlParameter>();
		}

		/// <summary>
		/// Constructor to populate the properties at construction time.
		/// </summary>
		/// <param name="sqlParams">SQL Parameters.  May be null.</param>
		/// <param name="retCode">A return code, usually provided from a SQL call.</param>
		/// <param name="errorMessage">String describing the full stack of exceptions, or "" if no exceptions.</param>
		public DBReturnValue(List<SqlParameter> sqlParams,
							 Int32 retCode,
							 String errorMessage)
		{

			this.SQLParams = sqlParams;
			this.RetCode = retCode;
			this.ErrorMessage = errorMessage;

		}

		/// <summary>
		/// The SQL parameters to use.  If there are none, the 
		/// list has zero members.  If the value is null, the 
		/// list is instantiated as empty.
		/// </summary>
		public List<SqlParameter> SQLParams
		{
			get
			{
				if (m_SQLParams == null)
				{
					m_SQLParams = new List<SqlParameter>();
				}

				return m_SQLParams;
			}
			set
			{
				if (value == null)
				{
					m_SQLParams = new List<SqlParameter>();
				}
				else
				{
					m_SQLParams = value;
				}
			}
		}

		/// <summary>
		/// A numeric return code to be interpreted by the caller.
		/// </summary>
		public Int32 RetCode
		{
			get
			{
				return m_RetCode;
			}
			set
			{
				m_RetCode = value;
			}
		}

		/// <summary>
		/// An error message coming back from either the function called, or from SQL Server.
		/// </summary>
		public String ErrorMessage
		{
			get
			{
				return m_ErrorMessage;
			}
			set
			{
				m_ErrorMessage = value;
			}
		}

		/// <summary>
		/// For calls that return data, the dataset goes here.
		/// </summary>
		public DataSet Data
		{
			get
			{
				return m_Dataset;
			}
			set
			{
				m_Dataset = value;
			}
		}

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
		~DBReturnValue()
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

				if (m_Dataset != null)
				{
					m_Dataset.Clear();
					m_Dataset.Dispose();
					m_Dataset = null;
				}


				// Set the flag that Dispose has been called and executed.
				m_blnDisposeHasBeenCalled = true;

			}

			catch (Exception exUnhandled)
			{

				exUnhandled.Data.Add("m_blnDisposeHasBeenCalled", m_blnDisposeHasBeenCalled.ToString());

				throw;


			}
		}

		#endregion IDisposable Implementation



	}  // END public class DBReturnValue
}
