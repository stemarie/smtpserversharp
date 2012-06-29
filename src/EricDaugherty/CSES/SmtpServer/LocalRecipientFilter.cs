namespace EricDaugherty.CSES.SmtpServer
{

	using System;
	using EricDaugherty.CSES.Common;
	
	
	/// <summary>
	/// Allows all email addresses addressed to the local domain specified
	/// in the constructor.
	/// </summary>	
	public class LocalRecipientFilter : IRecipientFilter {
		
		#region Variables
		
		private string domain;
		
		#endregion
		
		#region Constructors
		
		/// <summary>
		/// Specifies the domain to accept email for.
		/// </summary>
		public LocalRecipientFilter( string domain ) 
		{
			this.domain = domain.ToLower();
		}
		
		#endregion
		
		#region IRecipientFilter methods
		
		/// <summary>
		/// Accepts only local email.
		/// </summary>
		/// <param name='context'>The SMTPContext</param>
		/// <param name='recipient'>TODO - add parameter description</param>
		public virtual bool AcceptRecipient( SMTPContext context, EmailAddress recipient )
		{
			return domain.Equals( recipient.Domain );
		}
		
		#endregion
	}
}
