using System;
using System.IO;
using log4net;
	
namespace EricDaugherty.CSES.SmtpServer
{
	/// <summary>
	/// Stores Spooled SMTPMessages as files.
	/// </summary>
	public class FileMessageSpool : IMessageSpool
	{
		#region Variables
		
		private static readonly ILog log = LogManager.GetLogger( typeof( FileMessageSpool ) );
		
		#endregion
		
		/// <summary>
		/// Initialize the file spool with the correct directory.
		/// </summary>
		public FileMessageSpool( string directory )
		{
			if( !Directory.Exists( directory ) )
			{
				log.Warn( "FileSpool directory does not exist.  Attempting to create..." );
				try 
				{
					Directory.CreateDirectory( directory );
				}
				catch( Exception exception )
				{
					log.Error( String.Format( "Error creating FileSpool directory: {0}.  Exception {1}", directory, exception ), exception );
					throw exception;
				}
			}
		}
		
		/// <summary>
		/// Not Implemented.
		/// </summary>
		/// <remarks>
		/// Interface method from IMessageSpool.
		/// </remarks>
		/// <param name='message'>The message to spool.</param>
		public virtual bool SpoolMessage( SMTPMessage message )
		{
			throw new System.NotImplementedException();
		}
	}
}
