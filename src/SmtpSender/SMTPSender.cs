using log4net;
using src.Common;

namespace src.SmtpSender
{
	/// <summary>
	/// This class provides the ability to deliver
	/// an email message to a SMTP Server.
	/// </summary>
	/// <remarks>
	/// Incomplete.
	/// </remarks>
	public class SMTPSender
	{
		#region Variables
		
		private static ILog log = LogManager.GetLogger( typeof( SMTPSender ) );
		
		#endregion
		
		#region Constructors
		
		#endregion
		
		#region Public Methods
		
		/// <summary>
		/// Delivers an email message to the SMTP server for the specified
		/// domain.  All addresses in the array should be for the same domain.
		/// </summary>
		/// <param name="domain">
		/// The internet domain name to use to lookup the SMTP server address.
		/// </param>
		/// <param name="addresses">
		/// A set of addresses that should all be delivered to.
		/// </param>
		/// <param name="data">
		/// The raw message data.
		/// </param>
		public void DeliverMessage( string domain, EmailAddress[] addresses, string data )
		{
			
		}
		
		#endregion
		
		#region Private Methods
		
		private void Deliver( string domain )
		{
//			Dns.r
		}
		
		#endregion
	}
}
